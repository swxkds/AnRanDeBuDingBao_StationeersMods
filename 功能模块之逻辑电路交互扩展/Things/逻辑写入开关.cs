using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static void 拧螺丝(this LogicWriterSwitch 逻辑写入开关, Interactable 逻辑写入开关控件, 可选择项目 已选择项目)
        {
            switch (已选择项目.数据包.包头)
            {
                case 数据包.数据解包标志.物联网已上线设备:
                    switch (逻辑写入开关控件.Action)
                    {
                        case InteractableType.Button1: 拧螺丝一(逻辑写入开关, 已选择项目.数据包.链接物体); break;
                        default: break;
                    }
                    break;
                case 数据包.数据解包标志.逻辑参数:
                    switch (逻辑写入开关控件.Action)
                    {
                        case InteractableType.Button2: 拧螺丝二(逻辑写入开关, (LogicType)已选择项目.数据包.操作数["参数"]); break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static FieldInfo _lastSetting = typeof(LogicWriterSwitch).GetField("_lastSetting", BindingFlags.NonPublic | BindingFlags.Instance);
        private static void 拧螺丝一(LogicWriterSwitch 逻辑写入开关, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            逻辑写入开关.CurrentOutput = (Device)已选择;
            逻辑写入开关.LogicType = LogicType.None;
            _lastSetting.SetValue(逻辑写入开关, 0);
        }
        private static void 拧螺丝二(LogicWriterSwitch 逻辑写入开关, LogicType 参数类型)
        {
            // TODO:联机游戏请在此处发送数据包
            if (逻辑写入开关.CurrentOutput != null)
            {
                逻辑写入开关.LogicType = 参数类型;
                _lastSetting.SetValue(逻辑写入开关, 0);
            }
        }
        public static 数据包 生成消息(this LogicWriterSwitch 逻辑写入开关, Interactable 逻辑写入开关控件)
        {
            数据包 包 = new();
            包.链接物体 = 逻辑写入开关.CurrentOutput;

            switch (逻辑写入开关控件.Action)
            {
                case InteractableType.Button1:
                    包.包头 = 数据包.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或试剂引用 = 逻辑写入开关.OutputNetwork1DevicesSorted?.Where(d => d != (ILogicable)逻辑写入开关 && d.IsLogicWritable());
                    if (包.物联网已上线设备表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或试剂引用).Count(); }
                    break;
                case InteractableType.Button2:
                    包.包头 = 数据包.数据解包标志.逻辑参数;
                    包.操作数["IOCheck"] = (int)IOCheck.Writable;
                    break;
                default:
                    包.包头 = 数据包.数据解包标志.未知;
                    break;
            }
            return 包;
        }
    }

    [HarmonyPatch(typeof(LogicWriterSwitch), nameof(LogicWriterSwitch.InteractWith))]
    public class 逻辑写入开关交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicWriterSwitch __instance, Interactable interactable, Interaction interaction, bool doAction)
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


