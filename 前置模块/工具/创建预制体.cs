using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Assets.Scripts.Util;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using Reagents;
using System.Reflection;
using Assets.Scripts.Objects.Structures;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public static BuildState 创建施工阶段并进行通用初始化(Structure Arg_thing, MeshRenderer Arg_该施工阶段的渲染配置, (int 主手持有的该物品的PrefabHash, int 主手消耗数量, int 副手持有的该物品的PrefabHash, int 副手消耗数量, float 完成操作所需的进度条读条时长) Arg_装配, (int 主手持有的该物品的PrefabHash, int 主手消耗数量, float 完成操作所需的进度条读条时长) Arg_拆除, ToolUseType Arg_目标物体的项目建设性质 = ToolUseType.Construction)
        {
            var 施工阶段 = new BuildState
            {
                Visualizer = Arg_该施工阶段的渲染配置
            };

            // 直接绘制使用MeshRenderer渲染, 间接绘制会禁用MeshRenderer, 然后使用initialDrawData里的网格与材质并调用间接绘制API绘制
            Arg_thing.structureRenderMode = StructureRenderMode.Standard;
            HarmonyLib.Traverse.Create(施工阶段).Field("initialDrawData").SetValue(new Rendering.DrawData() { mesh = null, materials = null, shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off, });
            施工阶段.RenderMode = BuildStateRenderMode.OnMineAndPreviousStates;

            var 如何装拆 = new 施工材料和工时数据.装配与拆除所需的施工材料和工时数据(Arg_装配, Arg_拆除, 施工阶段, Arg_目标物体的项目建设性质);
            施工材料和工时数据.为目标物体的施工阶段组件添加施工材料和工时数据(如何装拆);

            return 施工阶段;
        }

        public static readonly int 壁灯哈希 = Animator.StringToHash("StructureWallLight");
        public const BindingFlags 私有字段匹配条件 = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        public static (Light 禁用阴影的地形图层专用灯光配置, Light 灯光配置, LensFlare 眩光配置) 复制原版游戏壁灯的灯光层级_此处仅仅是增加光源_灯泡网格的自发光需要单独配置<T>(T 挂墙小网格建筑, Vector3 光源位置_相对于父级轴心点, Vector3 眩光光源位置_相对于父级轴心点) where T : WallLight
        {
            Light 禁用阴影的地形图层专用灯光配置 = null;
            Light 灯光配置 = null;
            LensFlare 眩光配置 = null;

            // 光源
            var 类型指针 = typeof(WallLight);
            var 灯光 = 类型指针.GetField("light", 私有字段匹配条件);
            var 眩光 = 类型指针.GetField("lodFlare", 私有字段匹配条件);
            var 禁用阴影的地形图层专用灯光 = 类型指针.GetField("terrainLight", 私有字段匹配条件);

            // 会自动扫描所有灯光配置并添加到<可装配.Lights>, 在喷漆时改变灯光颜色
            var 原版_壁灯 = 施工材料和工时数据.查找施工材料<WallLight>(壁灯哈希);
            if (原版_壁灯)
            {
                var 原版_禁用阴影的地形图层专用灯光子层级 = 原版_壁灯.ThingTransform.Find("TerrainLight");
                if (原版_禁用阴影的地形图层专用灯光子层级)
                {
                    var 禁用阴影的地形图层专用灯光子层级 = UnityEngine.Object.Instantiate(原版_禁用阴影的地形图层专用灯光子层级, 挂墙小网格建筑.ThingTransform, worldPositionStays: false);
                    禁用阴影的地形图层专用灯光子层级.name = "禁用阴影的地形图层专用灯光子层级";
                    禁用阴影的地形图层专用灯光子层级.transform.localPosition = 光源位置_相对于父级轴心点;

                    禁用阴影的地形图层专用灯光配置 = 禁用阴影的地形图层专用灯光子层级.GetComponent<Light>();
                    if (禁用阴影的地形图层专用灯光配置)
                    {
                        禁用阴影的地形图层专用灯光.SetValue(挂墙小网格建筑, 禁用阴影的地形图层专用灯光配置);
                        禁用阴影的地形图层专用灯光配置.intensity = 0.2f;
                    }
                }

                var 原版_灯光子层级 = 原版_壁灯.ThingTransform.Find("Light");
                if (原版_灯光子层级)
                {
                    var 灯光子层级 = UnityEngine.Object.Instantiate(原版_灯光子层级, 挂墙小网格建筑.ThingTransform, worldPositionStays: false);
                    灯光子层级.name = "灯光子层级";
                    灯光子层级.transform.localPosition = 光源位置_相对于父级轴心点;

                    灯光配置 = 灯光子层级.GetComponent<Light>();
                    if (灯光配置)
                    {
                        灯光.SetValue(挂墙小网格建筑, 灯光配置);
                        灯光配置.intensity = 0.2f;
                    }

                    var 眩光子层级 = 灯光子层级.Find("lodFlare");
                    if (眩光子层级)
                    {
                        眩光子层级.name = "眩光子层级";
                        眩光子层级.transform.localPosition = 眩光光源位置_相对于父级轴心点;

                        眩光配置 = 眩光子层级.GetComponent<LensFlare>();
                        if (眩光配置)
                        {
                            眩光.SetValue(挂墙小网格建筑, 眩光配置);
                            眩光配置.brightness = 0.3f;
                        }
                    }
                }
            }

            return (禁用阴影的地形图层专用灯光配置, 灯光配置, 眩光配置);
        }

        public static (Light 禁用阴影的地形图层专用灯光配置, Light 灯光配置, LensFlare 眩光配置) 添加灯光<T>(T 挂墙小网格建筑, Mesh 自发光的灯泡, string 自发光的灯泡层级的名称, Vector3 光源位置_相对于父级轴心点, Vector3 眩光光源位置_相对于父级轴心点, Vector3 电源接口位置_相对于父级轴心点, float 耗电量) where T : WallLight
        {
            {
                // 电源接口
                挂墙小网格建筑.UsedPower = 耗电量;
                挂墙小网格建筑.添加控件(InteractableType.Powered, 是否创建UI按钮: false);
                挂墙小网格建筑.添加接口(电源接口位置_相对于父级轴心点, NetworkType.PowerAndData, ConnectionRole.None);
            }

            {
                // 电源开关
                var 电源开关子层级 = new GameObject("电源开关子层级");
                电源开关子层级.transform.SetParent(挂墙小网格建筑.ThingTransform, worldPositionStays: false);

                var 碰撞配置 = 电源开关子层级.AddComponent<BoxCollider>();
                碰撞配置.center = 自发光的灯泡.bounds.center;
                碰撞配置.size = 自发光的灯泡.bounds.size;

                挂墙小网格建筑.添加控件(InteractableType.OnOff, 是否创建UI按钮: false, 实体控件的碰撞体: 碰撞配置);
            }

            {
                // 自发光灯泡模型
                var 灯泡子层级 = new GameObject(自发光的灯泡层级的名称);
                灯泡子层级.transform.SetParent(挂墙小网格建筑.ThingTransform, worldPositionStays: false);

                灯泡子层级.AddComponent<MeshFilter>().sharedMesh = 自发光的灯泡;

                if (!GameManager.IsBatchMode)
                {
                    灯泡子层级.AddComponent<MeshRenderer>().sharedMaterial = 游戏内置喷漆颜色.游戏内置喷漆材质;
                }
            }

            // 增加光源
            return 复制原版游戏壁灯的灯光层级_此处仅仅是增加光源_灯泡网格的自发光需要单独配置(挂墙小网格建筑, 光源位置_相对于父级轴心点, 眩光光源位置_相对于父级轴心点); ;
        }

        public static void 为挂墙小网格建筑的碰撞图层与旋转方式进行通用初始化<T>(T 挂墙小网格建筑) where T : SmallGrid, ISmartRotatable
        {
            // 射线命中时的高亮选择框的显示尺寸是刚好一个小网格大小, 还是所有网格模型的包围盒大小, 仅仅是渲染效果, 实际对齐还是对齐到小网格的
            挂墙小网格建筑.SelectionDisplay = SelectionHighlightMethod.Bounds;

            // 框架、自动车床、门....检测碰撞时需要与所有覆盖的大、小网格比较, 判断是否可放置    
            // 墙体只需要对齐到大网格, 并根据位置计算出自己位于大网格的哪个方向(东南西北上下), 比较大网格指定方向是否可放置即可, 因此很多小网格建筑都可以穿墙放置 
            挂墙小网格建筑.StructureCollisionType = CollisionType.BlockCustom;

            // Grid:框架、自动车床、门....  Face:墙体(放置时对齐到2米尺寸网格的东、南、西、北、上、下六个面)  FaceMount:必须放置在墙体和框架上的物体
            挂墙小网格建筑.PlacementType = PlacementSnap.FaceMount;

            // 智能旋转预存了多种旋转模板, 描述下一个旋转轴和下一个旋转方向, 用于优化掉不必要的旋转   例: 十字电缆就不需要滚转旋转
            挂墙小网格建筑.SetConnectionType(SmartRotate.ConnectionType.FlatExhaustive);

            // 俯仰旋转、滚转旋转、偏航旋转  例: XY的意思是支持两种旋转方式, 其中X代表俯仰旋转(上下旋转), Y代表偏航旋转(左右旋转)   例:Z的意思是滚转旋转
            挂墙小网格建筑.RotationAxis = RotationAxis.Z;

            // 允许旋转的地方, 分别是墙体上、天花板上、地板上、天花板和地板上、墙体和天花板和地板上
            挂墙小网格建筑.AllowedRotations = AllowedRotations.All;

            // 小网格尺寸0.5, 所有建筑在放置时都是对齐到小网格坐标的, 因此建筑的碰撞尺寸必须是小网格尺寸的整数倍
            // 大网格尺寸2, 对齐到小网格时的布局分布为 0.25(半个小网格)/0.5/0.5/0.5/0.25(半个小网格), 即大网格中心有9个小网格, 四个边缘各有3个(半个小网格)
            挂墙小网格建筑.GridSize = SmallGrid.SmallGridSize;

            // 两个大网格之间的边缘处各有0.25格, 组合起来才够一个小网格, 因此假如大网格坐标为0, 小网格坐标就要为-0.25, 这样建筑放置时, 从负0.25处开始判断碰撞, 从0.25处结束碰撞, 不会与大网格中间的9个小网格冲突, 正好利用上了边缘
            // 直线电缆的碰撞尺寸为一个小网格, 但是直线电缆的渲染尺寸中宽度只有0.1, 因此放置在两个大网格之间的边缘处视觉效果良好, 如果边缘处放置了墙体, 同时又放置了渲染尺寸接近碰撞尺寸的物体, 就会出现穿模
            挂墙小网格建筑.GridOffset = SmallGrid.SmallGridOffset;

            // 管道、电线、设备、机械臂轨道(滑槽好像属于轨道), 即除了装饰面板外, 放置时都进行碰撞判断
            挂墙小网格建筑.SmallCollisionType = SmallGridBlock.PipesCablesAndDevices | SmallGridBlock.Rails;
        }

        [Tooltip("合并多个网格  注:一个模型有多个网格时, 将所有网格合并为一个, 如果该模型所有网格使用同一个材质, 就额外将所有三角形表合并为一个(不保留子网格)")]
        public static (Mesh 已合并Mesh, Material[] 所有subMesh材质) 合并多边形网格(Mesh[] Arg_所有Mesh, Material[] Arg_所有subMesh材质, string Arg_物体名称, bool Arg_保留子网格么 = true)
        {
            if (Arg_所有Mesh == null || Arg_所有Mesh.Length == 0 || Arg_所有subMesh材质 == null || Arg_所有subMesh材质.Length == 0)
            {
                前置模块.Log.LogError("传入的Mesh[]或材质[]数组为空, 无法合并多边形网格");
                return (null, null);
            }

            if (Arg_所有Mesh.Any(d => d.uv.Length <= 0))
            {
                前置模块.Log.LogError("传入的Mesh[]中存在网格缺少UV布局, 无法映射到材质中的UV纹理, 无法合并多边形网格");
                return (null, null);
            }

            for (var i = 0; i < Arg_所有Mesh.Length; i++)
            {
                if (Arg_所有Mesh[i].subMeshCount <= 0)
                {
                    前置模块.Log.LogError("传入的Mesh[]中存在subMeshCount=0的空网格(三角形面=0), 无法合并多边形网格");
                    return (null, null);
                }
                else if (Arg_所有Mesh[i].subMeshCount != 1)
                {
                    前置模块.Log.LogMessage("该网格在建模软件中导出时, 未清理材质信息, 导致网格中存在多个三角形表, 正在合并三角形表");
                    打印子网格计数(Arg_所有Mesh[i], Arg_物体名称);
                    Arg_所有Mesh[i] = 合并多边形网格(Arg_所有Mesh[i]);
                    前置模块.Log.LogMessage("合并三角形表完成");
                    打印子网格计数(Arg_所有Mesh[i], Arg_物体名称);
                }
            }

            if (Arg_保留子网格么)
            {
                if (Arg_所有Mesh.Length != Arg_所有subMesh材质.Length)
                {
                    前置模块.Log.LogError("传入的Mesh[]和材质[]的元素数不一致, 无法合并多边形网格");
                    return (null, null);
                }
            }
            else if (Arg_所有subMesh材质.Length != 1)
            {
                前置模块.Log.LogError("传入的材质[]元素数不为1, 无法合并多边形网格");
                return (null, null);
            }

            var Result = 合并多边形网格(Arg_所有Mesh, Arg_物体名称, Arg_保留子网格么);
            return (Result, Arg_所有subMesh材质);
        }

        [Tooltip("合并多个网格  注:一个模型由多个网格组成, Unity引擎的渲染组件只支持一个网格和多个材质, 因此需要将多个网格合并为一个网格和多个三角形表, 这样每种材质都对应着各自的所有三角形, 材质数量和三角形表数量要一致")]
        public static Mesh 合并多边形网格(Mesh[] Arg_所有Mesh, string Arg_物体名称, bool Arg_保留子网格么 = true)
        {
            if (Arg_所有Mesh == null || Arg_所有Mesh.Length == 0)
            {
                前置模块.Log.LogError("传入的Mesh[]为空, 无法合并多边形网格");
                return null;
            }

            if (Arg_所有Mesh.Any(d => d.subMeshCount != 1))
            {
                前置模块.Log.LogError("传入的Mesh[]存在subMeshCount不为1的网格, 无法合并多边形网格");
                return null;
            }

            if (Arg_所有Mesh.Any(d => d.uv.Length <= 0))
            {
                前置模块.Log.LogError("传入的Mesh[]中存在网格缺少UV布局, 无法映射到材质中的UV纹理, 无法合并多边形网格");
                return null;
            }

            var 待合并 = new List<CombineInstance>(Arg_所有Mesh.Length);
            for (var i = 0; i < Arg_所有Mesh.Length; i++)
            {
                待合并.Add(new CombineInstance
                {
                    mesh = Arg_所有Mesh[i],
                    subMeshIndex = 0,
                    transform = Matrix4x4.identity
                });
            }

            var Result = new Mesh() { name = Arg_物体名称 + "已合并" };
            Result.CombineMeshes(待合并.ToArray(), mergeSubMeshes: !Arg_保留子网格么, useMatrices: true);
            Result.RecalculateNormals();
            Result.RecalculateBounds();
            return Result;
        }

        [Tooltip("合并同一个网格内的所有子网格  注:一个模型由多个网格组成, 每个网格分配一个材质即可, 因此每个网格只需要一个子网格  注: 子网格就是将网格中的N个三角形划分到单独的一个三角形表中, 然后绘制时就可以连续取三角形, 材质数量和三角形表数量要一致")]
        public static Mesh 合并多边形网格(Mesh Arg_Mesh)
        {
            const bool 保留子网格么 = false;

            if (Arg_Mesh == null)
            {
                前置模块.Log.LogError("传入的Mesh为空, 无法合并子网格");
                return null;
            }

            if (Arg_Mesh.uv.Length <= 0)
            {
                前置模块.Log.LogError("传入的Mesh缺少UV布局, 无法映射到材质中的UV纹理, 无法合并多边形网格");
                return null;
            }

            var 待合并 = new List<CombineInstance>(Arg_Mesh.subMeshCount);
            for (var 子网格编号 = 0; 子网格编号 < Arg_Mesh.subMeshCount; 子网格编号++)
            {
                待合并.Add(new CombineInstance
                {
                    mesh = Arg_Mesh,
                    subMeshIndex = 子网格编号,
                    transform = Matrix4x4.identity
                });
            }

            var Result = new Mesh() { name = Arg_Mesh.name + "已合并" };
            Result.CombineMeshes(待合并.ToArray(), mergeSubMeshes: !保留子网格么, useMatrices: true);
            Result.RecalculateNormals();
            Result.RecalculateBounds();
            return Result;
        }

        public static void 打印子网格计数(Mesh[] Arg_所有Mesh, string 物体名称)
        {
            if (Arg_所有Mesh == null)
            {
                前置模块.Log.LogError("传入的Mesh[]为空, 无法打印子网格计数");
                return;
            }

            int 索引 = 0;
            前置模块.Log.LogMessage($"模型名称: {物体名称}\n{string.Join("\n", Arg_所有Mesh.Select(d => $"网格名称: {d.name}  网格索引: {索引++}  子网格计数: {d.subMeshCount}"))}");
        }

        public static void 打印子网格计数(Mesh Arg_Mesh, string 物体名称)
        {
            if (Arg_Mesh == null)
            {
                前置模块.Log.LogError("传入的Mesh为空, 无法打印子网格计数");
                return;
            }

            前置模块.Log.LogMessage($"模型名称: {物体名称}  网格名称: {Arg_Mesh.name}  子网格计数: {Arg_Mesh.subMeshCount}");
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
            public static void 为目标物体的施工阶段组件添加施工材料和工时数据(装配与拆除所需的施工材料和工时数据 数据)
            {
                if (数据.目标物体的施工阶段组件 == null) { return; }

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

                数据.目标物体的施工阶段组件.Tool = 本施工阶段的施工材料和工时数据;

                前置模块.Log.LogMessage($"{数据.目标物体的施工阶段组件}成功添加施工材料和工时数据\n{本施工阶段的施工材料和工时数据.GetToolsAsString()}");
            }
            public static void 为目标物体添加修复结构所需的施工材料和工时数据(修复所需的施工材料和工时数据 数据)
            {
                if (数据.目标物体 == null) { return; }

                var 本施工阶段的施工材料和工时数据 = new ToolBasic()
                {
                    ToolEntry = 数据.修复.主手持有的该物品的PrefabHash == 0 ? null : 查找施工材料<Item>(数据.修复.主手持有的该物品的PrefabHash),
                    EntryQuantity = 数据.修复.主手消耗数量,
                    ToolEntry2 = 数据.修复.副手持有的该物品的PrefabHash == 0 ? null : 查找施工材料<Item>(数据.修复.副手持有的该物品的PrefabHash),
                    EntryQuantity2 = 数据.修复.副手消耗数量,
                    EntryTime = 数据.修复.完成操作所需的进度条读条时长,
                };

                数据.目标物体.RepairTools = 本施工阶段的施工材料和工时数据;

                前置模块.Log.LogMessage($"{数据.目标物体.DisplayName}成功添加修复材料和工时数据\n{本施工阶段的施工材料和工时数据.GetRepairsAsString()}");
            }

            public static void 添加由模组扩展的施工材料(Thing thing)
            {
                if (!已发现施工材料缓存.ContainsKey(thing.PrefabHash))
                {
                    已发现施工材料缓存[thing.PrefabHash] = thing;
                }
            }

            public static T 查找施工材料<T>(int PrefabHash) where T : Thing
            {
                if (已发现施工材料缓存.TryGetValue(PrefabHash, out var 匹配)) { return (T)匹配; }

                var 索引 = WorldManager.Instance.SourcePrefabs.FindIndex(d => d.PrefabHash == PrefabHash && d.ReferenceId == 0);
                if (索引 >= 0)
                {
                    匹配 = WorldManager.Instance.SourcePrefabs[索引];
                    已发现施工材料缓存[PrefabHash] = 匹配;
                    return (T)匹配;
                }

                return null;
            }

            public enum 施工阶段组件工具提示
            {
                结构正常状态, 结构正常状态_所有施工阶段数组, 结构正常状态_施工阶段索引,
                结构损毁状态, 结构损毁状态_所有施工阶段数组_为了复用使用了数组_实际上损毁状态只需要一个施工阶段, 结构损毁状态_施工阶段索引,
                结构正常状态_建筑的生命值不是满值_修复建筑施工阶段
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
                public readonly Structure 目标物体;
                public 修复所需的施工材料和工时数据(装配所需的施工材料和工时数据 Arg_修复, Structure Arg_目标物体)
                {
                    修复 = Arg_修复;
                    目标物体 = Arg_目标物体;
                }
                public 修复所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, int 副手持有的该物品的PrefabHash, int 副手消耗数量, float 完成操作所需的进度条读条时长) Arg_修复, Structure Arg_目标物体)
                {
                    修复 = Arg_修复;
                    目标物体 = Arg_目标物体;
                }
            }

            public record 装配与拆除所需的施工材料和工时数据
            {
                public readonly 装配所需的施工材料和工时数据 装配;
                public readonly 拆除所需的施工材料和工时数据 拆除;
                public readonly BuildState 目标物体的施工阶段组件;
                public readonly ToolUseType 目标物体的项目建设性质;

                public 装配与拆除所需的施工材料和工时数据(装配所需的施工材料和工时数据 Arg_装配, 拆除所需的施工材料和工时数据 Arg_拆除, BuildState Arg_目标物体的施工阶段组件, ToolUseType Arg_目标物体的项目建设性质 = ToolUseType.Construction)
                {
                    装配 = Arg_装配;
                    拆除 = Arg_拆除;
                    目标物体的施工阶段组件 = Arg_目标物体的施工阶段组件;
                    目标物体的项目建设性质 = Arg_目标物体的项目建设性质;
                }

                public 装配与拆除所需的施工材料和工时数据((int 主手持有的该物品的PrefabHash, int 主手消耗数量, int 副手持有的该物品的PrefabHash, int 副手消耗数量, float 完成操作所需的进度条读条时长) Arg_装配, (int 主手持有的该物品的PrefabHash, int 主手消耗数量, float 完成操作所需的进度条读条时长) Arg_拆除, BuildState Arg_目标物体的施工阶段组件, ToolUseType Arg_目标物体的项目建设性质 = ToolUseType.Construction)
                {
                    装配 = Arg_装配;
                    拆除 = Arg_拆除;
                    目标物体的施工阶段组件 = Arg_目标物体的施工阶段组件;
                    目标物体的项目建设性质 = Arg_目标物体的项目建设性质;
                }
            }
        }

        [Tooltip("一个建筑可以有多个子层级, 每个子层级可以有不同的渲染模型和接口, 在建筑的不同阶段激活对应的子层级(例: 框架阶段子层级只有模型, 其它接口/实体按键都是隐藏的; 在完工阶段, 接口和实体按键则激活, 参与交互)")]
        public static Connection 添加接口(this SmallGrid 接口所属的建筑, Vector3 接口位置_相对于父级轴心点, NetworkType 接口类型, ConnectionRole 接口通道)
        {
            var 接口子层级 = new GameObject($"接口类型: {接口类型}  接口通道: {接口通道}  每个接口都需要独立的变换组件描述位置");
            接口子层级.transform.SetParent(接口所属的建筑.ThingTransform, worldPositionStays: false);

            var 球形碰撞体 = 接口子层级.AddComponent<SphereCollider>();

            球形碰撞体.radius = 0.05f;
            球形碰撞体.transform.localPosition = 接口位置_相对于父级轴心点;

            const bool 禁用Unity引擎内置物理碰撞功能_避免出现空气墙 = true;
            球形碰撞体.isTrigger = 禁用Unity引擎内置物理碰撞功能_避免出现空气墙;

            var 所有接口 = 接口所属的建筑.OpenEnds;

            var 新接口 = new Connection(接口所属的建筑)
            {
                ConnectionType = 接口类型,
                Transform = 球形碰撞体.transform,
                Collider = 球形碰撞体,
                ConnectionRole = 接口通道,
            };

            所有接口.Add(新接口);

            return 新接口;
        }

        public static Slot 添加槽位(this Thing thing, Slot.Class 槽位对应的道具类型, InteractableType 槽位对应的控件类型, BoxCollider 实体槽位的碰撞体 = null, string 指定NameID = null, int[] 槽位对应的所有结构哈希 = null)
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

            bool 该槽位是否只能存放特定Thing_PrefabHash的道具 = 槽位对应的所有结构哈希 != null && 槽位对应的所有结构哈希.Length > 0;
            if (该槽位是否只能存放特定Thing_PrefabHash的道具)
            {
                槽位.SpecificTypePrefabHashes = 槽位对应的所有结构哈希;
            }

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

            return 槽位;
        }

        public static Interactable 添加控件(this Thing thing, InteractableType 控件类型, bool 是否创建UI按钮, BoxCollider 实体控件的碰撞体 = null, string 指定NameID = null, string 控件快捷键 = null)
        {
            if (thing.Interactables == null) { thing.Interactables = new(); }

            var 已存在 = thing.Interactables.Find(t => t.Action == 控件类型);
            if (已存在 != null) { return 已存在; }

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
                控件.OriginalBounds = new Bounds(实体控件的碰撞体.center, 实体控件的碰撞体.size);
                控件.Bounds = 控件.OriginalBounds;  // 这两个包围盒互为缓存, 在不同方法中会覆盖掉另一方 例: Thing.SetupInteractables
            }
            else
            {
                控件.Collider = null;
                控件.FakeCollider = null;
                控件.OriginalBounds = default;
                控件.Bounds = 控件.OriginalBounds;
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

            if (实体控件的碰撞体)
            {
                控件.CanKeyInteract = false;       // 有些控件的状态由<进入和离开槽位>事件来变更,有些则是提供可点击按钮
                控件.KeyMap = string.Empty;
            }
            else
            {
                控件.CanKeyInteract = 是否创建UI按钮;       // 有些控件的状态由<进入和离开槽位>事件来变更,有些则是提供可点击按钮
                控件.KeyMap = 控件快捷键 == null ? string.Empty : 控件快捷键;
            }

            return 控件;
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

        private static T 为实体添加基本组件<T>(GameObject Arg_由AssetBundle加载的空预制体资源_实体, string Arg_NameID, Mesh Arg_ThingMesh, Material[] Arg_所有subMesh材质, Sprite[] Arg_缩略图, 游戏内置喷漆颜色.色板 Arg_默认颜色) where T : Thing
        {
            var 实体 = Arg_由AssetBundle加载的空预制体资源_实体;

            var 控制组件 = 实体.AddComponent<T>();
            控制组件.ThingTransform = 实体.transform;

            控制组件.PrefabName = 控制组件.name = 实体.name = Arg_NameID;  // 这几个name必须一致, 因为游戏程序有时候使用Thing.PrefabName, 有时候使用UnityEngine.Object.name
            控制组件.PrefabHash = Animator.StringToHash(Arg_NameID);
            控制组件.CustomName = string.Empty;                     // thing.DisplayName 

            var 多边形网格配置 = 实体.AddComponent<MeshFilter>();
            多边形网格配置.sharedMesh = Arg_ThingMesh;

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
            if (Arg_控制组件 && Arg_控制组件 is DynamicThing 道具)
            {
                var 多边形网格必须存在才能参与实体交互 = 道具.ThingTransform.GetComponent<MeshFilter>();
                if (多边形网格必须存在才能参与实体交互)
                {
                    var Arg_ThingMesh = 多边形网格必须存在才能参与实体交互.sharedMesh;

                    var 碰撞配置 = 道具.ThingTransform.GetOrAddComponent<BoxCollider>();
                    碰撞配置.center = Arg_ThingMesh.bounds.center;
                    碰撞配置.size = Arg_ThingMesh.bounds.size;

                    // [RequireComponent(typeof(Rigidbody))] public class DynamicThing, 道具类有该特性, 会自动添加刚体组件
                    var 运动配置 = 道具.ThingTransform.GetOrAddComponent<Rigidbody>();
                    运动配置.ResetInertiaTensor();
                    道具.RigidBody = 运动配置;
                }
            }
        }
        public static T 创建Thing预制体并进行通用初始化<T>(GameObject Arg_由AssetBundle加载的空预制体资源_实体, GameObject Arg_由AssetBundle加载的空预制体资源_蓝图, string Arg_NameID, Mesh Arg_ThingMesh, Material[] Arg_所有subMesh材质, Sprite[] Arg_缩略图, 游戏内置喷漆颜色.色板 Arg_默认颜色) where T : DynamicThing
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

        public static T 创建Thing预制体并进行通用初始化<T>(GameObject Arg_由AssetBundle加载的空预制体资源_实体, GameObject Arg_由AssetBundle加载的空预制体资源_蓝图, string Arg_NameID, Mesh Arg_模块化模型的部件1网格, Material Arg_模块化模型的部件1材质, Mesh Arg_模块化模型的所有部件网格_已合并Mesh, Sprite[] Arg_缩略图, 游戏内置喷漆颜色.色板 Arg_默认颜色) where T : Structure
        {
            var 实体 = Arg_由AssetBundle加载的空预制体资源_实体;
            var 控制组件 = 为实体添加基本组件<T>(实体, Arg_NameID, Arg_模块化模型的部件1网格, [Arg_模块化模型的部件1材质], Arg_缩略图, Arg_默认颜色);

            if (GameManager.IsBatchMode) { return 控制组件; }    // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用

            var 蓝图 = Arg_由AssetBundle加载的空预制体资源_蓝图;
            为蓝图添加高亮全息投影组件(蓝图, Arg_NameID, Arg_模块化模型的所有部件网格_已合并Mesh);
            控制组件.Blueprint = 蓝图;
            if (蓝图.TryGetComponent<Wireframe>(out var 线框绘制器)) { 控制组件.Wireframe = 线框绘制器; }                      // 放置蓝图时, 通过此引用, 修改蓝图渲染配置的颜色; 销毁实体时, 通过此引用, 销毁掉蓝图和Wireframe

            return 控制组件;
        }

        private static GameObject m_休眠的预制体根节点 = null;
        public static GameObject 休眠的预制体根节点
        {
            get
            {
                if (m_休眠的预制体根节点 == null)
                {
                    m_休眠的预制体根节点 = new GameObject("m_休眠的预制体根节点");
                    变更激活状态(m_休眠的预制体根节点, false);
                    UnityEngine.Object.DontDestroyOnLoad(m_休眠的预制体根节点);
                }
                return m_休眠的预制体根节点;
            }
        }

        public static GameObject 创建新的空预制体()
        {
            var 父级 = 休眠的预制体根节点;
            var New = new GameObject();
            New.transform.SetParent(父级.transform, worldPositionStays: false);
            return New;
        }
    }
}
