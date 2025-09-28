using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 拧螺丝(this DeviceInputOutputCircuit 气体设备, Interactable 气体设备控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    var __ = 已选择.链接物体;
                    switch (气体设备控件.Action)
                    {
                        case InteractableType.Button1:
                            拧螺丝N(气体设备, __, 0);
                            拧螺丝联机数据包.发送数据包(气体设备.ReferenceId, 已选择.链接物体.ReferenceId, 气体设备控件.InteractableId, 0, 0, 通用可选择项目.数据解包标志.物联网已上线设备);
                            break;
                        case InteractableType.Button2:
                            拧螺丝N(气体设备, __, 1);
                            拧螺丝联机数据包.发送数据包(气体设备.ReferenceId, 已选择.链接物体.ReferenceId, 气体设备控件.InteractableId, 0, 0, 通用可选择项目.数据解包标志.物联网已上线设备);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }

        private static void 拧螺丝N(DeviceInputOutputCircuit 气体设备, Thing 已选择, byte 螺丝编号)
        {
            // TODO:联机游戏请在此处发送数据包
            气体设备.Devices[螺丝编号] = 已选择 as ILogicable;
            ////气体设备.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
        }

        public static 通用可选择项目 生成消息(this DeviceInputOutputCircuit 气体设备, Interactable 气体设备控件)
        {
            通用可选择项目 包 = new();
            // 包.链接物体 = (Thing)气体设备.Devices[0];

            switch (气体设备控件.Action)
            {
                case InteractableType.Button1:
                case InteractableType.Button2:
                    包.解包标志 = 通用可选择项目.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或内部储物表或试剂引用 = 气体设备.DataNetworkDevicesSorted?.Where(d => d != (ILogicable)气体设备);
                    if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
                    break;
                default:
                    包.解包标志 = 通用可选择项目.数据解包标志.未知;
                    break;
            }
            
            return 包;
        }

        [HarmonyPatch(typeof(DeviceInputOutputCircuit), nameof(DeviceInputOutputCircuit.InteractWith))]
        public class 气体设备交互
        {
            [HarmonyPrefix]
            public static bool 交互(ref Thing.DelayedActionInstance __result, DeviceInputOutputCircuit __instance, Interactable interactable, Interaction interaction, bool doAction)
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


