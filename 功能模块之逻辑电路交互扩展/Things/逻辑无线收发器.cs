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
        public static void 拧螺丝(this LogicTransmitter 逻辑无线收发器, Interactable 逻辑无线收发器控件, 可选择项目 已选择项目)
        {
            switch (已选择项目.数据包.包头)
            {
                case 数据包.数据解包标志.物联网已上线设备:
                    switch (逻辑无线收发器控件.Action)
                    {
                        case InteractableType.Button1: 拧螺丝一(逻辑无线收发器, 已选择项目.数据包.链接物体); break;
                        default: break;
                    }
                    break;
                case 数据包.数据解包标志.物联网信号模式:
                    switch (逻辑无线收发器控件.Action)
                    {
                        case InteractableType.Mode: 拧信号模式螺丝(逻辑无线收发器, 已选择项目.数据包.操作数["参数"]); break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝一(LogicTransmitter 逻辑无线收发器, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            逻辑无线收发器.CurrentDevice = 已选择 as ITransmitable;
            逻辑无线收发器.Setting = 0;
        }
        private static void 拧信号模式螺丝(LogicTransmitter 逻辑无线收发器, int 信号模式)
        {
            // TODO:联机游戏请在此处发送数据包
            if (逻辑无线收发器.Mode == 信号模式) { return; }
            OnServer.Interact(逻辑无线收发器.InteractMode, (逻辑无线收发器.InteractMode.State != 1) ? 1 : 0);
            逻辑无线收发器.CurrentDevice = null;
            逻辑无线收发器.Setting = 0;
        }
        public static 数据包 生成消息(this LogicTransmitter 逻辑无线收发器, Interactable 逻辑无线收发器控件)
        {
            数据包 包 = new();
            包.链接物体 = (Thing)逻辑无线收发器.CurrentDevice;

            switch (逻辑无线收发器控件.Action)
            {
                case InteractableType.Button1:
                    包.包头 = 数据包.数据解包标志.物联网已上线设备;
                    if (逻辑无线收发器.Mode == 1)
                    { 包.操作数["参数"] = (int)数据包.信号模式.桥接; }
                    else
                    {
                        包.操作数["参数"] = (int)数据包.信号模式.直连;
                        包.物联网已上线设备表或试剂引用 = Transmitters.AllTransmitters?.Where(d => d != (ILogicable)逻辑无线收发器 && (d is ITransmitable) && (d.IsLogicReadable() || d.IsLogicWritable()));
                        if (包.物联网已上线设备表或试剂引用 == null)
                        { 包.操作数["在线设备数"] = -1; }
                        else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或试剂引用).Count(); }
                    }
                    break;
                case InteractableType.Mode:
                    包.包头 = 数据包.数据解包标志.物联网信号模式;
                    break;
                default:
                    包.包头 = 数据包.数据解包标志.未知;
                    break;
            }
            return 包;
        }
    }

    [HarmonyPatch(typeof(LogicTransmitter), nameof(LogicTransmitter.InteractWith))]
    public class 逻辑无线收发器交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicTransmitter __instance, Interactable interactable, Interaction interaction, bool doAction)
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


