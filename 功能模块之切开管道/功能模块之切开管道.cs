using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Localization2;
using BepInEx;
using Networks;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_qiekaiguandao", "功能模块之切开管道", "1.0.0")]
    public class 功能模块之切割管道 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之切开管道加载完成!");
            补丁 = new Harmony("功能模块之切开管道");
            补丁.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Piping), nameof(Piping.AttackWith))]
    public class 增加角磨机与气管_液管的交互事件
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, object __instance, Attack attack, bool doAction)
        {
            // 发起交互方的活动手上的工具是角磨机,并且管道不是炸开状态
            if (attack.SourceItem is AngleGrinder 角磨机 && __instance is Piping 管道 && 管道.IsBurst == Assets.Scripts.Networks.PipeBurst.None)
            {
                __result = 切开管道(管道, 角磨机, doAction);
                return false;
            }
            else
            {
                return true;    // 其他情况执行游戏自带的交互逻辑
            }
        }

        public static Thing.DelayedActionInstance 切开管道(Piping 管道, AngleGrinder 角磨机, bool doAction)
        {
            var 消息 = new Thing.DelayedActionInstance
            { Duration = 1, ActionMessage = "切开" };

            // 电池未安装或者没有电量
            if (!角磨机.IsOperable)
            {
                消息.Fail(GameStrings.DeviceNoPower);   // 仅显示工具提示面板:未通电,无法交互  
            }
            else
            {
                消息.AppendStateMessage("使用角磨机切开管道,使流体排出");   // 显示工具提示面板
                消息.Succeed();

                if (doAction)
                {
                    // 播放音效
                    角磨机.PlaySound(AngleGrinder.UnEquipGrinderHash, 1f, 1f);

                    var 管道大气网络 = (AtmosphericsNetwork)管道.StructureNetwork;
                    // var 管道大气 = AtmosphericsController.World.SampleGlobalAtmosphere(管道.WorldGrid);

                    // 设置管网抗压最弱管道,然后设置管道损伤至炸开
                    Traverse.Create(管道大气网络).Field("_weakestMember").SetValue(管道);

                    // 在Damage方法中同步标志已写入值,自动同步,请在服务器与客户端都安装本模组
                    管道.GetAsThing.DamageState.Damage(ChangeDamageType.Increment, 200, DamageUpdateType.Brute);    // 仅服务器调用
                    管道.DamageRecord |= Assets.Scripts.Networks.PipeBurst.Pressure;
                    if (管道.Stressed != true) { 管道.Stressed = true; }
                }
            }

            return 消息;
        }
    }
}

