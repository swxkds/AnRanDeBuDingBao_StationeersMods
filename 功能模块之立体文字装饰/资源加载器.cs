using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Scripts;
using Assets.Scripts.Util;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 功能模块之立体文字装饰_资源加载器
    {
        private static 功能模块之立体文字装饰_资源加载器 m_单例 = null;
        public static 功能模块之立体文字装饰_资源加载器 单例 { get { if (m_单例 == null) { m_单例 = new(); } return m_单例; } }

        private GameObject 休眠的根节点 = null;

        public (Mesh 已合并Mesh, Material[] 所有subMesh材质) 套件多边形网格与材质 { get; private set; }
        public (GameObject 实体, GameObject 蓝图) 套件预制体 { get; private set; }
        public (Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 对应不同喷漆颜色的缩略图) 套件纹理 { get; private set; }     // 制作UV纹理时, 只从原始纹理中采样, 这样原始纹理可以让缩略图复用

        public Dictionary<string, (Mesh 已合并Mesh, Material[] 所有subMesh材质)> 所有可装配多边形网格与材质 { get; private set; }
        public List<(GameObject 实体, GameObject 蓝图)> 所有可装配预制体 { get; private set; }
        public Dictionary<string, (Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 对应不同喷漆颜色的缩略图)> 所有可装配纹理 { get; private set; }     // 制作UV纹理时, 只从原始纹理中采样, 这样原始纹理可以让缩略图复用

        public 功能模块之立体文字装饰_资源加载器()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录

            var 所有不支持喷漆的网格_也从这个UV纹理上采样 = Singleton<GameManager>.Instance.TextureArrayColorMaterial.mainTexture;

            {
                {
                    const string 物体名称 = "(套件)立体文字装饰";
                    (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(dllDirectory, 物体名称);

                    套件纹理 = (所有不支持喷漆的网格_UV纹理, 缩略图数组);

                    var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                    var 外壳材质 = new Material(Shader.Find("StandardInstanced"))
                    {
                        color = Color.clear,
                        mainTexture = 所有不支持喷漆的网格_也从这个UV纹理上采样,
                        shaderKeywords = ["_EMISSION", "_GLOSSYREFLECTIONS_OFF",],
                    };

                    var 喷漆材质 = Singleton<GameManager>.Instance.TextureArrayColorMaterial;

                    套件多边形网格与材质 = 通用工具.合并多边形网格(所有Mesh, [喷漆材质, 外壳材质], 物体名称);
                    套件多边形网格与材质 = (通用工具.复制多边形网格(套件多边形网格与材质.已合并Mesh), [喷漆材质]);

                    通用工具.注销AssetBundle(资源视图fbx);
                }
            }

            {
                {
                    所有可装配多边形网格与材质 = new Dictionary<string, (Mesh 已合并Mesh, Material[] 所有subMesh材质)>();
                    所有可装配预制体 = new List<(GameObject 实体, GameObject 蓝图)>();
                    所有可装配纹理 = new Dictionary<string, (Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 对应不同喷漆颜色的缩略图)>();

                    {
                        const string 物体名称 = "立体文字装饰";
                        (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(dllDirectory, 物体名称);

                        所有可装配纹理.Add(物体名称, (所有不支持喷漆的网格_UV纹理, 缩略图数组));

                        var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                        var 外壳材质 = new Material(Shader.Find("StandardInstanced"))
                        {
                            color = Color.clear,
                            mainTexture = 所有不支持喷漆的网格_也从这个UV纹理上采样,
                            shaderKeywords = ["_EMISSION", "_GLOSSYREFLECTIONS_OFF",],
                        };

                        var 喷漆材质 = Singleton<GameManager>.Instance.TextureArrayColorMaterial;

                        所有可装配多边形网格与材质.Add(物体名称, 通用工具.合并多边形网格(所有Mesh, [喷漆材质, 外壳材质,], 物体名称));
                        所有可装配多边形网格与材质[物体名称] = (通用工具.复制多边形网格(所有可装配多边形网格与材质[物体名称].已合并Mesh), [喷漆材质]);

                        通用工具.注销AssetBundle(资源视图fbx);
                    }

                    {
                        const string 物体名称 = "居住";
                        (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(dllDirectory, 物体名称);

                        所有可装配纹理.Add(物体名称, (所有不支持喷漆的网格_UV纹理, 缩略图数组));

                        var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                        var 外壳材质 = new Material(Shader.Find("StandardInstanced"))
                        {
                            color = Color.clear,
                            mainTexture = 所有不支持喷漆的网格_也从这个UV纹理上采样,
                            shaderKeywords = ["_EMISSION", "_GLOSSYREFLECTIONS_OFF",],
                        };

                        var 喷漆材质 = Singleton<GameManager>.Instance.TextureArrayColorMaterial;

                        所有可装配多边形网格与材质.Add(物体名称, 通用工具.合并多边形网格(所有Mesh, [喷漆材质, 外壳材质,], 物体名称));
                        所有可装配多边形网格与材质[物体名称] = (通用工具.复制多边形网格(所有可装配多边形网格与材质[物体名称].已合并Mesh), [喷漆材质]);

                        通用工具.注销AssetBundle(资源视图fbx);
                    }
                }
            }

            {
                {

                    休眠的根节点 = new GameObject("休眠的根节点");
                    通用工具.变更激活状态(休眠的根节点, false);
                    UnityEngine.Object.DontDestroyOnLoad(休眠的根节点);

                    {
                        {
                            var 实体预制体 = new GameObject();
                            实体预制体.transform.SetParent(休眠的根节点.transform, worldPositionStays: false);

                            var 蓝图预制体 = new GameObject();
                            蓝图预制体.transform.SetParent(休眠的根节点.transform, worldPositionStays: false);

                            套件预制体 = (实体预制体, 蓝图预制体);
                        }
                    }

                    {
                        var 已建模汉字数量 = 所有可装配多边形网格与材质.Count;
                        for (var i = 0; i < 已建模汉字数量; i++)
                        {
                            var 实体 = UnityEngine.Object.Instantiate(套件预制体.实体, 休眠的根节点.transform, worldPositionStays: false);
                            var 蓝图 = UnityEngine.Object.Instantiate(套件预制体.蓝图, 休眠的根节点.transform, worldPositionStays: false);

                            所有可装配预制体.Add((实体, 蓝图));
                        }
                    }
                }
            }
        }
    }
}