using UnityEngine;
using Assets.Scripts.Objects;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public class 灯光配置
        {
            public static readonly float 野外光亮度系数 = 1.1f;
            public static readonly float 野外光范围系数 = 40f;
            public static readonly float 室内光亮度系数 = 0.8f;
            public static readonly float 室内光范围系数 = 2f;
            public LightType type;
            public Color color;
            public float intensity;
            public float range;
            public float spotAngle;
            public float bounceIntensity;
            public Flare flare;
            public Texture cookie;
            public float cookieSize;
            public float shadowStrength;
            public 灯光配置(Light 源)
            {
                type = 源.type;                   // 光源类型（点光、聚光、平行光等）
                color = 源.color;                 // 光源颜色
                intensity = 源.intensity;         // 光照强度（亮度）
                range = 源.range;                   // 光照距离范围
                spotAngle = 源.spotAngle;         // 聚光灯的张角（仅对 Spot 类型有效）
                bounceIntensity = 源.bounceIntensity; // 间接光反射强度（对场景光反弹效果）
                flare = 源.flare;                 // 光晕（镜头光晕效果，通常用于太阳灯等）
                cookie = 源.cookie;               // 光照 Cookie 纹理（投影花纹用）
                cookieSize = 源.cookieSize;       //  Cookie 投影尺寸（控制花纹缩放）
                shadowStrength = 源.shadowStrength; // 阴影强度
            }


            public void 应用灯光配置(Light 目标, float 亮度系数 = 1, float 范围系数 = 1)
            {
                if (目标 != null)
                {
                    目标.type = type;                        // 光源类型（点光、聚光、平行光等）
                    目标.color = color;                      // 光源颜色
                    目标.intensity = intensity * 亮度系数;   // 光照强度（亮度）0.8f; 
                    目标.range = range * 范围系数;           // 光照距离范围 5f;
                    目标.spotAngle = spotAngle;              // 聚光灯的张角（仅对 Spot 类型有效）
                    目标.bounceIntensity = bounceIntensity; // 间接光反射强度（对场景光反弹效果）
                    目标.flare = flare;                      // 光晕（镜头光晕效果，通常用于太阳灯等）
                    目标.cookie = cookie;                    // 光照 Cookie 纹理（投影花纹用）
                    目标.cookieSize = cookieSize;            //  Cookie 投影尺寸（控制花纹缩放）
                    目标.shadowStrength = shadowStrength; // 阴影强度

                    前置模块.Log.LogMessage($"成功修改<{目标.transform.GetComponentInParent<Thing>(includeInactive: true)?.PrefabName}>的灯光组件");
                }
            }
        }
    }
}