using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 批量阵列放置 : 批量拆除和装配
    {
        // 锚点不一定是模型的中心点
        public new Dictionary<(Grid3 锚点所在网格, 批量选择网格类型 网格索引方式), (Matrix4x4 锚点所在变换矩阵, Structure 放置结构)> 所有已选择 { get; protected set; }
        public new List<(Matrix4x4, Thing)> 所有已选择缓存 { get; protected set; }      // 间接渲染要求变换矩阵和建模网格
        private List<(Grid3 锚点所在网格, 批量选择网格类型 网格索引方式)> 已失效表;
        private const int 锚点数量上限 = 512;
        private const float 放置间隔 = 2f;
        private float 放置间隔计时 = 0;

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

        protected void 增加缓存物体((Grid3 锚点所在网格, 批量选择网格类型 网格索引方式) Arg_键, (Matrix4x4 锚点所在变换矩阵, Structure 放置结构) Arg_值)
        {
            所有已选择.Add(Arg_键, Arg_值);
            所有已选择缓存.Add(Arg_值);
        }

        protected void 减少缓存物体((Grid3 锚点所在网格, 批量选择网格类型 网格索引方式) Arg_键, (Matrix4x4 锚点所在变换矩阵, Structure 放置结构) Arg_值)
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

        private int 获取可变放置索引(Thing Arg_放置结构)
        {
            var 建造面板 = InventoryManager.Instance?.ConstructionPanel;
            if (建造面板 == null || !建造面板.IsVisible) { return -1; }

            var 槽位物品 = 建造面板.Parent;
            if (槽位物品 == null || 槽位物品.Constructables == null) { return -1; }

            return 槽位物品.Constructables.FindIndex(可建造 => 可建造 != null && 可建造.PrefabHash == Arg_放置结构.PrefabHash);
        }

        public bool 获取视线处网格(out (Grid3 锚点所在网格, Vector3 锚点坐标) 当前)
        {
            var Parent = 通用工具.获取活动手槽位和空闲手槽位().Item1.Get();
            switch (Parent)
            {
                case MultiConstructor 多变种:
                    var BuildIndex = 多变种.LastSelectedIndex;
                    InventoryManager.UpdatePlacement(多变种.Constructables[BuildIndex]);
                    break;
                case Constructor 单变种:
                    InventoryManager.UpdatePlacement(单变种.BuildStructure);
                    break;
            }

            if (InventoryManager.ConstructionCursor == null) { 当前 = (Grid3.zero, Vector3.zero); return false; }

            var 放置结构 = InventoryManager.ConstructionCursor;

            // 根据摄像机朝向计算出视线前方网格的向量, 第一个参数是步进/吸附精度(如果当前建造物是网格尺寸>0.5f的大网格, 用0.3f；否则是小网格, 用0.6f。
            var 摄像机前方网格 = InputHelpers.GetCameraForwardGrid((放置结构.GridSize > 0.5f) ? 0.3f : 0.6f, 放置结构.GetCursorOffset, out var 视线向量);
            var 微调 = (放置结构 is Objects.Pipes.IChute) ? (视线向量 * 0.1f) : Vector3.zero;
            var 锚点坐标 = 放置结构.GetWorldGrid(摄像机前方网格 + 微调);

            switch (放置结构)
            {
                case Assets.Scripts.Objects.Wall:
                    锚点坐标 -= 放置结构.ThingTransform.forward * 放置结构.GridSize / 2f;
                    放置结构.ThingTransformPosition = 锚点坐标;
                    当前 = (放置结构.GridPosition, 锚点坐标);
                    return true;
                case Objects.Structures.Frame:
                case SmallGrid:
                    放置结构.ThingTransformPosition = 锚点坐标;
                    当前 = (放置结构.GridPosition, 锚点坐标);
                    return true;
            }

            当前 = (Grid3.zero, Vector3.zero); return false;
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
                        放置间隔计时 += Time.deltaTime;
                        if (放置间隔计时 >= 放置间隔)
                        {
                            放置间隔计时 = 0;
                            清理已失效();
                        }

                        if (KeyManager.GetMouseUp("Secondary")) { 当前状态 = 批量操作任务状态.睡眠; break; }
                        break;
                    }
                case 批量操作任务状态.选择:
                    {
                        if (KeyManager.GetMouseUp("Secondary")) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        if (KeyManager.GetMouseUp("Primary") && 获取视线处网格(out var 锚点))
                        {
                            var Id = (锚点.锚点所在网格, 批量选择网格类型.功能建筑);
                            if (所有已选择.ContainsKey(Id))
                            {
                                减少缓存物体(Id, 所有已选择[Id]);
                            }
                            else
                            {
                                if (InventoryManager.ConstructionCursor != null)
                                {
                                    var 当前放置结构 = InventoryManager.ConstructionCursor;
                                    增加缓存物体(Id, (Matrix4x4.TRS(锚点.锚点坐标, 当前放置结构.ThingTransformRotation, Vector3.one), 当前放置结构));
                                }
                            }
                        }
                        break;
                    }
                case 批量操作任务状态.框选:
                    {

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
                        清理已失效();
                        快捷键配置.快捷轮盘菜单_批量阵列放置_高亮开关 = true;
                        功能模块之快捷轮盘菜单.Log.LogMessage("批量阵列放置：已选择锚点开启渲染");
                        break;
                    }
            }
        }
    }
}
