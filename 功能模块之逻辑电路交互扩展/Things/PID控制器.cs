using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 拧螺丝(this LogicPidController PID控制器, Interactable PID控制器控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    switch (PID控制器控件.Action)
                    {
                        case InteractableType.Button1:
                            拧螺丝一(PID控制器, 已选择.链接物体);
                            拧螺丝联机数据包.发送数据包(PID控制器.ReferenceId, 已选择.链接物体.ReferenceId, PID控制器控件.InteractableId, 0, 0, 通用可选择项目.数据解包标志.物联网已上线设备);
                            break;
                        default: break;
                    }
                    break;
                case 通用可选择项目.数据解包标志.逻辑参数:
                    switch (PID控制器控件.Action)
                    {
                        case InteractableType.Button2:
                            拧螺丝二(PID控制器, (LogicType)已选择.操作数["参数"]);
                            拧螺丝联机数据包.发送数据包(PID控制器.ReferenceId, 0, PID控制器控件.InteractableId, 0, 已选择.操作数["参数"], 通用可选择项目.数据解包标志.逻辑参数);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝一(LogicPidController PID控制器, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            PID控制器.CurrentDevice = 已选择 as Device;
            PID控制器.LogicType = LogicType.None;
            PID控制器.Setting = 0;

            ////PID控制器.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
        }
        private static void 拧螺丝二(LogicPidController PID控制器, LogicType 参数)
        {
            // TODO:联机游戏请在此处发送数据包
            if (PID控制器.CurrentDevice != null)
            {
                PID控制器.LogicType = 参数;
                PID控制器.Setting = 0;

                ////PID控制器.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
            }
        }
        public static 通用可选择项目 生成消息(this LogicPidController PID控制器, Interactable PID控制器控件)
        {
            通用可选择项目 包 = new();
            包.链接物体 = PID控制器.CurrentDevice;

            switch (PID控制器控件.Action)
            {
                case InteractableType.Button1:
                    包.解包标志 = 通用可选择项目.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或内部储物表或试剂引用 = PID控制器.InputNetwork1DevicesSorted?.Where(d => d != (ILogicable)PID控制器 && d.IsLogicReadable());
                    if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
                    break;
                case InteractableType.Button2:
                    包.解包标志 = 通用可选择项目.数据解包标志.逻辑参数;
                    包.操作数["IOCheck"] = (int)IOCheck.Readable;
                    break;
                default:
                    包.解包标志 = 通用可选择项目.数据解包标志.未知;
                    break;
            }
            return 包;
        }
    }

    [HarmonyPatch(typeof(LogicPidController), nameof(LogicPidController.InteractWith))]
    public class PID控制器交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicPidController __instance, Interactable interactable, Interaction interaction, bool doAction)
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


