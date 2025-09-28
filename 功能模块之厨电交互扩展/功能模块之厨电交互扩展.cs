using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Appliances;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using BepInEx;
using HarmonyLib;
using Reagents;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_chudianjiaohukuozhan", "功能模块之厨电交互扩展", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之厨电交互扩展 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之厨电交互扩展加载完成!");
            补丁 = new Harmony("功能模块之厨电交互扩展");
            补丁.PatchAll();
            // 不需要额外注册自定义数据包类型, 依赖的API是游戏内置的
            添加交互过程函数();
        }

        private static void 添加交互过程函数()
        {
            前置模块.添加交互过程函数([    

            // 微波炉
            (typeof(Microwave),
            (static (主物体, 控件) => ((Microwave)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((Microwave)主物体).按控件(控件, 可选择项目))),

            // 化学加工站
            (typeof(ChemistryStation),
            (static (主物体, 控件) => ((ChemistryStation)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((ChemistryStation)主物体).按控件(控件, 可选择项目))),

            // 打包机
            (typeof(BasicPackagingMachine),
            (static (主物体, 控件) => ((BasicPackagingMachine)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((BasicPackagingMachine)主物体).按控件(控件, 可选择项目))),

            // 原料加工机
            (typeof(ReagentProcessor),
            (static (主物体, 控件) => ((ReagentProcessor)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((ReagentProcessor)主物体).按控件(控件, 可选择项目)))
            ]);
        }

        private static 通用工具.槽位物品匹配条件 微波炉材料条件 = new(通用工具.道具类型.其它道具大类, 通用工具.比较方式.比较共同基类, null, typeof(IMicrowaveIngredient), null);
        private static 通用工具.槽位物品匹配条件 打包机材料条件 = new(通用工具.道具类型.其它道具大类, 通用工具.比较方式.比较共同基类, null, typeof(IPackageableIngredient), null);
        private static 通用工具.槽位物品匹配条件 化学加工站材料条件 = new(通用工具.道具类型.其它道具大类, 通用工具.比较方式.比较共同基类, null, typeof(IChemistryIngredient), null);
        private static 通用工具.槽位物品匹配条件 原料加工机材料条件 = new(通用工具.道具类型.其它道具大类, 通用工具.比较方式.比较共同基类, null, typeof(IProcessable), null);

        public static void 添加材料到厨电中(Appliance 厨电, Interactable 厨电控件, ReagentMixture 厨电可生产配方)
        {
            ReagentMixture 厨电中现有材料 = null;

            switch (厨电)
            {
                case ApplianceReagentImportBase:
                    {
                        厨电中现有材料 = 厨电.ReagentMixture;   // 显示厨电中现有材料
                        break;
                    }
                case ReagentProcessor 原料加工机:
                    {
                        var 材料 = 原料加工机.InputSlot.Get<Item>();
                        if (材料 == null)
                        {
                            厨电中现有材料 = ReagentMixture.Empty;
                            break;
                        }
                        else
                        {
                            float 投放一次的消耗数量 = 材料.QuantityPerUse;
                            float 实际数量 = 材料.QuantityPerUse;

                            if (材料 is IQuantity 堆垛件) { 实际数量 = 堆垛件.GetQuantity; }
                            else if (材料 is Item 耗材件) { 实际数量 = 耗材件.GetQuantity; }

                            var 投放一次的材料成分 = 材料.AddMixture;         // 投放一次的材料成分, 注意投放一次不是投放一份
                            厨电中现有材料 = new ReagentMixture(投放一次的材料成分) * (double)(实际数量 / 投放一次的消耗数量);
                            break;
                        }
                    }
            }

            if (厨电中现有材料 == null) { return; }

            var 当前厨电配方 = new Recipe(厨电可生产配方, null);
            var __ = 当前厨电配方.GetMissingReagents(厨电中现有材料);
            var 厨电中缺少材料 = new ReagentMixture(__);

            if (厨电中缺少材料.IsEmpty()) { Log.LogMessage($"厨电中不缺少任何材料, 无需添加"); return; }

            var 缺少的所有成分 = new List<(Reagent 成分, double 缺少的数量)>();
            foreach (var 成分 in Reagent.AllReagentsSorted)
            {
                if (厨电中缺少材料.Contains(成分)) { 缺少的所有成分.Add((成分, 厨电中缺少材料.Get(成分))); }    // 如果需要该成分, 就不需要判断现有材料是否包含该成分, 因为GetMissingReagents已经检查过
                else if (!厨电可生产配方.Contains(成分) && 厨电中现有材料.Contains(成分)) { Log.LogWarning($"厨电中存在配方以外的材料, 无法添加"); return; }             // 现有材料存在不需要的成分, 不能添加, 否则会变成废料
            }

            if (缺少的所有成分.Count == 0) { return; }

            IEnumerable<Slot> 匹配表 = null;
            switch (厨电)
            {
                case Microwave 微波炉:
                    {
                        匹配表 = 通用工具.槽位扫描_专用槽位优先().Where(槽位 => { return 通用工具.槽位物品过滤(槽位, 微波炉材料条件); });
                        break;
                    }
                case ChemistryStation 化学加工站:
                    {
                        匹配表 = 通用工具.槽位扫描_专用槽位优先().Where(槽位 => { return 通用工具.槽位物品过滤(槽位, 化学加工站材料条件); });
                        break;
                    }
                case BasicPackagingMachine 打包机:
                    {
                        匹配表 = 通用工具.槽位扫描_专用槽位优先().Where(槽位 => { return 通用工具.槽位物品过滤(槽位, 打包机材料条件); });
                        break;
                    }
                case ReagentProcessor 原料加工机:
                    {
                        匹配表 = 通用工具.槽位扫描_专用槽位优先().Where(槽位 => { return 通用工具.槽位物品过滤(槽位, 原料加工机材料条件); });
                        break;
                    }
            }

            if (匹配表 == null || 匹配表.Count() == 0) { Log.LogWarning($"在背包中没有找到厨电所需的材料, 无法添加"); return; }

            var 遍历 = 匹配表.GetEnumerator();

            switch (厨电)
            {
                case ApplianceReagentImportBase:
                    {
                        while (缺少的所有成分.Any(d => d.缺少的数量 > 0.0) && 遍历.MoveNext())
                        {
                            var 当前槽位 = 遍历.Current;
                            var 当前食材 = (IIngredient)当前槽位.Get();

                            // 计算该食材能使用几次, 以及需要使用几次
                            float 消耗数量 = 当前食材.QuantityPerUse;  // 该食材添加到微波炉一次时, 自身消耗的数量  举例:面粉是50, 糖是10    注: 配方数量是消耗数量的整数倍
                            float 实际数量 = 当前食材.QuantityPerUse;
                            int 可添加次数 = 1;     // 如果不是堆叠件, 则默认只能添加一次

                            if (当前食材 is IQuantity 堆垛件) { 实际数量 = 堆垛件.GetQuantity; }
                            else if (当前食材 is Item 耗材件) { 实际数量 = 耗材件.GetQuantity; }

                            if (实际数量 < 消耗数量)
                            {
                                消耗数量 = 实际数量;
                                可添加次数 = 0;           // 不允许零散添加, 只能添加整数倍, 这样好计算添加次数
                            }
                            else
                            {
                                可添加次数 = Mathf.FloorToInt(实际数量 / 消耗数量);     // 不允许零散添加, 只能添加整数倍, 这样好计算添加次数
                            }

                            if (可添加次数 == 0) { continue; }

                            // 每种食材只有一种成分
                            var 食材的所有成分 = (消耗数量 != 当前食材.QuantityPerUse) ? (new ReagentMixture(当前食材.AddMixture) * (double)(消耗数量 / 当前食材.QuantityPerUse)) : 当前食材.AddMixture;  // 该食材添加到微波炉一次时, 微波炉中增加的成分及数量

                            for (var i = 0; i < 缺少的所有成分.Count; i++)
                            {
                                (Reagent 成分, double 缺少的数量) = 缺少的所有成分[i];

                                var 食材中该成分的数量 = 食材的所有成分.Get(成分);

                                // 每种食材只有一种成分, 添加次数用完, 就不用判断其它成分
                                if (缺少的数量 > 0.0 && 食材中该成分的数量 > 0.0 && 可添加次数 > 0)     // 不允许零散添加, 只能添加整数倍, 这样好计算添加次数
                                {
                                    while (缺少的数量 > 0.0 && 可添加次数 > 0)
                                    {
                                        可添加次数--;
                                        缺少的数量 -= 食材中该成分的数量;
                                        厨电控件.PlayerInteractWith(当前槽位);
                                        Log.LogMessage($"厨电中添加了材料 {当前食材.ToTooltip()}");
                                    }

                                    缺少的所有成分[i] = (成分, 缺少的数量);
                                }
                            }

                            // 这是厨电添加材料的方式
                            // {
                            //     厨电.ReagentMixture.Add(食材成分);
                            //     当前食材.OnUseItem(消耗数量, 厨电);
                            // }
                        }
                        break;
                    }
                case ReagentProcessor 原料加工机:
                    {
                        while (遍历.MoveNext())
                        {
                            var 当前槽位 = 遍历.Current;
                            var 当前食材 = (IIngredient)当前槽位.Get();
                            var 投放一次的材料成分 = 当前食材.AddMixture;         // 投放一次的材料成分, 注意投放一次不是投放一份

                            for (var i = 0; i < 缺少的所有成分.Count; i++)
                            {
                                (Reagent 成分, double 缺少的数量) = 缺少的所有成分[i];

                                if (投放一次的材料成分.Contains(成分))
                                {
                                    var 材料 = 原料加工机.InputSlot.Get<Item>();
                                    if (材料 == null)
                                    {
                                        通用工具.合并目标槽位物品至满堆垛(当前槽位, 当前食材 as Stackable, 匹配表.GetEnumerator());
                                        厨电控件.PlayerInteractWith(当前槽位);
                                        return;
                                    }
                                    else
                                    {
                                        通用工具.合并目标槽位物品至满堆垛(原料加工机.InputSlot, 材料 as Stackable, 匹配表.GetEnumerator());
                                        return;
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
        }

        public static void 按控件_厨电(Appliance 厨电, Interactable 厨电控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.厨电配方:
                    switch (厨电控件.Action)
                    {
                        // "微波炉/化学加工站/打包机"这三种厨电的投放材料的碰撞体对应的控件           
                        case InteractableType.Activate:
                            if (厨电 is ApplianceReagentImportBase) { 添加材料到厨电中(厨电, 厨电控件, 已选择.链接厨电配方.配方); }
                            // 不需要额外发送自定义数据包, 依赖的API是游戏内置的
                            break;
                        // "原料加工机"的投放材料的碰撞体对应的控件
                        case InteractableType.Slot1:
                            if (厨电 is ReagentProcessor) { 添加材料到厨电中(厨电, 厨电控件, 已选择.链接厨电配方.配方); }
                            // 不需要额外发送自定义数据包, 依赖的API是游戏内置的
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }

        public static 通用可选择项目 生成消息_厨电(Appliance 厨电, Interactable 厨电控件, List<(ReagentMixture 配方, Item 成品)> 厨电的所有配方)
        {
            通用可选择项目 包 = new();

            switch (厨电控件.Action)
            {
                // "微波炉/化学加工站/打包机"这三种厨电的投放材料的碰撞体对应的控件
                case InteractableType.Activate:
                    if (厨电 is ApplianceReagentImportBase) { 生成消息_厨电(厨电的所有配方, ref 包); }
                    else { 包.解包标志 = 通用可选择项目.数据解包标志.未知; }
                    break;
                // "原料加工机"的投放材料的碰撞体对应的控件
                case InteractableType.Slot1:
                    if (厨电 is ReagentProcessor) { 生成消息_厨电(厨电的所有配方, ref 包); }
                    else { 包.解包标志 = 通用可选择项目.数据解包标志.未知; }
                    break;
                default:
                    包.解包标志 = 通用可选择项目.数据解包标志.未知;
                    break;
            }
            return 包;

            static void 生成消息_厨电(List<(ReagentMixture 配方, Item 成品)> 厨电的所有配方, ref 通用可选择项目 包)
            {
                包.解包标志 = 通用可选择项目.数据解包标志.厨电配方;
                包.物联网已上线设备表或内部储物表或试剂引用 = 厨电的所有配方;
                if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                { 包.操作数["厨电配方数"] = -1; }
                else { 包.操作数["厨电配方数"] = ((List<(ReagentMixture 配方, Item 成品)>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
            }
        }
    }
}