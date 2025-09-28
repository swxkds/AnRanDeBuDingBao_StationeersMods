using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Reagents;
using Assets.Scripts;
using System.Linq;
using Assets.Scripts.Objects.Pipes;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 拧螺丝联机数据包 : ProcessedMessage<拧螺丝联机数据包>
    {
        public long 目标Id;
        public long 螺丝链接物体Id;
        public int 目标控件Id;
        public int 试剂Id;
        public int 操作数;
        private 通用可选择项目.数据解包标志 包头;

        public static void 注册联机数据包包头类型()
        {
            // 请在进入游戏世界前注册
            前置模块.添加联机数据包消息类型(typeof(拧螺丝联机数据包));
        }

        public override void Process(long hostId)
        {
            var 当前交互主物体 = Thing.Find<Thing>(目标Id);
            var 当前交互控件 = 当前交互主物体.Interactables[目标控件Id];
            var 包 = new 通用可选择项目();
            包.解包标志 = 包头;

            switch (包头)
            {
                case 通用可选择项目.数据解包标志.试剂参数:
                    包.物联网已上线设备表或内部储物表或试剂引用 = Reagent.AllReagentsSorted.FirstOrDefault((id) => id == 试剂Id); break;
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    包.链接物体 = Thing.Find<Thing>(螺丝链接物体Id); break;
                case 通用可选择项目.数据解包标志.插槽编号:
                    包.操作数["插槽编号"] = 操作数; break;
                case 通用可选择项目.数据解包标志.逻辑参数:
                case 通用可选择项目.数据解包标志.插槽参数:
                case 通用可选择项目.数据解包标志.试剂模式:
                case 通用可选择项目.数据解包标志.统计模式:
                case 通用可选择项目.数据解包标志.基础数学运算符:
                case 通用可选择项目.数据解包标志.高等数学运算符:
                case 通用可选择项目.数据解包标志.比较运算符:
                case 通用可选择项目.数据解包标志.逻辑门运算符:
                case 通用可选择项目.数据解包标志.最大最小值运算符:
                case 通用可选择项目.数据解包标志.物联网信号模式:
                    包.操作数["参数"] = 操作数; break;
            }

            switch (当前交互主物体)
            {
                case LogicSlotReader 槽位读取器:
                    槽位读取器.拧螺丝(当前交互控件, 包);
                    break;
                case LogicCompare 逻辑比较器:
                    逻辑比较器.拧螺丝(当前交互控件, 包);
                    break;
                case LogicReader 逻辑读取器:
                    if (当前交互主物体 is LogicPidController PID控制器) { PID控制器.拧螺丝(当前交互控件, 包); }
                    else { 逻辑读取器.拧螺丝(当前交互控件, 包); }
                    break;
                case LogicMathUnary 逻辑高等数学:
                    逻辑高等数学.拧螺丝(当前交互控件, 包);
                    break;
                case LogicMath 逻辑基础数学:
                    逻辑基础数学.拧螺丝(当前交互控件, 包);
                    break;
                case LogicMirror 逻辑镜像:
                    逻辑镜像.拧螺丝(当前交互控件, 包);
                    break;
                case LogicGate 逻辑门:
                    逻辑门.拧螺丝(当前交互控件, 包);
                    break;
                case LogicTransmitter 逻辑无线收发器:
                    逻辑无线收发器.拧螺丝(当前交互控件, 包);
                    break;
                case LogicWriterSwitch 逻辑写入开关:
                    逻辑写入开关.拧螺丝(当前交互控件, 包);
                    break;
                case LogicWriter 逻辑写入器:
                    逻辑写入器.拧螺丝(当前交互控件, 包);
                    break;
                case LogicSelect 逻辑选择读取器:
                    逻辑选择读取器.拧螺丝(当前交互控件, 包);
                    break;
                case LogicMinMax 逻辑最大最小值:
                    逻辑最大最小值.拧螺丝(当前交互控件, 包);
                    break;
                case LogicBatchSlotReader 批量槽位读取器:
                    批量槽位读取器.拧螺丝(当前交互控件, 包);
                    break;
                case LogicBatchReader 批量读取器:
                    批量读取器.拧螺丝(当前交互控件, 包);
                    break;
                case LogicBatchWriter 批量写入器:
                    批量写入器.拧螺丝(当前交互控件, 包);
                    break;
                case ReagentReader 试剂读取器:
                    试剂读取器.拧螺丝(当前交互控件, 包);
                    break;
                case CircuitHousing IC外壳:
                    IC外壳.拧螺丝(当前交互控件, 包);
                    break;
                case DeviceInputOutputCircuit 气体设备:
                    气体设备.拧螺丝(当前交互控件, 包);
                    break;
            }
        }

        public override void Deserialize(RocketBinaryReader reader)
        {
            目标Id = reader.ReadInt64();
            螺丝链接物体Id = reader.ReadInt64();
            目标控件Id = reader.ReadInt32();
            试剂Id = reader.ReadInt32();
            操作数 = reader.ReadInt32();
            包头 = (通用可选择项目.数据解包标志)reader.ReadInt32();
        }

        public override void Serialize(RocketBinaryWriter writer)
        {
            writer.WriteInt64(目标Id);
            writer.WriteInt64(螺丝链接物体Id);
            writer.WriteInt32(目标控件Id);
            writer.WriteInt32(试剂Id);
            writer.WriteInt32(操作数);
            writer.WriteInt32((int)包头);
        }

        public static 拧螺丝联机数据包 创建数据包(long 当前交互物体Id_, long 螺丝链接物体Id_, int 当前交互控件Id_, int 试剂Id_, int 操作数_, 通用可选择项目.数据解包标志 包头_)
        {
            return new 拧螺丝联机数据包
            {
                目标Id = 当前交互物体Id_,
                螺丝链接物体Id = 螺丝链接物体Id_,
                目标控件Id = 当前交互控件Id_,
                试剂Id = 试剂Id_,
                操作数 = 操作数_,
                包头 = 包头_
            };
        }

        public static void 发送数据包(long 当前交互物体Id_, long 螺丝链接物体Id_, int 当前交互控件Id_, int 试剂Id_, int 操作数_, 通用可选择项目.数据解包标志 包头_)
        {
            if (NetworkManager.IsClient)
            {
                NetworkClient.SendToServer(创建数据包(当前交互物体Id_, 螺丝链接物体Id_, 当前交互控件Id_, 试剂Id_, 操作数_, 包头_), NetworkChannel.GeneralTraffic);
            }
        }
    }
}
