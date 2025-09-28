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
        public (Texture2DArray 对应不同喷漆颜色的UV纹理, Sprite[] 对应不同喷漆颜色的缩略图) 套件纹理 { get; private set; }     // 制作UV纹理时, 只从原始纹理中采样, 这样原始纹理可以让缩略图复用


        public const int 已建模汉字数量 = 2;
        public List<(Mesh 已合并Mesh, Material[] 所有subMesh材质)> 所有可装配多边形网格与材质 { get; private set; }
        public List<(GameObject 实体, GameObject 蓝图)> 所有可装配预制体 { get; private set; }
        public List<(Texture2DArray 对应不同喷漆颜色的UV纹理, Sprite[] 对应不同喷漆颜色的缩略图)> 所有可装配纹理 { get; private set; }     // 制作UV纹理时, 只从原始纹理中采样, 这样原始纹理可以让缩略图复用


        public 功能模块之立体文字装饰_资源加载器()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录

            {
                const string 物体名称 = "(套件)立体文字装饰";
                (AssetBundle 资源视图fbx, Texture2DArray UV纹理数组, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和uv纹理数组和所有缩略图(dllDirectory, 物体名称);
                套件纹理 = (UV纹理数组, 缩略图数组);
                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                var 实体材质1 = new Material(Shader.Find("StandardInstanced"))
                {
                    color = Color.gray,
                    mainTexture = null,
                    shaderKeywords = ["_EMISSION", "_GLOSSYREFLECTIONS_OFF",],
                };

                var 实体材质2 = new Material(Singleton<GameManager>.Instance.TextureArrayColorMaterial) { mainTexture = 套件纹理.对应不同喷漆颜色的UV纹理 };

                套件多边形网格与材质 = 通用工具.合并多边形网格(所有Mesh, [实体材质2, 实体材质1]);

                通用工具.注销AssetBundle(资源视图fbx);
            }

            {
                休眠的根节点 = new GameObject("休眠的根节点");
                通用工具.变更激活状态(休眠的根节点, false);
                UnityEngine.Object.DontDestroyOnLoad(休眠的根节点);

                var 实体预制体 = new GameObject();
                实体预制体.transform.SetParent(休眠的根节点.transform, worldPositionStays: false);

                var 蓝图预制体 = new GameObject();
                蓝图预制体.transform.SetParent(休眠的根节点.transform, worldPositionStays: false);

                套件预制体 = (实体预制体, 蓝图预制体);
            }

            所有可装配多边形网格与材质 = new List<(Mesh 已合并Mesh, Material[] 所有subMesh材质)>();
            所有可装配预制体 = new List<(GameObject 实体, GameObject 蓝图)>();

            for (var i = 0; i < 已建模汉字数量; i++)
            {
                var 实体 = UnityEngine.Object.Instantiate(套件预制体.实体, 休眠的根节点.transform, worldPositionStays: false);
                var 蓝图 = UnityEngine.Object.Instantiate(套件预制体.蓝图, 休眠的根节点.transform, worldPositionStays: false);

                所有可装配预制体.Add((实体, 蓝图));
            }

            {
                const string 物体名称 = "立体文字装饰";
                var 资源视图fbx = AssetBundle.LoadFromFile(Path.Combine(dllDirectory, "模型与纹理/" + 物体名称 + "/" + 物体名称 + "fbx_AssetBundle"));
                通用工具.打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(资源视图fbx);

                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                var 实体材质1 = new Material(Shader.Find("StandardInstanced"))
                {
                    color = Color.gray,
                    mainTexture = null,
                    shaderKeywords = ["_EMISSION", "_GLOSSYREFLECTIONS_OFF",],
                };

                var 实体材质2 = new Material(Singleton<GameManager>.Instance.TextureArrayColorMaterial) { mainTexture = 套件纹理.对应不同喷漆颜色的UV纹理 };

                所有可装配多边形网格与材质.Add(通用工具.合并多边形网格(所有Mesh, [实体材质2, 实体材质1, 实体材质1,]));

                通用工具.注销AssetBundle(资源视图fbx);
            }

            {
                const string 物体名称 = "居住";
                var 资源视图fbx = AssetBundle.LoadFromFile(Path.Combine(dllDirectory, "模型与纹理/" + 物体名称 + "/" + 物体名称 + "fbx_AssetBundle"));
                通用工具.打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(资源视图fbx);

                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                var 实体材质1 = new Material(Shader.Find("StandardInstanced"))
                {
                    color = Color.gray,
                    mainTexture = null,
                    shaderKeywords = ["_EMISSION", "_GLOSSYREFLECTIONS_OFF",],
                };

                var 实体材质2 = new Material(Singleton<GameManager>.Instance.TextureArrayColorMaterial) { mainTexture = 套件纹理.对应不同喷漆颜色的UV纹理 };

                所有可装配多边形网格与材质.Add(通用工具.合并多边形网格(所有Mesh, [实体材质1, 实体材质2, 实体材质1,]));

                通用工具.注销AssetBundle(资源视图fbx);
            }
        }
    }
}