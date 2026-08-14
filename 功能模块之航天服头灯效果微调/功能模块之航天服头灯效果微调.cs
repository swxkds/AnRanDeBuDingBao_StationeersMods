using HarmonyLib;
using Assets.Scripts.Objects.Items;
using BepInEx;
using UnityEngine;
using Assets.Scripts.Objects;
using Assets.Scripts;
using System;
using Assets.Scripts.Inventory;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.Util;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_hangtianfutoudengxiaoguoweidiao", "功能模块之航天服头灯效果微调", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之航天服头灯效果微调 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之航天服头灯效果微调加载完成!");
            补丁 = new Harmony("功能模块之航天服头灯效果微调");

            // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用
            if (GameManager.IsBatchMode)
            {
                // 服务器只需要事件的交互, 不需要添加图形化控件
                var 源 = AccessTools.Method(typeof(RoadFlare), nameof(RoadFlare.InteractWith));
                var 插入 = AccessTools.Method(typeof(信号弹交互), nameof(信号弹交互.交互));
                补丁.Patch(源, prefix: new(插入));
                return;
            }
            else
            {
                // 通过补丁添加事件交互, 并添加图形化控件
                补丁.PatchAll();
                前置模块.添加初始化事件(添加切换灯光模式按钮);  // 添加控件必须在Thing.Awake方法前调用, 因为Thing的初始化是数据驱动设计模式
                前置模块.添加初始化事件(添加信号弹切换为冷紫色按钮);
            }
        }

        private static void 添加信号弹切换为冷紫色按钮()
        {
            if (前置_资源加载器.单例.TryGetAllComponent<RoadFlare>(out var 信号弹))
            {
                foreach (RoadFlare 当前 in 信号弹)
                {
                    if (当前.PrefabName == "ItemRoadFlare")
                    {
                        当前.添加控件(InteractableType.Button17, 是否创建UI按钮: true, 指定NameID: "QieHuanLengGuangYuanSe");
                    }
                }
            }
        }

        private static void 添加切换灯光模式按钮()
        {
            if (前置_资源加载器.单例.TryGetAllComponent<PortableLight>(out var 便携式照明灯))
            {
                foreach (PortableLight 当前 in 便携式照明灯)
                {
                    switch (当前.PrefabName)
                    {
                        case "DynamicLight":
                            {
                                var __ = 当前.Lights.FirstOrDefault();
                                if (__ == null || __.Light == null) { continue; }
                                {
                                    工具.便携式照明灯光组件 = __.Light;
                                    工具.便携式照明灯光配置 = new 通用工具.灯光配置(__.Light);
                                    Log.LogMessage("成功找到便携式照明灯的灯光组件");
                                }
                                break;
                            }
                    }
                    break;
                }
            }

            if (工具.便携式照明灯光组件)
            {
                var 查找灯光组件事件 = static (Thing d) => { return d.Lights.FirstOrDefault()?.Light; };
                添加切换灯光模式按钮<GasMask>(["ItemEmergencySpaceHelmet", "ItemSpaceHelmet", "ItemHardsuitHelmet"], 查找灯光组件事件);  // 应急太空头盔 太空头盔 强化太空头盔
                添加切换灯光模式按钮<HarmSuitHelmet>(["ItemSuitHelmetHARM"], 查找灯光组件事件);  // HARM液冷重型太空头盔
                添加切换灯光模式按钮<Headlamp>(["ItemHardHat", "ItemWearLamp"], 查找灯光组件事件);  // 安全帽 头灯
                添加切换灯光模式按钮<Helmet>(["ItemMarineHelmet"], 查找灯光组件事件);  // 陆战队头盔
            }

            static void 添加切换灯光模式按钮<T>(string[] 所有匹配预制体名称, Func<Thing, Light> 查找灯光组件事件) where T : Thing
            {
                if (前置_资源加载器.单例.TryGetAllComponent<T>(out var 头灯))
                {
                    foreach (T 当前 in 头灯)
                    {
                        if (!所有匹配预制体名称.Contains(当前.PrefabName)) { continue; }
                        var key = (typeof(T), 当前.PrefabHash);
                        if (工具.所有灯光配置.ContainsKey(key)) { continue; }

                        var light = 查找灯光组件事件(当前);
                        if (light == null) { continue; }
                        {
                            工具.所有灯光配置.Add(key, (new 通用工具.灯光配置(light), 查找灯光组件事件));
                            当前.添加控件(InteractableType.Button17, 是否创建UI按钮: true, 指定NameID: "QieHuanDengGuangMoShi");
                        }
                    }
                }
            }
        }
    }

    public class 工具
    {
        public enum 灯光模式 { 原版, 野外光, 室内光, }
        public static readonly 灯光模式[] 灯光模式表 = (灯光模式[])Enum.GetValues(typeof(灯光模式));
        private static int 灯光模式计数 = -1;
        public static 灯光模式 下一个灯光模式
        {
            get
            {
                ++灯光模式计数;
                灯光模式计数 = 灯光模式计数 % 灯光模式表.Length;
                return 灯光模式表[灯光模式计数];
            }
        }
        public static Light 便携式照明灯光组件 = null;
        public static 通用工具.灯光配置 便携式照明灯光配置 = null;
        public static readonly Dictionary<(Type 实例类型, int 预制体哈希), (通用工具.灯光配置 原版灯光配置, Func<Thing, Light> 查找灯光组件事件)> 所有灯光配置 = new();

        public static bool 拦截交互<T>(ref Thing.DelayedActionInstance __result, Thing __instance, Interactable interactable, bool doAction) where T : Thing
        {
            if (__instance is not T 头灯 || interactable.Action != InteractableType.Button17) { return true; }
            var key = (typeof(T), 头灯.PrefabHash);
            if (!所有灯光配置.ContainsKey(key)) { return true; }

            __result = new Thing.DelayedActionInstance { Duration = 0, ActionMessage = ActionStrings.Set };
            __result.Succeed();

            if (doAction)
            {
                (通用工具.灯光配置 原版灯光配置, Func<Thing, Light> 查找灯光组件事件) = 所有灯光配置[key];
                var light = 查找灯光组件事件(头灯);

                switch (下一个灯光模式)
                {
                    case 灯光模式.原版:
                        原版灯光配置.应用灯光配置(light);
                        break;
                    case 灯光模式.野外光:
                        便携式照明灯光配置.应用灯光配置(light, 亮度系数: 通用工具.灯光配置.野外光亮度系数, 范围系数: 通用工具.灯光配置.野外光范围系数);
                        break;
                    case 灯光模式.室内光:
                        便携式照明灯光配置.应用灯光配置(light, 亮度系数: 通用工具.灯光配置.室内光亮度系数, 范围系数: 通用工具.灯光配置.室内光范围系数);
                        break;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(GasMask), nameof(GasMask.InteractWith))]
    public class 太空头盔交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Thing __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            switch (__instance.PrefabName)
            {
                case "ItemEmergencySpaceHelmet":                        // 应急太空头盔
                case "ItemSpaceHelmet":                                 // 太空头盔
                case "ItemHardsuitHelmet":                              // 强化太空头盔
                    return 工具.拦截交互<GasMask>(ref __result, __instance, interactable, doAction);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HarmSuitHelmet), nameof(HarmSuitHelmet.InteractWith))]
    public class 液冷重型太空头盔交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Thing __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            switch (__instance.PrefabName)
            {
                case "ItemSuitHelmetHARM":   // HARM液冷重型太空头盔
                    return 工具.拦截交互<HarmSuitHelmet>(ref __result, __instance, interactable, doAction);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Headlamp), nameof(Headlamp.InteractWith))]
    public class 头戴式灯交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Thing __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            switch (__instance.PrefabName)
            {
                case "ItemHardHat":     // 安全帽
                case "ItemWearLamp":    // 头灯
                    return 工具.拦截交互<Headlamp>(ref __result, __instance, interactable, doAction);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Helmet), nameof(Helmet.InteractWith))]
    public class 陆战队头盔交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Thing __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            switch (__instance.PrefabName)
            {
                case "ItemMarineHelmet":   // 陆战队头盔
                    return 工具.拦截交互<Helmet>(ref __result, __instance, interactable, doAction);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RoadFlare), nameof(RoadFlare.InteractWith))]
    public class 信号弹交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Thing __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (__instance is not RoadFlare 信号弹 || interactable.Action != InteractableType.Button17) { return true; }

            __result = new Thing.DelayedActionInstance { Duration = 0, ActionMessage = ActionStrings.Paint };
            __result.Succeed();

            if (doAction)
            {
                var 冷紫光 = GameManager.GetColorSwatch((int)通用工具.游戏内置喷漆颜色.色板.紫色);
                if (冷紫光 == null) { return true; }
                OnServer.SetCustomColor(信号弹, 冷紫光.Index);
            }

            return false;
        }
    }
}
