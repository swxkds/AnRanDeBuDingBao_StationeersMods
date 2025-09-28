using UnityEngine;
using Assets.Scripts.Objects;
using Assets.Scripts.Inventory;
using Assets.Scripts;
using System.Collections;
using System.Reflection;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public enum 通用读条动作中断条件类型
        {
            事件触发进入读条动作_读条期间鼠标右键单击一次则中断读条,
            鼠标左键长按时进入读条动作_读条期间鼠标左键弹起则中断读条
        }

        public static readonly FieldInfo FieldInfo_处理Inventory的鼠标左键消息么 = typeof(InventoryManager).GetField("_primaryAvailable", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfo_玩家动作特效_比如采矿时渲染四溅的体素 = typeof(InventoryManager).GetField("_parentAnimator", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool 提交_读条动作_任务(InventoryManager.DelegateEvent Arg_动作事件, Thing.DelayedActionInstance Arg_进度条配置, 通用读条动作中断条件类型 Arg_中断条件)
        {
            var manager = InventoryManager.Instance;
            var 正在执行的读条动作 = manager.ActionCoroutine;
            if (正在执行的读条动作 != null) { return false; }

            var 不满足读条动作条件 = Arg_进度条配置.IsDisabled;
            if (不满足读条动作条件) { return false; }

            manager.ActionCoroutine = manager.StartCoroutine(读条动作(Arg_动作事件, Arg_进度条配置, Arg_中断条件));
            return true;
        }

        private static IEnumerator 读条动作(InventoryManager.DelegateEvent Arg_动作事件, Thing.DelayedActionInstance Arg_进度条配置, 通用读条动作中断条件类型 Arg_中断条件)
        {
            var 读条完成时长 = Mathf.Max(Arg_进度条配置.Duration, 0);   // 不允许负数

            var manager = InventoryManager.Instance;
            manager.OnComplete = Arg_动作事件;                            // 读条完成后执行的事件

            manager.LastCompletedRatio = 0f;                      // 重置上次中断读条时的进度
            manager.UIProgressionBar.SetProgress(0f);              // 重置上次中断读条时的进度

            manager.UIProgressionBar.SetActionName(Arg_进度条配置.ActionMessage);       // 进度条UI
            manager.UIProgressionBar.SetItemName(Arg_进度条配置.OverrideTitle);

            开关进度条播放(manager, 开关: true);

            var 读条开始时间 = Time.time;
            var 最新时间 = 读条开始时间;
            while (最新时间 - 读条开始时间 < 读条完成时长)
            {
                var 已读条时长 = 最新时间 - 读条开始时间;
                var 进度 = 已读条时长 / 读条完成时长;

                最新时间 = Time.time;

                bool 中断么;
                switch (Arg_中断条件)
                {
                    case 通用读条动作中断条件类型.事件触发进入读条动作_读条期间鼠标右键单击一次则中断读条:
                        {
                            中断么 = KeyManager.GetMouseUp("Secondary"); // 单击的定义: 有按下有弹起
                            break;
                        }
                    case 通用读条动作中断条件类型.鼠标左键长按时进入读条动作_读条期间鼠标左键弹起则中断读条:
                        {
                            中断么 = KeyManager.GetMouseUp("Primary");
                            break;
                        }
                    default:
                        {
                            中断么 = false;
                            break;
                        }
                }

                if (中断么)    // 取消读条
                {
                    开关进度条播放(manager, 开关: false);

                    manager.LastCompletedRatio = 进度;    // 记录中断读条时的进度, 作用举例: 吃饭动作, 在读条完成时, 直接吃饱, 而中断了读条, 则根据读条进度吃一部分
                    manager.ActionCoroutine = null;   // 将当前任务置空, 上级调用者就不会跳过事件处理, 可以创建新的读条动作任务

                    yield break;
                }
                else
                {
                    manager.UIProgressionBar.SetProgress(进度);
                    yield return null;
                }
            }

            开关进度条播放(manager, 开关: false);

            manager.LastCompletedRatio = 1f;    // 记录中断读条时的进度, 作用举例: 吃饭动作, 在读条完成时, 直接吃饱, 而中断了读条, 则根据读条进度吃一部分
            manager.OnComplete?.Invoke();
            manager.ActionCoroutine = null;

            yield break;

            static void 开关进度条播放(InventoryManager manager, bool 开关)
            {
                FieldInfo_处理Inventory的鼠标左键消息么.SetValue(manager, !开关);                    // 是否忽视鼠标左键操作
                manager.UIProgressionBar.SetActive(active: 开关);                        // 进度条UI
                var 读条动画 = (Animator)FieldInfo_玩家动作特效_比如采矿时渲染四溅的体素.GetValue(manager);
                读条动画.SetBool(MovementController.CastingHash, value: 开关);           // 参数名 "Casting" 通常表示读条动作
            }
        }
    }
}
