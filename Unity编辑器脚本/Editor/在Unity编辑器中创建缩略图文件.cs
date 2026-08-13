using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

public class 在Unity编辑器中创建缩略图文件_输入窗口 : EditorWindow
{
    private string 物体名称 = "_"; // 默认值
    private const string 上次名称键 = "上次名称键_在Unity编辑器中创建缩略图文件";

    void OnEnable()
    {
        // 窗口切换到打开状态事件
        物体名称 = EditorPrefs.GetString(上次名称键, "_");           // 窗口打开时读取上次保存的名称，若没有则使用默认值
    }

    void OnGUI()
    {
        // 窗口每帧调用
        EditorGUILayout.LabelField("请输入物体名称: ", EditorStyles.boldLabel);
        物体名称 = EditorGUILayout.TextField("物体名称", 物体名称);

        if (GUILayout.Button("创建缩略图", GUILayout.Height(30)))
        {
            if (string.IsNullOrWhiteSpace(物体名称))
            {
                EditorUtility.DisplayDialog("错误", "物体名称不能为空！", "确定");
                return;
            }

            EditorPrefs.SetString(上次名称键, 物体名称);        // 保存当前名称到 EditorPrefs

            在Unity编辑器中创建缩略图文件.创建缩略图(物体名称);
            Close();
        }

        if (GUILayout.Button("取消", GUILayout.Height(30)))
        {
            Close();
        }
    }
}

public class 在Unity编辑器中创建缩略图文件 : EditorWindow
{
    [MenuItem("Tools/在Unity编辑器中创建缩略图文件")]
    static void 按钮点击事件()
    {
        var window = GetWindow<在Unity编辑器中创建缩略图文件_输入窗口>(true, "输入物体名称");
        window.Show();
    }

    [Tooltip("建议在Unity编辑器中提前创建好对应不同喷漆颜色的缩略图, 然后打包成AssetBundle, 因为Unity引擎在专业服务器版本中, 加载AssetBundle功能是开启的(因为只是反序列化, 没有图形计算), 但是创建Sprite和Texture2DArray等等是禁用的")]
    public static void 创建缩略图(string 物体名称)
    {
        var 所有纹理 = new Dictionary<游戏内置喷漆颜色_只读.色板, Texture2D>();

        var Unity项目Assets目录 = Application.dataPath;

        var 输出根目录 = Path.Combine(Unity项目Assets目录, 物体名称);
        if (!Directory.Exists(输出根目录)) { Directory.CreateDirectory(输出根目录); }

        var 输出纹理目录 = Path.Combine(输出根目录, "所有喷漆颜色的缩略图_纹理");
        if (!Directory.Exists(输出纹理目录)) { Directory.CreateDirectory(输出纹理目录); }

        var 所有纹理文件路径 = Directory.GetFiles(输出纹理目录);

        foreach (var 当前 in 所有纹理文件路径)
        {
            if (!当前.EndsWith(".png")) { continue; }
            var 路径 = FileUtil.GetProjectRelativePath(当前);

            int 索引 = Array.FindIndex(游戏内置喷漆颜色_只读.所有喷漆颜色, startIndex: 0, count: 游戏内置喷漆颜色_只读.所有喷漆颜色.Length,
            d =>
            {
                var 当前枚举 = d.ToString();
                var 匹配么 = 路径.Contains(当前枚举);
                if (匹配么) { Debug.Log($"当前路径: {路径}  当前枚举: {当前枚举}  匹配么: {匹配么}"); }
                return 匹配么;
            });

            if (索引 >= 0)
            {
                var 当前纹理 = AssetDatabase.LoadAssetAtPath<Texture2D>(路径);
                所有纹理[游戏内置喷漆颜色_只读.所有喷漆颜色[索引]] = 当前纹理;
            }
        }

        (var 颜色, var 纹理) = 所有纹理.First();
        if (游戏内置喷漆颜色_只读.所有喷漆颜色.Any(d => d == 颜色) && 纹理 != null)
        {
            var 多分辨率预存计数 = 纹理.mipmapCount;
            var 是否存在多分辨率预存 = 多分辨率预存计数 > 1;

            var 缩略图数组 = new Sprite[游戏内置喷漆颜色_只读.所有喷漆颜色.Count()];

            var 纹理区域信息 = new Rect(0, 0, 纹理.width, 纹理.height);
            var 纹理的轴心点 = new Vector2(0.5f, 0.5f);       // 归一化值, 纹理的中心点 == 纹理的轴心点

            foreach (var 当前 in 游戏内置喷漆颜色_只读.所有喷漆颜色)
            {
                var 当前depth = (int)当前;
                if (所有纹理.TryGetValue(当前, out var 当前纹理))
                {
                    缩略图数组[当前depth] = Sprite.Create(当前纹理, 纹理区域信息, 纹理的轴心点, pixelsPerUnit: 100);
                }
                else
                {
                    Debug.Log($"创建对应不同喷漆颜色的缩略图时, 缺少{当前}");
                    缩略图数组[当前depth] = 缩略图数组[0];
                }
            }

            var 输出缩略图目录 = Path.Combine(输出根目录, $"所有喷漆颜色缩略图");
            if (!Directory.Exists(输出缩略图目录)) { Directory.CreateDirectory(输出缩略图目录); }

            for (var i = 0; i < 缩略图数组.Length; ++i)
            {
                var 输出缩略图路径 = Path.Combine(输出缩略图目录, $"{游戏内置喷漆颜色_只读.所有喷漆颜色[i]}缩略图.asset");
                var 路径 = FileUtil.GetProjectRelativePath(输出缩略图路径);
                AssetDatabase.CreateAsset(缩略图数组[i], 路径);
            }
        }
        else
        {
            Debug.Log($"创建对应不同喷漆颜色的缩略图时, 创建失败, 因为没有任何颜色的纹理可用");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public class 游戏内置喷漆颜色_只读
    {
        public enum 色板
        {
            蓝色 = 0, 灰色, 绿色, 橙色, 红色, 黄色, 白色, 黑色, 棕色, 卡其色, 粉色, 紫色, 黑曜石色, 银色, 青铜色, 金色,
        }
        public static readonly 色板[] 所有喷漆颜色 = Enum.GetValues(typeof(色板)).Cast<色板>().ToArray();
    }
}