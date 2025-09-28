using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Objects.Items;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 植物瞬间生长数据包 : ProcessedMessage<植物瞬间生长数据包>
    {
        public long 目标Id;
        public static void 注册联机数据包包头类型()
        {
            // 请在进入游戏世界前注册
            前置模块.添加联机数据包消息类型(typeof(植物瞬间生长数据包));
        }

        public override void Process(long hostId)
        {
            var 当前交互主物体 = Thing.Find(目标Id) as Plant;
            if (当前交互主物体)
            { 当前交互主物体.SetNextStage(); }
        }

        public override void Deserialize(RocketBinaryReader reader)
        {
            目标Id = reader.ReadInt64();
        }

        public override void Serialize(RocketBinaryWriter writer)
        {
            writer.WriteInt64(目标Id);
        }

        public static 植物瞬间生长数据包 创建数据包(long 当前交互物体Id_)
        {
            return new 植物瞬间生长数据包
            {
                目标Id = 当前交互物体Id_,
            };
        }

        public static void 发送数据包(long 当前交互物体Id_)
        {
            if (NetworkManager.IsClient)
            {
                NetworkClient.SendToServer(创建数据包(当前交互物体Id_), NetworkChannel.GeneralTraffic);
            }
        }
    }
}
