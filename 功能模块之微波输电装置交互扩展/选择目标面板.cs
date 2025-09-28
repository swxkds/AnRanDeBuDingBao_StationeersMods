using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 选择目标面板 : InputWindowBase
    {
        public static Dictionary<Type, (Func<object, Interactable, 数据包_选择目标面板>, Action<object, Interactable, 选择目标面板_可选择项目>)> 重定向表 = new()
        {
            // 微波输电发射器
            [typeof(PowerTransmitter)] =
            (static (主物体, 控件) => ((PowerTransmitter)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((PowerTransmitter)主物体).拧螺丝(控件, 可选择项目)),
        };
        public static Thing.DelayedActionInstance 交互(WirelessPower 光线命中的主物体, Interactable 光线命中的控件, Interaction 交互双方, Labeller 贴标机, bool doAction)
        {
            var 消息 = new Thing.DelayedActionInstance
            { Duration = 0, ActionMessage = ActionStrings.Set };

            if (!贴标机.IsOperable) { 消息.Fail(GameStrings.DeviceNoPower); }
            else if (!贴标机.OnOff) { 消息.Fail(GameStrings.DeviceNotOn); }
            else
            {
                // 光线命中的主物体.GetType() 和 typeof(主物体类名) 指向同一个类型对象指针, 区别是一个写在实例内存中, 另一个以立即数的形式写在指令中
                var 类型对象指针 = 光线命中的主物体.GetType();

                if (!重定向表.TryGetValue(类型对象指针, out var 函数指针)) { return 消息.Fail("请等待交互实装__"); }
                var 数据包 = 函数指针.Item1(光线命中的主物体, 光线命中的控件);
                数据包.主物体 = 光线命中的主物体;
                数据包.控件 = 光线命中的控件;

                switch (数据包.包头)
                {
                    case 数据包_选择目标面板.数据解包标志.未知:
                        return null;
                    case 数据包_选择目标面板.数据解包标志.物联网已上线设备:
                        if ((数据包_选择目标面板.信号模式)数据包.操作数["参数"] == 数据包_选择目标面板.信号模式.桥接) { return 消息.Fail(GameStrings.ThingModeDoesNotSupportLinking); }
                        else if (数据包.物联网已上线设备表或试剂引用 == null || 数据包.操作数["在线设备数"] <= 0) { return 消息.Fail("物联网无上线设备"); }
                        else { 消息.AppendStateMessage("单击打开选择链接物面板"); 消息.Succeed(); }
                        break;
                    case 数据包_选择目标面板.数据解包标志.有线网已上线设备:
                        if (数据包.物联网已上线设备表或试剂引用 == null || 数据包.操作数["在线设备数"] <= 0) { return 消息.Fail("物联网无上线设备"); }
                        else { 消息.AppendStateMessage("单击打开选择链接物面板"); 消息.Succeed(); }
                        break;
                }

                if (doAction)
                {
                    光线命中的主物体.PlayPooledAudioSound(Defines.Sounds.WrenchOneShot, Vector3.zero);

                    if (交互双方.SourceThing is Human 玩家 && 玩家.State == EntityState.Alive && 玩家.OrganBrain != null && 玩家.OrganBrain.LocalControl)
                    {
                        if (打开选择面板("选择目标面板", 数据包))
                        {
                            // 对于不同的逻辑电路使用对应的函数对象,该函数对象捕获了这个逻辑电路的拧螺丝方法地址                            
                            触发点击事件时_面板要干什么 += (当前选择) => { 函数指针.Item2(光线命中的主物体, 光线命中的控件, 当前选择); };
                        }
                    }
                }
            }

            return 消息;
        }
        public GameObject 可选择项目预制体;
        public RectTransform 内容区垂直布局组预制体;    // 垂直布局组,用于对所有显示的按钮进行排版,有一个区域适配组件(设置了高度适配) 
        public RectTransform 已上线设备分支;
        public delegate void 选择面板事件(选择目标面板_可选择项目 按钮点击返回_当前焦点);
        public static event 选择面板事件 触发点击事件时_面板要干什么;
        public static 选择目标面板_可选择项目 当前选择;
        public static 数据包_选择目标面板.数据解包标志 当前面板类型 = 数据包_选择目标面板.数据解包标志.未知;
        public static InputPanelState 当前面板状态 = InputPanelState.None;
        public static 选择目标面板 单例;
        public TextMeshProUGUI 面板标题;
        public Transform 内容区垂直布局组父级;
        public ScrollRect 滚动组件;
        public TMP_InputField 面板搜索栏;
        public Button 关闭面板按钮;
        // -------------------------------------------------------------------------------------------------------------------------------------
        private Dictionary<long, Thing> 已发现表 = new();
        private Dictionary<long, Thing> 已显示表 = new();
        private Dictionary<long, Thing> 已失效表 = new();
        private Dictionary<long, 选择目标面板_可选择项目> 活跃项目表 = new();
        private Queue<选择目标面板_可选择项目> 休眠节点表 = new();
        public void 启用设备滚动区(数据包_选择目标面板 包)
        {
            单例.滚动组件.content = 已上线设备分支;
            设置激活状态(已上线设备分支, true);

            // 根据项目数量实时显隐内容 注:哈希字典特别适合查询操作
            已发现表.Clear();
            foreach (var __ in (IEnumerable<ILogicable>)包.物联网已上线设备表或试剂引用)
            {
                已发现表.Add(((Thing)__).ReferenceId, (Thing)__);
            }

            // 追踪新增加的项目
            foreach (var __ in 已发现表)
            {
                if (!已显示表.ContainsKey(__.Key))
                {
                    已显示表.Add(__.Key, __.Value);
                    包.链接物体 = __.Value;
                    构造或复用项目(__.Key, 包);
                }
            }

            已失效表.Clear();

            // 追踪已移除的项目
            foreach (var __ in 已显示表)
            {
                if (!已发现表.ContainsKey(__.Key))
                { 已失效表.Add(__.Key, __.Value); }
            }

            foreach (var __ in 已失效表)
            {
                已显示表.Remove(__.Key);
                休眠项目(__.Key);
            }

            foreach (var __ in 活跃项目表.Values)
            {
                设置激活状态(__.transform, true);
            }

            将此回调函数赋给全局语言变更事件();                // 立刻刷新一次显示文本
        }
        private void 构造或复用项目(long id, 数据包_选择目标面板 包)
        {
            选择目标面板_可选择项目 项目;

            if (休眠节点表.Count > 0) { 项目 = 休眠节点表.Dequeue(); }
            else { 项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 已上线设备分支).GetComponent<选择目标面板_可选择项目>(); 项目.构造初始化(); }

            项目.复用初始化(包);
            活跃项目表.Add(id, 项目);
            设置激活状态(项目.transform, true);
        }
        private void 休眠项目(long id)
        {
            // 将不使用的项目存放到隐藏池中
            if (!活跃项目表.TryGetValue(id, out var 项目)) { return; }
            设置激活状态(项目.transform, false);
            活跃项目表.Remove(id);
            休眠节点表.Enqueue(项目);
        }

        // -------------------------------------------------------------------------------------------------------------------------------------   
        private Dictionary<数据包_选择目标面板.数据解包标志, (RectTransform 分支, List<选择目标面板_可选择项目> 所有可选择项目表)> 分支缓存池 = new();    // 所有分支这里都有保存,打开面板时先隐藏所有分支
        // -------------------------------------------------------------------------------------------------------------------------------------
        public static void 关闭面板(InputPanelState 过渡状态 = InputPanelState.Cancelled)
        {
            CursorManager.SetCursor(isLocked: true);          // 锁定时,只有按下Alt键才显示光标
            当前面板状态 = 过渡状态;                          // 刷新面板状态,请在全局状态中插入检测本面板状态的代码

            单例.SetVisible(isVisble: false);                 // 游戏代码(非Unity引擎)移除本面板的输入焦点,并将面板设置为隐藏
            单例.面板搜索栏.text = string.Empty;

            当前选择 = null;
            触发点击事件时_面板要干什么 = null;               // 面板是通用的,具体的事件需要在打开面板时传入

            当前面板状态 = InputPanelState.None;                // 刷新面板状态

            foreach (var __ in 单例.分支缓存池.Values) { 设置激活状态(__.Item1, false); }

            const int 限制缓存数 = 200;
            if (单例.已上线设备分支.childCount > 限制缓存数)
            {
                for (var i = 单例.已上线设备分支.childCount - 1; i >= 限制缓存数; i--)
                { UnityEngine.Object.Destroy(单例.已上线设备分支.GetChild(i).gameObject); }
            }
        }
        public static void 将此回调函数赋给提交按钮的点击事件()
        {
            if (触发点击事件时_面板要干什么 != null) { 触发点击事件时_面板要干什么(当前选择); }
            关闭面板(InputPanelState.Submitted);
        }
        public void 面板搜索栏文本变更事件(string str)
        {
            // 统一转换为小写并去除前后空格
            string 条件 = str?.Trim().ToLower() ?? "";

            switch (当前面板类型)
            {
                case 数据包_选择目标面板.数据解包标志.物联网已上线设备:
                case 数据包_选择目标面板.数据解包标志.有线网已上线设备:
                    foreach (var 项目 in 活跃项目表.Values)
                    {
                        var 名称 = 项目.DisplayName.Trim().ToLower() ?? "";
                        var 显示么 = string.IsNullOrEmpty(条件) || 名称.Contains(条件);
                        项目.SetVisible(isVisble: 显示么);
                    }
                    break;
                default: break;
            }

            SetInputKeyState(!string.IsNullOrWhiteSpace(面板搜索栏.text));  // 游戏代码(非Unity引擎)设置本面板为输入焦点
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
        public static bool 打开选择面板(string 面板标题, 数据包_选择目标面板 包)
        {
            // 面板当前正在显示中时不可以重复开关面板;
            if (当前面板状态 != InputPanelState.None || 包.包头 == 数据包_选择目标面板.数据解包标志.未知) { return false; }

            单例.面板标题.text = 面板标题;
            当前面板状态 = InputPanelState.Waiting;         // 刷新面板状态,请在全局状态中插入检测本面板状态的代码

            CursorManager.SetCursor(isLocked: false);       // 解锁时,不需要按下Alt键就可以一直显示光标
            EventSystem.current.SetSelectedGameObject(单例.面板搜索栏.gameObject);  // 将搜索栏设置为输入焦点

            foreach (var __ in 单例.分支缓存池.Values) { 设置激活状态(__.Item1, false); }

            if (包.包头 == 数据包_选择目标面板.数据解包标志.有线网已上线设备)
            { 包.包头 = 数据包_选择目标面板.数据解包标志.物联网已上线设备; }

            当前面板类型 = 包.包头;

            switch (当前面板类型)
            {
                case 数据包_选择目标面板.数据解包标志.物联网已上线设备:
                    单例.启用设备滚动区(包);
                    break;
                case 数据包_选择目标面板.数据解包标志.有线网已上线设备:
                    单例.启用设备滚动区(包);
                    break;
            }

            单例.面板搜索栏文本变更事件(单例.面板搜索栏.text);      // 在第一次打开面板时, 手动调用一次搜索
            单例.SetVisible(isVisble: true);                        // 游戏代码(非Unity引擎)设置本面板为输入焦点,并将面板设置为显示

            // LayoutRebuilder.ForceRebuildLayoutImmediate(单例.RectTransform);
            return true;
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
        private static void 将此回调函数赋给全局语言变更事件()
        {
            if (当前面板类型 == 数据包_选择目标面板.数据解包标志.物联网已上线设备)
            { foreach (var value in 单例.活跃项目表.Values) { value.右侧文本.text = value.数据包.链接物体.ToTooltip() + "\n" + Localization.GetSlotTooltip(Slot.Class.None); } }
        }
        private static void 设置激活状态(Transform obj, bool 状态)
        {
            if (obj && obj.gameObject.activeSelf != 状态) { obj.gameObject.SetActive(状态); }
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
        public override void Initialize()
        {
            // 面板有三个事件需要处理  1.关闭面板按钮 2.搜索栏事件 3.内容区按钮被点击后的事件(注:使用函数对象,捕获可选择项目引用)
            // 内容区按钮是动态创建的,因此事件需要在实体构造完成后赋值(具体代码见<可选择项目.初始化>方法)
            base.Initialize();
            SetVisible(isVisble: false);

            Localization.OnLanguageChanged += 将此回调函数赋给全局语言变更事件;

            关闭面板按钮.onClick.RemoveAllListeners();
            面板搜索栏.onSubmit.RemoveAllListeners();
            面板搜索栏.onValueChanged.RemoveAllListeners();

            关闭面板按钮.onClick.AddListener(static () => 关闭面板());
            面板搜索栏.onSubmit.AddListener(面板搜索栏文本变更事件);
            面板搜索栏.onValueChanged.AddListener(面板搜索栏文本变更事件);

            已上线设备分支 = UnityEngine.Object.Instantiate(内容区垂直布局组预制体).GetComponent<RectTransform>();
            if (已上线设备分支.parent != 内容区垂直布局组父级) { 已上线设备分支.SetParent(内容区垂直布局组父级, false); }
            设置激活状态(已上线设备分支, false);
            分支缓存池[数据包_选择目标面板.数据解包标志.物联网已上线设备] = (已上线设备分支, null);
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
    }

}