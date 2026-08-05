using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;

[Tooltip("作为母版的.png文件请先在导入设置中将<读取/写入>勾选上, 将<压缩>切换为无")]
public class 在Unity编辑器中创建UV纹理数组和缩略图_生成_输入窗口 : EditorWindow
{
    private string 物体名称 = "_"; // 默认值
    private string 喷漆颜色 = "0,0,0,0.5";
    private const string 上次名称键 = "上次名称键_生成";
    private const string 上次喷漆颜色 = "上次喷漆颜色_生成";

    void OnEnable()
    {
        物体名称 = EditorPrefs.GetString(上次名称键, "_");           // 窗口打开时读取上次保存的名称，若没有则使用默认值
        喷漆颜色 = EditorPrefs.GetString(上次喷漆颜色, "0,0,0,0.5");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("请输入物体名称: ", EditorStyles.boldLabel);
        物体名称 = EditorGUILayout.TextField("物体名称", 物体名称);

        EditorGUILayout.LabelField("请输入喷漆颜色: ", EditorStyles.boldLabel);
        喷漆颜色 = EditorGUILayout.TextField("喷漆颜色", 喷漆颜色);

        if (GUILayout.Button("创建UV纹理数组和缩略图", GUILayout.Height(60)))
        {
            if (string.IsNullOrWhiteSpace(物体名称))
            {
                EditorUtility.DisplayDialog("错误", "物体名称不能为空！", "确定");
                return;
            }

            EditorPrefs.SetString(上次名称键, 物体名称);        // 保存当前名称到 EditorPrefs
            EditorPrefs.SetString(上次喷漆颜色, 喷漆颜色);

            static bool TryParseColor(string colorString, out UnityEngine.Color 结果)
            {
                结果 = UnityEngine.Color.clear;
                var parts = colorString.Split(',');
                if (parts.Length < 3 || parts.Length > 4) { return false; }

                float r, g, b, a = 1f;
                if (!float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out r)) { return false; }
                if (!float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out g)) { return false; }
                if (!float.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out b)) { return false; }
                if (parts.Length == 4 && !float.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out a)) { return false; }

                结果 = new UnityEngine.Color(r, g, b, a);
                return true;
            }

            TryParseColor(喷漆颜色, out var color);
            UnityEngine.Debug.Log(color);
            在Unity编辑器中创建UV纹理数组和缩略图_生成.创建UV纹理数组和缩略图(物体名称, color);
            Close();
        }

        if (GUILayout.Button("取消", GUILayout.Height(30)))
        {
            Close();
        }
    }
}

public class 在Unity编辑器中创建UV纹理数组和缩略图_生成 : EditorWindow
{
    [MenuItem("Tools/在Unity编辑器中创建UV纹理数组和缩略图_生成")]
    static void Init()
    {
        var window = GetWindow<在Unity编辑器中创建UV纹理数组和缩略图_生成_输入窗口>(true, "输入物体名称");
        window.Show();
    }

