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
        public static void 拧螺丝(this LogicSlotReader 槽位读取器, Interactable 槽位读取器控件, 可选择项目 已选择项目)
        {
            switch (已选择项目.数据包.包头)
            {
                case 数据包.数据解包标志.物联网已上线设备:
                    switch (槽位读取器控件.Action)
                    {
                        case InteractableType.Button1: 拧螺丝一(槽位读取器, 已选择项目.数据包.链接物体); break;
                        default: break;
                    }
                    break;
                case 数据包.数据解包标志.插槽编号:
                    switch (槽位读取器控件.Action)
                    {
                        case InteractableType.Button2: 拧螺丝二(槽位读取器, 已选择项目.数据包.操作数["插槽编号"]); break;
                        default: break;
                    }
                    break;
                case 数据包.数据解包标志.插槽参数:
                    switch (槽位读取器控件.Action)
                    {
                        case InteractableType.Button3: 拧螺丝三(槽位读取器, (LogicSlotType)已选择项目.数据包.操作数["参数"]); break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝一(LogicSlotReader 槽位读取器, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            槽位读取器.CurrentDevice = (Device)已选择;
            槽位读取器.LogicSlotType = LogicSlotType.None;
            槽位读取器.Setting = 0;
            槽位读取器.SlotIndex = -1;
        }
        private static void 拧螺丝二(LogicSlotReader 槽位读取器, int 插槽编号)
        {
            // TODO:联机游戏请在此处发送数据包
            if (槽位读取器.CurrentDevice != null)
            {
                槽位读取器.SlotIndex = 插槽编号;
                槽位读取器.Setting = 0;
            }
        }
        private static void 拧螺丝三(LogicSlotReader 槽位读取器, LogicSlotType 插槽类型)
        {
            // TODO:联机游戏请在此处发送数据包
            if (槽位读取器.CurrentDevice != null && 槽位读取器.SlotIndex != -1)
            {
                槽位读取器.LogicSlotType = 插槽类型;
                槽位读取器.Setting = 0;
            }
        }
        public static 数据包 生成消息(this LogicSlotReader 槽位读取器, Interactable 槽位读取器控件)
        {
            数据包 包 = new();
            包.链接物体 = 槽位读取器.CurrentDevice;

            switch (槽位读取器控件.Action)
            {
                case InteractableType.Button1:
                    包.包头 = 数据包.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或试剂引用 = 槽位读取器.InputNetwork1DevicesSorted?.Where(d => d != (ILogicable)槽位读取器 && d.IsLogicSlotReadable());
                    if (包.物联网已上线设备表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或试剂引用).Count(); }
                    break;
                case InteractableType.Button2:
                    包.包头 = 数据包.数据解包标志.插槽编号;
                    包.操作数["IOCheck"] = (int)IOCheck.Readable;
                    break;
                case InteractableType.Button3:
                    包.包头 = 数据包.数据解包标志.插槽参数;
                    包.操作数["IOCheck"] = (int)IOCheck.Readable;
                    包.操作数["插槽编号"] = 槽位读取器.SlotIndex;
                    break;
                default:
                    包.包头 = 数据包.数据解包标志.未知;
                    break;
            }
            return 包;
        }
    }

    [HarmonyPatch(typeof(LogicSlotReader), nameof(LogicSlotReader.InteractWith))]
    public class 槽位读取器交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicSlotReader __instance, Interactable interactable, Interaction interaction, bool doAction)
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


