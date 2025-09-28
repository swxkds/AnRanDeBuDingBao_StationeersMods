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
        public static void 拧螺丝(this LogicMirror 逻辑镜像, Interactable 逻辑镜像控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    switch (逻辑镜像控件.Action)
                    {
                        case InteractableType.Button1:
                            拧螺丝一(逻辑镜像, 已选择.链接物体);
                            拧螺丝联机数据包.发送数据包(逻辑镜像.ReferenceId, 已选择.链接物体.ReferenceId, 逻辑镜像控件.InteractableId, 0, 0, 通用可选择项目.数据解包标志.物联网已上线设备);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝一(LogicMirror 逻辑镜像, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            逻辑镜像.CurrentDevice = 已选择 as Device;
            
            //// 逻辑镜像.aaaNetworkUpdateFlags |= aaaushort.MaxValue;

            if (逻辑镜像.CurrentDevice == null || 逻辑镜像.InputNetwork1 == null || !逻辑镜像.InputNetwork1.DataDeviceList.Contains(逻辑镜像.CurrentDevice))
            {
                if (逻辑镜像.Error == 0)
                {
                    OnServer.Interact(逻辑镜像.InteractError, 1, false);
                }
                return;
            }

            if (逻辑镜像.Error == 1)
            {
                OnServer.Interact(逻辑镜像.InteractError, 0, false);
            }
            return;
        }
        public static 通用可选择项目 生成消息(this LogicMirror 逻辑镜像, Interactable 逻辑镜像控件)
        {
            通用可选择项目 包 = new();
            包.链接物体 = 逻辑镜像.CurrentDevice;

            switch (逻辑镜像控件.Action)
            {
                case InteractableType.Button1:
                    包.解包标志 = 通用可选择项目.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或内部储物表或试剂引用 = 逻辑镜像.InputNetwork1DevicesSorted?.Where(d => d != (ILogicable)逻辑镜像 && d.IsLogicReadable());
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
    }

    [HarmonyPatch(typeof(LogicMirror), nameof(LogicMirror.InteractWith))]
    public class 逻辑镜像交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicMirror __instance, Interactable interactable, Interaction interaction, bool doAction)
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


