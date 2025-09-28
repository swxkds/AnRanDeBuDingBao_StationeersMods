using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Appliances;
using Assets.Scripts.Objects.Items;
using HarmonyLib;
using Reagents;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 按控件(this Microwave 微波炉, Interactable 微波炉控件, 通用可选择项目 已选择)
        {
            功能模块之厨电交互扩展.按控件_厨电(微波炉, 微波炉控件, 已选择);
        }

        private static MethodInfo Microwave_Recipes = AccessTools.PropertyGetter(typeof(Microwave), "Recipes");
        private static List<(ReagentMixture 配方, Item 成品)> 微波炉的所有配方 = null;

        public static 通用可选择项目 生成消息(this Microwave 微波炉, Interactable 微波炉控件)
        {
            if (微波炉的所有配方 == null)
            {
                var 配方索引表 = (Dictionary<Recipe, Item>)Microwave_Recipes.Invoke(微波炉, null);
                微波炉的所有配方 = new(配方索引表.Count);

                foreach ((Recipe 配方, Item 成品) in 配方索引表)
                {
                    微波炉的所有配方.Add((new ReagentMixture(配方), 成品));
                }
            }

            return 功能模块之厨电交互扩展.生成消息_厨电(微波炉, 微波炉控件, 微波炉的所有配方);
        }
    }

    [HarmonyPatch(typeof(Microwave), nameof(Microwave.InteractWith))]
    public class 微波炉交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Microwave __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机 && __instance.IsChild)
            {
                switch (interactable.Action)
                {
                    case InteractableType.Activate:
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