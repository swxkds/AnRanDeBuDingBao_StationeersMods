using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 拧螺丝(this SatelliteDish 卫星天线, Interactable 卫星天线控件, 通用可选择项目 已选择)
        {
            switch (已选择.解包标志)
            {
                case 通用可选择项目.数据解包标志.贸易商船:
                    switch (卫星天线控件.Action)
                    {
                        case InteractableType.Button1:
                        case InteractableType.Button2:
                        case InteractableType.Button3:
                        case InteractableType.Button4:
                            拧螺丝(卫星天线, 已选择.链接贸易商船);
                            卫星天线数据包.发送数据包(卫星天线.ReferenceId, 已选择.链接贸易商船.ReferenceId, 卫星天线控件.InteractableId, 通用可选择项目.数据解包标志.贸易商船);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }

        private static void 拧螺丝(SatelliteDish 卫星天线, TraderContact 已选择商船)
        {
            // 客户端不需要校正, 服务器校正过程中相关变量的脏标记会改变, 通过原版游戏的同步机制自动发送同步数据包
            if (NetworkManager.IsClient) { return; }

            // TODO:联机游戏请在此处发送数据包
            // 目标夹角: ScannedContactUIData.DishAlignment;    // 目标夹角: ScannedContactData.LastScannedDegreeOffset;
            // 在<SatelliteDish.DishContactAngleCheck>方法中, 点乘计算可反推出DishForward和Angle是两个基于世界坐标系原点的单位位置向量
            // 在<ContactSlot.TryGenerateNewContact>方法中, Angle.y被赋值为正值, 因此商船位置不会出现在地平线以下

            var 天线前向 = 卫星天线.DishForward;
            var 目标方向 = 已选择商船.Angle.normalized;

            天线前向.y = 0;                         // 投影到水平面
            天线前向.Normalize();
            目标方向.y = 0;
            目标方向.Normalize();

            var 方位夹角 = Vector3.SignedAngle(天线前向, 目标方向, Vector3.up);                 // 以天线自身的 Up 轴为旋转轴

            // 计算俯仰角（转换为角度）
            var 商船俯仰角 = Mathf.Rad2Deg * Mathf.Asin(已选择商船.Angle.y / 已选择商船.Angle.magnitude);
            var 天线俯仰角 = Mathf.Rad2Deg * Mathf.Asin(卫星天线.DishForward.y / 卫星天线.DishForward.magnitude);

            // 俯仰角差值计算
            var 俯仰夹角 = 天线俯仰角 - 商船俯仰角;

            // 计算新的方位和俯仰角度
            var 方位 = Mathf.Repeat((float)(卫星天线.GetLogicValue(LogicType.Horizontal) + 方位夹角), 360);
            var 俯仰 = Mathf.Clamp((float)(卫星天线.GetLogicValue(LogicType.Vertical) + 俯仰夹角), 0, 90);

            卫星天线.SetLogicValue(LogicType.Horizontal, 方位);
            卫星天线.SetLogicValue(LogicType.Vertical, 俯仰);

            // var d = $"[{Mathf.Round(方位)}°,{Mathf.Round(俯仰)}°]";      
        }

        public static 通用可选择项目 生成消息(this SatelliteDish 卫星天线, Interactable 卫星天线控件)
        {
            通用可选择项目 包 = new();

            switch (卫星天线控件.Action)
            {
                case InteractableType.Button1:
                case InteractableType.Button2:
                case InteractableType.Button3:
                case InteractableType.Button4:
                    包.解包标志 = 通用可选择项目.数据解包标志.贸易商船;
                    包.物联网已上线设备表或内部储物表或试剂引用 = 卫星天线.DishScannedContacts.ScannedContactData.Select(d => d.Contact);// .Where(d => 扫描过滤_DishContactAngleCheck(d));
                    if (包.物联网已上线设备表或内部储物表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<TraderContact>)包.物联网已上线设备表或内部储物表或试剂引用).Count(); }
                    break;
                default:
                    包.解包标志 = 通用可选择项目.数据解包标志.未知;
                    break;
            }
            return 包;

            // bool 扫描过滤_DishContactAngleCheck(TraderContact contact)
            // {
            //     // contact 是商人, contact.Angle是商人的位置, 随机生成的一个单位向量, 其中y坐标为正值, 即生成的向量处于上半球, 和天线对应
            //     // 卫星天线.DishForward 是锅面的前向向量, 也是单位向量

            //     // 计算方位角（转换为角度）
            //     var 商人方位角 = Mathf.Rad2Deg * Mathf.Atan2(contact.Angle.x, contact.Angle.z);
            //     var 天线方位角 = Mathf.Rad2Deg * Mathf.Atan2(卫星天线.DishForward.x, 卫星天线.DishForward.z);

            //     // 使用Mathf.DeltaAngle计算最小角度差，自动处理角度范围问题
            //     var 方位夹角 = Mathf.DeltaAngle(天线方位角, 商人方位角);

            //     if (Mathf.Abs(方位夹角) < 90f) { return true; }
            //     return false;
            // }
        }
    }

    [HarmonyPatch(typeof(SatelliteDish), nameof(SatelliteDish.InteractWith))]
    public class 卫星天线交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, object __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机 && __instance is SatelliteDish 卫星天线)
            {
                __result = 通用选择面板.交互(卫星天线, interactable, interaction, 贴标机, doAction);
                if (__result == null) { return true; }
                return false;
            }
            return true;
        }
    }
}