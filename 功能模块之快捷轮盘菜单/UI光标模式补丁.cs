using Assets.Scripts.UI;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [HarmonyPatch(typeof(InputWindowBase), nameof(InputWindowBase.IsInputWindow), MethodType.Getter)]
    public class 快捷轮盘菜单_有驻留窗口么
    {
        [HarmonyPostfix]
        public static void 窗口是驻留状态么(ref bool __result)
        {
            // 作用举例1:
            // if (!InputWindowBase.IsInputWindow && !ConsoleWindow.IsOpen && KeyManager.GetButtonDown(KeyMap.MouseControl) && !Stationpedia.IsOpenAndLocked)
            // {
            // 	InputMouse.SetMouseControl(true);           // 允许鼠标控制角色视线晃动
            // 	PanelHands.Instance.HideSlotInfo();
            // }

            // 作用举例2:
            // if (!InputWindowBase.IsInputWindow)
            // {
            // 	CursorManager.SetCursor(true);              // 显示屏幕中心的视线准心
            // }

            // 搭配使用, 同时静止键盘控制角色移动:
            // <InputWindowBase.SetInputKeyState>以层级name为key将窗口状态写入KeyManager.InputState
            // <MovementController.HandleJump>判断if (KeyManager.InputState != KeyInputState.Game) { return; }

            if (__result == false) { __result = 快捷轮盘菜单.当前面板状态 == InputPanelState.Waiting; }
        }
    }

    [HarmonyPatch(typeof(InputWindowBase), nameof(InputWindowBase.Cancel))]
    public class 快捷轮盘菜单_关闭所有面板
    {
        [HarmonyPostfix]
        public static void 窗口关闭事件()
        {
            // 作用举例: 笔记本电脑没有电了, 可以调用<InputWindowBase.Cancel>一键关闭包含IC编辑窗口在内的所有嵌套窗口
            // 这里其实不需要将<通用通用选择面板.关闭面板>加上的, 加上是为了提醒自己可以这样写一键关闭功能

            // IC编辑窗口没有点击暂停游戏按钮,一键关闭; IC编辑窗口点击提交或者取消按钮后一键关闭
            if (KeyManager.InputState != KeyInputState.Paused || InputSourceCode.InputState != InputPanelState.Waiting)
            {
                快捷轮盘菜单.关闭快捷轮盘菜单();
            }
        }
    }
}