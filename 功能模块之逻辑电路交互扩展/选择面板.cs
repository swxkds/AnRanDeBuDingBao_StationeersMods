using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Reagents;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 选择面板 : InputWindowBase
    {
        public static Dictionary<Type, (Func<object, Interactable, 数据包>, Action<object, Interactable, 可选择项目>)> 重定向表 = new()
        {
            // 逻辑读取器
            [typeof(LogicReader)] =
            (static (主物体, 控件) => ((LogicReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicReader)主物体).拧螺丝(控件, 可选择项目)),
            // 批量读取器
            [typeof(LogicBatchReader)] =
            (static (主物体, 控件) => ((LogicBatchReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicBatchReader)主物体).拧螺丝(控件, 可选择项目)),
            // 试剂读取器
            [typeof(ReagentReader)] =
            (static (主物体, 控件) => ((ReagentReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((ReagentReader)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑写入器
            [typeof(LogicWriter)] =
            (static (主物体, 控件) => ((LogicWriter)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicWriter)主物体).拧螺丝(控件, 可选择项目)),
            // 批量写入器
            [typeof(LogicBatchWriter)] =
            (static (主物体, 控件) => ((LogicBatchWriter)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicBatchWriter)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑写入开关
            [typeof(LogicWriterSwitch)] =
            (static (主物体, 控件) => ((LogicWriterSwitch)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicWriterSwitch)主物体).拧螺丝(控件, 可选择项目)),

            // 槽位读取器
            [typeof(LogicSlotReader)] =
            (static (主物体, 控件) => ((LogicSlotReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicSlotReader)主物体).拧螺丝(控件, 可选择项目)),
            // 批量槽位读取器
            [typeof(LogicBatchSlotReader)] =
            (static (主物体, 控件) => ((LogicBatchSlotReader)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicBatchSlotReader)主物体).拧螺丝(控件, 可选择项目)),

            // 逻辑基础数学
            [typeof(LogicMath)] =
            (static (主物体, 控件) => ((LogicMath)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMath)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑高等数学
            [typeof(LogicMathUnary)] =
            (static (主物体, 控件) => ((LogicMathUnary)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMathUnary)主物体).拧螺丝(控件, 可选择项目)),

            // 逻辑镜像
            [typeof(LogicMirror)] =
            (static (主物体, 控件) => ((LogicMirror)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMirror)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑比较器
            [typeof(LogicCompare)] =
            (static (主物体, 控件) => ((LogicCompare)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicCompare)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑选择
            [typeof(LogicSelect)] =
            (static (主物体, 控件) => ((LogicSelect)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicSelect)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑门
            [typeof(LogicGate)] =
            (static (主物体, 控件) => ((LogicGate)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicGate)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑最小最大
            [typeof(LogicMinMax)] =
            (static (主物体, 控件) => ((LogicMinMax)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicMinMax)主物体).拧螺丝(控件, 可选择项目)),
            // 逻辑无线收发器
            [typeof(LogicTransmitter)] =
             (static (主物体, 控件) => ((LogicTransmitter)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicTransmitter)主物体).拧螺丝(控件, 可选择项目)),

            // IC外壳
            [typeof(CircuitHousing)] =
            (static (主物体, 控件) => ((CircuitHousing)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((CircuitHousing)主物体).拧螺丝(控件, 可选择项目)),
            // 火箭IC外壳
            [typeof(RocketCircuitHousing)] =
            (static (主物体, 控件) => ((CircuitHousing)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((CircuitHousing)主物体).拧螺丝(控件, 可选择项目)),

            // PID控制器
            [typeof(LogicPidController)] =
            (static (主物体, 控件) => ((LogicPidController)主物体).生成消息(控件),
            static (主物体, 控件, 可选择项目) => ((LogicPidController)主物体).拧螺丝(控件, 可选择项目)),
        };
        public static Thing.DelayedActionInstance 交互(LogicUnitBase 光线命中的主物体, Interactable 光线命中的控件, Interaction 交互双方, Labeller 贴标机, bool doAction)
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
                    case 数据包.数据解包标志.未知:
                        return null;
                    case 数据包.数据解包标志.试剂参数:
                        if (数据包.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else if ((LogicReagentMode)数据包.操作数["参数"] == LogicReagentMode.TotalContents) { return 消息.Fail("试剂总质量模式(TotalContents),不需要选择试剂"); }
                        else { 消息.AppendStateMessage("单击打开选择试剂面板"); 消息.Succeed(); }
                        break;
                    case 数据包.数据解包标志.逻辑参数:
                        if (数据包.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else { 消息.AppendStateMessage("单击打开选择逻辑参数面板"); 消息.Succeed(); }
                        break;
                    case 数据包.数据解包标志.插槽参数:
                        if (数据包.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else if (数据包.操作数["插槽编号"] == -1) { return 消息.Fail("请先设置插槽编号"); }
                        else { 消息.AppendStateMessage("单击打开选择插槽参数面板"); 消息.Succeed(); }
                        break;
                    case 数据包.数据解包标志.插槽编号:
                        if (数据包.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else { 消息.AppendStateMessage("单击打开选择插槽编号面板"); 消息.Succeed(); }
                        break;
                    case 数据包.数据解包标志.试剂模式:
                        if (数据包.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else { 消息.AppendStateMessage("单击打开选择试剂模式面板"); 消息.Succeed(); }
                        break;
                    case 数据包.数据解包标志.统计模式:
                        消息.AppendStateMessage("单击打开选择统计模式面板"); 消息.Succeed();
                        break;
                    case 数据包.数据解包标志.基础数学运算符:
                        消息.AppendStateMessage("单击打开选择基础数学运算符面板"); 消息.Succeed();
                        break;
                    case 数据包.数据解包标志.高等数学运算符:
                        消息.AppendStateMessage("单击打开选择高等数学运算符面板"); 消息.Succeed();
                        break;
                    case 数据包.数据解包标志.比较运算符:
                        消息.AppendStateMessage("单击打开选择比较运算符面板"); 消息.Succeed();
                        break;
                    case 数据包.数据解包标志.逻辑门运算符:
                        消息.AppendStateMessage("单击打开选择逻辑门运算符面板"); 消息.Succeed();
                        break;
                    case 数据包.数据解包标志.最大最小值运算符:
                        消息.AppendStateMessage("单击打开选择最大最小值运算符面板"); 消息.Succeed();
                        break;
                    case 数据包.数据解包标志.物联网信号模式:
                        消息.AppendStateMessage("单击打开选择物联网信号模式面板"); 消息.Succeed();
                        break;
                    case 数据包.数据解包标志.物联网已上线设备:
                        if ((数据包.信号模式)数据包.操作数["参数"] == 数据包.信号模式.桥接) { return 消息.Fail(GameStrings.ThingModeDoesNotSupportLinking); }
                        else if (数据包.物联网已上线设备表或试剂引用 == null || 数据包.操作数["在线设备数"] <= 0) { return 消息.Fail("物联网无上线设备"); }
                        else { 消息.AppendStateMessage("单击打开选择链接物面板"); 消息.Succeed(); }
                        break;
                    case 数据包.数据解包标志.有线网已上线设备:
                        if (数据包.物联网已上线设备表或试剂引用 == null || 数据包.操作数["在线设备数"] <= 0) { return 消息.Fail("物联网无上线设备"); }
                        else { 消息.AppendStateMessage("单击打开选择链接物面板"); 消息.Succeed(); }
                        break;
                }

                if (doAction)
                {
                    光线命中的主物体.PlayPooledAudioSound(Defines.Sounds.ScrewdriverSound, Vector3.zero);

                    if (交互双方.SourceThing is Human 玩家 && 玩家.State == EntityState.Alive && 玩家.OrganBrain != null && 玩家.OrganBrain.LocalControl)
                    {
                        if (打开选择面板("选择面板", 数据包))
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
        public delegate void 选择面板事件(可选择项目 按钮点击返回_当前焦点);
        public static event 选择面板事件 触发点击事件时_面板要干什么;
        public static 可选择项目 当前选择;
        public static 数据包.数据解包标志 当前面板类型 = 数据包.数据解包标志.未知;
        public static InputPanelState 当前面板状态 = InputPanelState.None;
        public static 选择面板 单例;
        public TextMeshProUGUI 面板标题;
        public Transform 内容区垂直布局组父级;
        public ScrollRect 滚动组件;
        public TMP_InputField 面板搜索栏;
        public Button 关闭面板按钮;
        // -------------------------------------------------------------------------------------------------------------------------------------
        private Dictionary<long, Thing> 已发现表 = new();
        private Dictionary<long, Thing> 已显示表 = new();
        private Dictionary<long, Thing> 已失效表 = new();
        private Dictionary<long, 可选择项目> 活跃项目表 = new();
        private Queue<可选择项目> 休眠节点表 = new();
        public void 启用设备滚动区(数据包 包)
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
        private void 构造或复用项目(long id, 数据包 包)
        {
            可选择项目 项目;

            if (休眠节点表.Count > 0) { 项目 = 休眠节点表.Dequeue(); }
            else { 项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 已上线设备分支).GetComponent<可选择项目>(); 项目.构造初始化(); }

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
        private Dictionary<(int PrefabHash, IOCheck 读写模式), (RectTransform 分支, List<可选择项目> 所有可选择项目表)> 逻辑分支缓存池 = new();
        private Dictionary<(int PrefabHash, IOCheck 读或写, int 插槽编号), (RectTransform 分支, List<可选择项目> 所有可选择项目表)> 插槽分支表 = new();
        private Dictionary<(int PrefabHash, IOCheck 读写模式), (RectTransform 分支, List<可选择项目> 所有可选择项目表)> 插槽编号分支缓存池 = new();
        private Dictionary<数据包.数据解包标志, (RectTransform 分支, List<可选择项目> 所有可选择项目表)> 分支缓存池 = new();    // 所有分支这里都有保存,打开面板时先隐藏所有分支
        private List<可选择项目> 当前分支所有可选择项目表 = new();      // 记载当前显示的分支下所有的项目,根据搜索条件对项目进行显示和隐藏渲染
        private void 启用只读滚动区<T>(数据包 包, IEnumerable<T> 参数表)
        {
        启用只读滚动区重新查找:

            if (分支缓存池.TryGetValue(包.包头, out (RectTransform 分支, List<可选择项目> 所有可选择项目表) __))
            {
                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                设置激活状态(__.分支, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;
                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    设置激活状态(可选择项目.transform, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组预制体).GetComponent<RectTransform>();
                设置激活状态(分支, false);
                var 所有可选择项目表 = new List<可选择项目>();

                var 类型对象指针 = typeof(T);
                if (类型对象指针 == typeof(Reagent))
                {
                    foreach (T 参数 in 参数表)
                    {
                        包.物联网已上线设备表或试剂引用 = 参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 分支).GetComponent<可选择项目>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                }
                else if (类型对象指针 == typeof(LogicType) ||
                类型对象指针 == typeof(LogicSlotType) ||
                类型对象指针 == typeof(int) ||
                类型对象指针 == typeof(LogicReagentMode) ||
                类型对象指针 == typeof(LogicBatchMethod) ||
                类型对象指针 == typeof(MathOperators) ||
                类型对象指针 == typeof(MathOperatorsUnary) ||
                类型对象指针 == typeof(ConditionOperation) ||
                类型对象指针 == typeof(GateOperators) ||
                类型对象指针 == typeof(数据包.ComparisonOperation) ||
                类型对象指针 == typeof(数据包.信号模式))
                {
                    foreach (T 参数 in 参数表)
                    {
                        包.操作数["参数"] = Convert.ToInt32(参数);
                        var 可选择项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 分支).GetComponent<可选择项目>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                }

                分支缓存池[包.包头] = (分支, 所有可选择项目表);
                LayoutRebuilder.ForceRebuildLayoutImmediate(分支);
                goto 启用只读滚动区重新查找;
            }
        }
        private void 启用逻辑滚动区(数据包 包)
        {
            var 读写模式 = (IOCheck)包.操作数["IOCheck"];

        启用逻辑滚动区重新查找:

            if (逻辑分支缓存池.TryGetValue((包.链接物体.PrefabHash, 读写模式), out (RectTransform 分支, List<可选择项目> 所有可选择项目表) __))
            {
                分支缓存池[包.包头] = __;

                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                设置激活状态(__.分支, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;

                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    设置激活状态(可选择项目.transform, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组预制体).GetComponent<RectTransform>();
                设置激活状态(分支, false);
                var 所有可选择项目表 = new List<可选择项目>();

                foreach (var 参数 in Logicable.LogicTypes)
                {
                    if (读写模式 == IOCheck.Readable && ((ILogicable)包.链接物体).CanLogicRead(参数))
                    {
                        包.操作数["参数"] = (int)参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 分支).GetComponent<可选择项目>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                    else if (读写模式 == IOCheck.Writable && ((ILogicable)包.链接物体).CanLogicWrite(参数))
                    {
                        包.操作数["参数"] = (int)参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 分支).GetComponent<可选择项目>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                }

                逻辑分支缓存池[(包.链接物体.PrefabHash, 读写模式)] = (分支, 所有可选择项目表);
                LayoutRebuilder.ForceRebuildLayoutImmediate(分支);
                goto 启用逻辑滚动区重新查找;
            }
        }
        private void 启用插槽编号滚动区(数据包 包)
        {
            var 读写模式 = (IOCheck)包.操作数["IOCheck"];

        启用插槽编号滚动区重新查找:

            if (插槽编号分支缓存池.TryGetValue((包.链接物体.PrefabHash, 读写模式), out (RectTransform 分支, List<可选择项目> 所有可选择项目表) __))
            {
                分支缓存池[包.包头] = __;

                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                设置激活状态(__.分支, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;

                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    设置激活状态(可选择项目.transform, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组预制体).GetComponent<RectTransform>();
                设置激活状态(分支, false);
                var 所有可选择项目表 = new List<可选择项目>();

                int 计数 = 0;
                foreach (var 参数 in 包.链接物体.Slots)
                {
                    if (读写模式 == IOCheck.Readable)
                    {
                        包.操作数["插槽编号"] = 计数++;
                        var 可选择项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 分支).GetComponent<可选择项目>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                }

                插槽编号分支缓存池[(包.链接物体.PrefabHash, 读写模式)] = (分支, 所有可选择项目表);
                LayoutRebuilder.ForceRebuildLayoutImmediate(分支);
                goto 启用插槽编号滚动区重新查找;
            }
        }
        private void 启用插槽滚动区(数据包 包)
        {
            var 读写模式 = (IOCheck)包.操作数["IOCheck"];
            var 插槽编号 = 包.操作数["插槽编号"];

        启用插槽滚动区重新查找:

            if (插槽分支表.TryGetValue((包.链接物体.PrefabHash, 读写模式, 插槽编号), out (RectTransform 分支, List<可选择项目> 所有可选择项目表) __))
            {
                分支缓存池[包.包头] = __;

                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                设置激活状态(__.分支, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;

                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    设置激活状态(可选择项目.transform, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组预制体).GetComponent<RectTransform>();
                设置激活状态(分支, false);
                var 所有可选择项目表 = new List<可选择项目>();

                foreach (var 参数 in Logicable.LogicSlotTypes)
                {
                    if (读写模式 == IOCheck.Readable && ((ILogicable)包.链接物体).CanLogicRead(参数, 插槽编号))
                    {
                        包.操作数["参数"] = (int)参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(可选择项目预制体, 分支).GetComponent<可选择项目>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                }

                插槽分支表[(包.链接物体.PrefabHash, 读写模式, 插槽编号)] = (分支, 所有可选择项目表);
                LayoutRebuilder.ForceRebuildLayoutImmediate(分支);
                goto 启用插槽滚动区重新查找;
            }
        }
        private IEnumerable<Reagent> 所有试剂表 = Reagent.AllReagentsSorted;
        private void 启用试剂滚动区(数据包 包) => 启用只读滚动区(包, 所有试剂表);
        private IEnumerable<数据包.信号模式> 信号模式表 = (IEnumerable<数据包.信号模式>)Enum.GetValues(typeof(数据包.信号模式));
        private void 启用信号模式滚动区(数据包 包) => 启用只读滚动区(包, 信号模式表);
        private IEnumerable<数据包.ComparisonOperation> 最大最小值运算符表 = (IEnumerable<数据包.ComparisonOperation>)Enum.GetValues(typeof(数据包.ComparisonOperation));
        private void 启用最大最小值滚动区(数据包 包) => 启用只读滚动区(包, 最大最小值运算符表);
        private IEnumerable<GateOperators> 逻辑门运算符表 = (IEnumerable<GateOperators>)Enum.GetValues(typeof(GateOperators));
        private void 启用逻辑门滚动区(数据包 包) => 启用只读滚动区(包, 逻辑门运算符表);
        private IEnumerable<MathOperatorsUnary> 高等数学运算符表 = (IEnumerable<MathOperatorsUnary>)Enum.GetValues(typeof(MathOperatorsUnary));
        private void 启用高等数学滚动区(数据包 包) => 启用只读滚动区(包, 高等数学运算符表);
        private IEnumerable<ConditionOperation> 比较运算符表 = (IEnumerable<ConditionOperation>)Enum.GetValues(typeof(ConditionOperation));
        private void 启用比较滚动区(数据包 包) => 启用只读滚动区(包, 比较运算符表);
        private IEnumerable<LogicReagentMode> 试剂模式表 = (IEnumerable<LogicReagentMode>)Enum.GetValues(typeof(LogicReagentMode));
        private void 启用试剂模式滚动区(数据包 包) => 启用只读滚动区(包, 试剂模式表);
        private IEnumerable<MathOperators> 基础数学运算符表 = (IEnumerable<MathOperators>)Enum.GetValues(typeof(MathOperators));
        private void 启用基础数学滚动区(数据包 包) => 启用只读滚动区(包, 基础数学运算符表);
        private IEnumerable<LogicBatchMethod> 统计模式表 = (IEnumerable<LogicBatchMethod>)Enum.GetValues(typeof(LogicBatchMethod));
        private void 启用统计滚动区(数据包 包) => 启用只读滚动区(包, 统计模式表);
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
                case 数据包.数据解包标志.物联网已上线设备:
                case 数据包.数据解包标志.有线网已上线设备:
                    foreach (var 项目 in 活跃项目表.Values)
                    {
                        var 名称 = 项目.DisplayName.Trim().ToLower() ?? "";
                        var 显示么 = string.IsNullOrEmpty(条件) || 名称.Contains(条件);
                        项目.SetVisible(isVisble: 显示么);
                    }
                    break;
                default:
                    foreach (var 项目 in 当前分支所有可选择项目表)
                    {
                        var 名称 = 项目.右侧文本.text.Trim().ToLower() ?? "";
                        var 显示么 = string.IsNullOrEmpty(条件) || 名称.Contains(条件);
                        项目.SetVisible(isVisble: 显示么);
                    }
                    break;
            }

            SetInputKeyState(!string.IsNullOrWhiteSpace(面板搜索栏.text));  // 游戏代码(非Unity引擎)设置本面板为输入焦点
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
        public static bool 打开选择面板(string 面板标题, 数据包 包)
        {
            // 面板当前正在显示中时不可以重复开关面板;
            if (当前面板状态 != InputPanelState.None || 包.包头 == 数据包.数据解包标志.未知) { return false; }

            单例.面板标题.text = 面板标题;
            当前面板状态 = InputPanelState.Waiting;         // 刷新面板状态,请在全局状态中插入检测本面板状态的代码

            CursorManager.SetCursor(isLocked: false);       // 解锁时,不需要按下Alt键就可以一直显示光标
            EventSystem.current.SetSelectedGameObject(单例.面板搜索栏.gameObject);  // 将搜索栏设置为输入焦点

            foreach (var __ in 单例.分支缓存池.Values) { 设置激活状态(__.Item1, false); }

            if (包.包头 == 数据包.数据解包标志.有线网已上线设备)
            { 包.包头 = 数据包.数据解包标志.物联网已上线设备; }

            当前面板类型 = 包.包头;

            switch (当前面板类型)
            {
                case 数据包.数据解包标志.试剂参数:
                    单例.启用试剂滚动区(包);
                    break;
                case 数据包.数据解包标志.逻辑参数:
                    单例.启用逻辑滚动区(包);
                    break;
                case 数据包.数据解包标志.插槽参数:
                    单例.启用插槽滚动区(包);
                    break;
                case 数据包.数据解包标志.插槽编号:
                    单例.启用插槽编号滚动区(包);
                    break;
                case 数据包.数据解包标志.试剂模式:
                    单例.启用试剂模式滚动区(包);
                    break;
                case 数据包.数据解包标志.统计模式:
                    单例.启用统计滚动区(包);
                    break;
                case 数据包.数据解包标志.基础数学运算符:
                    单例.启用基础数学滚动区(包);
                    break;
                case 数据包.数据解包标志.高等数学运算符:
                    单例.启用高等数学滚动区(包);
                    break;
                case 数据包.数据解包标志.比较运算符:
                    单例.启用比较滚动区(包);
                    break;
                case 数据包.数据解包标志.逻辑门运算符:
                    单例.启用逻辑门滚动区(包);
                    break;
                case 数据包.数据解包标志.最大最小值运算符:
                    单例.启用最大最小值滚动区(包);
                    break;
                case 数据包.数据解包标志.物联网信号模式:
                    单例.启用信号模式滚动区(包);
                    break;
                case 数据包.数据解包标志.物联网已上线设备:
                    单例.启用设备滚动区(包);
                    break;
                case 数据包.数据解包标志.有线网已上线设备:
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
            if (当前面板类型 == 数据包.数据解包标志.物联网已上线设备)
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
            分支缓存池[数据包.数据解包标志.物联网已上线设备] = (已上线设备分支, null);
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
    }

}