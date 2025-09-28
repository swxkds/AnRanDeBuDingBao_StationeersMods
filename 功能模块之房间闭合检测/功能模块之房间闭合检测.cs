using HarmonyLib;
using BepInEx;
using Assets.Scripts;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_fangjianbihejiance", "功能模块之房间闭合检测", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之房间闭合检测 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用
            if (GameManager.IsBatchMode) { return; }

            Log = Logger;
            Log.LogMessage("功能模块之房间闭合检测加载完成!");
            补丁 = new Harmony("功能模块之房间闭合检测");
            补丁.PatchAll();

            前置模块.添加初始化事件(快捷键配置.Initialize);
            前置模块.添加通用间接绘制构造参数(new() { (多图层_多物体_批量绘制.图层类型.房间, 0, null) }, static () => 快捷键配置.房间闭合检测_功能开关);
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.SetupKeyBindings))]
    public class 快捷键配置
    {
        public static void Postfix()
        {
            通用工具.创建游戏主菜单按键键位配置选项卡布局组(按键布局组名称);
            通用工具.创建游戏主菜单按键键位配置选项卡(按键名称兼索引key, 初始默认按键, 按键布局组名称);
        }

        public const string 按键布局组名称 = "FangJian";
        public const string 按键名称兼索引key = "FangJianBiHeJianCe";
        public const KeyCode 初始默认按键 = KeyCode.RightShift;
        public static bool 房间闭合检测_功能开关 { get; private set; }
        private static void 点击了监听键位() => 房间闭合检测_功能开关 = !房间闭合检测_功能开关;
        public static void Initialize()
        {
            new 通用工具.按键键位状态轮询组件(按键名称兼索引key, 初始默认按键, 点击了监听键位);
        }
        public static void Dispose()
        {
            if (通用工具.按键键位状态轮询组件.所有按键键位状态轮询组件.TryGetValue(按键名称兼索引key, out var 按键键位轮询组件))
            {
                按键键位轮询组件.Dispose();
            }
        }
    }
}
