using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Inventory;
using Assets.Scripts;
using Assets.Scripts.UI;
using UnityEngine.EventSystems;
using Assets.Scripts.Objects;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Assets.Scripts.GridSystem;
using System;
using System.Reflection;
using Assets.Scripts.Util;
using static meanran_xuexi_mods_xiaoyouhua.通用工具;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_kuaijielunpancaidan", "功能模块之快捷轮盘菜单", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之快捷轮盘菜单 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用
            if (GameManager.IsBatchMode) { return; }

            Log = Logger;
            Log.LogMessage("功能模块之快捷轮盘菜单加载完成!");
            补丁 = new Harmony("功能模块之快捷轮盘菜单");
            补丁.PatchAll();

            // 电动工具的四种耗电方式(不只是电动工具, 其它有耗材的工具比如焊枪的燃料消耗也在这四种方式的范围内)
            // 一: 扣除待机耗电和工作耗电, 读条动作完成后, 不额外扣除电量(也就是该工具的动作耗电=0), 这种方式中, 提供UI按钮让你打开电源和工控开关, 以解锁工具使用权
            // 二: 扣除待机耗电和工作耗电, 读条动作完成后, 额外扣除电量(也就是该工具的动作耗电>0), 这种方式中, 提供UI按钮让你打开电源和工控开关, 以解锁工具使用权
            // 三: 电源和工控开关始终关闭, 没有待机耗电和工作耗电, 读条动作完成后, 额外扣除电量(也就是该工具的动作耗电>0), 这种方式中, 没有任何UI按钮让你打开电源和工控开关, 只在满足动作条件时, 在读条动作中临时解锁工具使用权
            // 四: 工控开关始终关闭, 没有工作耗电, 但有待机耗电, 读条动作完成后, 额外扣除电量(也就是该工具的动作耗电>0), 这种方式中, 提供UI按钮让你打开电源, 以解锁工具使用权
            // 开始使用工具事件与结束使用工具事件数据包.注册联机数据包包头类型();

            前置模块.添加初始化事件(快捷键配置.Initialize);
            前置模块.添加初始化事件(快捷轮盘菜单.构造函数);

            {
                var StaticLambda = static () =>
                {
                    if (快捷轮盘菜单.单例 && 快捷轮盘菜单.单例.批量种收 != null)
                    {
                        return 快捷轮盘菜单.单例.批量种收.所有已选择缓存;
                    }
                    else
                    {
                        return null;
                    }
                };

                前置模块.添加通用间接绘制构造参数(new() { (多图层_多物体_批量绘制.图层类型.通用渲染, 0, StaticLambda) }, static () => 快捷键配置.快捷轮盘菜单_批量种植和收获_高亮开关);
            }

            {
                var StaticLambda = static () =>
                {
                    if (快捷轮盘菜单.单例 && 快捷轮盘菜单.单例.批量拆装 != null)
                    {
                        return 快捷轮盘菜单.单例.批量拆装.所有已选择缓存;
                    }
                    else
                    {
                        return null;
                    }
                };

                前置模块.添加通用间接绘制构造参数(new() { (多图层_多物体_批量绘制.图层类型.通用渲染, 1, StaticLambda) }, static () => 快捷键配置.快捷轮盘菜单_批量拆除和装配_高亮开关);
            }
        }
    }

    public partial class 快捷轮盘菜单 : GameBase, IModal, IPointerExitHandler, IPointerClickHandler, ICanvasRaycastFilter
    {
        public static 快捷轮盘菜单 单例;
        public static InputPanelState 当前面板状态 = InputPanelState.None;
        public bool UnlockCursor { get { return true; } }
        public Canvas RootCanvas { get; private set; }
        public RectTransform RootRectTransform { get; private set; }
        public static void 构造函数()
        {
            var Root = new GameObject("快捷轮盘菜单Root").AddComponent<Canvas>();
            Root.renderMode = RenderMode.ScreenSpaceOverlay;
            UnityEngine.Object.DontDestroyOnLoad(Root.gameObject);

            var VL = new GameObject("内容区").AddComponent<VerticalLayoutGroup>();
            VL.transform.SetParent(Root.transform, false);
            VL.gameObject.AddComponent<GraphicRaycaster>();

            VL.childAlignment = TextAnchor.MiddleCenter;
            VL.childControlWidth = false;               // 根据子级的LayoutElement风格组件的配置写入子级的sizeDelta
            VL.childControlHeight = false;
            VL.childForceExpandWidth = false;           // 子级sizeDelta之和+间距之和....小于本级的sizeDelta,则误差平均增加到所有子级的sizeDelta上
            VL.childForceExpandHeight = false;
            VL.childScaleWidth = false;
            VL.childScaleHeight = false;
            VL.spacing = 30;

            var VLRect = VL.GetOrAddComponent<RectTransform>();
            VLRect.pivot = new Vector2(0.5f, 0.5f);
            VLRect.anchoredPosition = Vector2.zero;

            var 吸附到内容 = VLRect.gameObject.AddComponent<ContentSizeFitter>();                // 读取本级和直属子级的UI组件的内容尺寸并经过间距...等处理得到总尺寸写入本级的布局区域
            吸附到内容.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            吸附到内容.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var 菜单 = new GameObject().AddComponent<快捷轮盘菜单>();
            菜单.name = $"{菜单.GetType().Name}";
            菜单.transform.SetParent(VLRect, false);

            单例 = 菜单;
            单例.GameObject = 菜单.gameObject;       // 本级,这三个变量指向同一个层级
            单例.Transform = 菜单.transform;         // 本级,这三个变量指向同一个层级
            单例.RootCanvas = Root;
            单例.RootRectTransform = VLRect;
            单例.初始化(VLRect);
            单例.transform.SetAsLastSibling();              // 按照顺序依次排列

            关闭快捷轮盘菜单();
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
        public static bool 打开快捷轮盘菜单()
        {
            // 面板当前正在显示中时不可以重复开关面板;
            if (当前面板状态 != InputPanelState.None) { return false; }

            // 取消建筑放置时的蓝图高亮全息投影
            InventoryManager.Instance.CheckCancelMultiConstructor();
            当前面板状态 = InputPanelState.Waiting;         // 刷新面板状态,请在全局状态中插入检测本面板状态的代码
            MouseModeController.AddModal(单例);
            CursorManager.SetCursor(isLocked: false);       // 解锁时,不需要按下Alt键就可以一直显示光标
            EventSystem.current.SetSelectedGameObject(单例.gameObject);  // 将层级设置为选中焦点
            单例.刷新快捷面板工具按钮区();
            单例.SetVisible(true);
            通用工具.变更激活状态(单例.RootRectTransform.gameObject, true);

            return true;
        }
        public static void 关闭快捷轮盘菜单(InputPanelState 过渡状态 = InputPanelState.Cancelled)
        {
            当前面板状态 = 过渡状态;                          // 刷新面板状态,请在全局状态中插入检测本面板状态的代码
            CursorManager.SetCursor(isLocked: true);          // 锁定时,只有按下Alt键才显示光标
            单例.SetVisible(false);
            通用工具.变更激活状态(单例.RootRectTransform.gameObject, false);
            当前面板状态 = InputPanelState.None;                // 刷新面板状态
            MouseModeController.RemoveModal(单例);
        }
        // -------------------------------------------------------------------------------------------------------------------------------------
        private Dictionary<(long, Slot), 快捷工具按钮> 活跃项目表 = new();
        private Queue<快捷工具按钮> 休眠节点表 = new();
        private List<Slot> 匹配表 = new();
        public static List<(道具类型, int)> 所有常用工具 = [
            (道具类型.扳手, 无效哈希),
            (道具类型.撬棍, 无效哈希),
            (道具类型.手钻, 无效哈希),
            (道具类型.剪线钳, 无效哈希),
            (道具类型.焊枪, 无效哈希),
            (道具类型.螺丝刀, 无效哈希),
            (道具类型.角磨机, 无效哈希),
            (道具类型.电缆, 无效哈希),
            (道具类型.贴标机, 无效哈希),
            (道具类型.笔记本电脑, 无效哈希),
            (道具类型.平板电脑, 无效哈希),
            (道具类型.采矿钻机, 无效哈希)];

        public void 刷新快捷面板工具按钮区()
        {
            匹配表.Clear();

            for (var i = 0; i < 所有常用工具.Count; ++i)
            {
                if (所有已创建道具匹配条件.TryGetValue(所有常用工具[i], out var 当前条件))
                {
                    var 槽位 = 查找可取出的槽位(当前条件);
                    if (槽位 != null) { 匹配表.Add(槽位); }
                }
                else
                {
                    前置模块.Log.LogDebug($"调用 {MethodBase.GetCurrentMethod().Name} 方法时, 未找到({所有常用工具[i].Item1})");
                }
            }

            for (var i = 活跃项目表.Keys.Count - 1; i >= 0; i--)
            {
                var 当前按钮 = 活跃项目表.Keys.ElementAt(i);
                休眠项目(当前按钮);
            }

            foreach (var 槽位 in 匹配表)
            {
                var thing = 槽位.Get();
                构造或复用项目((thing.ReferenceId, 槽位));
            }
        }
        private void 构造或复用项目((long, Slot) __)
        {
            快捷工具按钮 项目;

            if (休眠节点表.Count > 0) { 项目 = 休眠节点表.Dequeue(); }
            else { 项目 = UnityEngine.Object.Instantiate(快捷工具按钮<快捷工具按钮>.获取拷贝母体(RootCanvas), 快捷工具按钮布局区域).GetComponent<快捷工具按钮>(); 项目.构造初始化(); }
            项目.复用初始化(__.Item2);
            活跃项目表.Add(__, 项目);
            项目.transform.SetAsLastSibling();              // 按照顺序依次排列
            通用工具.变更激活状态(项目.gameObject, true);
        }
        private void 休眠项目((long, Slot) __)
        {
            // 将不使用的项目存放到隐藏池中
            if (!活跃项目表.TryGetValue(__, out var 项目)) { return; }
            通用工具.变更激活状态(项目.gameObject, false);
            活跃项目表.Remove(__);

            const int 限制缓存数 = 200;
            if (休眠节点表.Count > 限制缓存数)
            { UnityEngine.Object.Destroy(项目.gameObject); }
            else { 休眠节点表.Enqueue(项目); }
        }
        public RectTransform 快捷工具按钮布局区域;
        private TextMeshProUGUI 轮盘中心提示文本;
        private RectTransform 轮盘菜单;
        private RectTransform 高亮扇区;
        private float 外圈距离;
        private float 内圈距离;
        public int 扇区数量 => 所有快捷命令.Length;
        public static readonly 快捷命令[] 所有快捷命令 = (快捷命令[])Enum.GetValues(typeof(快捷命令));
        public enum 快捷命令 { 建造和升级 = 0, 拆卸, 维修, 双手物品收纳, 背包物品合并, 拾取补充主手, }
        public int 扇区索引 { get; private set; }
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (扇区索引)
            {
                case (int)快捷命令.建造和升级:
                    获取建造或升级工具和材料();
                    break;
                case (int)快捷命令.拆卸:
                    获取拆卸工具和材料();
                    break;
                case (int)快捷命令.维修:
                    获取维修工具和材料();
                    break;
                case (int)快捷命令.双手物品收纳:
                    槽位API.收纳双手槽位物品到背包().Forget();
                    break;
                case (int)快捷命令.背包物品合并:
                    槽位API.整理背包();
                    break;
                case (int)快捷命令.拾取补充主手:
                    槽位API.自动拾取补充活动手至满堆垛().Forget();
                    break;
            }

            关闭快捷轮盘菜单();
        }
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // 在轮盘内时, 每帧调用       
            //  RectTransform.pivot = new Vector2(0.5f, 0.5f);
            // 通过以上语句将RectTransform的本地坐标轴(pivot)设为几何的中心点, 这样RectTransform的position就表示几何的中心点
            var 相对 = 轮盘菜单.position - Input.mousePosition;
            var 距离 = Vector3.Magnitude(相对);

            if (距离 < 外圈距离)
            {
                if (距离 > 内圈距离)
                {
                    // 在扇区时
                    var 角度 = (Mathf.Atan2(相对.y, 相对.x) * Mathf.Rad2Deg) + 180;      // Atan2返回的结果是正180至负180之间
                    var 扇区角 = 工具.FULL_CIRCLE / 扇区数量;
                    扇区索引 = Mathf.Clamp(Mathf.FloorToInt(角度 / 扇区角), 0, 扇区数量 - 1);
                    轮盘中心提示文本.text = Enum.GetName(typeof(快捷命令), 所有快捷命令[扇区索引]);
                    高亮扇区.eulerAngles = new Vector3(0, 0, 扇区索引 * 扇区角);
                    通用工具.变更激活状态(高亮扇区.gameObject, true);
                    return true;
                }

                // 在内圈时
                扇区索引 = -1;
                轮盘中心提示文本.text = "关闭";
                通用工具.变更激活状态(高亮扇区.gameObject, false);
                return true;
            }

            return false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 一帧时长下, 鼠标大幅度位移, 从轮盘内移动到轮盘外
            扇区索引 = -1;
            通用工具.变更激活状态(高亮扇区.gameObject, false);
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.SetupKeyBindings))]
    public class 快捷键配置
    {
        public static void Postfix()
        {
            通用工具.创建游戏主菜单按键键位配置选项卡布局组(按键布局组名称);
            通用工具.创建游戏主菜单按键键位配置选项卡(按键名称兼索引key, 初始默认按键, 按键布局组名称);
        }

        // 快捷轮盘菜单自身的快捷键开关
        public const string 按键布局组名称 = "KuaiJie";
        public const string 按键名称兼索引key = "KuaiJieLunPan";
        public const KeyCode 初始默认按键 = KeyCode.Mouse2;

        private static void 点击了监听键位()
        {
            if (快捷轮盘菜单.单例 == null) { return; }
            if (快捷轮盘菜单.当前面板状态 == InputPanelState.None) { 快捷轮盘菜单.打开快捷轮盘菜单(); }
            else { 快捷轮盘菜单.关闭快捷轮盘菜单(); }
        }
        public static void Initialize()
        {
            new 通用工具.按键键位状态轮询组件(按键名称兼索引key, 初始默认按键, 点击了监听键位);
        }
        public static void Dispose()
        {
            if (通用工具.按键键位状态轮询组件.所有按键键位状态轮询组件.TryGetValue(按键名称兼索引key, out var 按键键位轮询组件))
            {
                按键键位轮询组件.Dispose();
            }
        }

        // 高亮显示布尔值开关
        public static bool 快捷轮盘菜单_批量种植和收获_高亮开关 { get; set; }
        public static bool 快捷轮盘菜单_批量拆除和装配_高亮开关 { get; set; }
    }














    public partial class 快捷轮盘菜单 : GameBase, IPointerClickHandler, ICanvasRaycastFilter
    {
        public 批量种植和收获 批量种收 { get; private set; }
        public 批量拆除和装配 批量拆装 { get; private set; }

        private void 初始化(RectTransform VLRect)
        {
            var 外圈直径 = 370;
            var 内圈直径 = (int)(外圈直径 * 0.35f);
            var 轮盘纹理 = 工具.创建背景贴图(外圈直径, 内圈直径, 扇区数量);
            var 高亮扇区贴图 = 工具.创建高亮扇区贴图(外圈直径, 内圈直径, 扇区数量, 收缩: 3, 外扩: 3);

            外圈距离 = 外圈直径 / 2;
            内圈距离 = 内圈直径 / 2;

            var 轮盘 = new GameObject("轮盘");
            轮盘.transform.SetParent(transform, false);

            var 轮盘纹理组件 = 轮盘.AddComponent<RawImage>();
            轮盘纹理组件.raycastTarget = true;

            轮盘纹理组件.texture = 轮盘纹理;
            var 纹理区域 = 轮盘纹理组件.rectTransform;
            纹理区域.sizeDelta = new Vector2(外圈直径, 外圈直径);
            纹理区域.pivot = new Vector2(0.5f, 0.5f);
            纹理区域.anchoredPosition = Vector2.zero;
            轮盘菜单 = 纹理区域;

            var 菜单Rect = this.GetOrAddComponent<RectTransform>();
            菜单Rect.sizeDelta = 纹理区域.sizeDelta;
            菜单Rect.pivot = 纹理区域.pivot;
            菜单Rect.anchoredPosition = 纹理区域.anchoredPosition;

            var 当前选择 = new GameObject("当前选择");
            当前选择.transform.SetParent(纹理区域, false);

            轮盘中心提示文本 = 当前选择.AddComponent<TextMeshProUGUI>();
            轮盘中心提示文本.raycastTarget = false;

            轮盘中心提示文本.font = 前置_资源加载器.单例.当前TMP字体;
            轮盘中心提示文本.fontSize = 15;
            轮盘中心提示文本.characterSpacing = 13;
            轮盘中心提示文本.alignment = TextAlignmentOptions.Center;

            var 提示区域 = 轮盘中心提示文本.rectTransform;
            提示区域.pivot = new Vector2(0.5f, 0.5f);
            提示区域.anchoredPosition = Vector2.zero;

            var 高亮 = new GameObject("高亮扇区");
            高亮.transform.SetParent(纹理区域, false);

            var 高亮扇区纹理组件 = 高亮.AddComponent<RawImage>();
            高亮扇区纹理组件.raycastTarget = false;

            高亮扇区纹理组件.texture = 高亮扇区贴图;
            var 高亮区域 = 高亮扇区纹理组件.rectTransform;
            高亮区域.sizeDelta = new Vector2(外圈直径, 外圈直径);
            高亮区域.pivot = new Vector2(0.5f, 0.5f);
            高亮区域.anchoredPosition = Vector2.zero;
            高亮扇区 = 高亮区域;

            {
                // 为每个扇区创建静态标签文字
                float 扇区角 = 工具.FULL_CIRCLE / 扇区数量;
                // 标签半径：取内外半径中间偏外一些的位置
                float outerR = 外圈直径 * 0.5f;
                float innerR = 内圈直径 * 0.5f;
                float labelRadius = (outerR + innerR) * 0.5f; // 可调：0.5=居中, 其它更靠扇区外/内

                for (var i = 0; i < 扇区数量; ++i)
                {
                    var 扇区文本 = new GameObject($"扇区文本_{i}");
                    扇区文本.transform.SetParent(纹理区域, false);

                    var 文本组件 = 扇区文本.AddComponent<TextMeshProUGUI>();
                    文本组件.raycastTarget = false;

                    文本组件.font = 前置_资源加载器.单例.当前TMP字体;
                    文本组件.text = Enum.GetName(typeof(快捷命令), 所有快捷命令[i]);
                    文本组件.fontSize = 15;
                    文本组件.alignment = TextAlignmentOptions.Center;
                    文本组件.enableWordWrapping = false;
                    文本组件.color = Color.white;

                    // 计算位置（以像素为单位的 anchoredPosition）
                    float midAngle = (i + 0.5f) * 扇区角; // 中心角度（度）
                    float rad = midAngle * Mathf.Deg2Rad;
                    float px = Mathf.Cos(rad) * labelRadius;
                    float py = Mathf.Sin(rad) * labelRadius;

                    var rt = 文本组件.rectTransform;
                    rt.sizeDelta = new Vector2(外圈直径 * 0.4f, 24); // 宽度可调，防止换行
                    rt.anchoredPosition = new Vector2(px, py);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                }
            }

            {
                var 工具 = new GameObject("快捷工具");
                工具.transform.SetParent(VLRect, false);

                var HL = 工具.AddComponent<HorizontalLayoutGroup>();
                HL.childAlignment = TextAnchor.MiddleCenter;
                HL.childControlWidth = false;               // 根据子级的LayoutElement风格组件的配置写入子级的sizeDelta
                HL.childControlHeight = false;
                HL.childForceExpandWidth = false;           // 子级sizeDelta之和+间距之和....小于本级的sizeDelta,则误差平均增加到所有子级的sizeDelta上
                HL.childForceExpandHeight = false;
                HL.childScaleWidth = false;
                HL.childScaleHeight = false;
                HL.spacing = 8;

                快捷工具按钮布局区域 = HL.GetOrAddComponent<RectTransform>();
                快捷工具按钮布局区域.pivot = new Vector2(0.5f, 0.5f);
                快捷工具按钮布局区域.anchoredPosition = Vector2.zero;

                var 吸附到内容 = 工具.AddComponent<ContentSizeFitter>();                // 读取本级和直属子级的UI组件的内容尺寸并经过间距...等处理得到总尺寸写入本级的布局区域
                吸附到内容.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                吸附到内容.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (批量种收 == null) { 批量种收 = 批量种植和收获.构造函数<批量种植和收获>(); }

            {
                var 种收 = new GameObject("批量种收");
                种收.transform.SetParent(VLRect, false);

                var HL = 种收.AddComponent<HorizontalLayoutGroup>();
                HL.childAlignment = TextAnchor.MiddleCenter;
                HL.childControlWidth = false;               // 根据子级的LayoutElement风格组件的配置写入子级的sizeDelta
                HL.childControlHeight = false;
                HL.childForceExpandWidth = false;           // 子级sizeDelta之和+间距之和....小于本级的sizeDelta,则误差平均增加到所有子级的sizeDelta上
                HL.childForceExpandHeight = false;
                HL.childScaleWidth = false;
                HL.childScaleHeight = false;
                HL.spacing = 8;

                var 种收命令布局区域 = HL.GetOrAddComponent<RectTransform>();
                种收命令布局区域.pivot = new Vector2(0.5f, 0.5f);
                种收命令布局区域.anchoredPosition = Vector2.zero;

                var 吸附到内容 = 种收.AddComponent<ContentSizeFitter>();
                吸附到内容.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                吸附到内容.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                var 拷贝母体 = 快捷工具按钮<批量操作悬停提示>.获取拷贝母体(RootCanvas);
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 种收命令布局区域);
                    按钮.构造初始化("开关高亮", "单击此按钮后, 关闭轮盘菜单窗口, 并开启高亮显示\n按鼠标右键关闭高亮显示",
                    static () =>
                    {
                        if (单例.批量种收.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量种收.目标状态 = 批量操作任务状态.开关高亮;
                        单例.批量种收.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 种收命令布局区域);
                    按钮.构造初始化("选择", "单击此按钮后, 关闭轮盘菜单窗口, 进入选择状态\n鼠标左键单击水培托盘添加或者取消高亮选择\n按鼠标右键退出选择状态",
                    static () =>
                    {
                        if (单例.批量种收.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量种收.目标状态 = 批量操作任务状态.选择;
                        单例.批量种收.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 种收命令布局区域);
                    按钮.构造初始化("种植", "活动手持有可种植的果实或者种子时, 从所有已选择水培托盘中找到空托盘\n在读条结束后种植所有空托盘, 读条期间按鼠标右键取消操作",
                    static () =>
                    {
                        if (单例.批量种收.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量种收.目标状态 = 批量操作任务状态.种植;
                        单例.批量种收.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 种收命令布局区域);
                    按钮.构造初始化("收获", "活动手持有收获目标(果实或者种子)并且堆垛未满时, 从所有已选择水培托盘中找到可收获托盘\n在读条结束后对每个可收获托盘收获一次, 读条期间按鼠标右键取消操作\n例: 手上是土豆种子, 则只收获已结种的土豆托盘\n例: 手上是土豆, 则跳过已结种的土豆托盘, 只收获未结种但已经成熟的土豆托盘",
                    static () =>
                    {
                        if (单例.批量种收.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量种收.目标状态 = 批量操作任务状态.收获;
                        单例.批量种收.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 种收命令布局区域);
                    按钮.构造初始化("收获所有", "收获指令的加强版, 对同一个可收获托盘尝试连续收获\n例: 土豆托盘有N个成熟果实, 该指令就连续收获直到托盘为空或者活动手堆垛已满",
                    static () =>
                    {
                        if (单例.批量种收.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量种收.目标状态 = 批量操作任务状态.收获所有;
                        单例.批量种收.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 种收命令布局区域);
                    按钮.构造初始化("清空高亮", "清空所有已选择水培托盘",
                    static () =>
                    {
                        if (单例.批量种收.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量种收.目标状态 = 批量操作任务状态.清空高亮;
                        单例.批量种收.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
            }



            if (批量拆装 == null) { 批量拆装 = 批量拆除和装配.构造函数<批量拆除和装配>(); }

            {
                var 拆装 = new GameObject("批量拆装");
                拆装.transform.SetParent(VLRect, false);

                var HL = 拆装.AddComponent<HorizontalLayoutGroup>();
                HL.childAlignment = TextAnchor.MiddleCenter;
                HL.childControlWidth = false;               // 根据子级的LayoutElement风格组件的配置写入子级的sizeDelta
                HL.childControlHeight = false;
                HL.childForceExpandWidth = false;           // 子级sizeDelta之和+间距之和....小于本级的sizeDelta,则误差平均增加到所有子级的sizeDelta上
                HL.childForceExpandHeight = false;
                HL.childScaleWidth = false;
                HL.childScaleHeight = false;
                HL.spacing = 8;

                var 拆装命令布局区域 = HL.GetOrAddComponent<RectTransform>();
                拆装命令布局区域.pivot = new Vector2(0.5f, 0.5f);
                拆装命令布局区域.anchoredPosition = Vector2.zero;

                var 吸附到内容 = 拆装.AddComponent<ContentSizeFitter>();
                吸附到内容.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                吸附到内容.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                var 拷贝母体 = 快捷工具按钮<批量操作悬停提示>.获取拷贝母体(RootCanvas);
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 拆装命令布局区域);
                    按钮.构造初始化("开关高亮", "单击此按钮后, 关闭轮盘菜单窗口, 并开启高亮显示\n按鼠标右键关闭高亮显示",
                    static () =>
                    {
                        if (单例.批量拆装.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量拆装.目标状态 = 批量操作任务状态.开关高亮;
                        单例.批量拆装.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 拆装命令布局区域);
                    按钮.构造初始化("选择", "单击此按钮后, 关闭轮盘菜单窗口, 进入选择状态\n鼠标左键单击建筑添加或者取消高亮选择(只可选择墙体和框架), 按鼠标右键退出选择状态",
                    static () =>
                    {
                        if (单例.批量拆装.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量拆装.目标状态 = 批量操作任务状态.选择;
                        单例.批量拆装.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 拆装命令布局区域);
                    按钮.构造初始化("选择支路", "单击此按钮后, 关闭轮盘菜单窗口, 进入选择支路状态\n鼠标左键单击建筑添加或者取消高亮选择, 按鼠标右键退出选择状态\n添加条件: 在当前已选择数等于0时, 添加支路上所有电缆(或管道或滑槽)\n取消条件: 取消支路上的所有电缆(或管道或滑槽)\n选择支路可能会选中一些无关建筑, 需要手动点击选择按钮进行减选",
                    static () =>
                    {
                        if (单例.批量拆装.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量拆装.目标状态 = 批量操作任务状态.选择支路;
                        单例.批量拆装.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 拆装命令布局区域);
                    按钮.构造初始化("框选", "单击此按钮后, 关闭轮盘菜单窗口, 进入框选状态\n鼠标左键单击建筑添加或者取消高亮选择(只可选择墙体和框架), 按鼠标右键退出框选状态和单选状态\n框选条件: 在当前已选择数等于0时, 添加一个建筑作为框选起点并作为框选目标类型(例: 墙体作为框选目标类型, 则框选时跳过框架类型)\n再次单击该建筑, 可取消框选起点\n有了框选起点后, 添加第二个建筑(墙体和框架都行)作为框选终点\n然后会自动扫描网格将所有找到的框选目标类型加入高亮, 并切换到选择状态(用于加选和减选)",
                    static () =>
                    {
                        if (单例.批量拆装.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量拆装.目标状态 = 批量操作任务状态.框选;
                        单例.批量拆装.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 拆装命令布局区域);
                    按钮.构造初始化("拆除", "活动手持有拆除工具, 并且空闲手可以容纳返还材料时(空手或者堆垛未满), 从所有已选择建筑中找到可拆除建筑\n在读条结束后执行拆除, 读条期间按鼠标右键取消操作\n同一个建筑的不同建造阶段都有对应的拆除条件, 需要多次切换拆除工具和清空空闲手\n为了防止拆除后返还材料直接爆在地上找不到, 只有拆除工具/返还材料/返还材料数量全部一致的建筑才能在同一批次拆除",
                    static () =>
                    {
                        if (单例.批量拆装.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量拆装.目标状态 = 批量操作任务状态.拆除;
                        单例.批量拆装.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 拆装命令布局区域);
                    按钮.构造初始化("装配", "双手持有装配工具和装配材料时, 从所有已选择建筑中找到可装配(升级)建筑\n在读条结束后执行装配, 读条期间按鼠标右键取消操作\n同一个建筑的不同建造阶段都有对应的装配条件, 需要多次切换装配工具与材料",
                    static () =>
                    {
                        if (单例.批量拆装.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量拆装.目标状态 = 批量操作任务状态.装配;
                        单例.批量拆装.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
                {
                    var 按钮 = UnityEngine.Object.Instantiate(拷贝母体, 拆装命令布局区域);
                    按钮.构造初始化("清空高亮", "清空所有已选择建筑",
                    static () =>
                    {
                        if (单例.批量拆装.当前状态 != 批量操作任务状态.睡眠) { return; }
                        单例.批量拆装.目标状态 = 批量操作任务状态.清空高亮;
                        单例.批量拆装.执行批量操作().Forget();
                    });
                    按钮.左侧缩略图.color = Color.green.SetAlpha(0.01f);
                    通用工具.变更激活状态(按钮.gameObject, true);
                }
            }
        }
    }
}










