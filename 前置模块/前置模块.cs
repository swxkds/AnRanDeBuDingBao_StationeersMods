using BepInEx;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", "前置模块", "1.0.0")]
    public class 前置模块 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        private void Awake()
        {
            Log = base.Logger;
            Log.LogMessage("前置模块加载完成!");
        }
    }
}
