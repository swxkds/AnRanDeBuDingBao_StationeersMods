using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Reagents;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 拧螺丝(this ReagentReader 试剂读取器, Interactable 试剂读取器控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    switch (试剂读取器控件.Action)
                    {
                        case InteractableType.Button1:
                            拧螺丝一(试剂读取器, 已选择.链接物体);
                            拧螺丝联机数据包.发送数据包(试剂读取器.ReferenceId, 已选择.链接物体.ReferenceId, 试剂读取器控件.InteractableId, 0, 0, 通用可选择项目.数据解包标志.物联网已上线设备);
                            break;
                        default: break;
                    }
                    break;
                case 通用可选择项目.数据解包标志.试剂模式:
                    switch (试剂读取器控件.Action)
                    {
                        case InteractableType.Button2:
                            拧螺丝二(试剂读取器, (LogicReagentMode)已选择.操作数["参数"]);
                            拧螺丝联机数据包.发送数据包(试剂读取器.ReferenceId, 0, 试剂读取器控件.InteractableId, 0, 已选择.操作数["参数"], 通用可选择项目.数据解包标志.试剂模式);
                            break;
                        default: break;
                    }
                    break;
                case 通用可选择项目.数据解包标志.试剂参数:
                    switch (试剂读取器控件.Action)
                    {
                        case InteractableType.Button3:
                            拧螺丝三(试剂读取器, (Reagents.Reagent)已选择.物联网已上线设备表或内部储物表或试剂引用);
                            拧螺丝联机数据包.发送数据包(试剂读取器.ReferenceId, 0, 试剂读取器控件.InteractableId, 0, ((Reagents.Reagent)已选择.物联网已上线设备表或内部储物表或试剂引用).ReagentId, 通用可选择项目.数据解包标志.试剂参数);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝一(ReagentReader 试剂读取器, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            试剂读取器.CurrentDevice = 已选择 as Device;
            试剂读取器.LogicReagentMode = LogicReagentMode.Contents;
            试剂读取器.Setting = 0;

            ////试剂读取器.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
        }
        private static void 拧螺丝二(ReagentReader 试剂读取器, LogicReagentMode 试剂模式)
        {
            // TODO:联机游戏请在此处发送数据包
            if (试剂读取器.CurrentDevice != null)
            {
                试剂读取器.LogicReagentMode = 试剂模式;
                试剂读取器.Setting = 0;

                ////试剂读取器.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
            }
        }
        private static void 拧螺丝三(ReagentReader 试剂读取器, Reagents.Reagent 试剂)
        {
            // TODO:联机游戏请在此处发送数据包
            if (试剂读取器.CurrentDevice != null)
            {
                试剂读取器.LogicReagent = 试剂;
                试剂读取器.Setting = 0;

                ////试剂读取器.aaaNetworkUpdateFlags |= aaaushort.MaxValue;
            }
        }
        public static 通用可选择项目 生成消息(this ReagentReader 试剂读取器, Interactable 试剂读取器控件)
        {
            通用可选择项目 包 = new();
            包.链接物体 = 试剂读取器.CurrentDevice;

            switch (试剂读取器控件.Action)
            {
                case InteractableType.Button1:
                    包.解包标志 = 通用可选择项目.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或内部储物表或试剂引用 = 试剂读取器.InputNetwork1DevicesSorted?.Where(d => d != (ILogicable)试剂读取器 && d is IRequireReagent);
                    if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
                    break;
                case InteractableType.Button2:
                    包.解包标志 = 通用可选择项目.数据解包标志.试剂模式;
                    break;
                case InteractableType.Button3:
                    包.解包标志 = 通用可选择项目.数据解包标志.试剂参数;
                    包.操作数["参数"] = (int)试剂读取器.LogicReagentMode;
                    break;
                default:
                    包.解包标志 = 通用可选择项目.数据解包标志.未知;
                    break;
            }
            return 包;
        }
    }

    [HarmonyPatch(typeof(ReagentReader), nameof(ReagentReader.InteractWith))]
    public class 试剂读取器交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, ReagentReader __instance, Interactable interactable, Interaction interaction, bool doAction)
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


