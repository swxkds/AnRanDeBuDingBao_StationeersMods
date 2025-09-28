using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 卫星天线数据包 : ProcessedMessage<卫星天线数据包>
    {
        public long 目标Id;
        public long 螺丝链接贸易商船Id;
        public int 目标控件Id;
        private 通用可选择项目.数据解包标志 包头;
        public static void 注册联机数据包包头类型()
        {
            // 请在进入游戏世界前注册
            前置模块.添加联机数据包消息类型(typeof(卫星天线数据包));
        }

        public override void Process(long hostId)
        {
            var 当前交互主物体 = Thing.Find<Thing>(目标Id);
            var 当前交互控件 = 当前交互主物体.Interactables[目标控件Id];
            var 包 = new 通用可选择项目();
            包.解包标志 = 包头;

            switch (包头)
            {
                case 通用可选择项目.数据解包标志.贸易商船:
                    包.链接贸易商船 = Referencable.Find<TraderContact>(螺丝链接贸易商船Id); break;
            }

            switch (当前交互主物体)
            {
                case SatelliteDish 卫星天线:
                    卫星天线.拧螺丝(当前交互控件, 包);
                    break;
            }
        }

        public override void Deserialize(RocketBinaryReader reader)
        {
            目标Id = reader.ReadInt64();
            螺丝链接贸易商船Id = reader.ReadInt64();
            目标控件Id = reader.ReadInt32();
            包头 = (通用可选择项目.数据解包标志)reader.ReadInt32();
        }

        public override void Serialize(RocketBinaryWriter writer)
        {
            writer.WriteInt64(目标Id);
            writer.WriteInt64(螺丝链接贸易商船Id);
            writer.WriteInt32(目标控件Id);
            writer.WriteInt32((int)包头);
        }

        public static 卫星天线数据包 创建数据包(long 当前交互物体Id_, long 螺丝链接物体Id_, int 当前交互控件Id_, 通用可选择项目.数据解包标志 包头_)
        {
            return new 卫星天线数据包
            {
                目标Id = 当前交互物体Id_,
                螺丝链接贸易商船Id = 螺丝链接物体Id_,
                目标控件Id = 当前交互控件Id_,
                包头 = 包头_
            };
        }

        public static void 发送数据包(long 当前交互物体Id_, long 螺丝链接物体Id_, int 当前交互控件Id_, 通用可选择项目.数据解包标志 包头_)
        {
            if (NetworkManager.IsClient)
            {
                NetworkClient.SendToServer(创建数据包(当前交互物体Id_, 螺丝链接物体Id_, 当前交互控件Id_, 包头_), NetworkChannel.GeneralTraffic);
            }
        }
    }
}
