using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Assets.Scripts.Util;
using Assets.Scripts.Objects;
using System.IO;
using Assets.Scripts.UI;
using Reagents;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {

        public class 游戏内置喷漆色板
        {
            public enum 游戏内置喷漆色板12种颜色
            {
                蓝色Blue = 0,
                灰色Gray, 绿色Green, 橙色Orange, 红色Red, 黄色Yellow, 白色White, 黑色Black, 棕色Brown, 土黄色Khaki, 粉色Pink, 紫色Purple,
            }
            public static readonly 游戏内置喷漆色板12种颜色[] 所有喷漆色板颜色 = Enum.GetValues(typeof(游戏内置喷漆色板12种颜色)).Cast<游戏内置喷漆色板12种颜色>().ToArray();
        }

        [Tooltip("建议在Unity编辑器中提前创建好对应不同喷漆颜色的UV纹理和缩略图, 然后打包成AssetBundle, 因为Unity引擎在专业服务器版本中, 加载AssetBundle功能是开启的(因为只是反序列化, 没有图形计算), 但是创建Texture2DArray和Sprite是禁用的")]
        public static (Texture2DArray 对应不同喷漆颜色的UV纹理, Sprite[] 对应不同喷漆颜色的缩略图) 创建UV纹理和缩略图(Dictionary<游戏内置喷漆色板.游戏内置喷漆色板12种颜色, Texture2D> 对应不同喷漆颜色的纹理)
        {
            (var 颜色, var 纹理) = 对应不同喷漆颜色的纹理.First();
            if (游戏内置喷漆色板.所有喷漆色板颜色.Any(d => d == 颜色) && 纹理 != null)
            {
                var 多分辨率预存计数 = 纹理.mipmapCount;
                var 是否存在多分辨率预存 = 多分辨率预存计数 > 1;

                var UV纹理 = new Texture2DArray(纹理.width, 纹理.height, 游戏内置喷漆色板.所有喷漆色板颜色.Count(), 纹理.format, 是否存在多分辨率预存);
                var 缩略图 = new Sprite[游戏内置喷漆色板.所有喷漆色板颜色.Count()];

                var 纹理区域信息 = new Rect(0, 0, 纹理.width, 纹理.height);
                var 纹理的轴心点 = new Vector2(0.5f, 0.5f);       // 归一化值, 纹理的中心点 == 纹理的轴心点

                foreach (var 当前 in 游戏内置喷漆色板.所有喷漆色板颜色)
                {
                    var 当前depth = (int)当前;
                    if (对应不同喷漆颜色的纹理.TryGetValue(当前, out var 当前纹理))
                    {
                        if (是否存在多分辨率预存)
                        {
                            for (var i = 0; i < 多分辨率预存计数; ++i)
                            {
                                Graphics.CopyTexture(src: 当前纹理, srcElement: 0, srcMip: i, dst: UV纹理, dstElement: 当前depth, dstMip: i);
                            }
                        }
                        else
                        {
                            Graphics.CopyTexture(src: 当前纹理, srcElement: 0, dst: UV纹理, dstElement: 当前depth);
                        }

                        缩略图[当前depth] = Sprite.Create(当前纹理, 纹理区域信息, 纹理的轴心点, pixelsPerUnit: 50);
                    }
                    else
                    {
                        前置模块.Log.LogDebug($"创建对应不同喷漆颜色的UV纹理和缩略图时, 缺少{当前}");

                        if (是否存在多分辨率预存)
                        {
                            for (var i = 0; i < 多分辨率预存计数; ++i)
                            {
                                Graphics.CopyTexture(src: UV纹理, srcElement: 0, srcMip: i, dst: UV纹理, dstElement: 当前depth, dstMip: i);
                            }
                        }
                        else
                        {
                            Graphics.CopyTexture(src: UV纹理, srcElement: 0, dst: UV纹理, dstElement: 当前depth);
                        }

                        缩略图[当前depth] = 缩略图[0];
                    }
                }

                return (UV纹理, 缩略图);
            }
            else
            {
                前置模块.Log.LogDebug($"创建对应不同喷漆颜色的UV纹理和缩略图时, 创建失败, 传入的原始纹理字典存在错误");
                return (null, null);
            }
        }

        public static (Mesh 已合并Mesh, Material[] 所有subMesh材质) 合并多边形网格(Mesh[] Arg_所有Mesh, Material[] Arg_所有subMesh材质)
        {
            if (Arg_所有Mesh == null || Arg_所有Mesh.Length == 0 || Arg_所有subMesh材质 == null || Arg_所有subMesh材质.Length == 0)
            {
                前置模块.Log.LogError("传入的Mesh或材质数组为空, 无法合并多边形网格");
                return (null, null);
            }

            if (Arg_所有Mesh.Any(d => d.subMeshCount != 1))
            {
                前置模块.Log.LogError("传入的Mesh的subMeshCount不为1, 无法合并多边形网格");
                return (null, null);
            }

            if (Arg_所有Mesh.Length != Arg_所有subMesh材质.Length)
            {
                前置模块.Log.LogError("传入的Mesh和材质数组长度不一致, 无法合并多边形网格");
                return (null, null);
            }

            var Result = 合并多边形网格(Arg_所有Mesh, Arg_保留子网格么: true);
            return (Result, Arg_所有subMesh材质);
        }

        public static Mesh 合并多边形网格(Mesh[] Arg_所有Mesh, bool Arg_保留子网格么 = false)
        {
            if (Arg_所有Mesh == null || Arg_所有Mesh.Length == 0)
            {
                前置模块.Log.LogError("传入的Mesh为空, 无法合并多边形网格");
                return null;
            }

            if (Arg_所有Mesh.Any(d => d.subMeshCount != 1))
            {
                前置模块.Log.LogError("传入的Mesh的subMeshCount不为1, 无法合并多边形网格");
                return null;
            }

            var 待合并 = new List<CombineInstance>(Arg_所有Mesh.Length);
            for (var i = 0; i < Arg_所有Mesh.Length; ++i)
            {
                // subMeshIndex: 建模时, 可以人为的将三角形索引数组划分片段, 每个片段有着自己的数组地址偏移和数据长度。通过subMeshIndex可以获取该片段的数组地址偏移和数据长度, 同时通过sharedMaterials[subMeshIndex]找到该片段的材质
                //               建模软件导出的片段划分信息, Unity引擎不一定能正确识别, 因此 Arg_所有Mesh 中的所有元素都只有一个subMesh(即数组地址偏移=0,数据长度=三角形索引数组长度), 通过在此函数中合并多边形网格, 将多个Mesh变成一个Mesh中的多个subMesh
                // transform: 多边形网格对象使用世界坐标系1:1比例    例: 使用Blender建模, 有导出前执行命令 "物体/应用/位置" "物体/应用/旋转" "物体/应用/缩放" , 让变换矩阵中的位置和旋转变成(0,0,0)、缩放变成(1,1,1), 此时法向、顶点坐标.....就变成世界坐标系1:1比例下的数据
                待合并.Add(new CombineInstance
                {
                    mesh = Arg_所有Mesh[i],
                    subMeshIndex = 0,
                    transform = Matrix4x4.identity
                });
            }

            var Result = new Mesh() { name = 待合并.First().mesh.name + "已合并" };
            Result.CombineMeshes(待合并.ToArray(), mergeSubMeshes: !Arg_保留子网格么, useMatrices: true);
            Result.RecalculateNormals();
            Result.RecalculateBounds();
            return Result;
        }

        [Tooltip("注: 目标物体.ReagentMixture: 对于自动车床/自动烤箱/微波炉/熔炉.......等等具有内部混合容器的设备, 此处保存了所有投入物体的成分组成之和")]
        public static void 为目标物体添加试剂成分表_每QuantityPerUse单位(Item 目标物体, Recipe _1单位成分数据)
        {
            // 如果实际数量小于QuantityPerUse, 则等比例获取试剂成分
            var 投放一次的消耗数量 = 目标物体.QuantityPerUse;

            var 投放一次的材料成分 = new ReagentMixture(目标物体);
            投放一次的材料成分.Add(_1单位成分数据 * 投放一次的消耗数量);

            目标物体.CreatedReagentMixture = 投放一次的材料成分;
        }

        public class 施工材料和工时数据
        {
            private static readonly Dictionary<int, Thing> 已发现施工材料缓存 = new();
            private static readonly List<装配与拆除所需的施工材料和工时数据> 所有待添加的的装配与拆除数据 = new();
            private static readonly List<修复所需的施工材料和工时数据> 所有待添加的的修复数据 = new();

            public static void 添加到待添加队列_因为需要等待游戏资源加载完成才能查找到施工材料(装配与拆除所需的施工材料和工时数据 数据)
            {
                lock (所有待添加的的装配与拆除数据)
                {
                    所有待添加的的装配与拆除数据.Add(数据);
                }
            }

            public static void 添加到待添加队列_因为需要等待游戏资源加载完成才能查找到施工材料(修复所需的施工材料和工时数据 数据)
            {
                lock (所有待添加的的修复数据)
                {
                    所有待添加的的修复数据.Add(数据);
                }
            }

            public static void 从待添加队列中取出所有数据并执行添加()
            {
                foreach (var 当前 in 所有待添加的的装配与拆除数据)
                {
                    为目标物体的施工阶段组件添加施工材料和工时数据(当前);
                }

                foreach (var 当前 in 所有待添加的的修复数据)
                {
                    为目标物体添加修复结构所需的施工材料和工时数据(当前);
                }
            }

            private static void 为目标物体的施工阶段组件添加施工材料和工时数据(装配与拆除所需的施工材料和工时数据 数据)
            {
                var 本施工阶段的施工材料和工时数据 = new ToolUse()
                {
                    ToolUseType = 数据.目标物体的项目建设性质,

                    ToolEntry = 数据.装配.主手持有的该物品的PrefabHash == 0 ? null : 查找施工材料<Item>(数据.装配.主手持有的该物品的PrefabHash),
                    EntryQuantity = 数据.装配.主手消耗数量,
                    ToolEntry2 = 数据.装配.副手持有的该物品的PrefabHash == 0 ? null : 查找施工材料<Item>(数据.装配.副手持有的该物品的PrefabHash),
                    EntryQuantity2 = 数据.装配.副手消耗数量,
                    EntryTime = 数据.装配.完成操作所需的进度条读条时长,

                    ToolExit = 数据.拆除.主手持有的该物品的PrefabHash == 0 ? null : 查找施工材料<Item>(数据.拆除.主手持有的该物品的PrefabHash),
                    ExitQuantity = 数据.拆除.主手消耗数量,
                    ExitTime = 数据.拆除.完成操作所需的进度条读条时长,
                };

                var 查找器数据 = 数据.目标物体的施工阶段组件;
                查找器数据.源资源_目标物体的施工阶段组件.Tool = 本施工阶段的施工材料和工时数据;

                var 目标物体 = 查找施工材料<Structure>(查找器数据.目标物体PrefabHash);
                if (目标物体)
                {
                    BuildState 当前施工阶段组件 = null;
                    switch (查找器数据.施工阶段对应的建筑结构状态)
                    {
                        case 建筑结构状态.结构正常状态:
                            if (查找器数据.施工阶段索引 >= 0 && 查找器数据.施工阶段索引 < 目标物体.BuildStates.Count)
                            {
                                当前施工阶段组件 = 目标物体.BuildStates[查找器数据.施工阶段索引];
                            }
                            break;
                        case 建筑结构状态.结构损毁状态:
                            if (查找器数据.施工阶段索引 >= 0 && 查找器数据.施工阶段索引 < 目标物体.BrokenBuildStates.Count)
                            {
                                当前施工阶段组件 = 目标物体.BrokenBuildStates[查找器数据.施工阶段索引].BuildState;      // BrokenBuildState.TotalReagentMixture: 在结构损毁状态时进行拆除, 获取的碎片道具的试剂成分组成
                            }
                            break;
                    }

                    if (当前施工阶段组件 == null) { return; }

                    当前施工阶段组件.Tool = 本施工阶段的施工材料和工时数据;

                    前置模块.Log.LogMessage($"{目标物体.DisplayName}的{查找器数据.施工阶段对应的建筑结构状态}, 第{查找器数据.施工阶段索引}个施工阶段, 成功添加施工材料和工时数据");
                }
            }
            private static void 为目标物体添加修复结构所需的施工材料和工时数据(修复所需的施工材料和工时数据 数据)
            {
                var 修复结构所需的施工材料和工时数据 = new ToolBasic()
                {
                    ToolEntry = 数据.修复.主手持有的该物品的PrefabHash == 0 ? null : 查找施工材料<Item>(数据.修复.主手持有的该物品的PrefabHash),
                    EntryQuantity = 数据.修复.主手消耗数量,
                    ToolEntry2 = 数据.修复.副手持有的该物品的PrefabHash == 0 ? null : 查找施工材料<Item>(数据.修复.副手持有的该物品的PrefabHash),
                    EntryQuantity2 = 数据.修复.副手消耗数量,
                    EntryTime = 数据.修复.完成操作所需的进度条读条时长,
                };

                var 查找器数据 = 数据.目标物体;
                查找器数据.源资源_目标物体.RepairTools = 修复结构所需的施工材料和工时数据;

                var 目标物体 = 查找施工材料<Structure>(查找器数据.目标物体PrefabHash);
                if (目标物体)
                {
                    目标物体.RepairTools = 修复结构所需的施工材料和工时数据;

                    前置模块.Log.LogMessage($"{目标物体.DisplayName}的{建筑结构状态.结构正常状态}, 成功添加修复结构所需的施工材料和工时数据");
                }
            }
            public static T 查找施工材料<T>(int PrefabHash) where T : Thing
            {
                if (已发现施工材料缓存.TryGetValue(PrefabHash, out var 匹配)) { return (T)匹配; }

                if (Prefab.TryFind<T>(PrefabHash, out var 当前))
                {
                    if (当前.PrefabHash == PrefabHash && 当前.ReferenceId == 0)
                    {
                        已发现施工材料缓存.Add(PrefabHash, 当前);
                        return 当前;
                    }

                }

                return null;
            }

            [Tooltip("装配举例一: 主手持有工具, 副手为空\n装配举例二: 主手持有工具, 副手为材料\n装配举例三: 主手持有材料, 副手为空\n消耗数量对于工具指的是电量或者焊枪燃气或者其它能源,对于材料指的是材料使用数量\n如果工具不需要能源(比如剪线钳), 消耗数量写1即可, 不会有实际消耗")]
            public readonly struct 装配所需的施工材料和工时数据
            {
                public readonly int 主手持有的该物品的PrefabHash;
                public readonly int 主手消耗数量;
                public readonly int 副手持有的该物品的PrefabHash;
                public readonly int 副手消耗数量;
                public readonly float 完成操作所需的进度条读条时长;

                public 装配所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, int 副手持有的该物品的PrefabHash, int 副手消耗数量, float 完成操作所需的进度条读条时长) Arg_装配)
                {
                    主手持有的该物品的PrefabHash = Arg_装配.主手持有的该物品的PrefabHash;
                    主手消耗数量 = Arg_装配.主手消耗数量;
                    副手持有的该物品的PrefabHash = Arg_装配.副手持有的该物品的PrefabHash;
                    副手消耗数量 = Arg_装配.副手消耗数量;
                    完成操作所需的进度条读条时长 = Arg_装配.完成操作所需的进度条读条时长;
                }

                // 隐式转换
                public static implicit operator 装配所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, int 副手持有的该物品的PrefabHash, int 副手消耗数量, float 完成操作所需的进度条读条时长) Arg_装配)
                {
                    return new 装配所需的施工材料和工时数据(Arg_装配);
                }
            }

            [Tooltip("拆除一般都是主手持有工具, 副手为空\n消耗数量对于工具指的是电量或者焊枪燃气或者其它能源,对于材料指的是材料使用数量\n如果工具不需要能源(比如剪线钳), 消耗数量写1即可, 不会有实际消耗")]
            public readonly struct 拆除所需的施工材料和工时数据
            {
                public readonly int 主手持有的该物品的PrefabHash;
                public readonly int 主手消耗数量;
                public readonly float 完成操作所需的进度条读条时长;

                public 拆除所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, float 完成操作所需的进度条读条时长) Arg_拆除)
                {
                    主手持有的该物品的PrefabHash = Arg_拆除.主手持有的该物品的PrefabHash;
                    主手消耗数量 = Arg_拆除.主手消耗数量;
                    完成操作所需的进度条读条时长 = Arg_拆除.完成操作所需的进度条读条时长;
                }

                // 隐式转换
                public static implicit operator 拆除所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, float 完成操作所需的进度条读条时长) Arg_拆除)
                {
                    return new 拆除所需的施工材料和工时数据(Arg_拆除);
                }
            }

            [Tooltip("修复举例一: 主手持有工具, 副手为空\n修复举例二: 主手持有工具, 副手为材料\n修复举例三: 主手持有材料, 副手为空\n消耗数量对于工具指的是电量或者焊枪燃气或者其它能源,对于材料指的是材料使用数量\n如果工具不需要能源(比如剪线钳), 消耗数量写1即可, 不会有实际消耗")]
            public readonly struct 修复所需的施工材料和工时数据
            {
                public readonly 装配所需的施工材料和工时数据 修复;
                public readonly 目标物体的修复施工组件查找器数据 目标物体;
                public 修复所需的施工材料和工时数据(装配所需的施工材料和工时数据 Arg_修复, 目标物体的修复施工组件查找器数据 Arg_目标物体)
                {
                    修复 = Arg_修复;
                    目标物体 = Arg_目标物体;
                }
                public 修复所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, int 副手持有的该物品的PrefabHash, int 副手消耗数量, float 完成操作所需的进度条读条时长) Arg_修复, (Structure 源资源_目标物体, int 目标物体PrefabHash) Arg_目标物体)
                {
                    修复 = Arg_修复;
                    目标物体 = Arg_目标物体;
                }
            }

            public enum 建筑结构状态
            {
                结构正常状态,
                结构损毁状态,
            }

            [Tooltip("游戏在加载Thing资源时, 会对源资源进行额外复制一份的操作, 然后进行一些改造, 然后在游戏中使用改造后的复制体, 因此直接修改源资源是没用的\n需要等待游戏资源加载完成才能查找到施工材料")]
            public readonly struct 目标物体的施工阶段组件查找器数据
            {
                [Tooltip("虽然源资源不会在游戏中使用, 但是给源资源也添加上施工材料和工时数据, 万一游戏又使用了源资源呢")]
                public readonly BuildState 源资源_目标物体的施工阶段组件;
                public readonly int 目标物体PrefabHash;

                [Tooltip("一个目标物体可以有多个施工阶段组件, 比如自动车床, 阶段一使用(套件)自动车床装配, 使用扳手拆除; 阶段二使用焊枪与铁板装配, 使用角磨机拆除; 阶段三使用电缆装配, 使用剪线钳拆除; 后续阶段省略......")]
                public readonly int 施工阶段索引;
                public readonly 建筑结构状态 施工阶段对应的建筑结构状态;

                public 目标物体的施工阶段组件查找器数据((BuildState 源资源_目标物体的施工阶段组件, int 目标物体PrefabHash, int 施工阶段索引, 建筑结构状态 施工阶段对应的建筑结构状态) Arg_查找器)
                {
                    源资源_目标物体的施工阶段组件 = Arg_查找器.源资源_目标物体的施工阶段组件;
                    目标物体PrefabHash = Arg_查找器.目标物体PrefabHash;
                    施工阶段索引 = Arg_查找器.施工阶段索引;
                    施工阶段对应的建筑结构状态 = Arg_查找器.施工阶段对应的建筑结构状态;
                }

                // 隐式转换
                public static implicit operator 目标物体的施工阶段组件查找器数据((BuildState 源资源_目标物体的施工阶段组件, int 目标物体PrefabHash, int 施工阶段索引, 建筑结构状态 施工阶段对应的建筑结构状态) Arg_查找器)
                {
                    return new 目标物体的施工阶段组件查找器数据(Arg_查找器);
                }
            }

            [Tooltip("游戏在加载Thing资源时, 会对源资源进行额外复制一份的操作, 然后进行一些改造, 然后在游戏中使用改造后的复制体, 因此直接修改源资源是没用的\n需要等待游戏资源加载完成才能查找到修复材料")]
            public readonly struct 目标物体的修复施工组件查找器数据
            {
                [Tooltip("虽然源资源不会在游戏中使用, 但是给源资源也添加上施工材料和工时数据, 万一游戏又使用了源资源呢")]
                public readonly Structure 源资源_目标物体;
                public readonly int 目标物体PrefabHash;

                public 目标物体的修复施工组件查找器数据((Structure 源资源_目标物体, int 目标物体PrefabHash) Arg_查找器)
                {
                    源资源_目标物体 = Arg_查找器.源资源_目标物体;
                    目标物体PrefabHash = Arg_查找器.目标物体PrefabHash;
                }

                // 隐式转换
                public static implicit operator 目标物体的修复施工组件查找器数据((Structure 源资源_目标物体, int 目标物体PrefabHash) Arg_查找器)
                {
                    return new 目标物体的修复施工组件查找器数据(Arg_查找器);
                }
            }
            public record 装配与拆除所需的施工材料和工时数据
            {
                public readonly 装配所需的施工材料和工时数据 装配;
                public readonly 拆除所需的施工材料和工时数据 拆除;
                public readonly 目标物体的施工阶段组件查找器数据 目标物体的施工阶段组件;
                public readonly ToolUseType 目标物体的项目建设性质;

                public 装配与拆除所需的施工材料和工时数据(装配所需的施工材料和工时数据 Arg_装配, 拆除所需的施工材料和工时数据 Arg_拆除, 目标物体的施工阶段组件查找器数据 Arg_目标物体的施工阶段组件, ToolUseType Arg_目标物体的项目建设性质 = ToolUseType.Construction)
                {
                    装配 = Arg_装配;
                    拆除 = Arg_拆除;
                    目标物体的施工阶段组件 = Arg_目标物体的施工阶段组件;
                    目标物体的项目建设性质 = Arg_目标物体的项目建设性质;
                }

                public 装配与拆除所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, int 副手持有的该物品的PrefabHash, int 副手消耗数量, float 完成操作所需的进度条读条时长) Arg_装配, (int 主手持有的该物品的PrefabHash, int 主手消耗数量, float 完成操作所需的进度条读条时长) Arg_拆除, (BuildState 源资源_目标物体的施工阶段组件, int 目标物体PrefabHash, int 施工阶段索引, 建筑结构状态 施工阶段对应的建筑结构状态) Arg_目标物体的施工阶段组件, ToolUseType Arg_目标物体的项目建设性质 = ToolUseType.Construction)
                {
                    装配 = Arg_装配;
                    拆除 = Arg_拆除;
                    目标物体的施工阶段组件 = Arg_目标物体的施工阶段组件;
                    目标物体的项目建设性质 = Arg_目标物体的项目建设性质;
                }
            }
        }

        public static void 添加槽位(this Thing thing, Slot.Class 槽位对应的道具类型, InteractableType 槽位对应的控件类型, BoxCollider 实体槽位的碰撞体 = null, string 指定NameID = null)
        {
            if (thing.Slots == null) { thing.Slots = new(); }
            var 槽位 = new Slot();
            thing.Slots.Add(槽位);

            var NameID = 指定NameID ?? Enum.GetName(typeof(Slot.Class), 槽位对应的道具类型);
            槽位.StringKey = NameID;
            槽位.StringHash = Animator.StringToHash(NameID);

            槽位.Parent = thing;

            槽位.IsInteractable = true;
            槽位.Action = 槽位对应的控件类型;       // 在Thing初始化时自动在Thing.Interactables和Thing.Slots中扫描, 并将控件与槽位关联(互相持有引用)

            槽位.Type = 槽位对应的道具类型;
            槽位.SlotTypeIcon = Slot.GetSlotTypeSprite(槽位对应的道具类型);
            const int 只能放入特定Thing_PrefabHash的道具 = 无效哈希;
            槽位.SpecificTypePrefabHash = 只能放入特定Thing_PrefabHash的道具;

            if (实体槽位的碰撞体)
            {
                槽位.Collider = 实体槽位的碰撞体;
                槽位.Size = 实体槽位的碰撞体.size;
                槽位.Location = 实体槽位的碰撞体.transform;
            }
            else
            {
                槽位.Collider = null;
                槽位.Size = default;
                槽位.Location = null;
            }

            槽位.HidesOccupant = true;          // 槽位物品会显示吗?
            槽位.OccupantCastsShadows = true;   // 槽位物品会投影吗?

            槽位.IsLocked = false;
            槽位.IsSwappable = true;

            槽位.RealWorldScale = false;         // 道具放入槽位后, 是否禁止<缩放以适应槽位的尺寸>, 即保持世界空间比例不变
            槽位.ScaleMultiplier = 1;           // 只有启用缩放时才生效, 但是如果实体槽位的Transform的缩放是1时, 此处写上1即可

            槽位.AllowDragging = false;         // DraggableThing是否可以放入槽位  例: 世界空间的氧气罐(DraggableThing)放入AllowDragging=true的双手槽位, 人物移动时, 氧气罐跟着走

            槽位.UseInternalAtmosphere = false; // 槽位是否使用其父级Thing的内部气体  例: 火箭客舱, 乘客需要呼吸
            槽位.EntityControlMode = MovementController.Mode.Seated;    // 乘客放入槽位后的姿态
            槽位.IsHiddenInSeat = false;        // 乘客放入槽位后的姿态是坐下时, 乘客可见吗? 
            槽位.OccupantAlwaysVisible = false;     // 乘客始终可见
        }

        public static void 添加控件(this Thing thing, InteractableType 控件类型, bool 是否创建UI按钮, BoxCollider 实体控件的碰撞体 = null, string 指定NameID = null, string 控件快捷键 = null)
        {
            if (thing.Interactables == null) { thing.Interactables = new(); }
            if (thing.Interactables.Any(t => t.Action == 控件类型)) { return; }
            var 控件 = new Interactable();
            thing.Interactables.Add(控件);

            var NameID = 指定NameID ?? Enum.GetName(typeof(InteractableType), 控件类型);
            控件.StringKey = NameID;
            控件.StringHash = Animator.StringToHash(NameID);
            控件.ActionName = NameID;

            控件.Parent = thing;

            控件.Action = 控件类型;

            if (实体控件的碰撞体)
            {
                控件.Collider = 实体控件的碰撞体;
                控件.FakeCollider = null;
                控件.Bounds = 实体控件的碰撞体.bounds;
                控件.OriginalBounds = 实体控件的碰撞体.bounds;
            }
            else
            {
                控件.Collider = null;
                控件.FakeCollider = null;
                控件.Bounds = default;
                控件.OriginalBounds = default;
            }

            if (thing.BaseAnimator)
            {
                控件.Animator = thing.BaseAnimator;
            }
            else
            {
                if (thing.TryGetComponent<Animator>(out var 动画管理组件))
                {
                    控件.Animator = 动画管理组件;
                }
                else
                {
                    控件.Animator = null;
                }
            }

            控件.JoinInProgressSync = true;
            控件.Layer = 0;

            if (实体控件的碰撞体) { return; }

            控件.CanKeyInteract = 是否创建UI按钮;       // 有些控件的状态由<进入和离开槽位>事件来变更,有些则是提供可点击按钮
            控件.KeyMap = 控件快捷键 == null ? string.Empty : 控件快捷键;
        }

        public enum 游戏内置物理运动启用条件类型
        {
            具有物理运动的道具_DynamicThing,
            仅具有静态碰撞体的建筑_Structure,
        }

        private static void 为蓝图添加高亮全息投影组件(GameObject Arg_由AssetBundle加载的空预制体资源_蓝图, string Arg_NameID, Mesh Arg_ThingMesh)
        {
            // 请在Unity编辑器中将多边形网格读写模式打开, <线框生成和子网格合并>会读取所有子网格并合并成一个新的多边形网格, 并遍历所有三角形生成线框绘制表(在Wireframe.OnRenderObject方法中遍历WireframeEdges并绘制)

            var 蓝图 = Arg_由AssetBundle加载的空预制体资源_蓝图;
            蓝图.name = Arg_NameID + "_Blueprint";

            var 多边形网格配置 = 蓝图.AddComponent<MeshFilter>();
            多边形网格配置.sharedMesh = Arg_ThingMesh;

            var 渲染配置 = 蓝图.AddComponent<MeshRenderer>();
            渲染配置.sharedMaterial = 材质_高亮全息投影_扫描线;

            var 线框生成和子网格合并 = new WireframeGenerator(蓝图.transform);      // 本级必须有MeshRenderer组件才会将MeshFilter视为有效
            var 已合并Mesh = 线框生成和子网格合并.CombinedMesh;
            多边形网格配置.sharedMesh = 已合并Mesh;

            var 线框绘制器 = 蓝图.AddComponent<Wireframe>();
            线框绘制器.WireframeEdges = 线框生成和子网格合并.Edges;       // 线框绘制表

            // 销毁时链式销毁
            线框绘制器.BlueprintTransform = 蓝图.transform;     // Wireframe是独立渲染的, 每次渲染前需要从蓝图transform中读取线框绘制坐标
            线框绘制器.BlueprintMeshFilter = 多边形网格配置;        // 由MeshRenderer负责渲染扫描线特效和表面颜色, Wireframe负责渲染线框
            线框绘制器.BlueprintRenderer = 渲染配置;               // Wireframe是独立渲染的, 每次渲染前需要从蓝图渲染配置的材质中读取颜色来配置线框绘制颜色
        }

        private static T 为实体添加基本组件<T>(GameObject Arg_由AssetBundle加载的空预制体资源_实体, string Arg_NameID, Mesh Arg_ThingMesh, Material[] Arg_所有subMesh材质, Sprite[] Arg_缩略图, 游戏内置喷漆色板.游戏内置喷漆色板12种颜色 Arg_默认颜色) where T : Thing
        {
            var 实体 = Arg_由AssetBundle加载的空预制体资源_实体;

            var 控制组件 = 实体.AddComponent<T>();

            控制组件.PrefabName = 控制组件.name = 实体.name = Arg_NameID;  // 这几个name必须一致, 因为游戏程序有时候使用Thing.PrefabName, 有时候使用UnityEngine.Object.name
            控制组件.PrefabHash = Animator.StringToHash(Arg_NameID);
            控制组件.CustomName = string.Empty;                     // thing.DisplayName 

            var 多边形网格配置 = 实体.AddComponent<MeshFilter>();
            多边形网格配置.sharedMesh = Arg_ThingMesh;

            var 碰撞体配置 = 实体.AddComponent<BoxCollider>();
            碰撞体配置.center = Arg_ThingMesh.bounds.center;
            碰撞体配置.size = Arg_ThingMesh.bounds.size;

            控制组件.ThingTransform = 实体.transform;
            尝试为实体添加运动组件(控制组件);

            // Thing.PaintableMaterial必须是12种内置喷漆材质的一种才会启用喷漆功能, 在游戏中进行喷漆时, 将UV纹理数组和缩略图数组切换到对应喷漆索引
            var 默认颜色ID = (int)Arg_默认颜色;
            控制组件.Thumbnail = Arg_缩略图?[默认颜色ID];
            控制组件.Thumbnails = Arg_缩略图;
            控制组件.PaintableMaterial = Singleton<GameManager>.Instance.CustomColors[默认颜色ID].Normal;

            if (GameManager.IsBatchMode) { return 控制组件; }    // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用

            var 渲染配置 = 实体.AddComponent<MeshRenderer>();
            渲染配置.sharedMaterials = Arg_所有subMesh材质;

            return 控制组件;
        }
        private static void 尝试为实体添加运动组件<T>(T Arg_控制组件) where T : Thing
        {
            if (Arg_控制组件 == null) { return; }
            if (Arg_控制组件 is not DynamicThing 道具) { return; }

            // [RequireComponent(typeof(Rigidbody))] public class DynamicThing, 道具类有该特性, 会自动添加刚体组件
            var 物理运动配置 = 道具.ThingTransform.GetOrAddComponent<Rigidbody>();
            物理运动配置.ResetInertiaTensor();
            道具.RigidBody = 物理运动配置;
        }
        public static T 创建Thing预制体并进行通用初始化<T>(GameObject Arg_由AssetBundle加载的空预制体资源_实体, GameObject Arg_由AssetBundle加载的空预制体资源_蓝图, string Arg_NameID, Mesh Arg_ThingMesh, Material[] Arg_所有subMesh材质, Sprite[] Arg_缩略图, 游戏内置喷漆色板.游戏内置喷漆色板12种颜色 Arg_默认颜色) where T : Thing
        {
            var 实体 = Arg_由AssetBundle加载的空预制体资源_实体;
            var 控制组件 = 为实体添加基本组件<T>(实体, Arg_NameID, Arg_ThingMesh, Arg_所有subMesh材质, Arg_缩略图, Arg_默认颜色);

            if (GameManager.IsBatchMode) { return 控制组件; }    // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用

            var 蓝图 = Arg_由AssetBundle加载的空预制体资源_蓝图;
            为蓝图添加高亮全息投影组件(蓝图, Arg_NameID, Arg_ThingMesh);
            控制组件.Blueprint = 蓝图;
            if (蓝图.TryGetComponent<Wireframe>(out var 线框绘制器)) { 控制组件.Wireframe = 线框绘制器; }                      // 放置蓝图时, 通过此引用, 修改蓝图渲染配置的颜色; 销毁实体时, 通过此引用, 销毁掉蓝图和Wireframe

            return 控制组件;
        }
    }
}
