using Assets.Scripts.Objects;
using Assets.Scripts;
using Assets.Scripts.Objects.Items;

namespace meanran_xuexi_mods_xiaoyouhua
{
       public static partial class 通用工具
    {
        public static bool 是工具腰带么(DynamicThing thing)
        {
            // 工具腰带和采矿腰带和采矿背包都有共同的基类ToolBelt, 不要用is和as这种继承链检查运算符
            if (thing == null) { return false; }
            var 实例类型 = thing.GetType();
            if ((实例类型 == typeof(ToolBelt) || 实例类型 == typeof(ToolBeltMk2)) && thing.SlotType == Slot.Class.Belt)
            { return true; }
            return false;
        }

        public static bool 是采矿腰带么(DynamicThing thing)
        {
            // 工具腰带和采矿腰带和采矿背包都有共同的基类ToolBelt, 不要用is和as这种继承链检查运算符
            if (thing == null) { return false; }
            var 实例类型 = thing.GetType();
            if ((实例类型 == typeof(MiningBelt) || 实例类型 == typeof(MiningBeltMk2)) && thing.SlotType == Slot.Class.Belt)
            { return true; }
            return false;
        }

        public static bool 是采矿背包么(DynamicThing thing)
        {
            // 工具腰带和采矿腰带和采矿背包都有共同的基类ToolBelt, 不要用is和as这种继承链检查运算符
            if (thing == null) { return false; }
            var 实例类型 = thing.GetType();
            if ((实例类型 == typeof(MiningBelt) || 实例类型 == typeof(MiningBeltMk2)) && thing.SlotType == Slot.Class.Back)
            { return true; }
            return false;
        }

        public static bool 是普通背包么(DynamicThing thing)
        {
            if (thing == null) { return false; }
            var 实例类型 = thing.GetType();
            if ((实例类型 == typeof(Jetpack) || 实例类型 == typeof(Backpack)) && thing.SlotType == Slot.Class.Back)
            { return true; }
            return false;
        }

    }
}