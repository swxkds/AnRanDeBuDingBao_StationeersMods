using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using System;
using System.Linq;
using Assets.Scripts.Objects.Electrical;
using UnityEngine;
using System.Collections.Concurrent;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public enum 道具类型
        {
            扳手, 撬棍, 手钻, 剪线钳, 焊枪, 螺丝刀, 角磨机, 电缆, 贴标机, 笔记本电脑, 平板电脑, 采矿钻机,     // 细分工具
            可堆垛大类, 工具大类, 其它道具大类,     // 可堆垛道具(可以合并和拆分)、工具类道具(有一个变量保存了备用工具)、不可堆垛也不是工具的道具
        }

        public class 槽位物品匹配条件
        {
            public 道具类型 类型;
            public 比较方式 比较方式;
            public Type[] 实例类型表;
            public Type 共同基类;
            public int[] 结构哈希表;

            public 槽位物品匹配条件(道具类型 Arg_类型, 比较方式 Arg_比较方式, Type[] Arg_实例类型表, Type Arg_共同基类, int[] Arg_结构哈希表)
            {
                类型 = Arg_类型;
                比较方式 = Arg_比较方式;
                实例类型表 = Arg_实例类型表;
                共同基类 = Arg_共同基类;
                结构哈希表 = Arg_结构哈希表;
            }
        }

        public static readonly int 矿镐哈希 = Animator.StringToHash("ItemPickaxe");
        public static readonly int 采矿钻机哈希 = Animator.StringToHash("ItemMiningDrill");
        public static readonly int 采矿钻机MkII哈希 = Animator.StringToHash("ItemMKIIMiningDrill");
        public static readonly int 气动采矿钻机哈希 = Animator.StringToHash("ItemMiningDrillPneumatic");
        public static readonly int 重型采矿钻机哈希 = Animator.StringToHash("ItemMiningDrillHeavy");

        public static readonly int 应急电焊枪哈希 = Animator.StringToHash("ItemEmergencyArcWelder");
        public static readonly int 气焊枪哈希 = Animator.StringToHash("ItemWeldingTorch");
        public static readonly int 电焊枪哈希 = Animator.StringToHash("ItemArcWelder");
        public static readonly int 电焊枪MkII哈希 = Animator.StringToHash("ItemMKIIArcWelder");

        public static readonly int 电缆哈希 = Animator.StringToHash("ItemCableCoil");
        public static readonly int 重型电缆哈希 = Animator.StringToHash("ItemCableCoilHeavy");
        public static readonly int 电缆线圈_超重型哈希 = Animator.StringToHash("ItemCableCoilSuperHeavy");
        public const int 无效哈希 = -1;
        public static readonly ConcurrentDictionary<(道具类型 类型, int 哈希), 槽位物品匹配条件> 所有已创建道具匹配条件 = new()
        {
            [(道具类型.扳手, 无效哈希)] = new 槽位物品匹配条件(道具类型.扳手, 比较方式.比较实例类型, [typeof(Wrench)], null, null),
            [(道具类型.撬棍, 无效哈希)] = new 槽位物品匹配条件(道具类型.撬棍, 比较方式.比较实例类型, [typeof(Crowbar)], null, null),
            [(道具类型.手钻, 无效哈希)] = new 槽位物品匹配条件(道具类型.手钻, 比较方式.比较实例类型, [typeof(Drill)], null, null),
            [(道具类型.剪线钳, 无效哈希)] = new 槽位物品匹配条件(道具类型.剪线钳, 比较方式.比较实例类型, [typeof(WireCutter)], null, null),
            [(道具类型.焊枪, 无效哈希)] = new 槽位物品匹配条件(道具类型.焊枪, 比较方式.比较共同基类 | 比较方式.比较结构哈希, null, typeof(IWelder), [应急电焊枪哈希, 气焊枪哈希, 电焊枪哈希, 电焊枪MkII哈希]),
            [(道具类型.螺丝刀, 无效哈希)] = new 槽位物品匹配条件(道具类型.螺丝刀, 比较方式.比较实例类型, [typeof(Screwdriver)], null, null),
            [(道具类型.角磨机, 无效哈希)] = new 槽位物品匹配条件(道具类型.角磨机, 比较方式.比较实例类型, [typeof(AngleGrinder)], null, null),
            [(道具类型.电缆, 无效哈希)] = new 槽位物品匹配条件(道具类型.电缆, 比较方式.比较结构哈希, null, null, [电缆哈希, 重型电缆哈希, 电缆线圈_超重型哈希]),            //   {(道具类型.电缆,   [typeof(MultiMergeConstructor)])} 
            [(道具类型.贴标机, 无效哈希)] = new 槽位物品匹配条件(道具类型.贴标机, 比较方式.比较实例类型, [typeof(Labeller)], null, null),
            [(道具类型.笔记本电脑, 无效哈希)] = new 槽位物品匹配条件(道具类型.笔记本电脑, 比较方式.比较实例类型, [typeof(Laptop)], null, null),
            [(道具类型.平板电脑, 无效哈希)] = new 槽位物品匹配条件(道具类型.平板电脑, 比较方式.比较实例类型, [typeof(AdvancedTablet), typeof(Tablet)], null, null),
            [(道具类型.采矿钻机, 无效哈希)] = new 槽位物品匹配条件(道具类型.采矿钻机, 比较方式.比较共同基类 | 比较方式.比较结构哈希, null, typeof(IMiningTool), [矿镐哈希, 采矿钻机哈希, 采矿钻机MkII哈希, 气动采矿钻机哈希, 重型采矿钻机哈希]),
        };


        public static 槽位物品匹配条件 创建通用道具匹配条件(道具类型 Arg_类型, int[] Arg_结构哈希表)
        {
            var 哈希key = Arg_结构哈希表.First();
            所有已创建道具匹配条件[(Arg_类型, 哈希key)] = new(Arg_类型, 比较方式.比较结构哈希, null, null, Arg_结构哈希表);
            return 所有已创建道具匹配条件[(Arg_类型, 哈希key)];
        }

        public static 槽位物品匹配条件 创建工具大类道具匹配条件(Item Arg_取出物品)
        {
            if (Arg_取出物品.ReplacementOf) { return 创建通用道具匹配条件(道具类型.工具大类, [Arg_取出物品.PrefabHash, Arg_取出物品.ReplacementOf.PrefabHash]); }
            else { return 创建通用道具匹配条件(道具类型.工具大类, [Arg_取出物品.PrefabHash]); }
        }

        [Flags]
        public enum 比较方式 : uint
        {
            // 标志位一定要用左移, 确保一个标志位只占用一个二进制且不会覆盖
            零值 = 0,
            比较实例类型 = 1 << 0,
            比较共同基类 = 1 << 1,
            比较结构哈希 = 1 << 2
        }

        public static bool 指定标志位存在么(比较方式 标志位, 比较方式 合成标志)
        {
            return (标志位 & 合成标志) != 比较方式.零值;
        }

        public static bool 槽位物品过滤(Slot 槽位, 槽位物品匹配条件 当前条件)
        {
            if (当前条件 == null) { return false; }

            var thing = 槽位.Get();
            if (thing == null) { return false; }

            var thingType = thing.GetType();
            if (指定标志位存在么(比较方式.比较实例类型, 当前条件.比较方式))
            { if (当前条件.实例类型表.Any(t => t == thingType)) { return true; } }

            if (指定标志位存在么(比较方式.比较共同基类, 当前条件.比较方式))
            { if (当前条件.共同基类.IsAssignableFrom(thingType)) { return true; } }

            if (指定标志位存在么(比较方式.比较结构哈希, 当前条件.比较方式))
            {
                var thingItem = thing as Item;
                if (当前条件.结构哈希表.Any(t => t == thing.PrefabHash || (thingItem && t == thingItem.ReplacementOf?.PrefabHash))) { return true; }
            }

            return false;
        }
    }
}