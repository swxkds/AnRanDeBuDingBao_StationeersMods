using System.Collections;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.UI;
using BepInEx;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_ceshi", "测试", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 测试 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("测试加载完成!");
            补丁 = new Harmony("测试");
            补丁.PatchAll();
            StartCoroutine(并发构造());  // Unity引擎对第三方协程库的支持是后期开发的,因此API实例的加载顺序挺靠后,在Awake时请使用Unity原生协程
        }

        private IEnumerator 并发构造()
        {
            // InventoryWindowManager.Instance.WindowGrid:垂直排版多个背包窗口的布局组
            // InventoryWindowManager.Instance.WindowPrefab:背包窗口克隆母体
            while (InventoryWindowManager.Instance == null
            || InventoryWindowManager.Instance.WindowGrid == null
            || InventoryWindowManager.Instance.WindowPrefab == null)
            { yield return null; }    // 等待下一帧

            CircuitHousing

            // if (前置_资源加载器.单例.逻辑组件字典.TryGetValue(typeof(InputPrefabs), out var 选择配方面板))
            // {

            // }


        }
    }
}

