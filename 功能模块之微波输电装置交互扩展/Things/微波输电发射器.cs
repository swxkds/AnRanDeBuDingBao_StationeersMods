using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using HarmonyLib;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 扩展方法
    {
        public static void 拧螺丝(this PowerTransmitter 微波输电发射器, Interactable 微波输电发射器控件, 选择目标面板_可选择项目 已选择项目)
        {
            switch (已选择项目.数据包.包头)
            {
                case 数据包_选择目标面板.数据解包标志.物联网已上线设备:
                    switch (微波输电发射器控件.Action)
                    {
                        case InteractableType.Button1:
                        case InteractableType.Button2:
                        case InteractableType.Button3:
                        case InteractableType.Button4:
                            拧螺丝(微波输电发射器, 已选择项目.数据包.链接物体); break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static Dictionary<PowerTransmitter, Coroutine> 追踪表 = new();
        private static void 拧螺丝(PowerTransmitter 微波输电发射器, Thing 已选择)
        {
            // TODO:联机游戏请在此处发送数据包
            if (追踪表.TryGetValue(微波输电发射器, out var __) && __ != null)
            {
                微波输电发射器.StopCoroutine(__);     // 立刻结束旧的调整动作
                追踪表.Remove(微波输电发射器);
                功能模块之微波输电装置交互扩展.Log.LogMessage("微波输电校正目标变更,强制结束旧任务");
            }

            var 状态机对象 = 微波输电发射器.StartCoroutine(并发调整(微波输电发射器, 已选择));
            追踪表[微波输电发射器] = 状态机对象;
        }
        private static IEnumerator 并发调整(PowerTransmitter 微波输电发射器, Thing 已选择)
        {
            var 运行时间 = 0.0;

            var 发 = 微波输电发射器;
            var 收 = (PowerReceiver)已选择;
            double 发之前H = -1.0;
            double 发之前V = -1.0;
            double 收之前H = -1.0;
            double 收之前V = -1.0;

            while (微波输电发射器 != null && 已选择 != null)
            {
                运行时间 += Time.deltaTime;
                if (运行时间 >= 300.0)
                {
                    if (追踪表.ContainsKey(微波输电发射器)) { 追踪表.Remove(微波输电发射器); }
                    功能模块之微波输电装置交互扩展.Log.LogMessage("微波输电校正超时,强制结束此次任务");
                    yield break;    // 使用 yield break 结束并销毁状态机对象
                }

                var 发H = 发.GetLogicValue(LogicType.Horizontal);
                var 发V = 发.GetLogicValue(LogicType.Vertical);
                var 收H = 收.GetLogicValue(LogicType.Horizontal);
                var 收V = 收.GetLogicValue(LogicType.Vertical);

                const double 误差 = 0.1;

                if (Math.Abs(发H - 发之前H) >= 误差 || Math.Abs(发V - 发之前V) >= 误差 || Math.Abs(收H - 收之前H) >= 误差 || Math.Abs(收V - 收之前V) >= 误差)
                {
                    发之前H = 发H; 发之前V = 发V; 收之前H = 收H; 收之前V = 收V;
                }
                else
                {
                    if (追踪表.ContainsKey(微波输电发射器)) { 追踪表.Remove(微波输电发射器); }
                    功能模块之微波输电装置交互扩展.Log.LogMessage("微波输电校正完成,结束此次任务");
                    yield break;    // 使用 yield break 结束并销毁状态机对象
                }

                var 发X = 发.GetLogicValue(LogicType.PositionX);
                var 发Y = 发.GetLogicValue(LogicType.PositionY);
                var 发Z = 发.GetLogicValue(LogicType.PositionZ);

                var 收X = 收.GetLogicValue(LogicType.PositionX);
                var 收Y = 收.GetLogicValue(LogicType.PositionY);
                var 收Z = 收.GetLogicValue(LogicType.PositionZ);

                var 相对 = new Vector3((float)(收X - 发X), (float)(收Y - 发Y), (float)(收Z - 发Z));

                var 方位差 = Mathf.Atan2(相对.x, 相对.z) * Mathf.Rad2Deg;
                var 俯仰差 = Mathf.Atan(相对.y / Mathf.Sqrt(相对.x * 相对.x + 相对.z * 相对.z)) * Mathf.Rad2Deg;
                俯仰差 = Mathf.Abs(俯仰差);

                发.SetLogicValue(LogicType.Horizontal, 前向向量转换旋转角度(发.Transform.forward) + 方位差);
                发.SetLogicValue(LogicType.Vertical, 90.0 + (相对.y >= 0 ? 俯仰差 : -俯仰差));

                收.SetLogicValue(LogicType.Horizontal, 前向向量转换旋转角度(收.Transform.forward) + 方位差 + 180.0);
                收.SetLogicValue(LogicType.Vertical, 90.0 + (相对.y <= 0 ? 俯仰差 : -俯仰差));

                yield return null;   // 等待下一帧
            }

            if (追踪表.ContainsKey(微波输电发射器)) { 追踪表.Remove(微波输电发射器); }
            功能模块之微波输电装置交互扩展.Log.LogMessage("微波输电校正目标消失,强制结束此次任务");
            yield break;
        }
        public static 数据包_选择目标面板 生成消息(this PowerTransmitter 微波输电发射器, Interactable 微波输电发射器控件)
        {
            数据包_选择目标面板 包 = new();

            switch (微波输电发射器控件.Action)
            {
                case InteractableType.Button1:
                case InteractableType.Button2:
                case InteractableType.Button3:
                case InteractableType.Button4:
                    包.包头 = 数据包_选择目标面板.数据解包标志.物联网已上线设备;
                    包.操作数["参数"] = (int)数据包_选择目标面板.信号模式.直连;
                    包.物联网已上线设备表或试剂引用 = Transmitters.AllTransmitters?.Where(d => d != (ILogicable)微波输电发射器 && (d is PowerReceiver));
                    if (包.物联网已上线设备表或试剂引用 == null)
                    { 包.操作数["在线设备数"] = -1; }
                    else { 包.操作数["在线设备数"] = ((IEnumerable<ILogicable>)包.物联网已上线设备表或试剂引用).Count(); }
                    break;
                default:
                    包.包头 = 数据包_选择目标面板.数据解包标志.未知;
                    break;
            }
            return 包;
        }
        public static double 角度制转换比例制(double value, double maxValue)
        {
            RocketMath.ModuloCorrect(value, maxValue);
            return value / maxValue;
        }
        public static double 前向向量转换旋转角度(Vector3 源_前向)
        {
            var __ = Vector3.Dot(源_前向, Vector3.forward);
            var 源_前向_绝对 = Vector3.forward;

            var bd = Vector3.Dot(源_前向, Vector3.back);
            if (bd > __)
            {
                __ = bd;
                源_前向_绝对 = Vector3.back;
            }
            var ld = Vector3.Dot(源_前向, Vector3.left);
            if (ld > __)
            {
                __ = ld;
                源_前向_绝对 = Vector3.left;
            }
            var rd = Vector3.Dot(源_前向, Vector3.right);
            if (rd > __)
            {
                __ = rd;
                源_前向_绝对 = Vector3.right;
            }

            if (源_前向_绝对 == Vector3.forward) { return 0; }
            else if (源_前向_绝对 == Vector3.back) { return 180; }
            else if (源_前向_绝对 == Vector3.left) { return 90; }
            else if (源_前向_绝对 == Vector3.right) { return 270; }
            return 0;
        }
    }

    [HarmonyPatch(typeof(PowerTransmitter), nameof(PowerTransmitter.InteractWith))]
    public class 微波输电发射器交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, PowerTransmitter __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机)
            {
                __result = 选择目标面板.交互(__instance, interactable, interaction, 贴标机, doAction);
                if (__result == null) { return true; }
                return false;
            }
            return true;
        }
    }
}