    [Tooltip("建议在Unity编辑器中提前创建好对应不同喷漆颜色的UV纹理数组和缩略图, 然后打包成AssetBundle, 因为Unity引擎在专业服务器版本中, 加载AssetBundle功能是开启的(因为只是反序列化, 没有图形计算), 但是创建Texture2DArray和Sprite是禁用的")]
    public static void 创建UV纹理数组和缩略图(string 物体名称, UnityEngine.Color 查找该颜色并替换成喷漆颜色)
    {
        Texture2D 母版 = null;

        var Unity项目Assets目录 = Application.dataPath;
        var 所有纹理文件路径 = Directory.GetFiles(Unity项目Assets目录).Where(d => d.EndsWith(".png") && d.Contains(物体名称));

        foreach (var 当前 in 所有纹理文件路径)
        {
            if (!当前.EndsWith(".png")) { continue; }
            var 路径 = FileUtil.GetProjectRelativePath(当前);
            UnityEngine.Debug.Log(路径);

            int 索引 = Array.FindIndex(游戏内置喷漆颜色_生成.所有喷漆颜色, startIndex: 0, count: 游戏内置喷漆颜色_生成.所有喷漆颜色.Length, d => 路径.Contains($"{d}"));
            if (索引 >= 0)
            {
                var 当前纹理 = AssetDatabase.LoadAssetAtPath<Texture2D>(路径);
                母版 = 当前纹理;
                break;
            }
        }

        if (母版 == null)
        {
            UnityEngine.Debug.Log($"创建UV纹理数组和缩略图时, 需要提供一张纹理图片作为母版, 在本函数中会复制母版并创建新的图片, 然后读取所有喷漆像素替换成对应的喷漆颜色");
            return;
        }

        var 多分辨率预存计数 = 母版.mipmapCount;
        var 是否存在多分辨率预存 = 多分辨率预存计数 > 1;

        // bool 禁用 = false;
        // 是否存在多分辨率预存 = 禁用;

        var 纹理区域信息 = new Rect(0, 0, 母版.width, 母版.height);
        var 纹理的轴心点 = new Vector2(0.5f, 0.5f);       // 归一化值, 纹理的中心点 == 纹理的轴心点

        var UV纹理数组 = new Texture2DArray(母版.width, 母版.height, 游戏内置喷漆颜色_生成.所有喷漆颜色.Count(), 母版.format, 是否存在多分辨率预存);
        var 缩略图数组 = new Sprite[游戏内置喷漆颜色_生成.所有喷漆颜色.Count()];
        var 所有喷漆颜色纹理 = 游戏内置喷漆颜色_生成.创建所有喷漆纹理图片(母版, 是否存在多分辨率预存, 查找该颜色并替换成喷漆颜色);

        foreach (var 当前颜色 in 游戏内置喷漆颜色_生成.所有喷漆颜色)
        {
            var 当前颜色depth = (int)当前颜色;
            var 当前纹理 = 所有喷漆颜色纹理[当前颜色depth];

            if (是否存在多分辨率预存)
            {
                for (var i = 0; i < 多分辨率预存计数; ++i)
                {
                    UnityEngine.Graphics.CopyTexture(src: 当前纹理, srcElement: 0, srcMip: i, dst: UV纹理数组, dstElement: 当前颜色depth, dstMip: i);
                }
            }
            else
            {
                UnityEngine.Graphics.CopyTexture(src: 当前纹理, srcElement: 0, dst: UV纹理数组, dstElement: 当前颜色depth);
            }

            缩略图数组[当前颜色depth] = Sprite.Create(当前纹理, 纹理区域信息, 纹理的轴心点, pixelsPerUnit: 100);
        }

        var 输出根目录 = Path.Combine(Unity项目Assets目录, 物体名称);
        if (!Directory.Exists(输出根目录)) { Directory.CreateDirectory(输出根目录); }

        var 输出UV纹理数组路径 = Path.Combine(输出根目录, $"UV纹理数组.asset");
        输出UV纹理数组路径 = FileUtil.GetProjectRelativePath(输出UV纹理数组路径);
        AssetDatabase.CreateAsset(UV纹理数组, 输出UV纹理数组路径);

        var 输出纹理目录 = Path.Combine(输出根目录, $"所有喷漆颜色纹理");
        if (!Directory.Exists(输出纹理目录)) { Directory.CreateDirectory(输出纹理目录); }

        for (var i = 0; i < 所有喷漆颜色纹理.Length; ++i)
        {
            // var 输出纹理路径 = Path.Combine(输出纹理目录, $"{游戏内置喷漆颜色.所有喷漆颜色[i]}纹理.asset");
            // var 路径 = FileUtil.GetProjectRelativePath(输出纹理路径);
            // AssetDatabase.CreateAsset(所有喷漆颜色纹理[i], 路径);
            var 输出纹理路径 = Path.Combine(输出纹理目录, $"{游戏内置喷漆颜色_生成.所有喷漆颜色[i]}纹理.png");
            File.WriteAllBytes(输出纹理路径, 所有喷漆颜色纹理[i].EncodeToPNG());
        }

        var 输出缩略图目录 = Path.Combine(输出根目录, $"所有喷漆颜色缩略图");
        if (!Directory.Exists(输出缩略图目录)) { Directory.CreateDirectory(输出缩略图目录); }

        for (var i = 0; i < 缩略图数组.Length; ++i)
        {
            var 输出缩略图路径 = Path.Combine(输出缩略图目录, $"{游戏内置喷漆颜色_生成.所有喷漆颜色[i]}缩略图.asset");
            var 路径 = FileUtil.GetProjectRelativePath(输出缩略图路径);
            AssetDatabase.CreateAsset(缩略图数组[i], 路径);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public class 游戏内置喷漆颜色_生成
    {
        public enum 色板
        {
            蓝色 = 0, 灰色, 绿色, 橙色, 红色, 黄色, 白色, 黑色, 棕色, 卡其色, 粉色, 紫色, 黑曜石色, 银色, 青铜色, 金色,
        }

        public static readonly Dictionary<色板, UnityEngine.Color> 所有喷漆颜色RGBA值 = new()
        {
            [色板.蓝色] = new UnityEngine.Color(0.129f, 0.165f, 0.647f, 1.000f),
            [色板.灰色] = new UnityEngine.Color(0.482f, 0.482f, 0.482f, 1.000f),
            [色板.绿色] = new UnityEngine.Color(0.247f, 0.608f, 0.224f, 1.000f),
            [色板.橙色] = new UnityEngine.Color(1.000f, 0.400f, 0.169f, 1.000f),
            [色板.红色] = new UnityEngine.Color(0.906f, 0.008f, 0.000f, 1.000f),
            [色板.黄色] = new UnityEngine.Color(1.000f, 0.737f, 0.106f, 1.000f),
            [色板.白色] = new UnityEngine.Color(0.906f, 0.906f, 0.906f, 1.000f),
            [色板.黑色] = new UnityEngine.Color(0.031f, 0.035f, 0.031f, 1.000f),
            [色板.棕色] = new UnityEngine.Color(0.388f, 0.235f, 0.169f, 1.000f),
            [色板.卡其色] = new UnityEngine.Color(0.388f, 0.388f, 0.247f, 1.000f),
            [色板.粉色] = new UnityEngine.Color(0.894f, 0.110f, 0.600f, 1.000f),
            [色板.紫色] = new UnityEngine.Color(0.451f, 0.173f, 0.655f, 1.000f),
            [色板.黑曜石色] = new UnityEngine.Color(0.400f, 0.420f, 0.460f, 1.000f),
            [色板.银色] = new UnityEngine.Color(0.860f, 0.870f, 0.900f, 1.000f),
            [色板.青铜色] = new UnityEngine.Color(0.720f, 0.450f, 0.250f, 1.000f),
            [色板.金色] = new UnityEngine.Color(0.830f, 0.690f, 0.320f, 1.000f)
        };
        public static readonly 色板[] 所有喷漆颜色 = Enum.GetValues(typeof(色板)).Cast<色板>().ToArray();

        public static Texture2D[] 创建所有喷漆纹理图片(Texture2D 源, bool 是否存在多分辨率预存, UnityEngine.Color 母版喷漆颜色)
        {
            var 所有喷漆颜色纹理 = new Texture2D[所有喷漆颜色.Count()];

            for (var i = 0; i < 所有喷漆颜色.Count(); i++)
            {
                var 新 = 创建喷漆纹理图片(源, 是否存在多分辨率预存, 查找颜色: 母版喷漆颜色, 替换颜色: 所有喷漆颜色RGBA值[所有喷漆颜色[i]]);
                所有喷漆颜色纹理[i] = 新;
            }

            return 所有喷漆颜色纹理;
        }

        public static Texture2D 创建喷漆纹理图片(Texture2D 源, bool 是否存在多分辨率预存, UnityEngine.Color 查找颜色, UnityEngine.Color 替换颜色)
        {
            UnityEngine.Debug.Log(源.format);

            var 源像素表 = 源.GetPixels();
            var 新 = new Texture2D(源.width, 源.height, 源.format, 是否存在多分辨率预存);

            for (var i = 0; i < 源像素表.Length; i++)
            {
                if (近似颜色比较(查找颜色, 源像素表[i]))
                {
                    // UnityEngine.Debug.Log($"查找颜色: {查找颜色}  替换颜色: {源像素表[i]}");
                    源像素表[i] = 替换颜色;
                }
            }

            新.SetPixels(源像素表);
            新.Apply();

            return 新;
        }

        public static bool 近似颜色比较(UnityEngine.Color a, UnityEngine.Color b, float 色差 = 0.1f)
        {
            return Mathf.Abs(a.r - b.r) < 色差 && Mathf.Abs(a.g - b.g) < 色差 && Mathf.Abs(a.b - b.b) < 色差 && Mathf.Abs(a.a - b.a) < 色差;
        }
    }
}