using Assets.Scripts.Objects;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using Objects.Items;
using Reagents;
using Assets.Scripts.Objects.Structures;
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
            var 资源视图 = 功能模块之立体文字装饰_资源加载器.单例.资源视图;

            const string NameID = "ItemKitLiTiWenZiZhuangShi";
            const 通用工具.游戏内置喷漆颜色.色板 默认颜色 = 通用工具.游戏内置喷漆颜色.色板.橙色;
            const string 缩略图资源目录 = "Assets/(套件)立体文字装饰/所有喷漆颜色缩略图/";
            var 缩略图 = 资源视图.查找所有喷漆颜色缩略图资源(缩略图资源目录, Arg_是否填充不存在颜色: false);

            var 套件 = 通用工具.创建Thing预制体并进行通用初始化<MultiConstructor>      // MultiConstructor: 可以放进背包的装配包道具, 一个装配包可以放置多个建筑
            (通用工具.创建新的空预制体(), 通用工具.创建新的空预制体(), NameID,
            通用工具.合并多边形网格([资源视图.查找资源<Mesh>("Assets/(套件)立体文字装饰/(套件)立体文字装饰_外壳网格.asset"), 资源视图.查找资源<Mesh>("Assets/(套件)立体文字装饰/(套件)立体文字装饰_内容网格.asset")], "(套件)立体文字装饰", Arg_保留子网格么: true),
             [通用工具.游戏内置喷漆颜色.游戏内置喷漆材质, 通用工具.游戏内置喷漆颜色.游戏内置喷漆材质],
            缩略图, 默认颜色);

            {
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
            }

            var 所有可装配的建筑 = 套件.Constructables;

            var 撬棍的PrefabHash = Animator.StringToHash("ItemCrowbar");
            var 角磨机的PrefabHash = Animator.StringToHash("ItemAngleGrinder");

            var 外壳一格网格 = 资源视图.查找资源<Mesh>("Assets/外壳一格网格.asset");
            var 外壳两格网格 = 资源视图.查找资源<Mesh>("Assets/外壳两格网格.asset");

            通用工具.施工材料和工时数据.添加由模组扩展的施工材料(套件);

            创建_发光字("101", "_101_", 缩略图, 默认颜色);
            创建_发光字("102", "_102_", 缩略图, 默认颜色);
            创建_发光字("103", "_103_", 缩略图, 默认颜色);
            创建_发光字("104", "_104_", 缩略图, 默认颜色);
            创建_发光字("105", "_105_", 缩略图, 默认颜色);
            创建_发光字("106", "_106_", 缩略图, 默认颜色);
            创建_发光字("107", "_107_", 缩略图, 默认颜色);
            创建_发光字("108", "_108_", 缩略图, 默认颜色);
            创建_发光字("109", "_109_", 缩略图, 默认颜色);

            创建_发光字("201", "_201_", 缩略图, 默认颜色);
            创建_发光字("202", "_202_", 缩略图, 默认颜色);
            创建_发光字("203", "_203_", 缩略图, 默认颜色);
            创建_发光字("204", "_204_", 缩略图, 默认颜色);
            创建_发光字("205", "_205_", 缩略图, 默认颜色);
            创建_发光字("206", "_206_", 缩略图, 默认颜色);
            创建_发光字("207", "_207_", 缩略图, 默认颜色);
            创建_发光字("208", "_208_", 缩略图, 默认颜色);
            创建_发光字("209", "_209_", 缩略图, 默认颜色);

            创建_发光字("会客厅", "_hui_ke_ting_", 缩略图, 默认颜色);
            创建_发光字("冶炼厂", "_ye_lian_chang_", 缩略图, 默认颜色);
            创建_发光字("医务室", "_yi_wu_shi_", 缩略图, 默认颜色);
            创建_发光字("卧室", "_wo_shi_", 缩略图, 默认颜色);
            创建_发光字("卫生间", "_wei_sheng_jian_", 缩略图, 默认颜色);
            创建_发光字("宿舍", "_su_she_", 缩略图, 默认颜色);
            创建_发光字("水培室", "_shui_pei_shi_", 缩略图, 默认颜色);
            创建_发光字("车间", "_che_jian_", 缩略图, 默认颜色);
            创建_发光字("食堂", "_shi_tang_", 缩略图, 默认颜色);
            创建_发光字("餐厅", "_can_ting_", 缩略图, 默认颜色);

            void 创建_发光字(string Arg_网格名称, string Arg_NameID, Sprite[] Arg_缩略图, 通用工具.游戏内置喷漆颜色.色板 Arg_默认颜色)
            {
                所有可装配的建筑.Add(功能模块之立体文字装饰.创建_发光字(外壳一格网格, 资源视图.查找资源<Mesh>($"Assets/{Arg_网格名称}一格网格.asset"), $"{Arg_NameID}1", Arg_缩略图, Arg_默认颜色, 套件.PrefabHash, 撬棍的PrefabHash, 角磨机的PrefabHash));
                所有可装配的建筑.Add(功能模块之立体文字装饰.创建_发光字(外壳两格网格, 资源视图.查找资源<Mesh>($"Assets/{Arg_网格名称}两格网格.asset"), $"{Arg_NameID}2", Arg_缩略图, Arg_默认颜色, 套件.PrefabHash, 撬棍的PrefabHash, 角磨机的PrefabHash));
            }

            return 套件;
        }

        public static 立体文字装饰 创建_发光字(Mesh Arg_外壳网格, Mesh Arg_灯泡网格, string Arg_NameID, Sprite[] Arg_缩略图, 通用工具.游戏内置喷漆颜色.色板 Arg_默认颜色, int Arg_装配工具的PrefabHash, int Arg_拆除工具的PrefabHash, int Arg_结构损毁状态_拆除工具的PrefabHash)
        {
            Arg_NameID = 立体文字装饰.外壳层级前缀 + Arg_NameID;

            var 模型网格 = 通用工具.合并多边形网格([Arg_外壳网格, Arg_灯泡网格], Arg_NameID, Arg_保留子网格么: false);

            var 挂墙小网格建筑 = 通用工具.创建Thing预制体并进行通用初始化<立体文字装饰>(通用工具.创建新的空预制体(), 通用工具.创建新的空预制体(), Arg_NameID, Arg_外壳网格, 通用工具.游戏内置喷漆颜色.游戏内置喷漆材质, 模型网格, Arg_缩略图, Arg_默认颜色);

            var 虽然用不到_但是依然要增加碰撞体将网格都包裹住_避免射线检测时交互区域太小直接穿透了 = 挂墙小网格建筑.ThingTransform.GetOrAddComponent<BoxCollider>();
            虽然用不到_但是依然要增加碰撞体将网格都包裹住_避免射线检测时交互区域太小直接穿透了.center = Arg_外壳网格.bounds.center;
            虽然用不到_但是依然要增加碰撞体将网格都包裹住_避免射线检测时交互区域太小直接穿透了.size = Arg_外壳网格.bounds.size;

            通用工具.为挂墙小网格建筑的碰撞图层与旋转方式进行通用初始化(挂墙小网格建筑);
            挂墙小网格建筑.SmallCollisionType &= ~SmallGridBlock.Cables;

            通用工具.添加灯光(挂墙小网格建筑, Arg_灯泡网格, 立体文字装饰.灯泡层级前缀, 光源位置_相对于父级轴心点: new Vector3(0, 0, 0.45f), 眩光光源位置_相对于父级轴心点: new Vector3(0, 0, -0.47f), 电源接口位置_相对于父级轴心点: new(0, 0.27f, 0), 电源接口朝向_左手坐标系的Z轴朝向就是接口朝向: new(90, 0, 0), 耗电量: 1);

            {
                // 结构正常状态_所有施工阶段
                if (挂墙小网格建筑.BuildStates == null) { 挂墙小网格建筑.BuildStates = new(); }
                var 结构正常状态_所有施工阶段 = 挂墙小网格建筑.BuildStates;
                var 新 = 通用工具.创建施工阶段并进行通用初始化(挂墙小网格建筑, 挂墙小网格建筑.ThingTransform.GetComponent<MeshRenderer>(), (Arg_装配工具的PrefabHash, 1, 0, 0, 0.5f), (Arg_拆除工具的PrefabHash, 1, 0.5f));
                结构正常状态_所有施工阶段.Add(新);
            }

            {
                // 结构正常状态_建筑的生命值不是满值_修复建筑施工阶段
                var 如何修复 = new 通用工具.施工材料和工时数据.修复所需的施工材料和工时数据((Arg_装配工具的PrefabHash, 1, 0, 0, 0.5f), 挂墙小网格建筑);
                通用工具.施工材料和工时数据.为目标物体添加修复结构所需的施工材料和工时数据(如何修复);
            }

            {
                // 结构损毁状态_所有施工阶段数组_为了复用使用了数组_实际上损毁状态只需要一个施工阶段
                if (挂墙小网格建筑.BrokenBuildStates == null) { 挂墙小网格建筑.BrokenBuildStates = new(); }
                var 结构损毁阶段_所有施工阶段 = 挂墙小网格建筑.BrokenBuildStates;

                MeshRenderer 渲染配置 = null;

                var 结构损毁状态 = new GameObject("结构损毁状态");
                结构损毁状态.transform.SetParent(挂墙小网格建筑.ThingTransform, worldPositionStays: false);

                // 渲染区域是损毁结构
                结构损毁状态.AddComponent<MeshFilter>().sharedMesh = Arg_外壳网格;
                if (!GameManager.IsBatchMode)
                {
                    渲染配置 = 结构损毁状态.AddComponent<MeshRenderer>();
                    渲染配置.sharedMaterial = 通用工具.游戏内置喷漆颜色.游戏内置喷漆材质;
                    渲染配置.enabled = false;
                }

                // 碰撞区域是整个模型, 避免损毁结构导致碰撞区域变化, 导致其它建筑装配时碰撞区域判断出问题
                var 碰撞配置 = 结构损毁状态.AddComponent<BoxCollider>();
                碰撞配置.center = 模型网格.bounds.center;
                碰撞配置.size = 模型网格.bounds.size;
                碰撞配置.enabled = false;

                var 新 = 通用工具.创建施工阶段并进行通用初始化(挂墙小网格建筑, 渲染配置, (0, 0, 0, 0, 0.5f), (Arg_结构损毁状态_拆除工具的PrefabHash, 1, 0.5f));
                新.Colliders = [碰撞配置];
                新.DamagedBuildState = true;
                新.LinkedGameObjects = [结构损毁状态];
                结构损毁阶段_所有施工阶段.Add(new BrokenBuildState() { BuildState = 新, TotalReagentMixture = ReagentMixture.Empty });
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

            return 挂墙小网格建筑;
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

