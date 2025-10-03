using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 功能模块之矿物扫描眼镜_资源加载器
    {
        private static 功能模块之矿物扫描眼镜_资源加载器 m_单例 = null;
        public static 功能模块之矿物扫描眼镜_资源加载器 单例
        {
            get
            {
                if (m_单例 == null) { m_单例 = new(); }
                return m_单例;
            }
        }
        private AssetBundle 资源管理器 = null;
        public GameObject 矿物扫描眼镜 = null;
        public GameObject 矿物扫描眼镜_蓝图 = null; // 有顶点集合,有三角形连线集合,用于线框图生成器生成线框
        public Sprite 缩略图 = null;
        public Dictionary<string, Texture2D> 贴图集合_字典 = null;   // 都是一样的贴图,只是部分喷涂区颜色不同,用于喷漆交互后切换到对应颜色的贴图
        private GameObject 休眠节点;
        public Texture2DArray 贴图集合 = null;
        public 功能模块之矿物扫描眼镜_资源加载器()
        {
            休眠节点 = new("防止矿物扫描眼镜母体被主动调用");
            UnityEngine.Object.DontDestroyOnLoad(休眠节点);
            休眠节点.SetActive(false);

            using (Stream 读 = Assembly.GetExecutingAssembly().GetManifestResourceStream("meanran_xuexi_mods.Resources.矿物扫描眼镜网格_贴图"))
            { 资源管理器 = AssetBundle.LoadFromStream(读); }
            打印所有资源();

            // Blender制作的.fbx模型,会被Unity识别成GameObject挂载了一个MeshFilter(Mesh保存在这里)和MeshRenderer(Materials保存在这里)组件
            // 如果该模型由多个子网格组成,会给GameObject增加新的子级GameObject,然后在子级GameObject中添加一个MeshFilter(子Mesh保存在这里)和MeshRenderer(子Materials保存在这里)组件
            // 由于着色器不是自己写的,需要在运行时查找,因此只保留GameObject和MeshFilter,其它全部删除掉,在运行时创建
            矿物扫描眼镜 = 获取资源<GameObject>("assets/矿物扫描眼镜/mesh/矿物扫描眼镜.prefab");
            矿物扫描眼镜_蓝图 = 获取资源<GameObject>("assets/矿物扫描眼镜/mesh/矿物扫描眼镜_蓝图.prefab");

            矿物扫描眼镜.transform.SetParent(休眠节点.transform, false);
            矿物扫描眼镜_蓝图.transform.SetParent(休眠节点.transform, false);

            // 调用<打印所有资源>方法,获取到的AssetName
            var __ = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜.png");
            缩略图 = Sprite.Create(__, new(0, 0, __.width, __.height), new(__.width / 2, __.height / 2), pixelsPerUnit: 50);

            贴图集合_字典 = new();
            贴图集合_字典["black"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_black.png");
            贴图集合_字典["blue"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_blue.png");
            贴图集合_字典["brown"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_brown.png");
            贴图集合_字典["gray"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_gray.png");
            贴图集合_字典["green"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_green.png");
            贴图集合_字典["khaki"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_khaki.png");
            贴图集合_字典["orange"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_orange.png");
            贴图集合_字典["pink"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_pink.png");
            贴图集合_字典["purple"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_purple.png");
            贴图集合_字典["red"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_red.png");
            贴图集合_字典["white"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_white.png");
            贴图集合_字典["yellow"] = 获取资源<Texture2D>("assets/矿物扫描眼镜/texture2d/矿物扫描眼镜_yellow.png");

            贴图集合 = new Texture2DArray(贴图集合_字典["yellow"].width, 贴图集合_字典["yellow"].height, 贴图集合_字典.Count, 贴图集合_字典["yellow"].format, mipChain: true);
            var 元素数 = -1;
            foreach (var 贴图 in 贴图集合_字典.Values)
            {
                元素数++;
                if (贴图.mipmapCount > 1)
                { for (var i = 0; i < 贴图.mipmapCount; i++) { Graphics.CopyTexture(贴图, 0, i, 贴图集合, 元素数, i); } }
                else { Graphics.CopyTexture(贴图, 0, 贴图集合, 元素数); }
            }
        }
        public void 释放所有资源()
        {
            try { 资源管理器.Unload(true); }
            catch (Exception e) { 功能模块之矿物扫描眼镜.Log.LogError($"AssetBundle释放所有资源失败,错误信息->{资源管理器} , {e}\nUnity引擎发癫,不用管"); }
        }
        public void 打印所有资源()
        {
            var __ = string.Join("\n", 资源管理器.GetAllAssetNames());
            功能模块之矿物扫描眼镜.Log.LogMessage($"AssetBundle管理以下资源:\n{__}");
        }
        public T 获取资源<T>(string name) where T : UnityEngine.Object
        {
            var __ = 资源管理器.LoadAsset<T>(name);
            if (__ != null) { return __; }
            else { return null; }
        }
    }
}

