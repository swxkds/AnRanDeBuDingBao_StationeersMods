using Assets.Scripts.Objects;
using HarmonyLib;
using Assets.Scripts.Objects.Items;
using BepInEx;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_youhuagenghuandianchi", "功能模块之优化更换电池", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之优化更换电池 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之优化更换电池加载完成!");
            补丁 = new Harmony("功能模块之优化更换电池");
            补丁.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Slot), nameof(Slot.AllowSwap), [typeof(Slot), typeof(DynamicThing)])]
    public class 在鼠标拖动模式时交换电池1
    {
        [HarmonyPostfix]
        public static void 交互(ref bool __result, Slot sourceSlot, DynamicThing destination)
        {
            拦截交互(ref __result, sourceSlot, destination);
        }

        public static void 拦截交互(ref bool __result, Slot 源槽位, DynamicThing 目标槽位当前物品)
        {
            if (__result) { return; }
            if (!KeyManager.GetMouseUp("Primary")) { return; }

            var 新电池 = 源槽位.Get<BatteryCell>();
            var 电动工具 = 目标槽位当前物品 as PowerTool;
            if (新电池 && 电动工具)
            {
                var 目标槽位 = 电动工具.BatterySlot;
                var 旧电池 = 目标槽位.Get<BatteryCell>();
                if (旧电池)
                {
                    通用工具.交换槽位物品(源槽位, 新电池, 目标槽位, 旧电池);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Slot), nameof(Slot.AllowSwap), [typeof(Slot), typeof(Slot)])]
    public class 在鼠标拖动模式时交换电池2
    {
        [HarmonyPostfix]
        public static void 交互(ref bool __result, Slot sourceSlot, Slot destinationSlot)
        {
            在鼠标拖动模式时交换电池1.拦截交互(ref __result, sourceSlot, destinationSlot.Get());
        }
    }
}

