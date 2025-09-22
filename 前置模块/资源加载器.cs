using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Assets.Scripts.Util;
using TMPro;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 前置_资源加载器
    {
        private static 前置_资源加载器 m_单例 = null;
        public static 前置_资源加载器 单例
        {
            get
            {
                if (m_单例 == null)
                    m_单例 = new 前置_资源加载器();
                return m_单例;
            }
        }
        public Font 当前Font字体 = null;
        public TMP_FontAsset 当前TMP字体 = null;
        public MonoBehaviour[] 逻辑组件表 = null;
        public GameObject[] 预制体表 = null;
        public Dictionary<Type, List<MonoBehaviour>> 逻辑组件字典 = new();
        public Dictionary<string, List<GameObject>> 预制体字典 = new();
        public TMP_FontAsset[] Tmp字体表 = null;
        public 前置_资源加载器()
        {
            using (Stream 读 = Assembly.GetExecutingAssembly().GetManifestResourceStream("meanran_xuexi_mods.Resources.TextFont.assets"))
            {
                当前Font字体 = AssetBundle.LoadFromStream(读).LoadAsset<Font>("ZiTi");
                if (当前Font字体) { 前置模块.Log.LogMessage($"成功加载Font字体: {当前Font字体.name}"); }
                else { 前置模块.Log.LogError($"加载Font字体失败"); }
            }

            逻辑组件表 = Resources.FindObjectsOfTypeAll<MonoBehaviour>();     // 所有控制性质的代码都继承MonoBehaviour
            HashSet<MonoBehaviour> 逻辑组件表去重 = [.. 逻辑组件表];
            逻辑组件表 = 逻辑组件表去重.ToArray();

            预制体表 = Resources.FindObjectsOfTypeAll<GameObject>();          // 所有实体都是以GameObject层级组织起来的
            HashSet<GameObject> 预制体表去重 = [.. 预制体表];
            预制体表 = 预制体表去重.ToArray();

            foreach (var v in 逻辑组件表)
            {
                var type = v.GetType();
                if (!逻辑组件字典.ContainsKey(type)) { 逻辑组件字典.Add(type, new List<MonoBehaviour>()); }
                逻辑组件字典[type].Add(v);
            }

            foreach (var v in 预制体表)
            {
                var type = v.name;
                if (!预制体字典.ContainsKey(type)) { 预制体字典.Add(type, new List<GameObject>()); }
                预制体字典[type].Add(v);
            }

            Tmp字体表 = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

            前置模块.Log.LogMessage($"从Unity中找到Tmp字体数量: {Tmp字体表.Length}");
            foreach (var obj in Tmp字体表)
            {
                if (obj && obj.name == "font_cjk")
                {
                    当前TMP字体 = obj;
                    break;
                }
            }

            if (当前TMP字体) { 前置模块.Log.LogMessage($"成功加载TMP字体: {当前TMP字体.name}"); }
            else { 前置模块.Log.LogError($"加载TMP字体失败"); }
        }
    }
}