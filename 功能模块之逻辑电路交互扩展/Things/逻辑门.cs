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
        public static void 拧螺丝(this LogicGate 逻辑门, Interactable 逻辑门控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    switch (逻辑门控件.Action)
                    {
                        case InteractableType.Button1:
                            拧螺丝一(逻辑门, 已选择.链接物体);
                            拧螺丝联机数据包.发送数据包(逻辑门.ReferenceId, 已选择.链接物体.ReferenceId, 逻辑门控件.InteractableId, 0, 0, 通用可选择项目.数据解包标志.物联网已上线设备);
                            break;
                        case InteractableType.Button2:
                            拧螺丝二(逻辑门, 已选择.链接物体);
                            拧螺丝联机数据包.发送数据包(逻辑门.ReferenceId, 已选择.链接物体.ReferenceId, 逻辑门控件.InteractableId, 0, 0, 通用可选择项目.数据解包标志.物联网已上线设备);
                            break;
                        default: break;
                    }
                    break;
                case 通用可选择项目.数据解包标志.逻辑门运算符:
                    switch (逻辑门控件.Action)
                    {
                        case InteractableType.Button3:
                            拧螺丝三(逻辑门, (GateOperators)已选择.操作数["参数"]);
                            拧螺丝联机数据包.发送数据包(逻辑门.ReferenceId, 0, 逻辑门控件.InteractableId, 0, 已选择.操作数["参数"], 通用可选择项目.数据解包标志.逻辑门运算符);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝一(LogicGate 逻辑门, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            逻辑门.Input1 = 已选择 as LogicUnitBase;
            逻辑门.Setting = 0;

            //// 逻辑门.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
        }
        private static void 拧螺丝二(LogicGate 逻辑门, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            逻辑门.Input2 = 已选择 as LogicUnitBase;
            逻辑门.Setting = 0;

            //// 逻辑门.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
        }
        private static void 拧螺丝三(LogicGate 逻辑门, GateOperators 逻辑门类型)
        {
            // TODO:联机游戏请在此处发送数据包
            逻辑门.Mode = (int)逻辑门类型;
            OnServer.Interact(逻辑门.InteractMode, 逻辑门.Mode, false);

            //// 逻辑门.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
        }
        public static 通用可选择项目 生成消息(this LogicGate 逻辑门, Interactable 逻辑门控件)
        {
            通用可选择项目 包 = new();
            // 包.链接物体 = 逻辑门.Input1;

            switch (逻辑门控件.Action)
            {
                case InteractableType.Button1:
                    包.解包标志 = 通用可选择项目.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或内部储物表或试剂引用 = 逻辑门.InputNetwork1DevicesSorted?.Where(d => d != (ILogicable)逻辑门 && d is LogicUnitBase && d.IsLogicReadable());
                    if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
                    break;
                case InteractableType.Button2:
                    包.解包标志 = 通用可选择项目.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或内部储物表或试剂引用 = 逻辑门.InputNetwork2DevicesSorted?.Where(d => d != (ILogicable)逻辑门 && d is LogicUnitBase && d.IsLogicReadable());
                    if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
                    break;
                case InteractableType.Button3:
                    包.解包标志 = 通用可选择项目.数据解包标志.逻辑门运算符;
                    break;
                default:
                    包.解包标志 = 通用可选择项目.数据解包标志.未知;
                    break;
            }
            return 包;
        }
    }

    [HarmonyPatch(typeof(LogicGate), nameof(LogicGate.InteractWith))]
    public class 逻辑门交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicGate __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机)
            {
                __result = 通用选择面板.交互(__instance, interactable, interaction, 贴标机, doAction);
                if (__result == null) { return true; }
                return false;
            }
            return true;
        }
    }

}


