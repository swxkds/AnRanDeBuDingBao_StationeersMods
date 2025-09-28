using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Appliances;
using Assets.Scripts.Objects.Items;
using HarmonyLib;
using Reagents;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 按控件(this ReagentProcessor 原料加工机, Interactable 原料加工机控件, 通用可选择项目 已选择)
        {
            功能模块之厨电交互扩展.添加材料到厨电中(原料加工机, 原料加工机控件, 已选择.链接厨电配方.配方);
        }

        private static MethodInfo ReagentProcessor_Recipes = AccessTools.PropertyGetter(typeof(ReagentProcessor), "Recipes");
        private static List<(ReagentMixture 配方, Item 成品)> 原料加工机的所有配方 = null;

        public static 通用可选择项目 生成消息(this ReagentProcessor 原料加工机, Interactable 原料加工机控件)
        {
            if (原料加工机的所有配方 == null)
            {
                var 配方索引表 = (Dictionary<int, Item>)ReagentProcessor_Recipes.Invoke(原料加工机, null);
                原料加工机的所有配方 = new(配方索引表.Count);

                // 成品仅用于展示, 实际投放材料时, 仅投放最大堆垛量的材料, 因此配方中仅包含最大堆垛量的材料
                foreach ((int 材料哈希, Item 成品) in 配方索引表)
                {
                    var 材料 = Prefab.Find<Item>(材料哈希);
                    if (材料)
                    {
                        var 投放一次的材料成分 = 材料.AddMixture;         // 投放一次的材料成分, 注意投放一次不是投放一份

                        if (材料 is IQuantity 堆垛件)
                        {
                            var 最大投放数 = 堆垛件.GetMaxQuantity;
                            var 投放一次的消耗数量 = 材料.QuantityPerUse;                       // 投放一次消耗的数量
                            var 可投放次数 = Mathf.FloorToInt(最大投放数 / 投放一次的消耗数量);
                            原料加工机的所有配方.Add((投放一次的材料成分 * 可投放次数, 成品));
                        }
                        else
                        {
                            原料加工机的所有配方.Add((投放一次的材料成分, 成品));
                        }
                    }
                }
            }

            return 功能模块之厨电交互扩展.生成消息_厨电(原料加工机, 原料加工机控件, 原料加工机的所有配方);
        }
    }

    [HarmonyPatch(typeof(ReagentProcessor), nameof(ReagentProcessor.InteractWith))]
    public class 原料加工机交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, ReagentProcessor __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机 && __instance.IsChild)
            {
                switch (interactable.Action)
                {
                    case InteractableType.Slot1:
                        __result = 通用选择面板.交互(__instance, interactable, interaction, 贴标机, doAction);
                        if (__result == null) { return true; }
                        return false;
                    default: break;
                }
            }
            return true;
        }
    }
}