using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 售货机数据包 : ProcessedMessage<售货机数据包>
    {
        public long 目标Id;
        private long 储物Id;
        public static void 注册联机数据包包头类型()
        {
            // 请在进入游戏世界前注册
            前置模块.添加联机数据包消息类型(typeof(售货机数据包));
        }

        public override void Process(long hostId)
        {
            var 当前交互主物体 = Thing.Find<Thing>(目标Id) as VendingMachine;
            if (当前交互主物体)
            {
                var i = 当前交互主物体.Slots.FindIndex((s) => s.Get()?.ReferenceId == 储物Id);
                if (i >= 2) { 当前交互主物体.CurrentIndex = i; }                // 售货机的0和1分别是进口和出口槽位
            }
        }

        public override void Deserialize(RocketBinaryReader reader)
        {
            目标Id = reader.ReadInt64();
            储物Id = reader.ReadInt64();
        }

        public override void Serialize(RocketBinaryWriter writer)
        {
            writer.WriteInt64(目标Id);
            writer.WriteInt64(储物Id);
        }

        public static 售货机数据包 创建数据包(long 当前交互物体Id_, long 当前选择储物Id_)
        {
            return new 售货机数据包
            {
                目标Id = 当前交互物体Id_,
                储物Id = 当前选择储物Id_
            };
        }

        public static void 发送数据包(long 当前交互物体Id_, long 当前选择储物Id_)
        {
            if (NetworkManager.IsClient)
            {
                NetworkClient.SendToServer(创建数据包(当前交互物体Id_, 当前选择储物Id_), NetworkChannel.GeneralTraffic);
            }
        }
    }
}
