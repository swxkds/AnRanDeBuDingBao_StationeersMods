using UnityEngine;
using Assets.Scripts.UI;
using TerrainSystem;
using Assets.Scripts;
using System.Reflection;
using System.IO;
using Assets.Scripts.Util;
using System;
using System.Linq;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {

        [Tooltip("使用<AssetRipper.GUI.Free.exe>打开游戏根目录的文件<rocketstation.exe>, AssetRipper工具会解包整个游戏程序, 然后选择导出目录并导出资源. 导出结果中可以看到<Assets/Texture2DArray/ColorPaletteArray.png>这张图片, 这就是游戏内置的喷漆系统材质(Singleton<GameManager>.Instance.TextureArrayColorMaterial)使用的UV纹理, 我们在建模时, 需要支持喷漆的子网格都从这个图片上采样\n一个Mesh有多少个子网格, Renderer.sharedMaterials这个材质数组就需要多少个材质, 两者按照索引一一对应(例: 子网格0就使用材质数组中第1个材质; 子网格1就使用材质数组中第2个材质")]
        public class 游戏内置喷漆颜色
        {
            public static Material 游戏内置喷漆材质 => Singleton<GameManager>.Instance.TextureArrayColorMaterial;
            public static Texture2DArray 游戏内置喷漆材质使用的UV纹理_注_不支持喷漆的子网格_也可以从这个UV纹理上采样 => (Texture2DArray)游戏内置喷漆材质.mainTexture;

            public enum 色板
            {
                蓝色 = 0, 灰色, 绿色, 橙色, 红色, 黄色, 白色, 黑色, 棕色, 卡其色, 粉色, 紫色, 黑曜石色, 银色, 青铜色, 金色,
            }
            public static readonly 色板[] 所有喷漆颜色 = Enum.GetValues(typeof(色板)).Cast<色板>().ToArray();
            public static void 打印游戏内置喷漆颜色() { 前置模块.Log.LogMessage(string.Join("\n", Singleton<GameManager>.Instance.CustomColors.Select(d => d.DisplayName + d.Color.ToString()))); }
        }

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
            打印已加载资源路径_资源路径传参给LoadAsset方法会返回该资源的引用(资源视图);
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