using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 前置_资源加载器
    {
        private static 前置_资源加载器 m_单例 = null;
        public static 前置_资源加载器 单例 { get { if (m_单例 == null) { m_单例 = new(); } return m_单例; } }

        public Font 当前Font字体 { get; private set; }
        public TMP_FontAsset[] 所有查找到的TMP字体 { get; private set; }
        public TMP_FontAsset 当前TMP字体 { get; private set; }
        public Shader[] 所有查找到的着色器 { get; private set; }
        private Dictionary<Type, List<MonoBehaviour>> 所有查找到的组件 = new();
        private Dictionary<string, List<GameObject>> 所有查找到的GameObject = new();

        public 前置_资源加载器()
        {
            using (Stream 读 = Assembly.GetExecutingAssembly().GetManifestResourceStream("meanran_xuexi_mods.Resources.TextFont.assets"))
            {
                当前Font字体 = AssetBundle.LoadFromStream(读).LoadAsset<Font>("ZiTi");
                if (当前Font字体) { 前置模块.Log.LogMessage($"成功加载Font字体: {当前Font字体.name}"); }
                else { 前置模块.Log.LogError($"加载Font字体失败"); }
            }

            所有查找到的TMP字体 = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

            当前TMP字体 = 所有查找到的TMP字体.FirstOrDefault(obj => obj.name == "font_cjk");
            if (当前TMP字体) { 前置模块.Log.LogMessage($"成功加载TMP字体: {当前TMP字体.name}"); }
            else { 前置模块.Log.LogError($"加载TMP字体失败"); }

            所有查找到的着色器 = Resources.FindObjectsOfTypeAll<Shader>();
            前置模块.Log.LogMessage($"成功加载着色器数量: {所有查找到的着色器.Length}  可调用方法<{nameof(通用工具.打印着色器所有参数信息)}>打印某个着色器所有参数信息");

            var 逻辑组件表 = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            HashSet<MonoBehaviour> 逻辑组件表去重 = [.. 逻辑组件表];

            foreach (var v in 逻辑组件表去重)
            {
                Type type = v.GetType();
                if (!所有查找到的组件.ContainsKey(type))
                {
                    所有查找到的组件.Add(type, new List<MonoBehaviour>());
                }
                所有查找到的组件[type].Add(v);
            }

            逻辑组件表去重.Clear();

            var 预制体表 = Resources.FindObjectsOfTypeAll<GameObject>();
            HashSet<GameObject> 预制体表去重 = [.. 预制体表];

            foreach (var v2 in 预制体表去重)
            {
                string type2 = v2.name;
                if (!所有查找到的GameObject.ContainsKey(type2))
                {
                    所有查找到的GameObject.Add(type2, new List<GameObject>());
                }
                所有查找到的GameObject[type2].Add(v2);
            }

            预制体表去重.Clear();

            var __ = 通用工具.材质_安然_高亮全息投影_扫描线;
        }

        public bool TryGetAllComponent<组件类型>(out IEnumerable<MonoBehaviour> Result_查找结果) where 组件类型 : MonoBehaviour
        {
            if (所有查找到的组件.TryGetValue(typeof(组件类型), out var 查找结果))
            {
                Result_查找结果 = 查找结果;
                return true;
            }
            else
            {
                Result_查找结果 = null;
                return false;
            }
        }

        public bool TryGetAllGameObject(string Arg_name, out IEnumerable<GameObject> Result_查找结果)
        {
            if (所有查找到的GameObject.TryGetValue(Arg_name, out var 查找结果))
            {
                Result_查找结果 = 查找结果;
                return true;
            }
            else
            {
                Result_查找结果 = null;
                return false;
            }
        }
    }
}