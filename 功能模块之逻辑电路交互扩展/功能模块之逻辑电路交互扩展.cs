using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using BepInEx;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_luojidianlujiaohukuozhan", "功能模块之逻辑电路交互扩展", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之逻辑电路交互扩展 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之逻辑电路交互扩展加载完成!");
            补丁 = new Harmony("功能模块之逻辑电路交互扩展");
            补丁.PatchAll();
            拧螺丝联机数据包.注册联机数据包包头类型();
            添加交互过程函数();
        }

        private static void 添加交互过程函数()
        {
            前置模块.添加交互过程函数([

            // 逻辑读取器
            (typeof(LogicReader),
            (static (主物体, 控件) => ((LogicReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicReader)主物体).拧螺丝(控件, 可选择项目))),

            // 批量读取器
            (typeof(LogicBatchReader),
            (static (主物体, 控件) => ((LogicBatchReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicBatchReader)主物体).拧螺丝(控件, 可选择项目))),

            // 试剂读取器
            (typeof(ReagentReader),
            (static (主物体, 控件) => ((ReagentReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((ReagentReader)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑写入器
            (typeof(LogicWriter),
            (static (主物体, 控件) => ((LogicWriter)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicWriter)主物体).拧螺丝(控件, 可选择项目))),

            // 批量写入器
            (typeof(LogicBatchWriter),
            (static (主物体, 控件) => ((LogicBatchWriter)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicBatchWriter)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑写入开关
            (typeof(LogicWriterSwitch),
            (static (主物体, 控件) => ((LogicWriterSwitch)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicWriterSwitch)主物体).拧螺丝(控件, 可选择项目))),

            // 槽位读取器
            (typeof(LogicSlotReader),
            (static (主物体, 控件) => ((LogicSlotReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicSlotReader)主物体).拧螺丝(控件, 可选择项目))),

            // 批量槽位读取器
            (typeof(LogicBatchSlotReader),
            (static (主物体, 控件) => ((LogicBatchSlotReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicBatchSlotReader)主物体).拧螺丝(控件, 可选择项目))),
            
            // 逻辑基础数学
            (typeof(LogicMath),
            (static (主物体, 控件) => ((LogicMath)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMath)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑高等数学
            (typeof(LogicMathUnary),
            (static (主物体, 控件) => ((LogicMathUnary)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMathUnary)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑镜像
            (typeof(LogicMirror),
            (static (主物体, 控件) => ((LogicMirror)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMirror)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑比较器
            (typeof(LogicCompare),
            (static (主物体, 控件) => ((LogicCompare)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicCompare)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑选择
            (typeof(LogicSelect),
            (static (主物体, 控件) => ((LogicSelect)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicSelect)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑门
            (typeof(LogicGate),
            (static (主物体, 控件) => ((LogicGate)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicGate)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑最小最大
            (typeof(LogicMinMax),
            (static (主物体, 控件) => ((LogicMinMax)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMinMax)主物体).拧螺丝(控件, 可选择项目))),

            // 逻辑无线收发器
            (typeof(LogicTransmitter),
            (static (主物体, 控件) => ((LogicTransmitter)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicTransmitter)主物体).拧螺丝(控件, 可选择项目))),

            // IC外壳
            (typeof(CircuitHousing),
            (static (主物体, 控件) => ((CircuitHousing)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((CircuitHousing)主物体).拧螺丝(控件, 可选择项目))),

            // 火箭IC外壳
            (typeof(RocketCircuitHousing),
            (static (主物体, 控件) => ((CircuitHousing)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((CircuitHousing)主物体).拧螺丝(控件, 可选择项目))),

            // PID控制器
            (typeof(LogicPidController),
            (static (主物体, 控件) => ((LogicPidController)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicPidController)主物体).拧螺丝(控件, 可选择项目))),

            // 气体设备(空调机/过滤机/电解机/氢燃烧器/一氧化二氮制造机)
            (typeof(AirConditioner),
            (static (主物体, 控件) => ((DeviceInputOutputCircuit)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((DeviceInputOutputCircuit)主物体).拧螺丝(控件, 可选择项目))),

            // 气体设备(空调机/过滤机/电解机/氢燃烧器/一氧化二氮制造机)
            (typeof(FiltrationMachine),
            (static (主物体, 控件) => ((DeviceInputOutputCircuit)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((DeviceInputOutputCircuit)主物体).拧螺丝(控件, 可选择项目))),

            // 气体设备(空调机/过滤机/电解机/氢燃烧器/一氧化二氮制造机)
            (typeof(ElectrolysisMachine),
            (static (主物体, 控件) => ((DeviceInputOutputCircuit)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((DeviceInputOutputCircuit)主物体).拧螺丝(控件, 可选择项目))),

            // 气体设备(空调机/过滤机/电解机/氢燃烧器/一氧化二氮制造机)
            (typeof(H2CombustorMachine),
            (static (主物体, 控件) => ((DeviceInputOutputCircuit)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((DeviceInputOutputCircuit)主物体).拧螺丝(控件, 可选择项目))),

            // 气体设备(空调机/过滤机/电解机/氢燃烧器/一氧化二氮制造机)
            (typeof(Nitrolyzer),
            (static (主物体, 控件) => ((DeviceInputOutputCircuit)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((DeviceInputOutputCircuit)主物体).拧螺丝(控件, 可选择项目)))
        ]);
        }
    }
}