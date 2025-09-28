using Assets.Scripts;
using BepInEx;
using meanran_xuexi_mods_xiaoyouhua.utils;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_zengqiangxianshi", "功能模块之增强现实", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之增强现实 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之增强现实加载完成!");
            补丁 = new Harmony("功能模块之增强现实");
            补丁.PatchAll();
            WorldManager.OnWorldStarted += () => HUD抬头显示器.构造函数();
        }
    }
}