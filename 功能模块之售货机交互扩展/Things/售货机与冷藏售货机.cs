using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 按控件(this VendingMachine 售货机, Interactable 售货机控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.内部储物:
                    switch (售货机控件.Action)
                    {
                        case InteractableType.Button1:
                        case InteractableType.Button2:
                            按控件(售货机, 已选择.链接物体);
                            售货机数据包.发送数据包(售货机.ReferenceId, 已选择.链接物体.ReferenceId);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }

        private static void 按控件(VendingMachine 售货机, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            var i = 售货机.Slots.FindIndex((s) => s.Get()?.ReferenceId == 已选择.ReferenceId);
            if (i >= 2) { 售货机.CurrentIndex = i; }                // 售货机的0和1分别是进口和出口槽位
        }

        public static 通用可选择项目 生成消息(this VendingMachine 售货机, Interactable 售货机控件)
        {
            通用可选择项目 包 = new();

            switch (售货机控件.Action)
            {
                case InteractableType.Button1:
                case InteractableType.Button2:
                    包.解包标志 = 通用可选择项目.数据解包标志.内部储物;
                    var __ = new List<Thing>();
                    for (var i = 2; i < 售货机.Slots.Count; i++)              // 售货机的0和1分别是进口和出口槽位
                    {
                        var 储物 = 售货机.Slots[i].Get();
                        if (储物) { __.Add(储物); }
                    }
                    包.物联网已上线设备表或内部储物表或试剂引用 = __;
                    if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                    { 包.操作数["内部储物数"] = -1; }
                    else { 包.操作数["内部储物数"] = ((IEnumerable<Thing>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
                    break;
                default:
                    包.解包标志 = 通用可选择项目.数据解包标志.未知;
                    break;
            }
            return 包;
        }

        [HarmonyPatch(typeof(VendingMachineRefrigerated), nameof(VendingMachineRefrigerated.InteractWith))]
        public class 冷藏售货机交互
        {
            [HarmonyPrefix]
            public static bool 交互(ref Thing.DelayedActionInstance __result, VendingMachineRefrigerated __instance, Interactable interactable, Interaction interaction, bool doAction)
            {
                if (interaction.SourceSlot.Get() is Labeller 贴标机)
                {
                    switch (interactable.Action)
                    {
                        case InteractableType.Button1:
                        case InteractableType.Button2:
                            __result = 通用选择面板.交互(__instance, interactable, interaction, 贴标机, doAction);
                            if (__result == null) { return true; }
                            return false;
                        default: break;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.InteractWith))]
        public class 售货机交互
        {
            [HarmonyPrefix]
            public static bool 交互(ref Thing.DelayedActionInstance __result, VendingMachine __instance, Interactable interactable, Interaction interaction, bool doAction)
            {
                if (interaction.SourceSlot.Get() is Labeller 贴标机)
                {
                    switch (interactable.Action)
                    {
                        case InteractableType.Button1:
                        case InteractableType.Button2:
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
}