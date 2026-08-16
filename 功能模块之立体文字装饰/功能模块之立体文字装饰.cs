using Assets.Scripts.Objects;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using Objects.Items;
using Reagents;
using Assets.Scripts.Objects.Structures;
using System.Linq;
using Assets.Scripts.Util;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts;

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

            const string NameID = "ItemKitLiTiWenZiZhuangShi";
            const 通用工具.游戏内置喷漆颜色.色板 默认颜色 = 通用工具.游戏内置喷漆颜色.色板.橙色;

            // MultiConstructor: 可以放进背包的装配包道具, 一个装配包可以放置多个建筑
            const string 物体名称 = "(套件)立体文字装饰";
            var 套件 = 通用工具.创建Thing预制体并进行通用初始化<MultiConstructor>(资源加载器.所有预制体[物体名称].实体, 资源加载器.所有预制体[物体名称].蓝图, NameID, 资源加载器.所有一体式多边形网格与材质[物体名称].已合并Mesh, 资源加载器.所有一体式多边形网格与材质[物体名称].所有subMesh材质, 资源加载器.所有纹理[物体名称].对应不同喷漆颜色的缩略图, 默认颜色);
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

            套件.SlotType = 道具可放入的槽位_每个槽位有对应的道具类型;
            套件.SortingClass = 道具的分拣大类;

            套件.CanDecay = 会腐烂么;                               // 如果可以腐烂,会添加到腐烂管理器中,每帧计算腐烂情况
            套件.DecayRate = 腐烂速度;                              // 腐烂速度
            套件.DecayedFoodPrefab = 腐烂后转换成的物品预制体;       // 腐烂后销毁原物品,然后在原地生成这个腐烂物品,使用原版的腐烂物品预制体就行

            套件.Quantity = 出厂数量;
            套件.MaxQuantity = 最大堆垛数量;

            var _1单位成分数据 = new Recipe { Iron = 2 };
            通用工具.为目标物体添加试剂成分表_每QuantityPerUse单位(套件, _1单位成分数据);

            套件.flashpointTemperature = 闪点温度;
            套件.autoignitionTemperature = 自燃温度;

            套件.添加控件(InteractableType.Button1, 是否创建UI按钮: true, 指定NameID: "SplitOne", 实体控件的碰撞体: null, 控件快捷键: string.Empty);        // 拆分一个
            套件.添加控件(InteractableType.Button2, 是否创建UI按钮: true, 指定NameID: "SplitHalf", 实体控件的碰撞体: null, 控件快捷键: string.Empty);             // 拆分一半

            var 所有可装配的建筑 = 套件.Constructables;

            var 壁灯的PrefabHash = Animator.StringToHash("StructureWallLight");

            int 撬棍的PrefabHash = Animator.StringToHash("ItemCrowbar");
            int 角磨机的PrefabHash = Animator.StringToHash("ItemAngleGrinder");
            通用工具.施工材料和工时数据.添加由模组扩展的施工材料(套件);

            for (var i = 1; i < 资源加载器.所有预制体.Count; i++)
            {
                立体文字装饰 挂墙小网格建筑 = null;
                Mesh 整个模型合并成一个Mesh = null;

                switch (i)
                {
                    case 1:     // 立体文字装饰 平面0.5X0.5
                        {
                            const string 物体名称1 = "立体文字装饰";
                            (Mesh 已合并Mesh, Material[] 所有subMesh材质) = 资源加载器.所有一体式多边形网格与材质[物体名称1];
                            挂墙小网格建筑 = 通用工具.创建Thing预制体并进行通用初始化<立体文字装饰>(资源加载器.所有预制体[物体名称1].实体, 资源加载器.所有预制体[物体名称1].蓝图, "SmallGridStructureLiTiWenZiZhuangShi" + i, 已合并Mesh, 所有subMesh材质.First(), 已合并Mesh, 资源加载器.所有纹理[物体名称1].对应不同喷漆颜色的缩略图, 默认颜色);
                            break;
                        }
                    case 2:     // 居住     平面0.5X1
                        {
                            const string 物体名称1 = "居住";
                            (Mesh[] 所有Mesh, Material[] 所有subMesh材质) = 资源加载器.所有分体式多边形网格与材质[物体名称1];
                            var 外壳Mesh = 所有Mesh.Last();
                            整个模型合并成一个Mesh = 通用工具.合并多边形网格(所有Mesh);

                            挂墙小网格建筑 = 通用工具.创建Thing预制体并进行通用初始化<立体文字装饰>(资源加载器.所有预制体[物体名称1].实体, 资源加载器.所有预制体[物体名称1].蓝图, "SmallGridStructureLiTiWenZiZhuangShi" + i, 外壳Mesh, 所有subMesh材质.Last(), 整个模型合并成一个Mesh, 资源加载器.所有纹理[物体名称1].对应不同喷漆颜色的缩略图, 默认颜色);

                            var 虽然用不到_但是依然要增加碰撞体将网格都包裹住_避免射线检测时交互区域太小直接穿透了 = 挂墙小网格建筑.ThingTransform.GetOrAddComponent<BoxCollider>();
                            虽然用不到_但是依然要增加碰撞体将网格都包裹住_避免射线检测时交互区域太小直接穿透了.center = 外壳Mesh.bounds.center;
                            虽然用不到_但是依然要增加碰撞体将网格都包裹住_避免射线检测时交互区域太小直接穿透了.size = 外壳Mesh.bounds.size;

                            var 立体文字 = 资源加载器.所有分体式多边形网格与材质[物体名称1].所有Mesh.First();
                            通用工具.添加灯光(挂墙小网格建筑, 立体文字, 光源位置_相对于父级轴心点: new Vector3(0, 0, 0.443f), 眩光光源位置_相对于父级轴心点: new Vector3(0, 0, -0.16f), 电源接口位置_相对于父级轴心点: new(0, 0, -0.27f), 耗电量: 1);
                            break;
                        }
                }

                if (挂墙小网格建筑 == null) { continue; }

                所有可装配的建筑.Add(挂墙小网格建筑);
                通用工具.初始化_挂墙小网格建筑_碰撞图层与旋转方式(挂墙小网格建筑);

                {
                    // 结构正常状态_所有施工阶段
                    if (挂墙小网格建筑.BuildStates == null) { 挂墙小网格建筑.BuildStates = new(); }
                    var 所有施工阶段 = 挂墙小网格建筑.BuildStates;
                    var 新 = 通用工具.创建施工阶段并进行通用初始化(挂墙小网格建筑, 挂墙小网格建筑.ThingTransform.GetComponent<MeshRenderer>(), (套件.PrefabHash, 1, 0, 0, 0.5f), (撬棍的PrefabHash, 1, 0.5f));
                    所有施工阶段.Add(新);
                }
                {
                    // 结构正常状态_建筑的生命值不是满值_修复建筑施工阶段
                    var 如何修复 = new 通用工具.施工材料和工时数据.修复所需的施工材料和工时数据((套件.PrefabHash, 1, 0, 0, 0.5f), 挂墙小网格建筑);
                    通用工具.施工材料和工时数据.为目标物体添加修复结构所需的施工材料和工时数据(如何修复);
                }
                {
                    if (整个模型合并成一个Mesh)
                    {
                        // 结构损毁状态_所有施工阶段数组_为了复用使用了数组_实际上损毁状态只需要一个施工阶段
                        if (挂墙小网格建筑.BrokenBuildStates == null) { 挂墙小网格建筑.BrokenBuildStates = new(); }
                        var 结构损毁阶段 = 挂墙小网格建筑.BrokenBuildStates;

                        MeshRenderer 渲染配置 = null;

                        var 结构损毁状态 = new GameObject("结构损毁状态");
                        结构损毁状态.transform.SetParent(挂墙小网格建筑.ThingTransform, worldPositionStays: false);

                        结构损毁状态.AddComponent<MeshFilter>().sharedMesh = 整个模型合并成一个Mesh;
                        if (!GameManager.IsBatchMode)
                        {
                            渲染配置 = 结构损毁状态.AddComponent<MeshRenderer>();
                            渲染配置.sharedMaterial = 通用工具.游戏内置喷漆颜色.游戏内置喷漆材质;
                            渲染配置.enabled = false;
                        }

                        var 碰撞配置 = 结构损毁状态.AddComponent<BoxCollider>();
                        碰撞配置.center = 整个模型合并成一个Mesh.bounds.center;
                        碰撞配置.size = 整个模型合并成一个Mesh.bounds.size;
                        碰撞配置.enabled = false;

                        var 新 = 通用工具.创建施工阶段并进行通用初始化(挂墙小网格建筑, 渲染配置, (0, 0, 0, 0, 0.5f), (角磨机的PrefabHash, 1, 0.5f));
                        新.Colliders = [碰撞配置];
                        新.DamagedBuildState = true;
                        新.LinkedGameObjects = [结构损毁状态];
                        结构损毁阶段.Add(new BrokenBuildState() { BuildState = 新, TotalReagentMixture = ReagentMixture.Empty });
                    }
                }
                {
                    var 原版_壁灯 = 通用工具.施工材料和工时数据.查找施工材料<WallLight>(通用工具.壁灯哈希);
                    if (原版_壁灯)
                    {
                        // 结构损毁状态拆除返回的残骸
                        var 类型指针 = typeof(Device);
                        var 所有残骸 = 类型指针.GetField("wreckagePrefabs", 通用工具.私有字段匹配条件);
                        var 原版_所有残骸配置 = 所有残骸.GetValue(原版_壁灯) as Wreckage[];
                        if (原版_所有残骸配置 != null)
                        {
                            所有残骸.SetValue(挂墙小网格建筑, 原版_所有残骸配置);
                        }
                    }
                }
            }

            return 套件;
        }
    }

    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
    public class 将立体文字装饰添加到游戏中
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

