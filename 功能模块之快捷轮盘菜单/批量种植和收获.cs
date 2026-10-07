using UnityEngine;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Items;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using System;
using Objects.Items;
using Assets.Scripts;
using Cysharp.Threading.Tasks;
using Assets.Scripts.GridSystem;
using System.Runtime.CompilerServices;
using System.Linq;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 批量种植和收获
    {
        public enum 批量操作任务状态
        {
            睡眠, 开关高亮, 选择, 清空高亮,
            种植, 收获, 收获所有,
            拆除, 装配, 选择支路, 框选
        }
        public enum 批量选择网格状态
        {
            没有框选, 开始框选, 结束框选,
        }
        public enum 批量选择建筑类型
        {
            墙体, 框架,
            水培托盘,
        }
        public enum 批量选择网格类型
        {
            墙体和框架,
            功能建筑,
        }
        protected 批量选择网格状态 框选状态 = 批量选择网格状态.没有框选;
        protected (Grid3 网格坐标, long 建筑Id, 批量选择建筑类型 建筑类型, 批量选择网格类型 网格类型) 框选起点;
        protected (Grid3 网格坐标, long 建筑Id, 批量选择建筑类型 建筑类型, 批量选择网格类型 网格类型) 框选终点;
        public 批量操作任务状态 当前状态 { get; protected set; }
        public 批量操作任务状态 目标状态 = 批量操作任务状态.睡眠;
        public Dictionary<long, Thing> 所有已选择 { get; protected set; }
        public List<Thing> 所有已选择缓存 { get; protected set; }
        private List<long> 已失效表;
        private Queue<Action> 事件队列;
        public void 添加事件(Action 事件) { 事件队列.Enqueue(事件); }
        public void 执行事件() { while (事件队列.Count > 0) { 事件队列.Dequeue().Invoke(); } }
        public int 获取事件数量() { return 事件队列.Count; }
        public void 清空事件() { 事件队列.Clear(); }
        public void Dispose()
        {
            if (所有已选择 != null)
            {
                所有已选择.Clear();
                所有已选择 = null;
            }

            if (所有已选择缓存 != null)
            {
                所有已选择缓存.Clear();
                所有已选择缓存 = null;
            }

            if (已失效表 != null)
            {
                已失效表.Clear();
                已失效表 = null;
            }

            if (事件队列 != null)
            {
                事件队列.Clear();
                事件队列 = null;
            }
        }
        public static T 构造函数<T>() where T : 批量种植和收获, new()
        {
            var _this = new T { 当前状态 = 批量操作任务状态.睡眠 };
            _this.所有已选择 ??= new();
            _this.所有已选择缓存 ??= new();
            _this.已失效表 ??= new();
            _this.事件队列 ??= new();
            return _this;
        }

        protected void Clear()
        {
            所有已选择.Clear();
            所有已选择缓存.Clear();
        }
        protected void 更新缓存()
        {
            所有已选择缓存.Clear();
            所有已选择缓存.AddRange(所有已选择.Values);
        }

        protected void 增加缓存物体(long Id, Thing New)
        {
            所有已选择.Add(Id, New);
            所有已选择缓存.Add(New);
        }

        protected void 增加缓存物体(Structure 建筑, 批量选择建筑类型 框选目标类型)
        {
            var Id = 建筑.ReferenceId;
            if (!所有已选择.ContainsKey(Id))
            {
                switch (框选目标类型)
                {
                    case 批量选择建筑类型.墙体:
                        if (建筑 is Assets.Scripts.Objects.Wall) { 增加缓存物体(Id, 建筑); }
                        break;
                    case 批量选择建筑类型.框架:
                        if (建筑 is Objects.Structures.Frame) { 增加缓存物体(Id, 建筑); }
                        break;
                    case 批量选择建筑类型.水培托盘:
                        if (建筑 is IHarvestable) { 增加缓存物体(Id, 建筑); }
                        break;
                }
            }
        }

        protected void 减少缓存物体(long Id, Thing Old)
        {
            所有已选择.Remove(Id);
            所有已选择缓存.Remove(Old);
        }

        protected void 清理已失效()
        {
            if (所有已选择 == null || 所有已选择.Count == 0) { return; }

            已失效表.Clear();

            // 顺向遍历时, 如果删除了元素, 会导致后续的所有元素向前移动, 容器元素数变小, 然后遍历器按照旧的索引获取元素, 就会出现索引错位, 甚至出现尾元素时索引溢出
            foreach (var __ in 所有已选择)
            {
                Thing value = __.Value;
                if (value == null)
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

        protected void 选择框选起点(Structure 建筑, 批量选择建筑类型 框选类型, 批量选择网格类型 网格类型)
        {
            if (所有已选择.Count == 0)
            {
                switch (网格类型)
                {
                    case 批量选择网格类型.墙体和框架:
                        {
                            var Id = 建筑.ReferenceId;
                            框选状态 = 批量选择网格状态.开始框选;
                            var 起点 = 建筑.GridPosition;
                            框选起点 = (起点, Id, 框选类型, 网格类型);
                            增加缓存物体(Id, 建筑);
                        }
                        break;
                    case 批量选择网格类型.功能建筑:
                        {
                            var Id = 建筑.ReferenceId;
                            框选状态 = 批量选择网格状态.开始框选;
                            var 模型所占用的所有小网格点 = (Grid3[])建筑.GridBounds.GetLocalSmallGrid(建筑.RegisteredPosition, 建筑.RegisteredRotation);
                            var min = 模型所占用的所有小网格点.First();
                            foreach (var 小网格点 in 模型所占用的所有小网格点) { min = new Grid3(Mathf.Min(min.x, 小网格点.x), Mathf.Min(min.y, 小网格点.y), Mathf.Min(min.z, 小网格点.z)); }
                            var 起点 = min;
                            框选起点 = (起点, Id, 框选类型, 网格类型);
                            增加缓存物体(Id, 建筑);
                        }
                        break;
                }
            }
        }
        protected void 选择框选终点(Structure 建筑, 批量选择建筑类型 框选类型, 批量选择网格类型 网格类型)
        {
            // 已选择的起点建筑必须是通过框选选择的
            if (所有已选择.Count == 1 && 框选状态 == 批量选择网格状态.开始框选)
            {
                var Id = 建筑.ReferenceId;
                if (所有已选择.ContainsKey(Id))
                {
                    // 取消当前框选起点
                    框选状态 = 批量选择网格状态.没有框选;
                    减少缓存物体(Id, 建筑);
                }
                else
                {
                    switch (网格类型)
                    {
                        case 批量选择网格类型.墙体和框架:
                            {
                                框选状态 = 批量选择网格状态.结束框选;
                                var 终点 = 建筑.GridPosition;
                                框选终点 = (终点, Id, 框选类型, 网格类型);
                                if (框选起点.建筑类型 == 框选终点.建筑类型) { 增加缓存物体(Id, 建筑); }    // 该建筑是框选类型, 则加入缓存, 否则仅作为坐标的提供者
                            }
                            break;
                        case 批量选择网格类型.功能建筑:
                            {
                                框选状态 = 批量选择网格状态.结束框选;
                                var 模型所占用的所有小网格点 = (Grid3[])建筑.GridBounds.GetLocalSmallGrid(建筑.RegisteredPosition, 建筑.RegisteredRotation);
                                var max = 模型所占用的所有小网格点.First();
                                foreach (var 小网格点 in 模型所占用的所有小网格点) { max = new Grid3(Mathf.Max(max.x, 小网格点.x), Mathf.Max(max.y, 小网格点.y), Mathf.Max(max.z, 小网格点.z)); }
                                var 终点 = max;
                                框选终点 = (终点, Id, 框选类型, 网格类型);
                                if (框选起点.建筑类型 == 框选终点.建筑类型) { 增加缓存物体(Id, 建筑); }    // 该建筑是框选类型, 则加入缓存, 否则仅作为坐标的提供者
                            }
                            break;
                    }
                }
            }
        }

        protected void 处理框选结果()
        {
            var 起点 = 框选起点.网格坐标;
            var 终点 = 框选终点.网格坐标;
            var 框选目标类型 = 框选起点.建筑类型;

            var min = new Grid3(Mathf.Min(起点.x, 终点.x), Mathf.Min(起点.y, 终点.y), Mathf.Min(起点.z, 终点.z));
            var max = new Grid3(Mathf.Max(起点.x, 终点.x), Mathf.Max(起点.y, 终点.y), Mathf.Max(起点.z, 终点.z));

            int 网格尺寸 = 0;
            int 已处理网格计数 = 0;

            switch (框选起点.网格类型)
            {
                case 批量选择网格类型.墙体和框架:
                    网格尺寸 = Grid3.Directions.First().z;
                    break;
                case 批量选择网格类型.功能建筑:
                    网格尺寸 = Mathf.RoundToInt(SmallGrid.SmallGridSize * 10f);
                    break;
            }

            if (网格尺寸 > 0)
            {
                for (var i = min.x; i <= max.x; i += 网格尺寸)
                {
                    for (var j = min.y; j <= max.y; j += 网格尺寸)
                    {
                        for (var k = min.z; k <= max.z; k += 网格尺寸)
                        {
                            ++已处理网格计数;

                            switch (框选起点.网格类型)
                            {
                                case 批量选择网格类型.墙体和框架:
                                    {
                                        var 网格单元 = GridController.World.GetCell(new Grid3(i, j, k));      // Cell: 网格单元, 和框架一样的大小, 网格单元持有内部所有的可放置设备(墙、框架、门.....)
                                        if (网格单元 == null) { continue; }
                                        for (var 当前方位 = StructureElement.South; 当前方位 >= StructureElement.Center; 当前方位--)
                                        {
                                            // 东、南、西、北、中、上、下
                                            var 当前放置结构 = 网格单元.Lookup[当前方位];
                                            if (当前放置结构 == null) { continue; }
                                            增加缓存物体(当前放置结构, 框选目标类型);
                                        }
                                    }
                                    break;
                                case 批量选择网格类型.功能建筑:
                                    {
                                        var 网格单元 = GridController.World.GetSmallCell(new Grid3(i, j, k));      // SmallCell: 小网格单元, 四分之一框架的大小
                                        if (网格单元 == null) { continue; }
                                        if (网格单元.Pipe != null && 网格单元.Pipe is IHarvestable)
                                        {
                                            var 当前放置结构 = 网格单元.Pipe;
                                            增加缓存物体(当前放置结构, 框选目标类型);
                                        }
                                        else if (网格单元.Device != null && 网格单元.Device is IHarvestable)
                                        {
                                            var 当前放置结构 = 网格单元.Device;
                                            增加缓存物体(当前放置结构, 框选目标类型);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            框选状态 = 批量选择网格状态.没有框选;
            当前状态 = 批量操作任务状态.选择;
            功能模块之快捷轮盘菜单.Log.LogMessage($"已扫描了 {已处理网格计数} 个网格单元");
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public virtual void Update()
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
                        if (KeyManager.GetMouseUp("Secondary")) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        // if (!所有已选择.ContainsKey(Human.LocalHuman.ReferenceId)) { 所有已选择.Add(Human.LocalHuman.ReferenceId, Human.LocalHuman); }

                        var 建筑 = 通用工具.获取视线处建筑类物体();

                        if (建筑 && KeyManager.GetMouseUp("Primary"))
                        {
                            var Id = 建筑.ReferenceId;
                            if (所有已选择.ContainsKey(Id))
                            {
                                减少缓存物体(Id, 建筑);
                            }
                            else
                            {
                                switch (建筑)
                                {
                                    case IHarvestable:
                                        增加缓存物体(Id, 建筑);
                                        break;
                                }
                            }
                        }

                        break;
                    }
                case 批量操作任务状态.框选:
                    {
                        if (KeyManager.GetMouseUp("Secondary")) { 框选状态 = 批量选择网格状态.没有框选; 当前状态 = 批量操作任务状态.睡眠; break; }

                        switch (框选状态)
                        {
                            case 批量选择网格状态.没有框选:
                                var 建筑 = 通用工具.获取视线处建筑类物体();
                                if (建筑 && KeyManager.GetMouseUp("Primary") && 建筑 is IHarvestable) { 选择框选起点(建筑, 批量选择建筑类型.水培托盘, 批量选择网格类型.功能建筑); }
                                break;
                            case 批量选择网格状态.开始框选:
                                建筑 = 通用工具.获取视线处建筑类物体();
                                if (建筑 && KeyManager.GetMouseUp("Primary") && 建筑 is IHarvestable) { 选择框选终点(建筑, 批量选择建筑类型.水培托盘, 批量选择网格类型.功能建筑); }
                                break;
                            case 批量选择网格状态.结束框选:
                                处理框选结果();
                                break;
                        }
                        break;
                    }
                case 批量操作任务状态.种植:
                    {
                        清理已失效();

                        if (所有已选择.Count == 0) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        var 活动手 = InventoryManager.ActiveHandSlot;
                        var 可种物 = 活动手.Get() as Plant;
                        if (可种物)
                        {
                            var 可种数量 = 可种物.Quantity;
                            foreach (var 建筑 in 所有已选择.Values)
                            {
                                if (可种数量 <= 0) { break; }
                                switch (建筑)
                                {
                                    case IHarvestable __:
                                        {
                                            var 已种 = __.GetPlant;
                                            if (已种 == null)
                                            {
                                                --可种数量;
                                                添加事件(() => __.InputSlot.Interactable.PlayerInteractWith(活动手));
                                            }
                                            break;
                                        }
                                }
                            }
                        }

                        当前状态 = 批量操作任务状态.睡眠;
                        break;
                    }
                case 批量操作任务状态.收获:
                    {
                        清理已失效();

                        if (所有已选择.Count == 0) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        var 活动手 = InventoryManager.ActiveHandSlot;
                        var 手槽物 = 活动手.Get();
                        var 可种植 = 手槽物 as Plant;
                        var 草 = 手槽物 as Hay;             // 柳枝稷和草的果实属于不可种植的草

                        if (手槽物 is Stackable 手槽物品 && (可种植 || 草))
                        {
                            var 可收数量 = 手槽物品.MaxQuantity - 手槽物品.Quantity;
                            foreach (var 建筑 in 所有已选择.Values)
                            {
                                if (可收数量 <= 0) { break; }
                                switch (建筑)
                                {
                                    case IHarvestable __:
                                        {
                                            var 已种 = __.GetPlant;
                                            if (已种)
                                            {
                                                if (已种.IsSeeding && 已种.SeedQuantity > 0 && 已种.SeedObject.PrefabHash == 手槽物品.PrefabHash)
                                                {
                                                    --可收数量;
                                                    添加事件(() => __.InputSlot.Interactable.PlayerInteractWith(活动手));
                                                }
                                                else if (已种.IsMature && 已种.HarvestQuantity > 0 && 已种.FruitObject.PrefabHash == 手槽物品.PrefabHash)
                                                {
                                                    --可收数量;
                                                    添加事件(() => __.InputSlot.Interactable.PlayerInteractWith(活动手));
                                                }
                                            }
                                            break;
                                        }
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
                case 批量操作任务状态.收获所有:
                    {
                        清理已失效();

                        if (所有已选择.Count == 0) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        var 活动手 = InventoryManager.ActiveHandSlot;
                        var 手槽物 = 活动手.Get();
                        var 可种植 = 手槽物 as Plant;
                        var 草 = 手槽物 as Hay;             // 柳枝稷和草的果实属于不可种植的草

                        if (手槽物 is Stackable 手槽物品 && (可种植 || 草))
                        {
                            var 可收数量 = 手槽物品.MaxQuantity - 手槽物品.Quantity;
                            foreach (var 建筑 in 所有已选择.Values)
                            {
                                if (可收数量 <= 0) { break; }
                                switch (建筑)
                                {
                                    case IHarvestable __:
                                        {
                                            var 已种 = __.GetPlant;
                                            if (已种)
                                            {
                                                if (已种.IsSeeding && 已种.SeedQuantity > 0 && 已种.SeedObject.PrefabHash == 手槽物品.PrefabHash)
                                                {
                                                    --可收数量;
                                                    添加事件(() => __.InputSlot.Interactable.PlayerInteractWith(活动手));
                                                }
                                                else if (已种.IsMature && 已种.HarvestQuantity > 0 && 已种.FruitObject.PrefabHash == 手槽物品.PrefabHash)
                                                {
                                                    var 作物数量 = 已种.HarvestQuantity;
                                                    while (Mathf.Min(可收数量, 作物数量) > 0)
                                                    {
                                                        --可收数量;
                                                        --作物数量;
                                                        添加事件(() => __.InputSlot.Interactable.PlayerInteractWith(活动手));
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }

                        当前状态 = 批量操作任务状态.睡眠;
                        break;
                    }
            }

            if (当前状态 == 之前状态) { return; }

            switch (当前状态)
            {
                case 批量操作任务状态.睡眠:
                    {
                        快捷键配置.快捷轮盘菜单_批量种植和收获_高亮开关 = false;
                        功能模块之快捷轮盘菜单.Log.LogMessage("批量种植和收获：已选择水培托盘关闭渲染");
                        break;
                    }
                default:
                    {
                        清理已失效();
                        快捷键配置.快捷轮盘菜单_批量种植和收获_高亮开关 = true;
                        功能模块之快捷轮盘菜单.Log.LogMessage("批量种植和收获：已选择水培托盘开启渲染");
                        break;
                    }
            }
        }

        public async UniTaskVoid 执行批量操作()
        {
            快捷轮盘菜单.关闭快捷轮盘菜单();
            清空事件();

            Update();
            while (当前状态 != 批量操作任务状态.睡眠)
            {
                // 当前玩家实体不存在, 结束批量操作
                if (InventoryManager.Parent == null) { break; }
                // 游戏暂停, 暂停批量操作
                if (!WorldManager.IsGamePaused) { Update(); }

                await UniTask.Yield();
            }

            // 非正常状态结束时, 重置为睡眠状态
            if (GameManager.GameState != GameState.Running || InventoryManager.Parent == null || WorldManager.IsGamePaused)
            {
                当前状态 = 批量操作任务状态.睡眠;
                清空事件();
            }
            else
            {
                var 计数 = 获取事件数量();

                // 功能模块之快捷轮盘菜单.Log.LogMessage($"批量种收事件数量: {计数}");

                if (计数 > 0)
                {
                    var 进度条配置 = new Thing.DelayedActionInstance
                    { Duration = Mathf.Min(0.5f * 计数, 22), ActionMessage = "批量操作", OverrideTitle = "所有已选择" };
                    进度条配置.AppendStateMessage("读条结束时, 执行批量操作");   // 显示工具提示面板
                    进度条配置.Succeed();

                    通用工具.提交_读条动作_任务(执行事件, 进度条配置, 通用工具.通用读条动作中断条件类型.事件触发进入读条动作_读条期间鼠标右键单击一次则中断读条);
                }
            }
        }
    }

    public class 批量操作悬停提示 : 快捷工具按钮
    {
        public string m_悬停显示;
        Action m_按钮点击事件;
        public override string DisplayName => "使用说明";
        public override void 按钮点击事件() { m_按钮点击事件?.Invoke(); }
        public void 构造初始化(string Arg_标题, string Arg_悬停显示, Action Arg_按钮点击事件)
        {
            base.构造初始化();
            右侧文本.text = Arg_标题;
            m_悬停显示 = Arg_悬停显示;
            m_按钮点击事件 = Arg_按钮点击事件;
        }
        public override string 交互提示面板内容() { return m_悬停显示; }
    }
}