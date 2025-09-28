// using Assets.Scripts.Networking;
// using Assets.Scripts.Objects;
// using Assets.Scripts;
// using Assets.Scripts.Objects.Items;

// namespace meanran_xuexi_mods_xiaoyouhua
// {
//     public class 开始使用工具事件与结束使用工具事件数据包 : ProcessedMessage<开始使用工具事件与结束使用工具事件数据包>
//     {
//         public long 工具Id;
//         public bool 工具的激活按钮按压了么;
//         public static void 注册联机数据包包头类型()
//         {
//             // 请在进入游戏世界前注册
//             前置模块.添加联机数据包消息类型(typeof(开始使用工具事件与结束使用工具事件数据包));
//         }

//         public override void Process(long hostId)
//         {
//             var 按压式工具 = Thing.Find(工具Id) as Tool;
//             if (按压式工具)
//             {
//                 if (工具的激活按钮按压了么)
//                 {
//                     按压式工具.OnPrimaryUseStart();  // 例: 手电钻使用时, 按住按钮钻头转动, 松开按钮钻头停止
//                 }
//                 else
//                 {
//                     按压式工具.OnPrimaryUseEnd();
//                 }
//             }
//         }

//         public override void Deserialize(RocketBinaryReader reader)
//         {
//             工具Id = reader.ReadInt64();
//             工具的激活按钮按压了么 = reader.ReadBoolean();
//         }

//         public override void Serialize(RocketBinaryWriter writer)
//         {
//             writer.WriteInt64(工具Id);
//             writer.WriteBoolean(工具的激活按钮按压了么);
//         }

//         public static 开始使用工具事件与结束使用工具事件数据包 创建数据包(long 当前主手工具Id_, bool 按压了么_)
//         {
//             return new 开始使用工具事件与结束使用工具事件数据包
//             {
//                 工具Id = 当前主手工具Id_,
//                 工具的激活按钮按压了么 = 按压了么_,
//             };
//         }

//         public static void 发送数据包(long 当前主手工具Id_, bool 按压了么_)
//         {
//             if (NetworkManager.IsClient)
//             {
//                 NetworkClient.SendToServer(创建数据包(当前主手工具Id_, 按压了么_), NetworkChannel.GeneralTraffic);
//             }
//         }
//     }
// }

