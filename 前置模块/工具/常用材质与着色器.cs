using UnityEngine;
using Assets.Scripts.UI;
using TerrainSystem;
using Assets.Scripts;
using System.Reflection;
using System.IO;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public static readonly int 着色器参数_扫描线开关 = Shader.PropertyToID("_ScanEnabled");
        public static readonly int 着色器参数_扫描线速度 = Shader.PropertyToID("_ScanSpeed");
        public static readonly int 着色器参数_扫描线旋转 = Shader.PropertyToID("_Direction");

        private static Material m_材质_高亮全息投影_扫描线 = null;
        public static Material 材质_高亮全息投影_扫描线
        {
            get
            {
                if (m_材质_高亮全息投影_扫描线 == null) { m_材质_高亮全息投影_扫描线 = 创建材质_高亮全息投影_扫描线(); }
                return m_材质_高亮全息投影_扫描线;
            }
        }

        private static Material 创建材质_高亮全息投影_扫描线()
        {
            var 着色器 = Shader.Find("Custom/Hologram");
            打印着色器所有参数信息(着色器);

            var 材质 = new Material(着色器)
            {
                globalIlluminationFlags = MaterialGlobalIlluminationFlags.None,
                enableInstancing = true,
                renderQueue = 5000,
                shaderKeywords = ["_EMISSION"],
            };

            材质.SetColor(着色器参数_Color, new Color(0, 0.6f, 1, 0.6f));
            材质.SetFloat(着色器参数_扫描线开关, 1);                    // 在着色器实例的 Render方法中, 是否跳过扫描线语句 
            材质.SetFloat(着色器参数_扫描线速度, 0.4f);             // 扫描线速度: 此着色器有一个定时器变量, 每过_ScanSpeed秒, 扫描线当前位置++     // 扫描线有一个当前位置参数, 从当前位置开始绘制等于线宽的扫描区域, 像素位置 >= 扫描线当前位置 && 像素位置 <= (扫描线当前位置 + 扫描线宽度) 时, 用扫描线像素代替网格表面像素
            const float 弧度 = 45f * Mathf.Deg2Rad;
            材质.SetVector(着色器参数_扫描线旋转, new Vector4(Mathf.Cos(弧度), Mathf.Sin(弧度), 0, 0));     // 二维旋转矩阵公式

            return 材质;
        }

        public static void 打印着色器所有参数信息(Shader Arg_着色器)
        {
            if (Arg_着色器 == null) { 前置模块.Log.LogError("传入的着色器为空, 无法打印着色器所有参数信息"); return; }

            前置模块.Log.LogMessage($"着色器名称: {Arg_着色器.name}");

            var propertyCount = Arg_着色器.GetPropertyCount();
            for (var i = 0; i < propertyCount; i++)
            {
                var propName = Arg_着色器.GetPropertyName(i);
                var propNameId = Arg_着色器.GetPropertyNameId(i);
                var propType = Arg_着色器.GetPropertyType(i);

                前置模块.Log.LogMessage($"着色器声明的参数 [名称: {propName}, ID: {Shader.PropertyToID(propName)} {propNameId}, 数据类型: {propType}]");
            }
        }

        public static readonly int 着色器参数_Color = Shader.PropertyToID("_Color");

        private static Material m_材质_高亮矿物 = null;
        public static Material 材质_高亮矿物
        {
            get
            {
                if (m_材质_高亮矿物 == null) { m_材质_高亮矿物 = 创建材质_高亮矿物(); }
                return m_材质_高亮矿物;
            }
        }
        public static Material 创建材质_高亮矿物()
        {
            var 着色器 = VoxelTerrain.Instance.oreVisualiserMaterial.shader;
            打印着色器所有参数信息(着色器);

            var New = UnityEngine.Object.Instantiate(VoxelTerrain.Instance.oreVisualiserMaterial);
            New.SetColor(着色器参数_Color, new Color(0, 0.6f, 1, 0.6f));
            New.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            New.enableInstancing = true;
            New.renderQueue = 5000;
            return New;
        }

        public static readonly int 着色器参数_不透明度 = Shader.PropertyToID("_Opacity");
        public static readonly int 着色器参数_消失距离 = Shader.PropertyToID("_FadeDistance");

        private static Material m_材质_高亮全息投影 = null;
        public static Material 材质_高亮全息投影
        {
            get
            {
                if (m_材质_高亮全息投影 == null) { m_材质_高亮全息投影 = 创建材质_高亮全息投影(); }
                return m_材质_高亮全息投影;
            }
        }

        private static Material 创建材质_高亮全息投影()
        {
            var 着色器 = Shader.Find("Unlit/MesonScanner");
            打印着色器所有参数信息(着色器);

            var 材质 = new Material(着色器)
            {
                globalIlluminationFlags = MaterialGlobalIlluminationFlags.None,
                renderQueue = 5000,
                shaderKeywords = ["_ALPHAPREMULTIPLY_ON", "_EMISSION", "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF"],
            };

            材质.SetFloat(着色器参数_不透明度, 0.6f);
            材质.SetFloat(着色器参数_消失距离, 20f);
            材质.SetColor(着色器参数_Color, new Color(0f, 1f, 1f, 1f));

            return 材质;
        }

        private static Material m_材质_安然_高亮全息投影_扫描线 = null;
        public static Material 材质_安然_高亮全息投影_扫描线
        {
            get
            {
                if (m_材质_安然_高亮全息投影_扫描线 == null) { m_材质_安然_高亮全息投影_扫描线 = 创建材质_安然_高亮全息投影_扫描线(); }
                return m_材质_安然_高亮全息投影_扫描线;
            }
        }
        public static Material 创建材质_安然_高亮全息投影_扫描线()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录

            var 资源视图 = AssetBundle.LoadFromFile(Path.Combine(dllDirectory, "着色器/custom_anrandebudingbao_hologram_AssetBundle"));
            打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(资源视图);
            var NewShader = 资源视图.LoadAsset<Shader>("assets/custom_anrandebudingbao_hologram.shader");
            var New = 资源视图.LoadAsset<Material>("assets/custom_anrandebudingbao_hologram.mat");

            打印着色器所有参数信息(NewShader);

            New.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            New.enableInstancing = true;
            New.renderQueue = 5000;

            return New;
        }
    }
}