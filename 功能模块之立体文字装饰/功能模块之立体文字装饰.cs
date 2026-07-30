using Assets.Scripts.Objects;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using Objects.Items;
using Reagents;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_litiwenzizhuangshi", "功能模块之立体文字装饰", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之立体文字装饰 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之立体文字装饰加载完成!");
            补丁 = new Harmony("功能模块之立体文字装饰");
            补丁.PatchAll();
        }

        [Tooltip("注: 创建新接口的操作是, 先画一个0.5*0.5*0.5的小网格立方体阵列, 接口位置就是每个小网格的四条边中心点(例: 十字电缆放置后刚好匹配四条边中心点), 总之就是建模可以随便画, 接口一定要定位在小网格的四条边中心点上")]
        public static MultiConstructor 创建_套件_立体文字装饰()
        {
            var 资源加载器 = 功能模块之立体文字装饰_资源加载器.单例;
            var 网格与材质 = 资源加载器.套件多边形网格与材质;

            const string NameID = "ItemKitLiTiWenZiZhuangShi";
            const 通用工具.游戏内置喷漆色板.游戏内置喷漆色板12种颜色 默认颜色 = 通用工具.游戏内置喷漆色板.游戏内置喷漆色板12种颜色.橙色Orange;

            // MultiConstructor: 可以放进背包的装配包道具, 一个装配包可以放置多个建筑
            var 装配包 = 通用工具.创建Thing预制体并进行通用初始化<MultiConstructor>(资源加载器.套件预制体.实体, 资源加载器.套件预制体.蓝图, NameID, 网格与材质.已合并Mesh, 网格与材质.所有subMesh材质, 资源加载器.套件纹理.对应不同喷漆颜色的缩略图, 默认颜色);
            // 装配包.PaintableMaterial = null;  // 如果需要喷漆, 则不变, 不要喷漆, 则用null覆盖

            const Slot.Class 道具可放入的槽位_每个槽位有对应的道具类型 = Slot.Class.None;
            const SortingClass 道具的分拣大类 = SortingClass.Kits;
            const bool 会腐烂么 = false;
            const float 腐烂速度 = 0;
            const DecayedFood 腐烂后转换成的物品预制体 = null;
            const int 出厂数量 = 1;
            const int 最大堆垛数量 = 20;
            const float 闪点温度 = 373.15f;
            const float 自燃温度 = 573.15f;

            装配包.SlotType = 道具可放入的槽位_每个槽位有对应的道具类型;
            装配包.SortingClass = 道具的分拣大类;

            装配包.CanDecay = 会腐烂么;                               // 如果可以腐烂,会添加到腐烂管理器中,每帧计算腐烂情况
            装配包.DecayRate = 腐烂速度;                              // 腐烂速度
            装配包.DecayedFoodPrefab = 腐烂后转换成的物品预制体;       // 腐烂后销毁原物品,然后在原地生成这个腐烂物品,使用原版的腐烂物品预制体就行

            装配包.Quantity = 出厂数量;
            装配包.MaxQuantity = 最大堆垛数量;

            var _1单位成分数据 = new Recipe { Iron = 2 };
            通用工具.为目标物体添加试剂成分表_每QuantityPerUse单位(装配包, _1单位成分数据);

            装配包.flashpointTemperature = 闪点温度;
            装配包.autoignitionTemperature = 自燃温度;

            装配包.添加控件(InteractableType.Button1, 是否创建UI按钮: true, 指定NameID: "SplitOne", 实体控件的碰撞体: null, 控件快捷键: string.Empty);        // 拆分一个
            装配包.添加控件(InteractableType.Button2, 是否创建UI按钮: true, 指定NameID: "SplitHalf", 实体控件的碰撞体: null, 控件快捷键: string.Empty);             // 拆分一半

            var 所有可装配的建筑 = 装配包.Constructables;

            int 撬棍的PrefabHash = Animator.StringToHash("ItemCrowbar");

            for (var i = 0; i < 资源加载器.所有可装配预制体.Count; i++)
            {
                (GameObject 实体预制体, GameObject 蓝图预制体) = 资源加载器.所有可装配预制体[i];
                (Mesh 已合并Mesh, Material[] 所有subMesh材质) = 资源加载器.所有可装配多边形网格与材质[i];

                var 可装配 = 通用工具.创建Thing预制体并进行通用初始化<立体文字装饰>(实体预制体, 蓝图预制体, "SmallGridStructureLiTiWenZiZhuangShi" + i, 已合并Mesh, 所有subMesh材质, 资源加载器.套件纹理.对应不同喷漆颜色的缩略图, 默认颜色);
                所有可装配的建筑.Add(可装配);

                switch (i)
                {
                    case 0:     // 立体文字装饰 平面0.5X0.5
                        {
                            var 数据与电力接口子级 = new GameObject();
                            数据与电力接口子级.transform.SetParent(可装配.ThingTransform, false);
                            var 球形碰撞体 = 数据与电力接口子级.AddComponent<SphereCollider>();
                            球形碰撞体.radius = 0.1f;
                            球形碰撞体.transform.localPosition = new(0, 0, -0.2f);

                            var 可装配的所有接口 = 可装配.OpenEnds;
                            var 新接口 = new Connection(可装配)
                            {
                                ConnectionType = NetworkType.PowerAndData,
                                Transform = 球形碰撞体.transform,
                                Collider = 球形碰撞体,
                                ConnectionRole = ConnectionRole.None,
                            };
                            可装配的所有接口.Add(新接口);

                            break;
                        }
                    case 1:     // 居住     平面0.5X1
                        {
                            var 数据与电力接口子级 = new GameObject();
                            数据与电力接口子级.transform.SetParent(可装配.ThingTransform, false);
                            var 球形碰撞体 = 数据与电力接口子级.AddComponent<SphereCollider>();
                            球形碰撞体.radius = 0.1f;
                            球形碰撞体.transform.localPosition = new(0, 0, -0.2f);

                            var 可装配的所有接口 = 可装配.OpenEnds;
                            var 新接口 = new Connection(可装配)
                            {
                                ConnectionType = NetworkType.PowerAndData,
                                Transform = 球形碰撞体.transform,
                                Collider = 球形碰撞体,
                                ConnectionRole = ConnectionRole.None,
                            };
                            可装配的所有接口.Add(新接口);

                            break;
                        }
                }

                // 高亮选择框的显示尺寸是刚好一个小网格大小, 还是物体的包围盒大小, 仅仅是渲染效果, 实际对齐还是对齐到小网格的
                可装配.SelectionDisplay = SelectionHighlightMethod.Bounds;

                // 框架/墙体、门/其它物体, 选择其它物体即可
                可装配.StructureCollisionType = CollisionType.BlockCustom;

                // Grid:框架  Face:墙体、门(放置时对齐到2米尺寸网格的东、南、西、北、上、下六个面)  FaceMount:必须放置在墙体和框架上的物体(对齐到父物体的两个法向面上)    注:其它物体一般也是Grid, 比如自动车床属于Grid
                可装配.PlacementType = PlacementSnap.FaceMount;

                // 电力接口、数据接口、电线对接口、管道对接口、滑槽对接口等等是不可见的碰撞区域, 此处枚举值0-11表示弯头、三通、四通、直通等, 这样在放置建筑时, 就会查找该碰撞区域, 判断是否堵住了别人的电力接口、数据接口 或者 放置电缆时, 判断是否与其它电缆产生物理连接
                // ConnectionType不同的枚举值对应着Grid、Face、FaceMount放置时如何判断是否与另一个物体是否存在物理连接
                可装配.ConnectionType = Assets.Scripts.Util.SmartRotate.ConnectionType.FlatExhaustive;

                // 俯仰旋转、滚转旋转、偏航旋转  例: XY的意思是支持两种旋转方式, 其中X代表俯仰旋转, Y代表偏航旋转
                可装配.RotationAxis = RotationAxis.Z;

                // 允许旋转的地方, 分别是墙体上、天花板上、地板上、天花板和地板上、墙体和天花板和地板上
                可装配.AllowedRotations = AllowedRotations.All;

                // 小网格尺寸0.5, 所有建筑在放置时都是对齐到小网格坐标的, 因此建筑的碰撞尺寸必须是小网格尺寸的整数倍
                // 大网格尺寸2, 对齐到小网格时的布局分布为 0.25(半个小网格)/0.5/0.5/0.5/0.25(半个小网格), 即大网格中心有9个小网格, 四个边缘各有3个(半个小网格)
                可装配.GridSize = SmallGrid.SmallGridSize;

                // 两个大网格之间的边缘处各有0.25格, 组合起来才够一个小网格, 因此假如大网格坐标为0, 小网格坐标就要为-0.25, 这样建筑放置时, 从负0.25处开始判断碰撞, 从0.25处结束碰撞, 不会与大网格中间的9个小网格冲突, 正好利用上了边缘
                // 直线电缆的碰撞尺寸为一个小网格, 但是直线电缆的渲染尺寸中宽度只有0.1, 因此放置在两个大网格之间的边缘处视觉效果良好, 如果边缘处放置了墙体, 同时又放置了渲染尺寸接近碰撞尺寸的物体, 就会出现穿模
                可装配.GridOffset = SmallGrid.SmallGridOffset;

                // 管道、电线、设备、机械臂轨道(滑槽好像属于轨道), 即除了装饰面板外, 放置时都进行碰撞判断
                可装配.SmallCollisionType = SmallGridBlock.PipesCablesAndDevices | SmallGridBlock.Rails;

                if (可装配.BuildStates == null) { 可装配.BuildStates = new(); }
                var 所有施工阶段 = 可装配.BuildStates;

                var 施工阶段 = new BuildState();
                所有施工阶段.Add(施工阶段);

                var 该施工阶段的渲染配置 = 可装配.ThingTransform.GetComponent<MeshRenderer>();
                施工阶段.Visualizer = 该施工阶段的渲染配置;

                var 该施工阶段对应的子级根节点 = 施工阶段.LinkedGameObjects;
                该施工阶段对应的子级根节点.Add(可装配.ThingTransform.gameObject);

                var 如何装拆 = new 通用工具.施工材料和工时数据.装配与拆除所需的施工材料和工时数据((装配包.PrefabHash, 1, 0, 0, 0.5f), (撬棍的PrefabHash, 1, 0.5f), (施工阶段, 可装配.PrefabHash, 所有施工阶段.FindIndex(d => d == 施工阶段), 通用工具.施工材料和工时数据.建筑结构状态.结构正常状态));
                通用工具.施工材料和工时数据.添加到待添加队列_因为需要等待游戏资源加载完成才能查找到施工材料(如何装拆);

                var 如何修复 = new 通用工具.施工材料和工时数据.修复所需的施工材料和工时数据((装配包.PrefabHash, 1, 0, 0, 0.5f), (可装配, 可装配.PrefabHash));
                通用工具.施工材料和工时数据.添加到待添加队列_因为需要等待游戏资源加载完成才能查找到施工材料(如何修复);

                施工阶段.RenderMode = BuildStateRenderMode.OnMineAndPreviousStates;

                // 直接绘制使用MeshRenderer渲染, 间接绘制会禁用MeshRenderer, 然后使用间接绘制API绘制
                可装配.structureRenderMode = StructureRenderMode.Standard;
                Traverse.Create(施工阶段).Field("initialDrawData").SetValue(new Rendering.DrawData() { mesh = null, materials = [], shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off, });

                // 作为生产设备时, 该等级的生产速度加成, 生产能耗加成, 材料消耗加成, 产出加成
                // 施工阶段.ManufactureDat = new BuildStateManufacturingDat() { BuildTimeMultiplier = 1, EnergyCostMultiplier = 1, ItemSpawnMultiplier = 1, MachinesTier = MachineTier.Undefined, MaterialCostMultiplier = 1, };
            }

            return 装配包;
        }
    }

    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
    public class 将矿物扫描眼镜添加到游戏中
    {
        [HarmonyPrefix]
        public static void 执行()
        {
            NewPrefab = 功能模块之立体文字装饰.创建_套件_立体文字装饰();
            WorldManager.Instance.SourcePrefabs.Add(NewPrefab);
            WorldManager.Instance.SourcePrefabs.AddRange(NewPrefab.Constructables);
            功能模块之立体文字装饰.Log.LogMessage("成功将[(套件)立体文字装饰]添加到游戏中");
        }

        public static MultiConstructor NewPrefab = null;
    }
}

