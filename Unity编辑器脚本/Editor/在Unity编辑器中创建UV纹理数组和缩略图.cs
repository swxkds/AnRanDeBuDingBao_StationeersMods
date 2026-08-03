using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

public class 在Unity编辑器中创建UV纹理数组和缩略图_输入窗口 : EditorWindow
{
    private string 物体名称 = "_"; // 默认值
    private const string 上次名称键 = "LastObjectName";

    void OnEnable()
    {
        物体名称 = EditorPrefs.GetString(上次名称键, "_");           // 窗口打开时读取上次保存的名称，若没有则使用默认值
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("请输入物体名称: ", EditorStyles.boldLabel);
        物体名称 = EditorGUILayout.TextField("物体名称", 物体名称);

        if (GUILayout.Button("创建UV纹理数组和缩略图", GUILayout.Height(60)))
        {
            if (string.IsNullOrWhiteSpace(物体名称))
            {
                EditorUtility.DisplayDialog("错误", "物体名称不能为空！", "确定");
                return;
            }

            EditorPrefs.SetString(上次名称键, 物体名称);        // 保存当前名称到 EditorPrefs

            在Unity编辑器中创建UV纹理数组和缩略图.创建UV纹理数组和缩略图(物体名称);
            Close();
        }

        if (GUILayout.Button("取消", GUILayout.Height(30)))
        {
            Close();
        }
    }
}

public class 在Unity编辑器中创建UV纹理数组和缩略图 : EditorWindow
{
    [MenuItem("Tools/在Unity编辑器中创建UV纹理数组和缩略图")]
    static void Init()
    {
        var window = GetWindow<在Unity编辑器中创建UV纹理数组和缩略图_输入窗口>(true, "输入物体名称");
        window.Show();
    }

    [Tooltip("建议在Unity编辑器中提前创建好对应不同喷漆颜色的UV纹理数组和缩略图, 然后打包成AssetBundle, 因为Unity引擎在专业服务器版本中, 加载AssetBundle功能是开启的(因为只是反序列化, 没有图形计算), 但是创建Texture2DArray和Sprite是禁用的")]
    public static void 创建UV纹理数组和缩略图(string 物体名称)
    {
        var 所有纹理 = new Dictionary<游戏内置喷漆色板12种颜色, Texture2D>();

        var Unity项目Assets目录 = Application.dataPath;
        var 纹理目录 = Path.Combine(Unity项目Assets目录, "对应不同喷漆颜色的纹理");
        var 所有纹理文件路径 = Directory.GetFiles(纹理目录);

        foreach (var 当前 in 所有纹理文件路径)
        {
            var 路径 = FileUtil.GetProjectRelativePath(当前);
            if (!路径.EndsWith(".png")) { continue; }
            Debug.Log(路径);

            int 索引 = Array.FindIndex(所有喷漆色板颜色, startIndex: 0, count: 所有喷漆色板颜色.Length, d => 路径.Contains($"{d}"));
            if (索引 >= 0)
            {
                var 当前纹理 = AssetDatabase.LoadAssetAtPath<Texture2D>(路径);
                所有纹理[所有喷漆色板颜色[索引]] = 当前纹理;
            }
        }

        (var 颜色, var 纹理) = 所有纹理.First();
        if (所有喷漆色板颜色.Any(d => d == 颜色) && 纹理 != null)
        {
            var 多分辨率预存计数 = 纹理.mipmapCount;
            var 是否存在多分辨率预存 = 多分辨率预存计数 > 1;

            var UV纹理数组 = new Texture2DArray(纹理.width, 纹理.height, 所有喷漆色板颜色.Count(), 纹理.format, 是否存在多分辨率预存);
            var 缩略图数组 = new Sprite[所有喷漆色板颜色.Count()];

            var 纹理区域信息 = new Rect(0, 0, 纹理.width, 纹理.height);
            var 纹理的轴心点 = new Vector2(0.5f, 0.5f);       // 归一化值, 纹理的中心点 == 纹理的轴心点

            foreach (var 当前 in 所有喷漆色板颜色)
            {
                var 当前depth = (int)当前;
                if (所有纹理.TryGetValue(当前, out var 当前纹理))
                {
                    if (是否存在多分辨率预存)
                    {
                        for (var i = 0; i < 多分辨率预存计数; ++i)
                        {
                            Graphics.CopyTexture(src: 当前纹理, srcElement: 0, srcMip: i, dst: UV纹理数组, dstElement: 当前depth, dstMip: i);
                        }
                    }
                    else
                    {
                        Graphics.CopyTexture(src: 当前纹理, srcElement: 0, dst: UV纹理数组, dstElement: 当前depth);
                    }

                    缩略图数组[当前depth] = Sprite.Create(当前纹理, 纹理区域信息, 纹理的轴心点, pixelsPerUnit: 100);
                }
                else
                {
                    Debug.Log($"创建对应不同喷漆颜色的UV纹理数组和缩略图数组时, 缺少{当前}");

                    if (是否存在多分辨率预存)
                    {
                        for (var i = 0; i < 多分辨率预存计数; ++i)
                        {
                            Graphics.CopyTexture(src: UV纹理数组, srcElement: 0, srcMip: i, dst: UV纹理数组, dstElement: 当前depth, dstMip: i);
                        }
                    }
                    else
                    {
                        Graphics.CopyTexture(src: UV纹理数组, srcElement: 0, dst: UV纹理数组, dstElement: 当前depth);
                    }

                    缩略图数组[当前depth] = 缩略图数组[0];
                }
            }

            AssetDatabase.CreateAsset(UV纹理数组, $"Assets/{物体名称}_UV纹理数组.asset");

            var 缩略图目录 = Path.Combine(Unity项目Assets目录, $"{物体名称}的所有缩略图");
            if (!Directory.Exists(缩略图目录)) { Directory.CreateDirectory(缩略图目录); }

            for (var i = 0; i < 缩略图数组.Length; ++i)
            {
                AssetDatabase.CreateAsset(缩略图数组[i], FileUtil.GetProjectRelativePath(Path.Combine(缩略图目录, $"{物体名称}_缩略图_{所有喷漆色板颜色[i]}.asset")));
            }
        }
        else
        {
            Debug.Log($"创建对应不同喷漆颜色的UV纹理数组和缩略图时, 创建失败, 传入的原始纹理字典存在错误");
        }
    }

    public enum 游戏内置喷漆色板12种颜色
    {
        蓝色Blue = 0,
        灰色Gray, 绿色Green, 橙色Orange, 红色Red, 黄色Yellow, 白色White, 黑色Black, 棕色Brown, 土黄色Khaki, 粉色Pink, 紫色Purple,
    }
    public static readonly 游戏内置喷漆色板12种颜色[] 所有喷漆色板颜色 = Enum.GetValues(typeof(游戏内置喷漆色板12种颜色)).Cast<游戏内置喷漆色板12种颜色>().ToArray();
}