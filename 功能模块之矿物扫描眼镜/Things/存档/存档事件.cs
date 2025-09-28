using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public partial class 矿物扫描眼镜 : PowerTool
    {
        public override ThingSaveData SerializeSave()
        {
            // 继承自存档基类的存档序列化类,确保序列化对象在传参过程中不拷贝
            ThingSaveData saveData = new 矿物扫描眼镜SaveData();
            InitialiseSaveData(ref saveData);
            return saveData;
        }

        protected override void InitialiseSaveData(ref ThingSaveData saveData)
        {
            // 将this写入到存档序列化对象中,确保序列化对象在传参过程中不拷贝
            base.InitialiseSaveData(ref saveData);
            ((矿物扫描眼镜SaveData)saveData).能源矿物消耗时间 = this.能源矿物消耗计时;
        }

        public override void DeserializeSave(ThingSaveData saveData)
        {
            // 将存档序列化对象写入到this中
            base.DeserializeSave(saveData);
            this.能源矿物消耗计时 = ((矿物扫描眼镜SaveData)saveData).能源矿物消耗时间;
        }

        public override void SerializeOnJoin(RocketBinaryWriter writer)
        {
            // 新玩家加入时, 将this写入字节流, 然后转换成联机数据包发送出去
            base.SerializeOnJoin(writer);
            writer.WriteSingle(this.能源矿物消耗计时);
        }

        public override void DeserializeOnJoin(RocketBinaryReader reader)
        {
            // 新玩家加入时, 从接收到的数据包中读取字节流并写入this
            base.DeserializeOnJoin(reader);
            this.能源矿物消耗计时 = reader.ReadSingle();
        }

        public override void BuildUpdate(RocketBinaryWriter writer, ushort networkUpdateType)
        {
            // 联机数据包之同步物品状态 => 将this写入字节流, 然后转换成联机数据包发送出去
            base.BuildUpdate(writer, networkUpdateType);
            writer.WriteSingle(this.能源矿物消耗计时);
        }

        public override void ProcessUpdate(RocketBinaryReader reader, ushort networkUpdateType)
        {
            // 联机数据包之同步物品状态 => 从接收到的数据包中读取字节流并写入this
            base.ProcessUpdate(reader, networkUpdateType);
            this.能源矿物消耗计时 = reader.ReadSingle();
        }
    }
}