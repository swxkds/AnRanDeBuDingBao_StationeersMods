using System.IO;
using System.Reflection;
using Assets.Scripts;
using Assets.Scripts.Util;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 功能模块之矿物扫描眼镜_资源加载器
    {
        private static 功能模块之矿物扫描眼镜_资源加载器 m_单例 = null;
        public static 功能模块之矿物扫描眼镜_资源加载器 单例 { get { if (m_单例 == null) { m_单例 = new(); } return m_单例; } }

        private GameObject 休眠的根节点 = null;
        public (Mesh 已合并Mesh, Material[] 所有subMesh材质) 多边形网格与材质 { get; private set; }
        public (GameObject 实体, GameObject 蓝图) 预制体 { get; private set; }
        public (Texture2DArray 对应不同喷漆颜色的UV纹理, Sprite[] 对应不同喷漆颜色的缩略图) 纹理 { get; private set; }     // 制作UV纹理时, 只从原始纹理中采样, 这样原始纹理可以让缩略图复用

        public 功能模块之矿物扫描眼镜_资源加载器()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录
            const string 物体名称 = "矿物扫描眼镜";

            {
                (AssetBundle 资源视图fbx, Texture2DArray UV纹理数组, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和uv纹理数组和所有缩略图(dllDirectory, 物体名称);
                纹理 = (UV纹理数组, 缩略图数组);
                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                var 实体材质 = new Material(Singleton<GameManager>.Instance.TextureArrayColorMaterial) { mainTexture = 纹理.对应不同喷漆颜色的UV纹理 };
                var 镜片材质 = new Material(Shader.Find("Custom/Stationeers Transparent")) { color = Color.white.SetAlpha(0.04f) };

                多边形网格与材质 = 通用工具.合并多边形网格(所有Mesh, [实体材质, 镜片材质]);

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

                预制体 = (实体预制体, 蓝图预制体);
            }
        }
    }
}

