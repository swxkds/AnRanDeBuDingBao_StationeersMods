using System.IO;
using System.Reflection;
using Assets.Scripts.Util;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 功能模块之矿物扫描眼镜_资源加载器
    {
        private static 功能模块之矿物扫描眼镜_资源加载器 m_单例 = null;
        public static 功能模块之矿物扫描眼镜_资源加载器 单例 { get { if (m_单例 == null) { m_单例 = new(); } return m_单例; } }

        public (Mesh 已合并Mesh, Material[] 所有subMesh材质) 多边形网格与材质 { get; private set; }
        public (GameObject 实体, GameObject 蓝图) 预制体 { get; private set; }
        public (Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 对应不同喷漆颜色的缩略图) 纹理 { get; private set; }

        public 功能模块之矿物扫描眼镜_资源加载器()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录

            {
                const string 物体名称 = "矿物扫描眼镜";
                (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(dllDirectory, 物体名称);
                纹理 = (所有不支持喷漆的网格_UV纹理, 缩略图数组);
                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                var 喷漆材质 = 通用工具.游戏内置喷漆颜色.游戏内置喷漆材质;
                var 镜片材质 = new Material(Shader.Find("Custom/Stationeers Transparent")) { color = Color.white.SetAlpha(0.04f) };

                多边形网格与材质 = 通用工具.合并多边形网格(所有Mesh, [喷漆材质, 镜片材质], 物体名称);

                通用工具.注销AssetBundle(资源视图fbx);

                预制体 = (通用工具.创建新的空预制体(), 通用工具.创建新的空预制体());
            }
        }
    }
}

