using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Assets.Scripts.Objects.Items;
using BepInEx;
using Assets.Scripts.Networking;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_tiaoshi_mokuai_zhi_zhiwushunjianshengzhang", "调试模块之植物瞬间生长", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 调试模块之植物瞬间生长 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("调试模块之植物瞬间生长加载完成!");
            补丁 = new Harmony("调试模块之植物瞬间生长");
            补丁.PatchAll();
            
            植物瞬间生长数据包.注册联机数据包包头类型();
        }
    }

    [HarmonyPatch(typeof(HydroponicsTrayDevice), nameof(HydroponicsTrayDevice.AttackWith))]
    public class 增加水培托盘1的交互事件
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Thing __instance, Attack attack, bool doAction)
        {
            // 没有按创造工具的销毁模式按键, 也没有按创造工具的复制模式按键
            // 发起交互方的活动手上的工具是创造工具
            if (!attack.IsDestroy && !attack.IsCopy && attack.SourceItem is AuthoringTool 创造工具 && __instance is IHarvestable 水培托盘)
            {
                __result = 变更植物到生长的下一阶段(水培托盘, 创造工具, doAction);
                if (__result == null) { return true; }
                return false;
            }
            else
            {
                return true;    // 其他情况执行游戏自带的交互逻辑
            }
        }

        public static Thing.DelayedActionInstance 变更植物到生长的下一阶段(IHarvestable 水培托盘, AuthoringTool 创造工具, bool doAction)
        {
            var 消息 = new Thing.DelayedActionInstance
            { Duration = 0.5f, ActionMessage = "加速" };

            var 种植物 = 水培托盘.GetPlant;
            if (种植物 == null)
            {
                消息.AppendStateMessage("未发现种植物");   // 显示工具提示面板
                return 消息.Fail();
            }
            else
            {
                消息.AppendStateMessage("变更植物到生长的下一阶段");   // 显示工具提示面板
                消息.Succeed();
            }

            if (doAction)
            {
                // 服务器校正过程中相关变量的脏标记会改变, 通过原版游戏的同步机制自动发送同步数据包
                if (!NetworkManager.IsClient)
                {
                    种植物.SetNextStage();
                }
                植物瞬间生长数据包.发送数据包(种植物.ReferenceId);
            }

            return 消息;
        }
    }

    [HarmonyPatch(typeof(HydroponicTray), nameof(HydroponicTray.AttackWith))]
    public class 增加水培托盘2的交互事件
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, Thing __instance, Attack attack, bool doAction)
        {
            // 没有按创造工具的销毁模式按键, 也没有按创造工具的复制模式按键
            // 发起交互方的活动手上的工具是创造工具
            if (!attack.IsDestroy && !attack.IsCopy && attack.SourceItem is AuthoringTool 创造工具 && __instance is IHarvestable 水培托盘)
            {
                __result = 增加水培托盘1的交互事件.变更植物到生长的下一阶段(水培托盘, 创造工具, doAction);
                if (__result == null) { return true; }
                return false;
            }
            else
            {
                return true;    // 其他情况执行游戏自带的交互逻辑
            }
        }
    }
}

