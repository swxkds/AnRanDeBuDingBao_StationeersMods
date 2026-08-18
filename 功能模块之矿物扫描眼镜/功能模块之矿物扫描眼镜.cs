using Assets.Scripts.Objects;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using Assets.Scripts;
using Objects.Items;
using Reagents;
using Assets.Scripts.Util;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_kuangwusaomiaoyanjing", "功能模块之矿物扫描眼镜", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之矿物扫描眼镜 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之矿物扫描眼镜加载完成!");
            补丁 = new Harmony("功能模块之矿物扫描眼镜");
            补丁.PatchAll();

            WorldManager.OnWorldStarted += HUD抬头显示器.构造函数;
            前置模块.添加通用间接绘制构造参数(new() { (多图层_多物体_批量绘制.图层类型.可开采资源, 0, null) }, static () => HUD抬头显示器.显示状态);
        }

        public static Thing 创建矿物扫描眼镜()
        {
            var 资源视图 = 功能模块之矿物扫描眼镜_资源加载器.单例.资源视图;

            const string NameID = "KuangWuSaoMiaoYanJing";
            const 通用工具.游戏内置喷漆颜色.色板 默认颜色 = 通用工具.游戏内置喷漆颜色.色板.蓝色;
            const string 缩略图资源目录 = "Assets/矿物扫描眼镜/所有喷漆颜色缩略图/";

            var 控制组件 = 通用工具.创建Thing预制体并进行通用初始化<矿物扫描眼镜>
            (通用工具.创建新的空预制体(), 通用工具.创建新的空预制体(), NameID,
            通用工具.合并多边形网格([资源视图.查找资源<Mesh>("Assets/矿物扫描眼镜/镜框网格.asset"), 资源视图.查找资源<Mesh>("Assets/矿物扫描眼镜/镜片网格.asset")], "矿物扫描眼镜", Arg_保留子网格么: true),
            [通用工具.游戏内置喷漆颜色.游戏内置喷漆材质, new Material(Shader.Find("Custom/Stationeers Transparent")) { color = Color.white.SetAlpha(0.04f) }],
            资源视图.查找所有喷漆颜色缩略图资源(缩略图资源目录), 默认颜色);

            const Slot.Class 道具可放入的槽位_每个槽位有对应的道具类型 = Slot.Class.Glasses;
            const SlotWearAction 穿戴到玩家装备栏时_眼镜会遮挡玩家脸部 = SlotWearAction.HidePlayer;
            const SortingClass 道具的分拣大类 = SortingClass.Clothing;
            const float 待机耗电量 = 100;
            const float 工作时额外耗电量 = 0;
            const bool 会腐烂么 = false;
            const float 腐烂速度 = 0;
            const DecayedFood 腐烂后转换成的物品预制体 = null;
            const float 闪点温度 = 373.15f;
            const float 自燃温度 = 573.15f;

            控制组件.SlotType = 道具可放入的槽位_每个槽位有对应的道具类型;
            控制组件.SlotWearAction = 穿戴到玩家装备栏时_眼镜会遮挡玩家脸部;
            控制组件.SortingClass = 道具的分拣大类;

            控制组件.UsedPowerPassive = 待机耗电量;                         // 待机耗电量
            控制组件.UsedPowerActive = 工作时额外耗电量;                      // 工作时额外耗电量

            控制组件.CanDecay = 会腐烂么;                               // 如果可以腐烂,会添加到腐烂管理器中,每帧计算腐烂情况
            控制组件.DecayRate = 腐烂速度;                              // 腐烂速度
            控制组件.DecayedFoodPrefab = 腐烂后转换成的物品预制体;       // 腐烂后销毁原物品,然后在原地生成这个腐烂物品,使用原版的腐烂物品预制体就行

            var _1单位成分数据 = new Recipe { Copper = 5, Steel = 5, };
            通用工具.为目标物体添加试剂成分表_每QuantityPerUse单位(控制组件, _1单位成分数据);

            控制组件.flashpointTemperature = 闪点温度;
            控制组件.autoignitionTemperature = 自燃温度;

            控制组件.添加槽位(Slot.Class.Battery, InteractableType.Slot1, 实体槽位的碰撞体: null);
            控制组件.添加槽位(Slot.Class.Ore, InteractableType.Slot3, 实体槽位的碰撞体: null, 槽位对应的所有结构哈希: [矿物扫描眼镜.能源矿物ID]);
            控制组件.添加控件(InteractableType.Powered, 是否创建UI按钮: false, 实体控件的碰撞体: null, 控件快捷键: string.Empty);        // 是否供电
            控制组件.添加控件(InteractableType.OnOff, 是否创建UI按钮: true, 实体控件的碰撞体: null, 控件快捷键: 快捷键配置.按键名称兼索引key_控件快捷键实时读取配置选项卡获取实时的键位);             // 是否开机

            return 控制组件;
        }
    }

    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
    public class 将矿物扫描眼镜添加到游戏中
    {
        [HarmonyPrefix]
        public static void 执行()
        {
            NewPrefab = 功能模块之矿物扫描眼镜.创建矿物扫描眼镜();
            WorldManager.Instance.SourcePrefabs.Add(NewPrefab);
            功能模块之矿物扫描眼镜.Log.LogMessage("成功将矿物扫描眼镜添加到游戏中");
        }

        public static Thing NewPrefab = null;
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.SetupKeyBindings))]
    public class 快捷键配置
    {
        public static void Postfix()
        {
            // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用
            if (GameManager.IsBatchMode) { return; }

            通用工具.创建游戏主菜单按键键位配置选项卡布局组(按键布局组名称);
            通用工具.创建游戏主菜单按键键位配置选项卡(按键名称兼索引key_控件快捷键实时读取配置选项卡获取实时的键位, 初始默认按键, 按键布局组名称);
        }

        public const string 按键布局组名称 = "KuangWu";
        public const string 按键名称兼索引key_控件快捷键实时读取配置选项卡获取实时的键位 = "KuangWuSaoMiaoYanJing";        // 控件快捷键实时读取配置选项卡获取实时的键位
        public const KeyCode 初始默认按键 = KeyCode.M;
    }
}

