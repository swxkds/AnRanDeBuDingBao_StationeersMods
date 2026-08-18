using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public sealed class 热加载资源管理器 : IDisposable
        {
            private readonly List<AssetBundle> 所有已加载AssetBundle资源视图 = new List<AssetBundle>();
            private readonly List<string> 加载路径备份 = new List<string>();
            public bool IsDisposed { get; private set; }
            public int 已加载AssetBundle资源视图计数 => 所有已加载AssetBundle资源视图.Count;

            public void Initialize(IEnumerable<string> Arg_所有加载路径)
            {
                IsDisposed = false;
                Dispose();

                var 去重 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var 当前 in Arg_所有加载路径)
                {
                    try
                    {
                        var 完整路径 = Path.GetFullPath(当前);

                        if (去重.Add(完整路径))
                        {
                            加载路径备份.Add(完整路径);
                        }
                    }
                    catch (Exception e)
                    {
                        前置模块.Log.LogError($"解析该AssetBundle加载路径时出现<无法解析的路径格式>错误: {当前}\n{e}");
                    }
                }

                去重.Clear();

                foreach (var 当前 in 加载路径备份)
                {
                    if (!File.Exists(当前))
                    {
                        前置模块.Log.LogWarning($"解析该AssetBundle加载路径时, 出现<文件不存在>错误, 已跳过加载: {当前}");
                        continue;
                    }

                    try
                    {
                        var 新 = AssetBundle.LoadFromFile(当前);

                        if (新 == null)
                        {
                            前置模块.Log.LogError($"解析该AssetBundle加载路径时, 出现<文件并非AssetBundle>错误, 已跳过加载: {当前}");
                            continue;
                        }

                        所有已加载AssetBundle资源视图.Add(新);

                        前置模块.Log.LogMessage($"AssetBundle加载成功: {当前} -> {新.name}");
                    }
                    catch (Exception e)
                    {
                        前置模块.Log.LogError($"解析该AssetBundle加载路径时, 出现<其它>错误: {当前}\n{e}");
                    }
                }

                IsDisposed = false;
            }

            public void Dispose()
            {
                if (IsDisposed) { return; }

                foreach (var 当前 in 所有已加载AssetBundle资源视图)
                {
                    注销AssetBundle(当前, 注销方式: AssetBundle注销方式.我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉);
                }

                所有已加载AssetBundle资源视图.Clear();
                加载路径备份.Clear();
                IsDisposed = true;
            }

            public void Clear()
            {
                if (IsDisposed) { return; }

                foreach (var 当前 in 所有已加载AssetBundle资源视图)
                {
                    注销AssetBundle(当前, 注销方式: AssetBundle注销方式.仅注销资源视图_资源依旧保留在Unity资源管理器中);
                }

                所有已加载AssetBundle资源视图.Clear();
                加载路径备份.Clear();
                IsDisposed = true;
            }

            public T 查找资源<T>(string Arg_资源路径) where T : UnityEngine.Object
            {
                if (IsDisposed)
                {
                    前置模块.Log.LogWarning("AssetBundle资源视图已经释放, 无法查找资源");
                    return null;
                }

                foreach (var 当前 in 所有已加载AssetBundle资源视图)
                {
                    var 匹配 = 当前.LoadAsset<T>(Arg_资源路径);
                    if (匹配)
                    {
                        return 匹配;
                    }
                }
                return null;
            }

            public T[] 查找资源<T>() where T : UnityEngine.Object
            {
                if (IsDisposed)
                {
                    前置模块.Log.LogWarning("AssetBundle资源视图已经释放, 无法查找资源");
                    return Array.Empty<T>();
                }

                var 查找结果 = new List<T>();

                foreach (var 当前 in 所有已加载AssetBundle资源视图)
                {
                    var 匹配表 = 当前.LoadAllAssets<T>();

                    if (匹配表 == null || 匹配表.Count() <= 0)
                    {
                        continue;
                    }

                    查找结果.AddRange(匹配表);
                }

                return 查找结果.ToArray();
            }

            public T[] 查找指定目录中资源<T>(string Arg_父目录) where T : UnityEngine.Object
            {
                if (IsDisposed)
                {
                    前置模块.Log.LogWarning("AssetBundle资源视图已经释放, 无法查找资源");
                    return Array.Empty<T>();
                }

                var 查找结果 = new List<T>();

                foreach (var 当前 in 所有已加载AssetBundle资源视图)
                {
                    var 所有的资源索引路径 = 当前.GetAllAssetNames();
                    foreach (var 资源路径 in 所有的资源索引路径)
                    {
                        if (资源路径.StartsWith(Arg_父目录, StringComparison.OrdinalIgnoreCase))
                        {
                            var 匹配 = 当前.LoadAsset<T>(资源路径);
                            if (匹配 == null) { continue; }
                            查找结果.Add(匹配);
                        }
                    }
                }

                return 查找结果.ToArray();
            }

            public Sprite[] 查找所有喷漆颜色缩略图资源(string Arg_父目录)
            {
                var 查找结果 = 查找指定目录中资源<Sprite>(Arg_父目录);
                if (查找结果 == null || 查找结果.Count() <= 0)
                {
                    return Array.Empty<Sprite>();
                }

                var 缩略图数组 = new Sprite[游戏内置喷漆颜色.所有喷漆颜色.Count()];
                for (var i = 0; i < 缩略图数组.Length && i < 查找结果.Length; i++)
                {
                    var 索引 = Array.FindIndex(查找结果, d => d.name == $"{游戏内置喷漆颜色.所有喷漆颜色[i]}缩略图");
                    if (索引 >= 0)
                    {
                        缩略图数组[i] = 查找结果[索引];
                    }
                    else
                    {
                        缩略图数组[i] = 查找结果[0];
                        前置模块.Log.LogError($"解析该资源目录路径时, 缺少{$"{游戏内置喷漆颜色.所有喷漆颜色[i]}缩略图"}: {Arg_父目录}");
                    }
                }

                return 缩略图数组;
            }

            public void 打印已加载资源路径()
            {
                if (IsDisposed)
                {
                    前置模块.Log.LogWarning("AssetBundle资源视图已经释放, 无法打印已加载资源路径");
                    return;
                }

                foreach (var 当前 in 所有已加载AssetBundle资源视图)
                {
                    打印已加载资源路径_资源路径传参给LoadAsset方法会返回该资源的引用(当前);
                }
            }
        }

        [Tooltip("AssetBundle.LoadFrom...系列命令会调用Unity资源管理器API将资源加载到Unity资源管理器中, 同时构造一个AssetBundle自己的资源管理器(资源视图)实例用来额外保存资源索引\n一般通过AssetBundle.LoadAsset或者AssetBundle.LoadAllAssets获取到资源引用后, 就可以注销资源视图了\n注: 调用Unity资源管理器API方法(Resources.FindObjectsOfTypeAll)查找第三方模组加载的资源时, 记得要等游戏初始化完成, 否则查找资源的时候第三方模组还没有加载, 是找不到资源的")]
        public enum AssetBundle注销方式
        {
            仅注销资源视图_资源依旧保留在Unity资源管理器中,
            我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉
        }

        public static 热加载资源管理器 加载AssetBundle(string Arg_dll模组文件所在目录, IEnumerable<string> Arg_所有加载路径_相对于dll模组文件所在目录)
        {
            var 资源视图 = new 热加载资源管理器();
            资源视图.Initialize(Arg_所有加载路径_相对于dll模组文件所在目录.Select(d => Path.Combine(Arg_dll模组文件所在目录, d)));
            return 资源视图;
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

        public static void 打印已加载资源路径_资源路径传参给LoadAsset方法会返回该资源的引用(AssetBundle Arg_资源视图)
        {
            if (Arg_资源视图 == null)
            {
                前置模块.Log.LogError("传入的AssetBundle资源视图为空, 无法打印已加载资源索引路径");
                return;
            }

            // AssetBundle.GetAllAssetNames方法返回的并不是资源名称, 而是资源索引路径, 传参给AssetBundle.LoadAsset方法就可以返回该资源的引用
            var 所有的资源索引路径 = Arg_资源视图.GetAllAssetNames();
            前置模块.Log.LogMessage($"AssetBundle({Arg_资源视图.name})正在打印以下已加载资源索引路径:\n{string.Join("\n", 所有的资源索引路径)}");
        }
    }
}