using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 拧螺丝(this CircuitHousing IC外壳, Interactable IC外壳控件, 可选择项目 已选择项目)
        {
            switch (已选择项目.数据包.包头)
            {
                case 数据包.数据解包标志.物联网已上线设备:
                    var __ = 已选择项目.数据包.链接物体;
                    switch (IC外壳控件.Action)
                    {
                        case InteractableType.Button1: 拧螺丝N(IC外壳, __, 0); break;
                        case InteractableType.Button2: 拧螺丝N(IC外壳, __, 1); break;
                        case InteractableType.Button3: 拧螺丝N(IC外壳, __, 2); break;
                        case InteractableType.Button4: 拧螺丝N(IC外壳, __, 3); break;
                        case InteractableType.Button5: 拧螺丝N(IC外壳, __, 4); break;
                        case InteractableType.Button6: 拧螺丝N(IC外壳, __, 5); break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝N(CircuitHousing IC外壳, Thing 已选择, byte 螺丝编号)
        {
            // TODO:联机游戏请在此处发送数据包
            IC外壳.Devices[螺丝编号] = (ILogicable)已选择;
        }
        public static 数据包 生成消息(this CircuitHousing IC外壳, Interactable IC外壳控件)
        {
            数据包 包 = new();
            // 包.链接物体 = (Thing)IC外壳.Devices[0];

            包.包头 = 数据包.数据解包标志.有线网已上线设备;
            包.物联网已上线设备表或试剂引用 = IC外壳.InputNetwork1DevicesSorted?.Where(d => d != (ILogicable)IC外壳);
            if (包.物联网已上线设备表或试剂引用 == null)
            { 包.操作数["在线设备数"] = -1; }
            else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或试剂引用).Count(); }

            return 包;
        }
    }

    [HarmonyPatch(typeof(CircuitHousing), nameof(CircuitHousing.InteractWith))]
    public class IC外壳交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, CircuitHousing __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机)
            {
                __result = 选择面板.交互(__instance, interactable, interaction, 贴标机, doAction);
                    if (__result == null) { return true; }
                return false;
            }
            return true;
        }
    }
}








