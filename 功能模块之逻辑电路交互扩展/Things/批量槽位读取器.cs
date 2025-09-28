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
        public static void 拧螺丝(this LogicBatchSlotReader 批量槽位读取器, Interactable 批量槽位读取器控件, 可选择项目 已选择项目)
        {
            switch (已选择项目.数据包.包头)
            {
                case 数据包.数据解包标志.物联网已上线设备:
                    switch (批量槽位读取器控件.Action)
                    {
                        case InteractableType.Button1: 拧螺丝一(批量槽位读取器, 已选择项目.数据包.链接物体); break;
                        default: break;
                    }
                    break;
                case 数据包.数据解包标志.插槽编号:
                    switch (批量槽位读取器控件.Action)
                    {
                        case InteractableType.Button2: 拧螺丝二(批量槽位读取器, 已选择项目.数据包.操作数["插槽编号"]); break;
                        default: break;
                    }
                    break;
                case 数据包.数据解包标志.插槽参数:
                    switch (批量槽位读取器控件.Action)
                    {
                        case InteractableType.Button3: 拧螺丝三(批量槽位读取器, (LogicSlotType)已选择项目.数据包.操作数["参数"]); break;
                        default: break;
                    }
                    break;
                case 数据包.数据解包标志.统计模式:
                    switch (批量槽位读取器控件.Action)
                    {
                        case InteractableType.Button4: 拧螺丝四(批量槽位读取器, (LogicBatchMethod)已选择项目.数据包.操作数["参数"]); break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void 拧螺丝一(LogicBatchSlotReader 批量槽位读取器, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            批量槽位读取器.CurrentPrefabHash = 已选择.GetPrefabHash();
            批量槽位读取器.LogicSlotType = LogicSlotType.None;
            批量槽位读取器.Setting = 0;
            批量槽位读取器.SlotIndex = -1;
        }
        private static void 拧螺丝二(LogicBatchSlotReader 批量槽位读取器, int 插槽编号)
        {
            // TODO:联机游戏请在此处发送数据包
            if (批量槽位读取器.CurrentPrefab != null)
            {
                批量槽位读取器.SlotIndex = 插槽编号;
                批量槽位读取器.Setting = 0;
            }
        }
        private static void 拧螺丝三(LogicBatchSlotReader 批量槽位读取器, LogicSlotType 插槽类型)
        {
            // TODO:联机游戏请在此处发送数据包
            if (批量槽位读取器.CurrentPrefab != null && 批量槽位读取器.SlotIndex != -1)
            {
                批量槽位读取器.LogicSlotType = 插槽类型;
                批量槽位读取器.Setting = 0;
            }
        }
        private static void 拧螺丝四(LogicBatchSlotReader 批量槽位读取器, LogicBatchMethod 统计类型)
        {
            // TODO:联机游戏请在此处发送数据包
            批量槽位读取器.BatchMethod = 统计类型;
            批量槽位读取器.Setting = 0;
        }
        public static 数据包 生成消息(this LogicBatchSlotReader 批量槽位读取器, Interactable 批量槽位读取器控件)
        {
            数据包 包 = new();
            包.链接物体 = 批量槽位读取器.CurrentPrefab;

            switch (批量槽位读取器控件.Action)
            {
                case InteractableType.Button1:
                    包.包头 = 数据包.数据解包标志.有线网已上线设备;
                    包.物联网已上线设备表或试剂引用 = 批量槽位读取器.InputNetwork1DevicesSorted?.Where(d => d != (ILogicable)批量槽位读取器 && d.IsLogicSlotReadable());
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
                    包.操作数["插槽编号"] = 批量槽位读取器.SlotIndex;
                    break;
                case InteractableType.Button4:
                    包.包头 = 数据包.数据解包标志.统计模式;
                    break;
                default:
                    包.包头 = 数据包.数据解包标志.未知;
                    break;
            }
            return 包;
        }
    }

    [HarmonyPatch(typeof(LogicBatchSlotReader), nameof(LogicBatchSlotReader.InteractWith))]
    public class 批量槽位读取器交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicBatchSlotReader __instance, Interactable interactable, Interaction interaction, bool doAction)
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


