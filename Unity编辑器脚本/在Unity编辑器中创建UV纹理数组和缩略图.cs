using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

public class 在Unity编辑器中创建UV纹理数组和缩略图 : MonoBehaviour
{
    public enum 游戏内置喷漆色板12种颜色
    {
        蓝色Blue = 0,
        灰色Gray, 绿色Green, 橙色Orange, 红色Red, 黄色Yellow, 白色White, 黑色Black, 棕色Brown, 土黄色Khaki, 粉色Pink, 紫色Purple,
    }
    public static readonly 游戏内置喷漆色板12种颜色[] 所有喷漆色板颜色 = Enum.GetValues(typeof(游戏内置喷漆色板12种颜色)).Cast<游戏内置喷漆色板12种颜色>().ToArray();

    public readonly List<游戏内置喷漆色板12种颜色> 所有颜色 = new(所有喷漆色板颜色);
    public readonly List<Texture2D> 所有纹理 = new();
    public string 物体名称 = "矿物扫描眼镜";

    public void Start() => 创建UV纹理数组和缩略图();

    [Tooltip("建议在Unity编辑器中提前创建好对应不同喷漆颜色的UV纹理数组和缩略图, 然后打包成AssetBundle, 因为Unity引擎在专业服务器版本中, 加载AssetBundle功能是开启的(因为只是反序列化, 没有图形计算), 但是创建Texture2DArray和Sprite是禁用的")]
    public void 创建UV纹理数组和缩略图()
    {
        var Unity项目Assets目录 = Application.dataPath;

        var 缩略图目录 = Path.Combine(Unity项目Assets目录, $"{物体名称}的所有缩略图");
        if (!Directory.Exists(缩略图目录)) { Directory.CreateDirectory(缩略图目录); }

        var 纹理目录 = Path.Combine(Unity项目Assets目录, "对应不同喷漆颜色的纹理");
        var 所有纹理文件路径 = Directory.GetFiles(纹理目录);

        foreach (var __ in 所有颜色)
        {
            所有纹理.Add(null);
        }

        foreach (var 当前完整路径 in 所有纹理文件路径)
        {
            var 当前路径 = FileUtil.GetProjectRelativePath(当前完整路径);
            if (!当前路径.EndsWith(".png")) { continue; }
            Debug.Log(当前路径);

            int 匹配 = 所有颜色.FindIndex(d => 当前路径.Contains($"{d}"));
            if (匹配 >= 0)
            {
                var 当前纹理 = AssetDatabase.LoadAssetAtPath<Texture2D>(当前路径);
                所有纹理[匹配] = 当前纹理;
            }
        }

        var 对应不同喷漆颜色的纹理 = new Dictionary<游戏内置喷漆色板12种颜色, Texture2D>();
        for (var i = 0; i < 所有纹理.Count; ++i) { if (所有纹理[i] != null) { 对应不同喷漆颜色的纹理[所有颜色[i]] = 所有纹理[i]; } }

        (var 颜色, var 纹理) = 对应不同喷漆颜色的纹理.First();
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
                if (对应不同喷漆颜色的纹理.TryGetValue(当前, out var 当前纹理))
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

                    缩略图数组[当前depth] = Sprite.Create(当前纹理, 纹理区域信息, 纹理的轴心点, pixelsPerUnit: 50);
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

}
