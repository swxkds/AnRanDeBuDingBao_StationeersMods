using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Util;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 批量阵列放置 : 批量拆除和装配
    {
        // 锚点不一定是模型的中心点
        public new Dictionary<(Grid3 网格坐标, StructureElement 方向), (Matrix4x4 锚点所在变换矩阵, Structure 放置结构, int 放置结构索引)> 所有已选择 { get; protected set; }
        public new List<(Matrix4x4, Thing)> 所有已选择缓存 { get; protected set; }      // 间接渲染要求变换矩阵和建模网格
        private List<(Grid3, StructureElement)> 已失效表;
        private const float 关闭高亮间隔 = 2f;
        private float 关闭高亮间隔计时 = 0;
        private bool 开始长按右键 = false;

        public static new T 构造函数<T>() where T : 批量阵列放置, new()
        {
            var _this = new T { 当前状态 = 批量操作任务状态.睡眠 };
            _this.所有已选择 ??= new();
            _this.所有已选择缓存 ??= new();
            _this.已失效表 ??= new();
            _this.事件队列 ??= new();
            return _this;
        }

        private bool 放置被阻挡了么(Structure Arg_放置结构, Matrix4x4 Arg_变换矩阵)
        {
            // Arg_放置结构必须是InventoryManager._constructionCursors中的某个引用。 原版放置时提供了一个高亮物体, 修改位置与旋转后调用原版逻辑判断是否被阻挡
            var 探针 = Arg_放置结构;
            if (探针 == null) { return true; }

            var 原位置 = 探针.ThingTransformPosition;
            var 原旋转 = 探针.ThingTransformRotation;

            try
            {
                探针.ThingTransformPosition = Arg_变换矩阵.GetPosition();
                探针.ThingTransformRotation = Arg_变换矩阵.rotation;

                var 判定结果 = 探针.CanConstruct();
                return !判定结果.CanConstruct;
            }
            finally
            {
                探针.ThingTransformPosition = 原位置;
                探针.ThingTransformRotation = 原旋转;
            }
        }

        protected void 增加缓存物体((Grid3 网格坐标, StructureElement 方向) Arg_键, (Matrix4x4 锚点所在变换矩阵, Structure 放置结构) Arg_值, int 放置结构索引)
        {
            所有已选择.Add(Arg_键, (Arg_值.锚点所在变换矩阵, Arg_值.放置结构, 放置结构索引));
            所有已选择缓存.Add(Arg_值);
        }

        protected void 减少缓存物体((Grid3 网格坐标, StructureElement 方向) Arg_键, (Matrix4x4 锚点所在变换矩阵, Structure 放置结构) Arg_值)
        {
            所有已选择.Remove(Arg_键);
            所有已选择缓存.Remove(Arg_值);
        }

        protected new void 更新缓存()
        {
            所有已选择缓存.Clear();
            foreach (var 当前 in 所有已选择.Values) { 所有已选择缓存.Add((当前.锚点所在变换矩阵, 当前.放置结构)); }
        }

        protected new void Clear()
        {
            所有已选择.Clear();
            所有已选择缓存.Clear();
        }

        public new void Dispose()
        {
            base.Dispose();

            所有已选择?.Clear();
            所有已选择 = null;

            所有已选择缓存?.Clear();
            所有已选择缓存 = null;

            已失效表?.Clear();
            已失效表 = null;
        }

        protected new void 清理已失效()
        {
            if (所有已选择 == null || 所有已选择.Count == 0) { return; }

            已失效表.Clear();

            // 顺向遍历时, 如果删除了元素, 会导致后续的所有元素向前移动, 容器元素数变小, 然后遍历器按照旧的索引获取元素, 就会出现索引错位, 甚至出现尾元素时索引溢出
            foreach (var __ in 所有已选择)
            {
                if (放置被阻挡了么(__.Value.放置结构, __.Value.锚点所在变换矩阵))
                {
                    已失效表.Add(__.Key);
                }
            }

            bool 是否需要更新缓存 = false;

            foreach (var key in 已失效表)
            {
                if (!是否需要更新缓存) { 是否需要更新缓存 = true; }
                所有已选择.Remove(key);
            }

            if (是否需要更新缓存) { 更新缓存(); }
        }

        public bool 获取视线处网格(out (Grid3 锚点所在网格, StructureElement 方向, Vector3 锚点坐标, int 放置结构索引) 当前)
        {
            int BuildIndex;
            var Parent = 通用工具.获取活动手槽位和空闲手槽位().Item1.Get();
            switch (Parent)
            {
                case MultiConstructor 多变种:
                    BuildIndex = 多变种.LastSelectedIndex;
                    var 当前放置结构 = 多变种.Constructables[BuildIndex];
                    InventoryManager.UpdatePlacement(当前放置结构);
                    break;
                case Constructor 单变种:
                    BuildIndex = -2;
                    当前放置结构 = 单变种.BuildStructure;
                    InventoryManager.UpdatePlacement(当前放置结构);
                    break;
                default:
                    BuildIndex = -1;
                    InventoryManager.ConstructionCursor = null;
                    break;
            }

            if (InventoryManager.ConstructionCursor == null) { 当前 = (Grid3.zero, StructureElement.Center, Vector3.zero, BuildIndex); return false; }

            var 放置结构 = InventoryManager.ConstructionCursor;

            // 根据摄像机朝向计算出视线前方网格的向量, 第一个参数是步进/吸附精度(如果当前建造物是网格尺寸>0.5f的大网格, 用0.3f；否则是小网格, 用0.6f。
            var 摄像机前方网格 = InputHelpers.GetCameraForwardGrid((放置结构.GridSize > 0.5f) ? 0.3f : 0.6f, 放置结构.GetCursorOffset, out var 视线向量);
            var 微调 = (放置结构 is Objects.Pipes.IChute) ? (视线向量 * 0.1f) : Vector3.zero;
            var 锚点坐标 = 放置结构.GetWorldGrid(摄像机前方网格 + 微调);

            switch (放置结构)
            {
                case Assets.Scripts.Objects.Wall:
                    var 墙体坐标 = 锚点坐标 - 放置结构.ThingTransform.forward * 放置结构.GridSize / 2f;
                    放置结构.ThingTransformPosition = 墙体坐标;
                    当前 = (放置结构.GridPosition, 墙体方向(墙体坐标.ToGridPosition(), 放置结构.GridPosition), 墙体坐标, BuildIndex);
                    return true;
                case Objects.Structures.Frame:
                case SmallGrid:
                    放置结构.ThingTransformPosition = 锚点坐标;
                    当前 = (放置结构.GridPosition, StructureElement.Center, 锚点坐标, BuildIndex);
                    return true;
            }

            当前 = (Grid3.zero, StructureElement.Center, Vector3.zero, BuildIndex); return false;

            static StructureElement 墙体方向(Grid3 锚点坐标, Grid3 大网格中心坐标)
            {
                // {
                //     var 夹角 = Vector3.Dot(源_前向, Vector3.forward);
                //     var 方向 = StructureElement.North;

                //     var bd = Vector3.Dot(源_前向, Vector3.back);
                //     if (bd > 夹角)
                //     {
                //         夹角 = bd;
                //         方向 = StructureElement.South;
                //     }
                //     var ld = Vector3.Dot(源_前向, Vector3.left);
                //     if (ld > 夹角)
                //     {
                //         夹角 = ld;
                //         方向 = StructureElement.West;
                //     }
                //     var rd = Vector3.Dot(源_前向, Vector3.right);
                //     if (rd > 夹角)
                //     {
                //         夹角 = rd;
                //         方向 = StructureElement.East;
                //     }
                //     var ud = Vector3.Dot(源_前向, Vector3.up);
                //     if (ud > 夹角)
                //     {
                //         夹角 = ud;
                //         方向 = StructureElement.Up;
                //     }
                //     var dd = Vector3.Dot(源_前向, Vector3.down);
                //     if (dd > 夹角)
                //     {
                //         夹角 = dd;
                //         方向 = StructureElement.Down;
                //     }
                // }

                var 方向 = 大网格中心坐标 + Grid3.Face.North;
                if (锚点坐标 == 方向) { return StructureElement.North; }

                方向 = 大网格中心坐标 + Grid3.Face.East;
                if (锚点坐标 == 方向) { return StructureElement.East; }

                方向 = 大网格中心坐标 + Grid3.Face.South;
                if (锚点坐标 == 方向) { return StructureElement.South; }

                方向 = 大网格中心坐标 + Grid3.Face.West;
                if (锚点坐标 == 方向) { return StructureElement.West; }

                方向 = 大网格中心坐标 + Grid3.Face.Up;
                if (锚点坐标 == 方向) { return StructureElement.Up; }

                方向 = 大网格中心坐标 + Grid3.Face.Down;
                if (锚点坐标 == 方向) { return StructureElement.Down; }

                return StructureElement.Center;
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public override void Update()
        {
            var 之前状态 = 当前状态;
            switch (当前状态)
            {
                case 批量操作任务状态.睡眠:
                    {
                        当前状态 = 目标状态;
                        break;
                    }
                case 批量操作任务状态.开关高亮:
                    {
                        清理已失效();

                        if (KeyManager.GetMouseUp("Secondary")) { 当前状态 = 批量操作任务状态.睡眠; break; }
                        break;
                    }
                case 批量操作任务状态.选择:
                    {
                        if (KeyManager.GetMouse("Secondary") && 开始长按右键)
                        {
                            关闭高亮间隔计时 += Time.deltaTime;
                            if (关闭高亮间隔计时 >= 关闭高亮间隔)
                            {
                                开始长按右键 = false;
                                关闭高亮间隔计时 = 0;
                                当前状态 = 批量操作任务状态.睡眠; break;
                            }
                        }
                        else if (KeyManager.GetMouse("Secondary") && !开始长按右键)
                        {
                            开始长按右键 = true;
                            关闭高亮间隔计时 = 0;
                        }

                        if (KeyManager.GetMouseUp("Primary") && 获取视线处网格(out var 锚点))
                        {
                            var Id = (锚点.锚点所在网格, 锚点.方向);
                            if (所有已选择.ContainsKey(Id))
                            {
                                减少缓存物体(Id, (所有已选择[Id].锚点所在变换矩阵, 所有已选择[Id].放置结构));
                            }
                            else
                            {
                                var 当前放置结构 = InventoryManager.ConstructionCursor;
                                if (当前放置结构 != null)
                                {
                                    增加缓存物体(Id, (Matrix4x4.TRS(锚点.锚点坐标, 当前放置结构.ThingTransformRotation, Vector3.one), 当前放置结构), 锚点.放置结构索引);
                                }
                            }
                        }
                        break;
                    }
                case 批量操作任务状态.框选:
                    {

                        break;
                    }
                case 批量操作任务状态.装配:
                    {
                        清理已失效();

                        if (所有已选择.Count == 0) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        (Item 材料, int 数量) 安装A = (null, 0);
                        (Item 材料, int 数量) 安装B = (null, 0);
                        bool 是否可装配 = false;

                        int 可装次数 = 0;

                        var (活动手, 空闲手) = 通用工具.获取活动手槽位和空闲手槽位();
                        var 主手物 = 活动手.Get<Item>();
                        var 副手物 = 空闲手.Get<Item>();

                        Tool 主手槽工具 = null;

                        // 建造阶段里的材料配方里的材料若是工具, 则消耗数量始终为0
                        foreach (var 当前 in 所有已选择.Values)
                        {
                            Structure 建筑 = 当前.放置结构;
                            var 建造阶段 = 建筑.BuildStates.FirstOrDefault();
                            if (建造阶段 == null) { continue; }
                            {
                                var 材料配方 = 建造阶段.Tool;
                                if (材料配方 == null) { continue; }

                                if (材料配方.ToolEntry)
                                {
                                    if (材料配方.IsToolEntry(主手物))
                                    {
                                        主手槽工具 = 主手物 as Tool;
                                        if (主手槽工具 && !主手槽工具.IsOperable)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                if (材料配方.ToolEntry2)
                                {
                                    if (材料配方.IsToolEntry2(副手物))
                                    {
                                        if (副手物 is Tool 副手槽工具 && !副手槽工具.IsOperable)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                // 材料配方中的安装配方固定是单材料或者一工具一材料或者单工具, 材料配方中的拆除配方固定是一工具, 安装消耗就是安装配方中的材料
                                // 例: 铁框架需要焊枪与铁板安装, 安装A材料为焊枪, 安装B材料为铁板, 此时消耗的为B材料铁板
                                // 例: 铁墙只需要铁板安装, 因此安装A为铁板, 安装B材料为空, 此时消耗的材料为A材料铁板
                                // 例: 自动车床最后阶段只需要螺丝刀安装, 因此安装A为螺丝刀, 安装B材料为空, 此时消耗的材料为无
                                if ((材料配方.ToolEntry == null || 材料配方.ToolEntry is Tool) && (材料配方.ToolEntry2 == null || 材料配方.ToolEntry2 is Tool))
                                {
                                    可装次数 = int.MaxValue;
                                }
                                else if (材料配方.ToolEntry is not Tool)
                                {
                                    switch (材料配方.ToolEntry)
                                    {
                                        // 如果材料不是可堆叠材料, 可装次数为0
                                        case Stackable 可堆叠材料A:
                                            if (主手物 && 主手物.PrefabHash == 可堆叠材料A.PrefabHash)
                                            {
                                                可装次数 = Mathf.FloorToInt(((Stackable)主手物).Quantity / 材料配方.EntryQuantity);
                                            }
                                            break;
                                    }
                                }
                                else if (材料配方.ToolEntry2 is not Tool)
                                {
                                    switch (材料配方.ToolEntry2)
                                    {
                                        // 如果材料不是可堆叠材料, 可装次数为0
                                        case Stackable 可堆叠材料B:
                                            // 主手在上面已经判断过了, 此处不需要重复判断主手是否匹配主手材料
                                            if (副手物 && 副手物.PrefabHash == 可堆叠材料B.PrefabHash)
                                            {
                                                可装次数 = Mathf.FloorToInt(((Stackable)副手物).Quantity / 材料配方.EntryQuantity2);
                                            }
                                            break;
                                    }
                                }

                                if (可装次数 == 0) { continue; }

                                是否可装配 = true;
                                安装A = (材料配方.ToolEntry, 材料配方.EntryQuantity);
                                安装B = (材料配方.ToolEntry2, 材料配方.EntryQuantity2);

                                break;
                            }
                        }

                        if (!是否可装配) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        foreach (var 当前 in 所有已选择.Values)
                        {
                            Structure 建筑 = 当前.放置结构;
                            if (可装次数 <= 0) { break; }

                            var 建造阶段 = 建筑.BuildStates.FirstOrDefault();
                            if (建造阶段 == null) { continue; }

                            var 材料配方 = 建造阶段.Tool;
                            if (材料配方 == null) { continue; }

                            if ((材料配方.ToolEntry == null || 材料配方.ToolEntry is Tool) && (材料配方.ToolEntry2 == null || 材料配方.ToolEntry2 is Tool))
                            {
                                if (材料配方.IsToolEntry(主手物))
                                {
                                    添加事件(() => 放置建筑(当前.锚点所在变换矩阵, 建筑, 当前.放置结构索引));
                                    continue;
                                }
                            }
                            else if (材料配方.ToolEntry is not Tool)
                            {
                                if (材料配方.ToolEntry.PrefabHash == 安装A.材料.PrefabHash && 材料配方.EntryQuantity == 安装A.数量)
                                {
                                    --可装次数;
                                    添加事件(() => 放置建筑(当前.锚点所在变换矩阵, 建筑, 当前.放置结构索引));
                                    continue;
                                }
                            }
                            else if (材料配方.ToolEntry2 is not Tool)
                            {
                                if (材料配方.IsToolEntry(主手物) && 材料配方.ToolEntry2.PrefabHash == 安装B.材料.PrefabHash && 材料配方.EntryQuantity2 == 安装B.数量)
                                {
                                    --可装次数;
                                    添加事件(() => 放置建筑(当前.锚点所在变换矩阵, 建筑, 当前.放置结构索引));
                                    continue;
                                }
                            }
                        }

                        当前状态 = 批量操作任务状态.睡眠;
                        break;
                    }
                case 批量操作任务状态.清空高亮:
                    {
                        Clear();
                        当前状态 = 批量操作任务状态.睡眠;
                        break;
                    }
            }

            if (当前状态 == 之前状态) { return; }

            switch (当前状态)
            {
                case 批量操作任务状态.睡眠:
                    {
                        快捷键配置.快捷轮盘菜单_批量阵列放置_高亮开关 = false;
                        功能模块之快捷轮盘菜单.Log.LogMessage("批量阵列放置：已选择锚点关闭渲染");
                        break;
                    }
                default:
                    {
                        开始长按右键 = false;
                        关闭高亮间隔计时 = 0;
                        清理已失效();
                        快捷键配置.快捷轮盘菜单_批量阵列放置_高亮开关 = true;
                        功能模块之快捷轮盘菜单.Log.LogMessage("批量阵列放置：已选择锚点开启渲染");
                        break;
                    }
            }
        }

        private void 放置建筑(Matrix4x4 锚点所在变换矩阵, Structure 放置结构, int 放置结构索引)
        {
            if (放置结构索引 == -1) { return; }

            var 背包管理器 = InventoryManager.Instance;
            var 玩家 = InventoryManager.Parent;

            var 位置 = 锚点所在变换矩阵.GetPosition();
            var 旋转 = 锚点所在变换矩阵.rotation;

            if (NetworkManager.IsClient)
            {
                var 建造数据包 = new CreateStructureMessage
                {
                    // 优先通过主手获取槽位物品
                    ConstructorId = 背包管理器.ActiveHand.Slot.Get()?.ReferenceId ?? 0,
                    // 建造时可能需要的副手物品
                    OffhandOccupantReferenceId = 背包管理器.InactiveHand.Slot.Get()?.ReferenceId ?? 0,
                    // 放置蓝图的位置 = 建筑的位置
                    LocalPosition = 位置.ToGridPosition(),
                    // 放置蓝图的旋转 = 建筑的旋转
                    Rotation = 旋转,
                    // 发送数据包的玩家Id
                    CreatorSteamId = (ulong)InventoryManager.ParentBrain.ReferenceId,
                    // 主手物品.Constructables[放置蓝图Id]获取到放置蓝图引用
                    OptionIndex = 放置结构索引,
                    // 若引用Id没找到物品, 则通过PrefabHash找到物品, 此处一般用于游戏框架设计时加上的, 实际用不用另说
                    PrefabHash = 背包管理器.ActiveHand.Slot.Get()?.PrefabHash ?? 0,
                    AuthoringMode = InventoryManager.IsAuthoringMode,
                };

                NetworkClient.SendToServer(建造数据包, NetworkChannel.GeneralTraffic);
                return;
            }

            if (放置结构索引 == -2)
            {
                OnServer.UseItemPrimary(玩家, 背包管理器.ActiveHand.SlotId, 位置, 旋转, InventoryManager.ParentBrain.ClientId, 放置结构);
            }
            else if (放置结构索引 >= 0)
            {
                OnServer.UseMultiConstructor(玩家, 背包管理器.ActiveHand.SlotId, 背包管理器.InactiveHand.SlotId, 位置, 旋转, 放置结构索引, InventoryManager.IsAuthoringMode, InventoryManager.ParentBrain.ClientId, 放置结构);
            }
        }
    }
}
