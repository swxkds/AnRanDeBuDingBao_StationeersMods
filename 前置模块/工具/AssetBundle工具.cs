using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        [Tooltip("AssetBundle.LoadFrom...系列命令会调用Unity资源管理器API将资源加载到Unity资源管理器中, 同时构造一个AssetBundle自己的资源管理器(资源视图)实例用来额外保存资源索引\n一般通过AssetBundle.LoadAsset或者AssetBundle.LoadAllAssets获取到资源引用后, 就可以注销资源视图了\n注: 调用Unity资源管理器API方法(Resources.FindObjectsOfTypeAll)查找第三方模组加载的资源时, 记得要等游戏初始化完成, 否则查找资源的时候第三方模组还没有加载, 是找不到资源的")]
        public enum AssetBundle注销方式
        {
            仅注销资源视图_资源依旧保留在Unity资源管理器中,
            我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉
        }

        [Tooltip("使用<AssetRipper.GUI.Free.exe>打开游戏根目录的文件<rocketstation.exe>, AssetRipper工具会解包整个游戏程序, 然后选择导出目录并导出资源. 导出结果中可以看到<Assets/Texture2DArray/ColorPaletteArray.png>这张图片, 这就是游戏内置的喷漆系统材质(Singleton<GameManager>.Instance.TextureArrayColorMaterial)使用的UV纹理, 我们在建模时, 需要支持喷漆的子网格都从这个图片上采样\n一个Mesh有多少个子网格, Renderer.sharedMaterials这个材质数组就需要多少个材质, 两者按照索引一一对应(例: 子网格0就使用材质数组中第1个材质; 子网格1就使用材质数组中第2个材质")]
        public static (AssetBundle 资源视图fbx, Texture2D 所有不支持喷漆的网格_UV纹理, Sprite[] 缩略图数组) 加载本地的AssetBundle文件_fbx和UV纹理和所有缩略图(string dll模组文件所在目录, string 物体名称)
        {
            AssetBundle 资源视图fbx = null;
            Texture2D 所有不支持喷漆的网格_UV纹理 = null;
            Sprite[] 缩略图数组 = null;

            {
                var 路径 = Path.Combine(dll模组文件所在目录, $"模型与纹理/{物体名称}/{物体名称}fbx");
                if (File.Exists(路径))
                {
                    资源视图fbx = AssetBundle.LoadFromFile(路径);
                    打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(资源视图fbx);
                }
            }

            {
                var 路径 = Path.Combine(dll模组文件所在目录, $"模型与纹理/{物体名称}/{物体名称}的所有不支持喷漆的网格_UV纹理");
                if (File.Exists(路径))
                {
                    var 资源视图UV纹理 = AssetBundle.LoadFromFile(路径);
                    打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(资源视图UV纹理);
                    所有不支持喷漆的网格_UV纹理 = 资源视图UV纹理.LoadAsset<Texture2D>($"Assets/{物体名称}/所有不支持喷漆的网格_UV纹理.png");
                    注销AssetBundle(资源视图UV纹理, AssetBundle注销方式.仅注销资源视图_资源依旧保留在Unity资源管理器中);
                }
            }

            {
                var 路径 = Path.Combine(dll模组文件所在目录, $"模型与纹理/{物体名称}/{物体名称}的所有喷漆颜色缩略图");
                if (File.Exists(路径))
                {
                    var 资源视图所有缩略图 = AssetBundle.LoadFromFile(路径);
                    打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(资源视图所有缩略图);
                    缩略图数组 = new Sprite[游戏内置喷漆颜色.所有喷漆颜色.Count()];
                    for (var i = 0; i < 缩略图数组.Length; i++)
                    {
                        var 当前 = 资源视图所有缩略图.LoadAsset<Sprite>($"Assets/{物体名称}/所有喷漆颜色缩略图/{游戏内置喷漆颜色.所有喷漆颜色[i]}缩略图.asset");
                        if (当前)
                        {
                            缩略图数组[i] = 当前;
                        }
                        else
                        {
                            缩略图数组[i] = 缩略图数组[0];
                            前置模块.Log.LogError($"加载本地AssetBundle文件时_: {物体名称}的缩略图缺少 {游戏内置喷漆颜色.所有喷漆颜色[i]}");
                        }
                    }
                    注销AssetBundle(资源视图所有缩略图, AssetBundle注销方式.仅注销资源视图_资源依旧保留在Unity资源管理器中);
                }
            }

            return (资源视图fbx, 所有不支持喷漆的网格_UV纹理, 缩略图数组);
        }

        public static void 注销AssetBundle(AssetBundle Arg_资源视图, AssetBundle注销方式 注销方式 = AssetBundle注销方式.我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉)
        {
            if (Arg_资源视图 == null)
            {
                前置模块.Log.LogError("传入的AssetBundle资源视图为空, 无法注销");
                return;
            }

            try
            {
                switch (注销方式)
                {
                    case AssetBundle注销方式.仅注销资源视图_资源依旧保留在Unity资源管理器中:
                        Arg_资源视图.Unload(unloadAllLoadedObjects: false);
                        break;
                    case AssetBundle注销方式.我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉:
                        Arg_资源视图.Unload(unloadAllLoadedObjects: true);
                        break;
                }
            }
            catch (Exception e)
            {
                前置模块.Log.LogError($"AssetBundle注销失败,错误信息->{Arg_资源视图} , {e}\nUnity引擎发癫,不用管");
            }
        }

        public static void 打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(AssetBundle Arg_资源视图)
        {
            if (Arg_资源视图 == null)
            {
                前置模块.Log.LogError("传入的AssetBundle资源视图为空, 无法打印已加载资源索引路径");
                return;
            }

            // AssetBundle.GetAllAssetNames方法返回的并不是资源名称, 而是资源索引路径, 传参给AssetBundle.LoadAsset方法就可以返回该资源的引用
            var 所有的资源索引路径 = Arg_资源视图.GetAllAssetNames();
            前置模块.Log.LogMessage($"AssetBundle正在打印以下已加载资源索引路径:\n{string.Join("\n", 所有的资源索引路径)}");
        }
    }
}