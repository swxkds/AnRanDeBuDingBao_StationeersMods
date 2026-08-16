using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 功能模块之立体文字装饰_资源加载器
    {
        private static 功能模块之立体文字装饰_资源加载器 m_单例 = null;
        public static 功能模块之立体文字装饰_资源加载器 单例 { get { if (m_单例 == null) { m_单例 = new(); } return m_单例; } }

        public Dictionary<string, (Mesh 已合并Mesh, Material[] 所有subMesh材质)> 所有一体式多边形网格与材质 { get; private set; }
        public Dictionary<string, (Mesh[] 所有Mesh, Material[] 所有subMesh材质)> 所有分体式多边形网格与材质 { get; private set; }
        public Dictionary<string, (GameObject 实体, GameObject 蓝图)> 所有预制体 { get; private set; }
        public Dictionary<string, (Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 对应不同喷漆颜色的缩略图)> 所有纹理 { get; private set; }

        public 功能模块之立体文字装饰_资源加载器()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录

            所有一体式多边形网格与材质 = new Dictionary<string, (Mesh 已合并Mesh, Material[] 所有subMesh材质)>();
            所有分体式多边形网格与材质 = new Dictionary<string, (Mesh[] 所有Mesh, Material[] 所有subMesh材质)>();
            所有预制体 = new Dictionary<string, (GameObject 实体, GameObject 蓝图)>();
            所有纹理 = new Dictionary<string, (Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 对应不同喷漆颜色的缩略图)>();

            var 喷漆材质 = 通用工具.游戏内置喷漆颜色.游戏内置喷漆材质;
            var 喷漆UV纹理 = 通用工具.游戏内置喷漆颜色.游戏内置喷漆材质使用的UV纹理_注_不支持喷漆的子网格_也可以从这个UV纹理上采样;

            {
                const string 物体名称 = "(套件)立体文字装饰";
                (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(dllDirectory, 物体名称);
                所有纹理.Add(物体名称, (所有不支持喷漆的网格_UV纹理, 缩略图数组));
                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();
                所有一体式多边形网格与材质.Add(物体名称, 通用工具.合并多边形网格(所有Mesh, [喷漆材质], 物体名称, Arg_保留子网格么: false));
                通用工具.注销AssetBundle(资源视图fbx);

                所有预制体.Add(物体名称, (通用工具.创建新的空预制体(), 通用工具.创建新的空预制体()));
            }

            {
                const string 物体名称 = "立体文字装饰";
                (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(dllDirectory, 物体名称);
                所有纹理.Add(物体名称, (所有不支持喷漆的网格_UV纹理, 缩略图数组));
                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();
                所有一体式多边形网格与材质.Add(物体名称, 通用工具.合并多边形网格(所有Mesh, [喷漆材质], 物体名称, Arg_保留子网格么: false));
                通用工具.注销AssetBundle(资源视图fbx);

                所有预制体.Add(物体名称, (通用工具.创建新的空预制体(), 通用工具.创建新的空预制体()));
            }

            {
                const string 物体名称 = "居住";
                (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) = 通用工具.加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(dllDirectory, 物体名称);
                所有纹理.Add(物体名称, (所有不支持喷漆的网格_UV纹理, 缩略图数组));
                var 所有Mesh = 资源视图fbx.LoadAllAssets<Mesh>();

                所有分体式多边形网格与材质.Add(物体名称, (所有Mesh.Select(d => 通用工具.复制多边形网格(d, Arg_保留子网格么: false)).ToArray(), [喷漆材质, 喷漆材质]));
                通用工具.注销AssetBundle(资源视图fbx);

                所有预制体.Add(物体名称, (通用工具.创建新的空预制体(), 通用工具.创建新的空预制体()));
            }
        }
    }
}

