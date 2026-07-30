using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class 通用选择面板 : InputWindowBase, IModal
    {
        public bool UnlockCursor { get { return true; } }
        public static Thing.DelayedActionInstance 交互(Thing 视线处主物体, Interactable 视线处控件, Interaction 交互双方, Labeller 贴标机, bool doAction, string 交互动作名称 = null)
        {
            var 消息 = new Thing.DelayedActionInstance
            { Duration = 0, ActionMessage = 交互动作名称 ?? ActionStrings.Set };

            if (!贴标机.IsOperable) { 消息.Fail(GameStrings.DeviceNoPower); }
            else if (!贴标机.OnOff) { 消息.Fail(GameStrings.DeviceNotOn); }
            else
            {
                // 光线命中的主物体.GetType() 和 typeof(主物体类名) 指向同一个类型对象指针, 区别是一个写在实例内存中, 另一个以立即数的形式写在指令中
                var 类型指针 = 视线处主物体.GetType();

                if (!前置模块.交互过程函数表.TryGetValue(类型指针, out var 函数指针)) { return 消息.Fail("请等待交互实装__"); }
                var 原始项目信息 = 函数指针.Item1(视线处主物体, 视线处控件);
                原始项目信息.主物体 = 视线处主物体;
                原始项目信息.控件 = 视线处控件;

                switch (原始项目信息.解包标志)
                {
                    case 通用可选择项目.数据解包标志.未知:
                        return null;
                    case 通用可选择项目.数据解包标志.试剂参数:
                        if (原始项目信息.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else if ((LogicReagentMode)原始项目信息.操作数["参数"] == LogicReagentMode.TotalContents) { return 消息.Fail("试剂总质量模式(TotalContents),不需要选择试剂"); }
                        else { 消息.AppendStateMessage("单击打开选择试剂面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.逻辑参数:
                        if (原始项目信息.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else { 消息.AppendStateMessage("单击打开选择逻辑参数面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.插槽参数:
                        if (原始项目信息.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else if (原始项目信息.操作数["插槽编号"] == -1) { return 消息.Fail("请先设置插槽编号"); }
                        else { 消息.AppendStateMessage("单击打开选择插槽参数面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.插槽编号:
                        if (原始项目信息.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else { 消息.AppendStateMessage("单击打开选择插槽编号面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.试剂模式:
                        if (原始项目信息.链接物体 == null) { return 消息.Fail("请先设置螺丝链接物体"); }
                        else { 消息.AppendStateMessage("单击打开选择试剂模式面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.统计模式:
                        消息.AppendStateMessage("单击打开选择统计模式面板"); 消息.Succeed();
                        break;
                    case 通用可选择项目.数据解包标志.基础数学运算符:
                        消息.AppendStateMessage("单击打开选择基础数学运算符面板"); 消息.Succeed();
                        break;
                    case 通用可选择项目.数据解包标志.高等数学运算符:
                        消息.AppendStateMessage("单击打开选择高等数学运算符面板"); 消息.Succeed();
                        break;
                    case 通用可选择项目.数据解包标志.比较运算符:
                        消息.AppendStateMessage("单击打开选择比较运算符面板"); 消息.Succeed();
                        break;
                    case 通用可选择项目.数据解包标志.逻辑门运算符:
                        消息.AppendStateMessage("单击打开选择逻辑门运算符面板"); 消息.Succeed();
                        break;
                    case 通用可选择项目.数据解包标志.最大最小值运算符:
                        消息.AppendStateMessage("单击打开选择最大最小值运算符面板"); 消息.Succeed();
                        break;
                    case 通用可选择项目.数据解包标志.物联网信号模式:
                        消息.AppendStateMessage("单击打开选择物联网信号模式面板"); 消息.Succeed();
                        break;
                    case 通用可选择项目.数据解包标志.物联网已上线设备:
                        if ((通用可选择项目.信号模式)原始项目信息.操作数["参数"] == 通用可选择项目.信号模式.桥接) { return 消息.Fail(GameStrings.ThingModeDoesNotSupportLinking); }
                        else if (原始项目信息.物联网已上线设备表或内部储物表或试剂引用 == null || 原始项目信息.操作数["在线设备数"] <= 0) { return 消息.Fail("物联网无上线设备"); }
                        else { 消息.AppendStateMessage("单击打开选择链接物面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.有线网已上线设备:
                        if (原始项目信息.物联网已上线设备表或内部储物表或试剂引用 == null || 原始项目信息.操作数["在线设备数"] <= 0) { return 消息.Fail("物联网无上线设备"); }
                        else { 消息.AppendStateMessage("单击打开选择链接物面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.内部储物:
                        if (原始项目信息.物联网已上线设备表或内部储物表或试剂引用 == null || 原始项目信息.操作数["内部储物数"] <= 0) { return 消息.Fail("售货机内部未发现储物"); }
                        else { 消息.AppendStateMessage("单击打开选择储物面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.贸易商船:
                        if (原始项目信息.物联网已上线设备表或内部储物表或试剂引用 == null || 原始项目信息.操作数["在线设备数"] <= 0) { return 消息.Fail("未扫描到贸易商船"); }
                        else { 消息.AppendStateMessage("单击打开选择商船面板"); 消息.Succeed(); }
                        break;
                    case 通用可选择项目.数据解包标志.厨电配方:
                        if (原始项目信息.物联网已上线设备表或内部储物表或试剂引用 == null || 原始项目信息.操作数["厨电配方数"] <= 0) { return 消息.Fail("未发现厨电配方"); }
                        else { 消息.AppendStateMessage("单击打开选择配方面板"); 消息.Succeed(); }
                        break;
                }

                if (doAction)
                {
                    视线处主物体.PlayPooledAudioSound(Defines.Sounds.ScrewdriverSound, Vector3.zero);

                    if (交互双方.SourceThing is Human 玩家 && 玩家.State == EntityState.Alive && 玩家.OrganBrain != null && 玩家.OrganBrain.LocalControl)
                    {
                        if (打开面板("通用选择面板", 原始项目信息))
                        {
                            // 对于不同的逻辑电路使用对应的函数对象,该函数对象捕获了这个逻辑电路的拧螺丝方法地址                            
                            按钮点击时面板要干什么 += (当前选择项目) => { 函数指针.Item2(视线处主物体, 视线处控件, 当前选择项目.项目信息); };
                        }
                    }
                }
            }

            return 消息;
        }

        public static 通用选择面板按钮 当前选择项目;                // 按钮点击时将项目引用赋给<当前选择项目>, 然后调用<按钮点击时面板要干什么(当前选择项目)>
        public static event 选择面板事件 按钮点击时面板要干什么;     // 每次打开面板时, 将相关事件赋给<按钮点击时面板要干什么>
        public delegate void 选择面板事件(通用选择面板按钮 当前选择项目);
        public GameObject 通用选择面板按钮拷贝母体;
        public RectTransform 内容区垂直布局组拷贝母体;    // 垂直布局组,用于对所有显示的按钮进行排版,有一个区域适配组件(设置了高度适配) 
        public RectTransform 已上线设备分支;
        public static 通用可选择项目.数据解包标志 当前面板类型 = 通用可选择项目.数据解包标志.未知;
        public static InputPanelState 当前面板状态 = InputPanelState.None;
        public static 通用选择面板 单例;
        public TextMeshProUGUI 面板标题;
        public Transform 内容区垂直布局组父级;
        public ScrollRect 滚动组件;
        public TMP_InputField 面板搜索栏;
        public Button 关闭面板按钮;
        // -------------------------------------------------------------------------------------------------------------------------------------
        private Dictionary<long, Thing> 已发现表 = new();
        private Dictionary<long, Thing> 已显示表 = new();
        private Dictionary<long, Thing> 已失效表 = new();
        private Dictionary<long, 通用选择面板按钮> 活跃项目表 = new();
        private Queue<通用选择面板按钮> 休眠节点表 = new();
        public void 启用设备滚动区(通用可选择项目 包)
        {
            单例.滚动组件.content = 已上线设备分支;
            通用工具.变更激活状态(已上线设备分支.gameObject, true);

            if (包.解包标志 == 通用可选择项目.数据解包标志.贸易商船)
            {
                for (var i = 活跃项目表.Keys.Count - 1; i >= 0; i--)
                {
                    var 当前按钮 = 活跃项目表.Keys.ElementAt(i);
                    休眠项目(当前按钮);
                }

                switch (包.物联网已上线设备表或内部储物表或试剂引用)
                {
                    case IEnumerable<TraderContact> traderContactList:
                        foreach (var __ in traderContactList)
                        {
                            包.链接贸易商船 = __;       // 确保此包不被按钮引用, 否则按钮的链接贸易商船会被覆盖
                            构造或复用项目(__.ReferenceId, 包);
                        }
                        break;
                }
            }
            else if (包.解包标志 == 通用可选择项目.数据解包标志.厨电配方)
            {
                for (var i = 活跃项目表.Keys.Count - 1; i >= 0; i--)
                {
                    var 当前按钮 = 活跃项目表.Keys.ElementAt(i);
                    休眠项目(当前按钮);
                }

                switch (包.物联网已上线设备表或内部储物表或试剂引用)
                {
                    case List<(ReagentMixture, Item)> 厨电的所有配方:
                        foreach ((ReagentMixture 配方, Item 成品) in 厨电的所有配方)
                        {
                            包.链接厨电配方 = (配方, 成品);       // 确保此包不被按钮引用, 否则按钮的链接厨电配方会被覆盖
                            var 虚拟ReferenceId = (成品.ReferenceId == 0) ? ((成品.PrefabHash > 0) ? ~成品.PrefabHash : 成品.PrefabHash) : 成品.ReferenceId;
                            构造或复用项目(虚拟ReferenceId, 包);
                        }
                        break;
                }
            }
            else
            {
                // 根据项目数量实时显隐内容 注:哈希字典特别适合查询操作
                已发现表.Clear();

                switch (包.物联网已上线设备表或内部储物表或试剂引用)
                {
                    case IEnumerable<Thing> thingList:
                        foreach (var __ in thingList)
                        {
                            if (__ != null && !__.BeingDestroyed)
                            {
                                已发现表.Add(__.ReferenceId, __);
                            }
                        }
                        break;
                    case IEnumerable<ILogicable> logicableList:
                        foreach (var __ in logicableList)
                        {
                            if (__ != null && __ is Thing thing && !thing.BeingDestroyed)
                            {
                                已发现表.Add(__.ReferenceId, thing);
                            }
                        }
                        break;
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
            }

            foreach (var __ in 活跃项目表.Values)
            {
                通用工具.变更激活状态(__.gameObject, true);
            }

            // 贸易商船的名称是固定的, 无法修改, 因此无需刷新文本
            将此回调函数赋给全局语言变更事件();                // 立刻刷新一次显示文本
        }
        private void 构造或复用项目(long id, 通用可选择项目 包)
        {
            通用选择面板按钮 项目;

            if (休眠节点表.Count > 0) { 项目 = 休眠节点表.Dequeue(); }
            else { 项目 = UnityEngine.Object.Instantiate(通用选择面板按钮拷贝母体, 已上线设备分支).GetComponent<通用选择面板按钮>(); 项目.构造初始化(); }

            项目.复用初始化(包);
            活跃项目表.Add(id, 项目);
            项目.transform.SetAsLastSibling();              // 按照顺序依次排列
            通用工具.变更激活状态(项目.gameObject, true);
        }
        private void 休眠项目(long id)
        {
            // 将不使用的项目存放到隐藏池中
            if (!活跃项目表.TryGetValue(id, out var 项目)) { return; }
            通用工具.变更激活状态(项目.gameObject, false);
            活跃项目表.Remove(id);

            const int 限制缓存数 = 200;
            if (休眠节点表.Count > 限制缓存数)
            { UnityEngine.Object.Destroy(项目.gameObject); }
            else { 休眠节点表.Enqueue(项目); }
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        private Dictionary<(int PrefabHash, IOCheck 读写模式), (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表)> 逻辑分支缓存池 = new();
        private Dictionary<(int PrefabHash, IOCheck 读或写, int 插槽编号), (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表)> 插槽分支表 = new();
        private Dictionary<(int PrefabHash, IOCheck 读写模式), (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表)> 插槽编号分支缓存池 = new();
        private Dictionary<通用可选择项目.数据解包标志, (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表)> 分支缓存池 = new();    // 所有分支这里都有保存,打开面板时先隐藏所有分支
        private List<通用选择面板按钮> 当前分支所有可选择项目表 = new();      // 记载当前显示的分支下所有的项目,根据搜索条件对项目进行显示和隐藏渲染
        private void 启用只读滚动区<T>(通用可选择项目 包, IEnumerable<T> 参数表)
        {
        启用只读滚动区重新查找:

            if (分支缓存池.TryGetValue(包.解包标志, out (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表) __))
            {
                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                通用工具.变更激活状态(__.分支.gameObject, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;
                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    通用工具.变更激活状态(可选择项目.gameObject, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组拷贝母体).GetComponent<RectTransform>();
                通用工具.变更激活状态(分支.gameObject, false);
                var 所有可选择项目表 = new List<通用选择面板按钮>();

                var 类型指针 = typeof(T);
                if (
                类型指针 == typeof(Reagent))
                {
                    foreach (T 参数 in 参数表)
                    {
                        包.物联网已上线设备表或内部储物表或试剂引用 = 参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(通用选择面板按钮拷贝母体, 分支).GetComponent<通用选择面板按钮>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                }
                else if (
                类型指针 == typeof(LogicReagentMode) ||
                类型指针 == typeof(LogicBatchMethod) ||
                类型指针 == typeof(MathOperators) ||
                类型指针 == typeof(MathOperatorsUnary) ||
                类型指针 == typeof(ConditionOperation) ||
                类型指针 == typeof(GateOperators) ||
                类型指针 == typeof(通用可选择项目.ComparisonOperation) ||
                类型指针 == typeof(通用可选择项目.信号模式))
                {
                    foreach (T 参数 in 参数表)
                    {
                        包.操作数["参数"] = Convert.ToInt32(参数);
                        var 可选择项目 = UnityEngine.Object.Instantiate(通用选择面板按钮拷贝母体, 分支).GetComponent<通用选择面板按钮>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                }

                分支缓存池[包.解包标志] = (分支, 所有可选择项目表);
                LayoutRebuilder.ForceRebuildLayoutImmediate(分支);
                goto 启用只读滚动区重新查找;
            }
        }
        private void 启用逻辑滚动区(通用可选择项目 包)
        {
            var 读写模式 = (IOCheck)包.操作数["IOCheck"];

        启用逻辑滚动区重新查找:

            if (逻辑分支缓存池.TryGetValue((包.链接物体.PrefabHash, 读写模式), out (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表) __))
            {
                分支缓存池[包.解包标志] = __;

                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                通用工具.变更激活状态(__.分支.gameObject, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;

                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    通用工具.变更激活状态(可选择项目.gameObject, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组拷贝母体).GetComponent<RectTransform>();
                通用工具.变更激活状态(分支.gameObject, false);
                var 所有可选择项目表 = new List<通用选择面板按钮>();

                foreach (var 参数 in Logicable.LogicTypes)
                {
                    if (读写模式 == IOCheck.Readable && ((ILogicable)包.链接物体).CanLogicRead(参数))
                    {
                        包.操作数["参数"] = (int)参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(通用选择面板按钮拷贝母体, 分支).GetComponent<通用选择面板按钮>();
                        可选择项目.构造初始化();
                        可选择项目.复用初始化(包);
                        所有可选择项目表.Add(可选择项目);
                    }
                    else if (读写模式 == IOCheck.Writable && ((ILogicable)包.链接物体).CanLogicWrite(参数))
                    {
                        包.操作数["参数"] = (int)参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(通用选择面板按钮拷贝母体, 分支).GetComponent<通用选择面板按钮>();
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
        private void 启用插槽编号滚动区(通用可选择项目 包)
        {
            var 读写模式 = (IOCheck)包.操作数["IOCheck"];

        启用插槽编号滚动区重新查找:

            if (插槽编号分支缓存池.TryGetValue((包.链接物体.PrefabHash, 读写模式), out (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表) __))
            {
                分支缓存池[包.解包标志] = __;

                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                通用工具.变更激活状态(__.分支.gameObject, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;

                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    通用工具.变更激活状态(可选择项目.gameObject, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组拷贝母体).GetComponent<RectTransform>();
                通用工具.变更激活状态(分支.gameObject, false);
                var 所有可选择项目表 = new List<通用选择面板按钮>();

                for (var i = 0; i < 包.链接物体.Slots.Count; i++)
                {
                    // if (读写模式 == IOCheck.Readable)
                    // {
                    包.操作数["插槽编号"] = i;
                    var 可选择项目 = UnityEngine.Object.Instantiate(通用选择面板按钮拷贝母体, 分支).GetComponent<通用选择面板按钮>();
                    可选择项目.构造初始化();
                    可选择项目.复用初始化(包);
                    所有可选择项目表.Add(可选择项目);
                    // }
                }

                插槽编号分支缓存池[(包.链接物体.PrefabHash, 读写模式)] = (分支, 所有可选择项目表);
                LayoutRebuilder.ForceRebuildLayoutImmediate(分支);
                goto 启用插槽编号滚动区重新查找;
            }
        }
        private void 启用插槽滚动区(通用可选择项目 包)
        {
            var 读写模式 = (IOCheck)包.操作数["IOCheck"];
            var 插槽编号 = 包.操作数["插槽编号"];

        启用插槽滚动区重新查找:

            if (插槽分支表.TryGetValue((包.链接物体.PrefabHash, 读写模式, 插槽编号), out (RectTransform 分支, List<通用选择面板按钮> 所有可选择项目表) __))
            {
                分支缓存池[包.解包标志] = __;

                if (__.分支.parent != 内容区垂直布局组父级) { __.分支.SetParent(内容区垂直布局组父级, false); }
                单例.滚动组件.content = __.分支;
                通用工具.变更激活状态(__.分支.gameObject, true);

                var 缩略图 = 包.链接物体 ? 包.链接物体.Thumbnail : 包.主物体.Thumbnail;

                foreach (var 可选择项目 in __.所有可选择项目表)
                {
                    if (缩略图 != null) { 可选择项目.左侧缩略图.sprite = 缩略图; }
                    通用工具.变更激活状态(可选择项目.gameObject, true);
                }

                当前分支所有可选择项目表 = __.所有可选择项目表;
                return;
            }
            else
            {
                var 分支 = UnityEngine.Object.Instantiate(内容区垂直布局组拷贝母体).GetComponent<RectTransform>();
                通用工具.变更激活状态(分支.gameObject, false);
                var 所有可选择项目表 = new List<通用选择面板按钮>();

                foreach (var 参数 in Logicable.LogicSlotTypes)
                {
                    // 读写模式 == IOCheck.Readable && ((ILogicable)包.链接物体).CanLogicRead(参数, 插槽编号)
                    if (((ILogicable)包.链接物体).CanLogicRead(参数, 插槽编号))
                    {
                        包.操作数["参数"] = (int)参数;
                        var 可选择项目 = UnityEngine.Object.Instantiate(通用选择面板按钮拷贝母体, 分支).GetComponent<通用选择面板按钮>();
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
        private void 启用试剂滚动区(通用可选择项目 __) => 启用只读滚动区(__, 所有试剂表);
        private IEnumerable<通用可选择项目.信号模式> 信号模式表 = (IEnumerable<通用可选择项目.信号模式>)Enum.GetValues(typeof(通用可选择项目.信号模式));
        private void 启用信号模式滚动区(通用可选择项目 __) => 启用只读滚动区(__, 信号模式表);
        private IEnumerable<通用可选择项目.ComparisonOperation> 最大最小值运算符表 = (IEnumerable<通用可选择项目.ComparisonOperation>)Enum.GetValues(typeof(通用可选择项目.ComparisonOperation));
        private void 启用最大最小值滚动区(通用可选择项目 __) => 启用只读滚动区(__, 最大最小值运算符表);
        private IEnumerable<GateOperators> 逻辑门运算符表 = (IEnumerable<GateOperators>)Enum.GetValues(typeof(GateOperators));
        private void 启用逻辑门滚动区(通用可选择项目 __) => 启用只读滚动区(__, 逻辑门运算符表);
        private IEnumerable<MathOperatorsUnary> 高等数学运算符表 = (IEnumerable<MathOperatorsUnary>)Enum.GetValues(typeof(MathOperatorsUnary));
        private void 启用高等数学滚动区(通用可选择项目 __) => 启用只读滚动区(__, 高等数学运算符表);
        private IEnumerable<ConditionOperation> 比较运算符表 = (IEnumerable<ConditionOperation>)Enum.GetValues(typeof(ConditionOperation));
        private void 启用比较滚动区(通用可选择项目 __) => 启用只读滚动区(__, 比较运算符表);
        private IEnumerable<LogicReagentMode> 试剂模式表 = (IEnumerable<LogicReagentMode>)Enum.GetValues(typeof(LogicReagentMode));
        private void 启用试剂模式滚动区(通用可选择项目 __) => 启用只读滚动区(__, 试剂模式表);
        private IEnumerable<MathOperators> 基础数学运算符表 = (IEnumerable<MathOperators>)Enum.GetValues(typeof(MathOperators));
        private void 启用基础数学滚动区(通用可选择项目 __) => 启用只读滚动区(__, 基础数学运算符表);
        private IEnumerable<LogicBatchMethod> 统计模式表 = (IEnumerable<LogicBatchMethod>)Enum.GetValues(typeof(LogicBatchMethod));
        private void 启用统计滚动区(通用可选择项目 __) => 启用只读滚动区(__, 统计模式表);
        // -------------------------------------------------------------------------------------------------------------------------------------

        public static void 按钮点击事件()
        {
            if (按钮点击时面板要干什么 != null) { 按钮点击时面板要干什么(当前选择项目); }
            关闭面板(InputPanelState.Submitted);
        }
        public void 面板搜索栏文本变更事件(string str)
        {
            // 统一转换为小写并去除前后空格
            string 条件 = str?.Trim().ToLower() ?? "";

            switch (当前面板类型)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                case 通用可选择项目.数据解包标志.有线网已上线设备:
                case 通用可选择项目.数据解包标志.内部储物:
                case 通用可选择项目.数据解包标志.贸易商船:
                case 通用可选择项目.数据解包标志.厨电配方:
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
        public static bool 打开面板(string 面板标题, 通用可选择项目 原始项目信息)
        {
            // 面板当前正在显示中时不可以重复开关面板;
            if (当前面板状态 != InputPanelState.None || 原始项目信息.解包标志 == 通用可选择项目.数据解包标志.未知) { return false; }

            单例.面板标题.text = 面板标题;
            当前面板状态 = InputPanelState.Waiting;         // 刷新面板状态,请在全局状态中插入检测本面板状态的代码
            MouseModeController.AddModal(单例);

            CursorManager.SetCursor(isLocked: false);       // 解锁时,不需要按下Alt键就可以一直显示光标
            EventSystem.current.SetSelectedGameObject(单例.面板搜索栏.gameObject);  // 将搜索栏设置为输入焦点

            foreach (var __ in 单例.分支缓存池.Values) { 通用工具.变更激活状态(__.Item1.gameObject, false); }

            if (原始项目信息.解包标志 == 通用可选择项目.数据解包标志.有线网已上线设备)
            { 原始项目信息.解包标志 = 通用可选择项目.数据解包标志.物联网已上线设备; }

            当前面板类型 = 原始项目信息.解包标志;

            switch (当前面板类型)
            {
                case 通用可选择项目.数据解包标志.试剂参数:
                    单例.启用试剂滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.逻辑参数:
                    单例.启用逻辑滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.插槽参数:
                    单例.启用插槽滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.插槽编号:
                    单例.启用插槽编号滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.试剂模式:
                    单例.启用试剂模式滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.统计模式:
                    单例.启用统计滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.基础数学运算符:
                    单例.启用基础数学滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.高等数学运算符:
                    单例.启用高等数学滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.比较运算符:
                    单例.启用比较滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.逻辑门运算符:
                    单例.启用逻辑门滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.最大最小值运算符:
                    单例.启用最大最小值滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.物联网信号模式:
                    单例.启用信号模式滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    单例.启用设备滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.有线网已上线设备:
                    单例.启用设备滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.内部储物:
                    单例.启用设备滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.贸易商船:
                    单例.启用设备滚动区(原始项目信息);
                    break;
                case 通用可选择项目.数据解包标志.厨电配方:
                    单例.启用设备滚动区(原始项目信息);
                    break;
            }

            单例.面板搜索栏文本变更事件(单例.面板搜索栏.text);      // 在第一次打开面板时, 手动调用一次搜索
            单例.SetVisible(isVisble: true);                        // 游戏代码(非Unity引擎)设置本面板为输入焦点,并将面板设置为显示

            // LayoutRebuilder.ForceRebuildLayoutImmediate(单例.RectTransform);
            return true;
        }

        public static void 关闭面板(InputPanelState 过渡状态 = InputPanelState.Cancelled)
        {
            CursorManager.SetCursor(isLocked: true);          // 锁定时,只有按下Alt键才显示光标
            当前面板状态 = 过渡状态;                          // 刷新面板状态,请在全局状态中插入检测本面板状态的代码

            单例.SetVisible(isVisble: false);                 // 游戏代码(非Unity引擎)移除本面板的输入焦点,并将面板设置为隐藏
            单例.面板搜索栏.text = string.Empty;

            当前选择项目 = null;
            按钮点击时面板要干什么 = null;               // 面板是通用的,具体的事件需要在打开面板时传入

            当前面板状态 = InputPanelState.None;                // 刷新面板状态
            MouseModeController.RemoveModal(单例);

            foreach (var __ in 单例.分支缓存池.Values) { 通用工具.变更激活状态(__.Item1.gameObject, false); }
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        private static void 将此回调函数赋给全局语言变更事件()
        {
            switch (当前面板类型)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    {
                        foreach (var value in 单例.活跃项目表.Values)
                        {
                            value.右侧文本.text = value.项目信息.链接物体.ToTooltip() + "\n" + Localization.GetSlotTooltip(Slot.Class.None);
                        }
                        break;
                    }
                case 通用可选择项目.数据解包标志.内部储物:
                    {
                        foreach (var value in 单例.活跃项目表.Values)
                        {
                            var 数量 = (!(value.项目信息.链接物体 is Stackable { Quantity: > 1 } stackable)) ? 1 : stackable.Quantity;
                            const string 数量颜色 = "#FFED29";
                            value.右侧文本.text = $"{value.项目信息.链接物体.ToTooltip()} x<color={数量颜色}>{数量}</color>";
                        }
                        break;
                    }
                case 通用可选择项目.数据解包标志.贸易商船:
                    {
                        foreach (var value in 单例.活跃项目表.Values)
                        {
                            const string 颜色 = "#FFED29";
                            value.右侧文本.text = $"<color=green>{value.项目信息.链接贸易商船.DisplayName}</color> <color={颜色}>{value.项目信息.链接贸易商船.Angle.ToString(null, null)}</color>";
                        }
                        break;
                    }
                case 通用可选择项目.数据解包标志.厨电配方:
                    {
                        foreach (var value in 单例.活跃项目表.Values)
                        {
                            value.右侧文本.text = value.项目信息.链接厨电配方.成品.ToTooltip() + "\n" + Localization.GetSlotTooltip(Slot.Class.None);
                        }
                        break;
                    }
            }
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

            已上线设备分支 = UnityEngine.Object.Instantiate(内容区垂直布局组拷贝母体).GetComponent<RectTransform>();
            if (已上线设备分支.parent != 内容区垂直布局组父级) { 已上线设备分支.SetParent(内容区垂直布局组父级, false); }
            通用工具.变更激活状态(已上线设备分支.gameObject, false);
            分支缓存池[通用可选择项目.数据解包标志.物联网已上线设备] = (已上线设备分支, null);
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
    }
}