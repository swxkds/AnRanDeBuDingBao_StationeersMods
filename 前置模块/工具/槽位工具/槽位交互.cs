using Assets.Scripts.Objects;
using Assets.Scripts;
using Assets.Scripts.Objects.Items;
using System.Collections.Generic;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public static void 丢弃槽位物品(Slot 源槽位, DynamicThing 源槽位当前物品)
        {
            // 移动到世界空间时,物品的物理组件会启用; 移动到槽位时,物品的物理组件会停用
            if (源槽位 == null || 源槽位当前物品 == null) { return; }
            OnServer.MoveToWorld(源槽位当前物品);
        }

        public static void 交换槽位物品(Slot 源槽位, DynamicThing 源槽位当前物品, Slot 目标槽位, DynamicThing 目标槽位当前物品)
        {
            // 需要先判断源槽位与目标槽位的槽位类型是否一致, 或者目标槽位是通用槽位类型None
            // 移动到世界空间时,物品的物理组件会启用; 移动到槽位时,物品的物理组件会停用
            if (源槽位 == null || 源槽位当前物品 == null || 目标槽位 == null || 目标槽位当前物品 == null) { return; }
            OnServer.MoveToWorld(源槽位当前物品);
            OnServer.MoveToSlot(目标槽位当前物品, 源槽位);
            OnServer.MoveToSlot(源槽位当前物品, 目标槽位);
        }

        public static void 挤占槽位物品(DynamicThing 源物品, Slot 目标槽位, DynamicThing 目标槽位当前物品)
        {
            // 需要先判断源槽位与目标槽位的槽位类型是否一致, 或者目标槽位是通用槽位类型None
            // 移动到世界空间时,物品的物理组件会启用; 移动到槽位时,物品的物理组件会停用
            if (源物品 == null || 目标槽位 == null || 目标槽位当前物品 == null) { return; }
            OnServer.MoveToWorld(目标槽位当前物品);
            OnServer.MoveToSlot(源物品, 目标槽位);
        }

        public static void 移动槽位物品(DynamicThing 源物品, Slot 目标槽位, DynamicThing 目标槽位当前物品)
        {
            // 移动到世界空间时,物品的物理组件会启用; 移动到槽位时,物品的物理组件会停用
            if (源物品 == null || 目标槽位 == null || 目标槽位当前物品 != null) { return; }
            OnServer.MoveToSlot(源物品, 目标槽位);
        }

        public static void 移动物品到任意非双手空槽位(DynamicThing 源物品, ref IEnumerator<Slot> 槽位表)
        {
            // 槽位表需要排序, 让通用的None排在后面, 优先移动到专用槽位
            // 1.不要移动到空的双手槽  2.多个物品可以顺序移动到空槽, 因此共用一个ref槽位表
            if (源物品 == null || 槽位表 == null) { return; }

            while (槽位表.MoveNext())
            {
                var 当前槽位 = 槽位表.Current;
                var 当前物品 = 当前槽位.Get();
                if (!当前槽位.IsHandSlot && 可以移动到目标槽位么(源物品, 当前槽位, 当前物品))
                {
                    移动槽位物品(源物品, 当前槽位, 当前物品);
                    return;
                }
            }
        }

        public static void 合并槽位物品(IMergeable 源物品, Stackable 目标物品)
        {
            // 源物品: 作为合并材料补充目标物品的堆垛
            if (源物品 == null || 目标物品 == null) { return; }
            if (目标物品.Quantity >= 目标物品.MaxQuantity) { return; }
            Thing.Merge(目标物品, 源物品);
        }

        public static void 合并目标槽位物品至满堆垛(Slot 目标槽位, Stackable 目标槽位当前物品, IEnumerator<Slot> 槽位表)
        {
            // 从任何槽位处获取材料, 包括从左手倒右手补充堆垛
            if (目标槽位 == null || 目标槽位当前物品 == null || 槽位表 == null) { return; }

            var 取出物品 = 目标槽位当前物品;

            槽位物品匹配条件 条件;
            
            if (所有已创建道具匹配条件.TryGetValue((道具类型.可堆垛大类, 取出物品.PrefabHash), out 条件)) { }
            else { 条件 = 创建通用道具匹配条件(道具类型.可堆垛大类, [取出物品.PrefabHash]); }

            while (槽位表.MoveNext())
            {
                if (目标槽位当前物品.Quantity >= 目标槽位当前物品.MaxQuantity) { return; }
                var 当前槽位 = 槽位表.Current;
                if (当前槽位 != 目标槽位 && 槽位物品过滤(当前槽位, 条件))
                {
                    合并槽位物品(当前槽位.Get() as Stackable, 目标槽位当前物品);
                }
            }
        }

        public static bool 可以移动到目标槽位么(DynamicThing 源物品, Slot 目标槽位, DynamicThing 目标槽位当前物品)
        {
            // 通用槽位类型None
            if (源物品 == null || 目标槽位 == null || 目标槽位当前物品 != null) { return false; }
            return 源物品.SlotType == 目标槽位.Type || 目标槽位.Type == Slot.Class.None;
        }

        public static bool 可以交换槽位物品么(Slot 源槽位, DynamicThing 源槽位当前物品, Slot 目标槽位, DynamicThing 目标槽位当前物品)
        {
            // 通用槽位类型None
            if (源槽位 == null || 源槽位当前物品 == null || 目标槽位 == null || 目标槽位当前物品 == null) { return false; }
            return (源槽位当前物品.SlotType == 目标槽位.Type || 目标槽位.Type == Slot.Class.None) && (目标槽位当前物品.SlotType == 源槽位.Type || 源槽位.Type == Slot.Class.None);
        }

        public static bool 可以挤占槽位物品么(DynamicThing 源物品, Slot 目标槽位, DynamicThing 目标槽位当前物品)
        {
            // 通用槽位类型None
            if (源物品 == null || 目标槽位 == null || 目标槽位当前物品 == null) { return false; }
            return 源物品.SlotType == 目标槽位.Type || 目标槽位.Type == Slot.Class.None;
        }
    }
}