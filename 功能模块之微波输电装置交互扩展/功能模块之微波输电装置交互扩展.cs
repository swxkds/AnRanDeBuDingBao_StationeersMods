using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_weiboshudianzhuangzhijiaohukuozhan", "功能模块之微波输电装置交互扩展", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之微波输电装置交互扩展 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之微波输电装置交互扩展加载完成!");
            补丁 = new Harmony("功能模块之微波输电装置交互扩展");
            补丁.PatchAll();
            StartCoroutine(并发构造());  // Unity引擎对第三方协程库的支持是后期开发的,因此API实例的加载顺序挺靠后,在Awake时请使用Unity原生协程
        }
        private IEnumerator 并发构造()
        {
            // InventoryWindowManager.Instance.WindowGrid:垂直排版多个背包窗口的布局组
            // InventoryWindowManager.Instance.WindowPrefab:背包窗口克隆母体
            while (InventoryWindowManager.Instance == null
            || InventoryWindowManager.Instance.WindowGrid == null
            || InventoryWindowManager.Instance.WindowPrefab == null)
            { yield return null; }    // 等待下一帧

            if (前置_资源加载器.单例.逻辑组件字典.TryGetValue(typeof(InputPrefabs), out var 选择配方面板))
            {
                var inputPrefabs = (InputPrefabs)选择配方面板.FirstOrDefault();
                if (inputPrefabs == null)
                {
                    补丁.UnpatchAll();
                    Log.LogMessage("功能模块之微波输电装置交互扩展_构造选择面板失败");
                }
                else
                {
                    var 面板标题 = inputPrefabs.TitleText;
                    面板标题.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之微波输电装置交互扩展工具.修改字体尺寸(面板标题, 23);
                    面板标题.overflowMode = TextOverflowModes.Overflow;

                    var 取消按钮 = inputPrefabs.transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>();
                    取消按钮.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之微波输电装置交互扩展工具.修改字体尺寸(取消按钮, 23);

                    var 搜索栏 = inputPrefabs.SearchBar;
                    搜索栏.fontAsset = 前置_资源加载器.单例.当前TMP字体;

                    var 输入框输入显示 = 搜索栏.textComponent;
                    输入框输入显示.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之微波输电装置交互扩展工具.修改字体尺寸(输入框输入显示, 22);

                    var 输入框淡色提示 = (TMP_Text)搜索栏.placeholder;
                    输入框淡色提示.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之微波输电装置交互扩展工具.修改字体尺寸(输入框淡色提示, 22);

                    var 配方大类标题 = inputPrefabs.ControlGroupPrefab.Title;
                    配方大类标题.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之微波输电装置交互扩展工具.修改字体尺寸(配方大类标题, 29);

                    var 配方描述 = inputPrefabs.PrefabReference.Text;
                    配方描述.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之微波输电装置交互扩展工具.修改字体尺寸(配方描述, 27);

                    var 新 = UnityEngine.Object.Instantiate(inputPrefabs.gameObject, inputPrefabs.transform.parent, false);

                    新.gameObject.name = "选择目标面板";
                    选择目标面板.单例 = 新.gameObject.AddComponent<选择目标面板>();
                    var 旧 = 新.GetComponent<InputPrefabs>();

                    选择目标面板.单例.面板标题 = 旧.TitleText;
                    选择目标面板.单例.面板标题.text = "选择目标面板";

                    选择目标面板.单例.内容区垂直布局组预制体 = 旧.GroupParents;
                    选择目标面板.单例.内容区垂直布局组预制体.name = "选择目标面板分支布局预制体";

                    选择目标面板.单例.内容区垂直布局组父级 = 选择目标面板.单例.内容区垂直布局组预制体.parent;
                    选择目标面板.单例.内容区垂直布局组预制体.SetParent(null, false);
                    选择目标面板.单例.内容区垂直布局组预制体.gameObject.SetActive(false);

                    for (int i = 选择目标面板.单例.内容区垂直布局组预制体.childCount - 1; i >= 0; i--)
                    { UnityEngine.Object.DestroyImmediate(选择目标面板.单例.内容区垂直布局组预制体.GetChild(i).gameObject); }     // 预制体自带一些大类层级需要删除掉

                    var __ = UnityEngine.Object.Instantiate(inputPrefabs.PrefabReference);
                    __.transform.SetParent(null, false);
                    __.gameObject.SetActive(false);
                    var 新__ = __.gameObject.AddComponent<选择目标面板_可选择项目>();
                    var 旧__ = __.GetComponent<PrefabReference>();
                    新__.左侧缩略图 = 旧__.Thumbnail;
                    新__.右侧文本 = 旧__.Text;
                    新__.按钮 = 旧__.GetComponent<Button>();

                    UnityEngine.Object.Destroy(旧__);
                    选择目标面板.单例.可选择项目预制体 = 新__.gameObject;
                    选择目标面板.单例.可选择项目预制体.name = "选择目标面板_可选择项目预制体";

                    选择目标面板.单例.面板搜索栏 = 旧.SearchBar;
                    选择目标面板.单例.UiComponentRenderer = 旧.UiComponentRenderer; // 等于null
                    选择目标面板.单例.GameObject = 旧.GameObject;       // 本级,这三个变量指向同一个层级
                    选择目标面板.单例.Transform = 旧.Transform;         // 本级,这三个变量指向同一个层级
                    选择目标面板.单例.RectTransform = 旧.RectTransform; // 本级,这三个变量指向同一个层级

                    选择目标面板.单例.关闭面板按钮 = 选择目标面板.单例.transform.GetChild(0).GetChild(4).GetComponent<Button>();
                    选择目标面板.单例.滚动组件 = 选择目标面板.单例.transform.GetChild(0).GetChild(7).GetComponent<ScrollRectNoDrag>();

                    UnityEngine.Object.Destroy(旧);
                    选择目标面板.单例.Initialize();

                    Log.LogMessage($"功能模块之微波输电装置交互扩展_构造选择面板成功");
                }
            }
        }
    }

    public class 选择目标面板_可选择项目 : UserInterfaceAnimated, IScreenSpaceTooltip
    {
        public 数据包_选择目标面板 数据包;
        public string DisplayName => 数据包.包头 == 数据包_选择目标面板.数据解包标志.物联网已上线设备 ? 数据包.链接物体.DisplayName : 右侧文本.text;
        public bool TooltipIsVisible => IsVisible;
        public Image 左侧缩略图;
        public TextMeshProUGUI 右侧文本;
        public Button 按钮;
        public void 将此回调函数赋给提交按钮的点击事件()
        {
            // 在构造按钮时,将这个方法赋给按钮的onClick;
            // 左侧缩略图和右侧文本和按钮和可选择项目都在同一个布局,可选择项目相当于是对外暴露的API(管理纹理与文本、按钮事件)
            选择目标面板.当前选择 = this;
            选择目标面板.将此回调函数赋给提交按钮的点击事件();
        }
        public void 构造初始化()
        {
            GameObject = gameObject;        // 原版游戏有些函数依赖GameObject
            按钮.onClick.RemoveAllListeners();
            按钮.onClick.AddListener(将此回调函数赋给提交按钮的点击事件);
        }
        public void 复用初始化(数据包_选择目标面板 包)
        {
            if (数据包 == null) { 数据包 = 数据包_选择目标面板.拷贝构造(ref 包); }
            else { 数据包_选择目标面板.拷贝赋值(ref 包, ref 数据包); }

            if (数据包.包头 == 数据包_选择目标面板.数据解包标志.物联网已上线设备)
            {
                // 每次显示前,更新右侧文本为该物体被贴标机修改过的最新DisplayName
                左侧缩略图.sprite = 数据包.链接物体.Thumbnail;
                右侧文本.text = 数据包.链接物体.ToTooltip() + "\n" + Localization.GetSlotTooltip(Slot.Class.None);
            }
        }
        public string 交互提示面板内容() { return Localization.GetSlotTooltip(Slot.Class.None) + "\n模组制作真好玩"; }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            // 此函数由Unity引擎自动调用,当光标进入感应区域时调用
            base.OnPointerEnter(eventData);
            PanelToolTip.Instance.SetUpToolTip(DisplayName, 交互提示面板内容(), this);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            // 此函数由Unity引擎自动调用,当光标离开感应区域时调用
            base.OnPointerExit(eventData);
            PanelToolTip.Instance.ClearToolTip();
        }
        public void DoUpdate()
        {
            // 在OnPointerEnter方法中将this赋给PanelToolTipScreenSpace._tooltipToUpdate,并激活交互提示面板
            // 在主循环中由PanelToolTipScreenSpace.LateUpdate方法调用_tooltipToUpdate.DoUpdate方法实时变更显示内容
            PanelToolTip.Instance.SetInfoText(交互提示面板内容());
        }
    }

    public class 数据包_选择目标面板
    {
        public enum 数据解包标志
        {
            未知,
            物联网已上线设备,有线网已上线设备
        }
        public enum 信号模式 { 直连, 桥接 }
        public 数据解包标志 包头;
        public Thing 主物体;
        public Interactable 控件;
        public Thing 链接物体;
        public object 物联网已上线设备表或试剂引用;
        public Dictionary<string, int> 操作数;
        public 数据包_选择目标面板() { 操作数 = new(); }
        public 数据包_选择目标面板(数据解包标志 包头, Thing 主物体, Interactable 控件, Thing 链接物体, object 物联网已上线设备表或试剂引用, params (string, int)[] 操作数) : this()
        {
            this.包头 = 包头;
            this.主物体 = 主物体;
            this.控件 = 控件;
            this.链接物体 = 链接物体;
            this.物联网已上线设备表或试剂引用 = 物联网已上线设备表或试剂引用;
            foreach (var __ in 操作数) { this.操作数.Add(__.Item1, __.Item2); }
        }
        public static void 拷贝赋值(ref 数据包_选择目标面板 源, ref 数据包_选择目标面板 目标)
        {
            目标.包头 = 源.包头;
            目标.主物体 = 源.主物体;
            目标.控件 = 源.控件;
            目标.链接物体 = 源.链接物体;
            目标.物联网已上线设备表或试剂引用 = 源.物联网已上线设备表或试剂引用;
            目标.操作数.Clear();
            目标.操作数.AddRange(源.操作数);
        }
        public static 数据包_选择目标面板 拷贝构造(ref 数据包_选择目标面板 源)
        {
            var 新包 = new 数据包_选择目标面板(源.包头, 源.主物体, 源.控件, 源.链接物体, 源.物联网已上线设备表或试剂引用);
            新包.操作数.AddRange(源.操作数);
            return 新包;
        }
    }

    public static class 功能模块之微波输电装置交互扩展工具
    {
        public static void 修改字体尺寸(TMP_Text component, float fontSize)
        {
            if (component == null)
            {
                功能模块之微波输电装置交互扩展.Log.LogDebug($"TMP_Text是空引用,无法修改字体尺寸");
                return;
            }

            if (component.fontSize < fontSize)
            {
                component.fontSize = fontSize;
                component.fontSizeMin = fontSize;
            }
            if (component.fontSizeMax < fontSize) { component.fontSizeMax = fontSize; }
        }

        public static void 修改字体尺寸(UnityEngine.UI.Text component, int fontSize)
        {
            if (component == null)
            {
                功能模块之微波输电装置交互扩展.Log.LogDebug($"Text是空引用,无法修改字体尺寸");
                return;
            }

            if (component.fontSize < fontSize)
            {
                component.fontSize = fontSize;
                component.resizeTextMinSize = fontSize;
            }
            if (component.resizeTextMaxSize < fontSize) { component.resizeTextMaxSize = fontSize; }
        }
    }

    [HarmonyPatch(typeof(InputWindowBase), nameof(InputWindowBase.IsInputWindow), MethodType.Getter)]
    public class 修改_全局状态变更_获取面板状态_选择目标面板
    {
        [HarmonyPostfix]
        public static void 获取面板状态(ref bool __result)
        {
            // __result = true, 则全局状态 = Waiting , 跳过玩家的交互系统以专注于面板操作
            if (__result != true) { __result = 选择目标面板.当前面板状态 == InputPanelState.Waiting; }
        }
    }

    [HarmonyPatch(typeof(InputWindowBase), nameof(InputWindowBase.Cancel))]
    public class 修改_全局状态变更_关闭所有面板事件_选择目标面板
    {
        [HarmonyPostfix]
        public static void 关闭所有面板()
        {
            // 全局状态是提交或者退出,关闭所有面板
            if (KeyManager.InputState != KeyInputState.Paused || InputSourceCode.InputState != InputPanelState.Waiting)
            {
                选择目标面板.关闭面板();
            }
        }
    }
}