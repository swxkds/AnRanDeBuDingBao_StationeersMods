using Assets.Scripts.Objects.Electrical;
using BepInEx;
using HarmonyLib;
using Objects.Electrical;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_weixingtianxianjiaohukuozhan", "功能模块之卫星天线交互扩展", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之卫星天线交互扩展 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之卫星天线交互扩展加载完成!");
            补丁 = new Harmony("功能模块之卫星天线交互扩展");
            补丁.PatchAll();
            卫星天线数据包.注册联机数据包包头类型();
            添加交互过程函数();
        }

        private static void 添加交互过程函数()
        {
            前置模块.添加交互过程函数([

            // 卫星天线
            (typeof(SatelliteDish),
            (static (主物体, 控件) => ((SatelliteDish)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((SatelliteDish)主物体).拧螺丝(控件, 可选择项目))),

            // 小型卫星天线
            (typeof(SmallSatelliteDish),
            (static (主物体, 控件) => ((SatelliteDish)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((SatelliteDish)主物体).拧螺丝(控件, 可选择项目))),

            // 中型卫星天线
            (typeof(MediumSatelliteDish),
            (static (主物体, 控件) => ((SatelliteDish)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((SatelliteDish)主物体).拧螺丝(控件, 可选择项目))),

            // 大型卫星天线
            (typeof(LargeSatelliteDish),
            (static (主物体, 控件) => ((SatelliteDish)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((SatelliteDish)主物体).拧螺丝(控件, 可选择项目))),

            ]);
        }
    }
}