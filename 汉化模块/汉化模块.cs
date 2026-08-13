using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using Assets.Scripts.UI.HelperHints;
using Assets.Scripts.UI.ImGuiUi;
using BepInEx;
using CharacterCustomisation;
using HarmonyLib;
using ImGuiNET;
using Objects.Items;
using TerrainSystem;
using TMPro;
using UI.ImGuiUi;
using UI.ImGuiUi.ImGuiWindows;
using UI.LoadGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.Objects.Motherboards;
using UI.PhaseChange;
using UI.Motherboard;
using Util;
using UI.Dropdown;
using UI;
using UnityEngine.Rendering;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_hanhua_mokuai", "汉化模块", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 汉化模块 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private static 汉化模块 单例 = null;
        private void Awake()
        {
            // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用
            if (GameManager.IsBatchMode) { return; }

            Log = Logger;
            Log.LogMessage("汉化模块加载完成!");
            补丁 = new Harmony("汉化模块");
            补丁.PatchAll();

            var a = 资源加载器.单例;

            关闭字体警报();
            单例 = this;
            前置模块.添加初始化事件(汉化硬编码);
        }

        private static void 汉化硬编码()
        {
            InventoryWindowManager.Instance.WindowGrid.spacing = 5f;
            Log.LogMessage($"成功修改背包UI间距");

            var 进度条标题 = (TextMeshProUGUI)Traverse.Create(InventoryManager.Instance.UIProgressionBar).Field("_progressBarItemName").GetValue();
            var 进度条内容 = (TextMeshProUGUI)Traverse.Create(InventoryManager.Instance.UIProgressionBar).Field("_progressBarAction").GetValue();

            汉化模块工具.修改字体尺寸(进度条标题, 24);
            汉化模块工具.修改字体尺寸(进度条内容, 24);
            Log.LogMessage($"成功修改动作进度条字体尺寸");

            var 背包窗口克隆母体 = InventoryWindowManager.Instance.WindowPrefab;

            var 插槽母体 = 背包窗口克隆母体.ButtonPrefab;
            汉化模块工具.修改字体尺寸(插槽母体.Text, 21);
            汉化模块工具.修改字体尺寸(插槽母体.Text2, 22);
            // 汉化模块工具.修改字体尺寸(插槽母体.SecondaryName, 插槽母体.SecondaryName.fontSize + 2);  // 物品栏中物品的名称,不改物品栏尺寸就不要改字体尺寸
            汉化模块工具.修改字体尺寸((TMP_Text)Traverse.Create(插槽母体).Field("QuantityText").GetValue(), 22);

            var 控件母体 = 背包窗口克隆母体.InteractionPrefab;  // 例:太空服上的 加压/减压/提高温度 控件按钮
            汉化模块工具.修改字体尺寸(控件母体.Text, 22);
            汉化模块工具.修改字体尺寸(控件母体.Text2, 22);
            汉化模块工具.修改字体尺寸(控件母体.SecondaryName, 22);
            汉化模块工具.修改字体尺寸((TMP_Text)Traverse.Create(控件母体).Field("QuantityText").GetValue(), 22);

            unsafe
            {
                QuantityFontMinSize = QuantityFontMinSizeRefGetter();
                QuantityFontMaxSize = QuantityFontMaxSizeRefGetter();
                QuantitySizeDeltaX = QuantitySizeDeltaXRefGetter();
                QuantitySizeDeltaY = QuantitySizeDeltaYRefGetter();
                ref var MinSize = ref *(float*)QuantityFontMinSize.ToPointer();  //     14f;      
                ref var MaxSize = ref *(float*)QuantityFontMaxSize.ToPointer(); //     20f;  
                MinSize = 22f;
                MaxSize = 22f;
            }

            if (前置_资源加载器.单例.TryGetAllComponent<WindowTitleBar>(out var 标题栏))
            {
                foreach (WindowTitleBar component in 标题栏)
                {
                    汉化模块工具.修改字体尺寸(component.TitleText, 22);
                    Log.LogMessage($"成功修改背包窗口标题栏字体尺寸");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<ClothingPanel>(out var 玩家服装栏))
            {
                foreach (ClothingPanel clothingPanel in 玩家服装栏)
                {
                    foreach (var component in clothingPanel.transform.GetComponentsInChildren<TMP_Text>(includeInactive: true))
                    {
                        if (component.gameObject.name == "Key")
                        { 汉化模块工具.修改字体尺寸(component, 28); }   // 服装栏的编号是数字
                        else
                        { 汉化模块工具.修改字体尺寸(component, 22); }
                    }
                    Log.LogMessage($"成功修改玩家服装栏字体尺寸");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<PlayerStateWindow>(out var 玩家状态栏))
            {
                foreach (PlayerStateWindow playerStateWindow in 玩家状态栏)
                {
                    var 根节点 = playerStateWindow.transform;
                    修改玩家状态栏布局宽度(根节点.Find("PanelPortrait")); // 肖像
                    var 健康 = 根节点.GetChild(2);    // 健康
                    var 卫生 = 健康.Find("PanelHygiene");   // 卫生
                    修改玩家状态栏字体尺寸(卫生, "ValueHygiene", 26);   // 卫生数值
                    修改玩家状态栏字体尺寸(卫生, "ValueHygiene/TextUnitHunger", 23, 文本偏移: 10, TextAlignmentOptions.Right);  // 卫生单位
                    var 情绪 = 健康.Find("PanelMood");  // 情绪
                    修改玩家状态栏字体尺寸(情绪, "ValueMood", 26);  // 情绪数值
                    修改玩家状态栏字体尺寸(情绪, "ValueMood/TextUnitHunger", 23, 文本偏移: 10, TextAlignmentOptions.Right); // 情绪单位
                    var 食品质量 = 健康.Find("PanelFoodQuality");   // 食品质量
                    修改玩家状态栏字体尺寸(食品质量, "ValueFoodQuality", 26);   // 食品质量数值
                    修改玩家状态栏字体尺寸(食品质量, "ValueFoodQuality/TextUnitHunger", 23, 文本偏移: 10, TextAlignmentOptions.Right); // 食品质量单位
                    修改玩家状态栏字体尺寸(健康, "Header/InfoTextExtended");   // 面板健康标题
                    var 饱食 = 健康.Find("PanelHunger");    // 饱食
                    修改玩家状态栏字体尺寸(饱食, "ValueHunger", 26);    // 饱食数值
                    修改玩家状态栏字体尺寸(饱食, "ValueHunger/TextUnitHunger", 23, 文本偏移: 10, TextAlignmentOptions.Right);   // 饱食单位
                    var 饮水 = 健康.Find("PanelHydration"); // 饮水
                    修改玩家状态栏字体尺寸(饮水, "ValueHydration", 26); // 饮水数值
                    修改玩家状态栏字体尺寸(饮水, "ValueHydration/TextUnitHydration", 23, 文本偏移: 5, TextAlignmentOptions.Right);  // 饮水单位
                    var 导航 = 根节点.Find("PanelExternalNavigation");    // 导航
                    修改玩家状态栏布局宽度(导航);
                    var 外部环境 = 导航.Find("PanelExternal");  // 外部环境
                    修改玩家状态栏字体尺寸(外部环境, "Header/InfoTextExtended"); // 外部环境标题
                    var 外部压力 = 外部环境.Find("PanelPressure");  // 外部环境压力
                    var GaugeBG = 外部压力.Find("PressureGaugeBG"); // 外部环境压力计
                    修改玩家状态栏压力表宽度(GaugeBG.gameObject);
                    修改玩家状态栏字体尺寸(外部压力, "ValuePressure", 26, 文本偏移: -83); // 外部环境压力计数值
                    修改玩家状态栏字体尺寸(外部压力, "TextUnitPressure", 23, 文本偏移: -12, TextAlignmentOptions.Right);    // 外部环境压力计单位
                    var 外部温度 = 外部环境.Find("PanelTemp");  // 外部环境温度
                    修改玩家状态栏字体尺寸(外部温度, "ValueTemp", 26, 文本偏移: -82);   // 外部环境温度数值
                    修改玩家状态栏字体尺寸(外部温度, "ValueTemp/TextUnitTemp", 23, 文本偏移: 50, TextAlignmentOptions.Right);   // 外部环境温度单位
                    var 朝向 = 导航.Find("PanelNavigation");    // 朝向
                    var 方位 = 朝向.Find("PanelCompass");     // 方位
                    修改玩家状态栏字体尺寸(方位, "ValueCompass", 26, 文本偏移: -82);  // 方位数值
                    修改玩家状态栏字体尺寸(方位, "ValueCompass/TextUnitCompass", 23, 文本偏移: 43, TextAlignmentOptions.Right);   // 方位单位
                    var 速度 = 朝向.Find("PanelVelocity");  // 移动速度
                    修改玩家状态栏字体尺寸(速度, "ValueVelocity", 26, 文本偏移: -80);   // 速度数值
                    修改玩家状态栏字体尺寸(速度, "ValueVelocity/TextUnitVelocity", 23, 文本偏移: 32, TextAlignmentOptions.Right);   // 速度单位
                    var 内部环境 = 根节点.Find("PanelVerticalGroup/Internals/PanelInternal"); // 内部环境
                    修改玩家状态栏布局宽度(内部环境);
                    修改玩家状态栏字体尺寸(内部环境, "Header/InfoTextExtended");    // 内部环境标题
                    var 内部压力 = 内部环境.Find("PanelPressure");
                    var 内部GaugeBG = 内部压力.Find("PressureGaugeBG"); // 内部环境压力计
                    修改玩家状态栏压力表宽度(内部GaugeBG.gameObject);       // 内部环境压力
                    修改玩家状态栏字体尺寸(内部压力, "TitlePressureSetting");   // 内部环境设定压力名称
                    修改玩家状态栏字体尺寸(内部压力, "ValuePressureSetting", 26);   // 内部环境设定压力数值
                    修改玩家状态栏字体尺寸(内部压力, "ValuePressure", 26, 文本偏移: -70);   // 内部环境压力数值
                    修改玩家状态栏字体尺寸(内部压力, "TextUnitPressure", 23, 文本偏移: 0, TextAlignmentOptions.Right);  // 内部环境压力单位
                    var 内部温度 = 内部环境.Find("PanelTemp");  // 内部环境温度
                    修改玩家状态栏字体尺寸(内部温度, "TitleTempSetting");   // 内部环境设定温度名称
                    修改玩家状态栏字体尺寸(内部温度, "ValueTempSetting", 26);   // 内部环境设定温度数值
                    修改玩家状态栏字体尺寸(内部温度, "ValueTemp", 26, 文本偏移: -67);   // 内部环境温度数值
                    var component = 内部温度?.Find("ValueTemp")?.GetComponent<TMP_Text>();
                    if (component) { component.font = 前置_资源加载器.单例.当前TMP字体; }
                    修改玩家状态栏字体尺寸(内部温度, "ValueTemp/TextUnitTemp", 23, 文本偏移: 38, TextAlignmentOptions.Right);  // 内部环境温度单位
                    var 喷气背包 = 根节点.Find("PanelVerticalGroup/PanelJetpack");    // 喷气背包
                    修改玩家状态栏布局宽度(喷气背包.Find("Header"));
                    修改玩家状态栏布局宽度(喷气背包.Find("PanelThrust"));
                    修改玩家状态栏布局宽度(喷气背包.Find("PanelPressureDelta"));
                    修改玩家状态栏字体尺寸(喷气背包, "Header/InfoTextExtended");    // 喷气背包标题
                    var 推力 = 喷气背包.Find("PanelThrust");        // 喷气背包推力
                    修改玩家状态栏字体尺寸(推力, "TitleThurstSetting"); // 喷气背包设定推力名称
                    修改玩家状态栏字体尺寸(推力, "ValueThrustSetting", 26); // 喷气背包设定推力数值
                    var 剩余压力 = 喷气背包.Find("PanelPressureDelta"); // 喷气背包剩余压力
                    修改玩家状态栏字体尺寸(剩余压力, "ValuePressure", 26, 文本偏移: -66);   // 喷气背包剩余压力数值
                    修改玩家状态栏字体尺寸(剩余压力, "TextUnitPressure", 23, 文本偏移: 0, TextAlignmentOptions.Right);   // 喷气背包剩余压力单位

                    if (健康 && 健康.name == "PanelHealth") // 卫生
                    {
                        var 健康VL = 健康.GetComponent<HorizontalOrVerticalLayoutGroup>();  // 健康布局组
                        if (健康VL)
                        {
                            健康VL.childForceExpandWidth = false;
                            健康VL.childForceExpandHeight = true;           // 高度拉伸同步                  
                        }
                        else { Log.LogDebug($"未找到玩家状态栏->面板健康布局组"); }
                        foreach (Transform c in 健康.transform)     // 健康
                        {
                            if (c.name == "PanelFoodQuality" || c.name == "PanelHygiene" || c.name == "PanelMood")
                            { c.gameObject.SetActive(true); }
                        }
                    }

                    var 外部VL = 外部环境.GetComponent<VerticalLayoutGroup>();
                    外部VL.childForceExpandWidth = false;
                    外部VL.childForceExpandHeight = true;       // 高度拉伸同步
                    var 导航VL = 朝向.GetComponent<VerticalLayoutGroup>();
                    导航VL.childForceExpandWidth = false;
                    导航VL.childForceExpandHeight = true;   // 高度拉伸同步

                    var 适配 = 根节点.GetComponent<ContentSizeFitter>();
                    适配.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    var Hl = 根节点.GetComponent<HorizontalLayoutGroup>();
                    Hl.childForceExpandWidth = false;
                    Hl.childForceExpandHeight = true;       // 高度拉伸同步
                    var HlRect = Hl.GetComponent<RectTransform>();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(HlRect);

                    Log.LogMessage($"成功修改玩家状态栏字体尺寸");
                }
            }

            if (前置_资源加载器.单例.TryGetAllGameObject("ValueDaysPassed", out var 玩家状态栏生存天数))
            {
                foreach (var 生存天数 in 玩家状态栏生存天数)
                {
                    foreach (var component in 生存天数.transform.GetComponentsInChildren<TMP_Text>(includeInactive: true))
                    {
                        汉化模块工具.修改字体尺寸(component, 32);
                        component.font = 前置_资源加载器.单例.当前TMP字体;
                    }
                    Log.LogMessage($"成功修改玩家状态栏生存天数字体尺寸");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<PanelHands>(out var 玩家双手栏))
            {
                foreach (PanelHands panelHands in 玩家双手栏)
                {
                    foreach (var component in panelHands.HandArray)
                    {
                        var 部位标题 = component.transform.GetChild(0).GetComponent<TMP_Text>();
                        汉化模块工具.修改字体尺寸(部位标题, 22);

                        var __ = component.transform.GetChild(1).GetChild(1).GetChild(2);
                        switch (component.gameObject.name)
                        {
                            case "LeftHand":
                                {
                                    var 鼠标右键提示 = __.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
                                    鼠标右键提示.fontSize = 鼠标右键提示.fontSizeMin = 22;
                                    鼠标右键提示.font = 前置_资源加载器.单例.当前TMP字体;

                                    var 按键文本 = 鼠标右键提示;
                                    var 按键布局 = 按键文本.transform.parent.GetComponent<HorizontalLayoutGroup>();
                                    var 按键图标 = 按键布局.transform.GetChild(0).GetComponent<Image>();
                                    var 按键管理 = 按键布局.GetComponent<HotkeyDisplay>();
                                    {
                                        // HotkeyDisplay => 提示面板按键与图标组件在Refresh方法中控制窗口组件的激活状态
                                        // 窗口组件(UiComponentRenderer)通过层级管理, 可以一键将活动状态写入整个层级树. 窗口由UI组件组成,每个UI组件由四要素组成:1.鼠标交互 2.布局尺寸调整 3.纹理显示 4.文本显示
                                        // Image.raycastTarget => 纹理兼任鼠标交互; LayoutElement.ignoreLayout:布局尺寸调整; 文本显示(color.a = (active ? 1 : 0))
                                        {
                                            // 添加样式组件(LayoutElement),布局重建阶段父级的布局组件会读取子级的样式值,重写子级的RectTransform
                                            // 渲染阶段,渲染器会读取RectTransform的值,在指定位置绘制指定尺寸的内容
                                            按键文本.GetOrAddComponent<LayoutElement>().preferredWidth = 210;   // 左键235,右键210
                                            var TextComponent = 按键文本 as TextMeshProUGUI;

                                            向UiComponentRenderer中添加新UI元素(TextComponent, 按键管理.KeyRenderer);
                                            向UiComponentRenderer中添加新UI元素(TextComponent, 按键管理.ButtonRenderer);
                                        }
                                        {
                                            var 样式 = 按键图标.GetOrAddComponent<LayoutElement>();
                                            样式.minWidth = 样式.preferredWidth = 28;   // 左键40,右键28
                                            样式.minHeight = 样式.preferredHeight = 25; // 左键30,右键25

                                            向UiComponentRenderer中添加新UI元素(按键图标, 按键管理.KeyRenderer);
                                            向UiComponentRenderer中添加新UI元素(按键图标, 按键管理.ButtonRenderer);
                                        }
                                        {
                                            // 爷爷级读取父级的样式组件(LayoutElement)的值,重写父级的RectTransform
                                            按键布局.GetOrAddComponent<LayoutElement>().preferredHeight = 38;
                                            按键布局.GetOrAddComponent<LayoutElement>().preferredWidth = 225;   // 左键不用设置,右键=225
                                            // 启用父级布局组件的子级RectTransform重写功能,否则子级中添加的样式组件(LayoutElement)是无效的
                                            按键布局.childControlHeight = 按键布局.childControlWidth = true;
                                            按键布局.spacing = -10;
                                            按键布局.SetEnable(true);
                                        }
                                    }
                                }
                                break;
                            case "RightHand":
                                {
                                    var 鼠标右键提示 = __.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
                                    鼠标右键提示.fontSize = 鼠标右键提示.fontSizeMin = 22;
                                    鼠标右键提示.font = 前置_资源加载器.单例.当前TMP字体;

                                    var 按键文本 = 鼠标右键提示;
                                    var 按键布局 = 按键文本.transform.parent.GetComponent<HorizontalLayoutGroup>();
                                    var 按键图标 = 按键布局.transform.GetChild(0).GetComponent<Image>();
                                    var 按键管理 = 按键布局.GetComponent<HotkeyDisplay>();
                                    {
                                        // HotkeyDisplay => 提示面板按键与图标组件在Refresh方法中控制窗口组件的激活状态
                                        // 窗口组件(UiComponentRenderer)通过层级管理, 可以一键将活动状态写入整个层级树. 窗口由UI组件组成,每个UI组件由四要素组成:1.鼠标交互 2.布局尺寸调整 3.纹理显示 4.文本显示
                                        // Image.raycastTarget => 纹理兼任鼠标交互; LayoutElement.ignoreLayout:布局尺寸调整; 文本显示(color.a = (active ? 1 : 0))
                                        {
                                            // 添加样式组件(LayoutElement),布局重建阶段父级的布局组件会读取子级的样式值,重写子级的RectTransform
                                            // 渲染阶段,渲染器会读取RectTransform的值,在指定位置绘制指定尺寸的内容
                                            按键文本.GetOrAddComponent<LayoutElement>().preferredWidth = 210;   // 左键235,右键210
                                            var TextComponent = 按键文本 as TextMeshProUGUI;

                                            向UiComponentRenderer中添加新UI元素(TextComponent, 按键管理.KeyRenderer);
                                            向UiComponentRenderer中添加新UI元素(TextComponent, 按键管理.ButtonRenderer);
                                        }
                                        {
                                            var 样式 = 按键图标.GetOrAddComponent<LayoutElement>();
                                            样式.minWidth = 样式.preferredWidth = 28;   // 左键40,右键28
                                            样式.minHeight = 样式.preferredHeight = 25; // 左键30,右键25

                                            向UiComponentRenderer中添加新UI元素(按键图标, 按键管理.KeyRenderer);
                                            向UiComponentRenderer中添加新UI元素(按键图标, 按键管理.ButtonRenderer);
                                        }
                                        {
                                            // 爷爷级读取父级的样式组件(LayoutElement)的值,重写父级的RectTransform
                                            按键布局.GetOrAddComponent<LayoutElement>().preferredHeight = 38;
                                            按键布局.GetOrAddComponent<LayoutElement>().preferredWidth = 225;   // 左键不用设置,右键=225
                                            // 启用父级布局组件的子级RectTransform重写功能,否则子级中添加的样式组件(LayoutElement)是无效的
                                            按键布局.childControlHeight = 按键布局.childControlWidth = true;
                                            按键布局.spacing = -10;
                                            按键布局.SetEnable(true);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    Log.LogMessage($"成功修改玩家双手栏字体尺寸");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<Tooltip>(out var 工具及鼠标左键提示))
            {
                foreach (Tooltip tooltip in 工具及鼠标左键提示)
                {
                    汉化模块工具.修改字体尺寸(tooltip.TooltipTitle, 25);
                    tooltip.TooltipTitle.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.TooltipAction, 26);
                    tooltip.TooltipAction.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.TooltipState, 24);
                    tooltip.TooltipState.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.TooltipExtended, 24);
                    tooltip.TooltipExtended.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.ToolTipBuildStateInfo, 24);
                    tooltip.ToolTipBuildStateInfo.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.ToolTipRepairStateInfo, 24);
                    tooltip.ToolTipRepairStateInfo.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.ToolTipDeconstructBuildStateInfo, 24);
                    tooltip.ToolTipDeconstructBuildStateInfo.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.ToolTipPlacementType, 24);
                    tooltip.ToolTipPlacementType.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(tooltip.TooltipNumberOfBuildStates, 24);
                    tooltip.TooltipNumberOfBuildStates.font = 前置_资源加载器.单例.当前TMP字体;

                    var 鼠标左键提示 = tooltip.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<TMP_Text>();
                    汉化模块工具.修改字体尺寸(鼠标左键提示, 24);
                    鼠标左键提示.font = 前置_资源加载器.单例.当前TMP字体;

                    var 按键文本 = 鼠标左键提示;
                    var 按键布局 = 按键文本.transform.parent.GetComponent<HorizontalLayoutGroup>();
                    var 按键图标 = 按键布局.transform.GetChild(0).GetComponent<Image>();
                    var 按键管理 = 按键布局.GetComponent<HotkeyDisplay>();
                    {
                        // HotkeyDisplay => 提示面板按键与图标组件在Refresh方法中控制窗口组件的激活状态
                        // 窗口组件(UiComponentRenderer)通过层级管理, 可以一键将活动状态写入整个层级树. 窗口由UI组件组成,每个UI组件由四要素组成:1.鼠标交互 2.布局尺寸调整 3.纹理显示 4.文本显示
                        // Image.raycastTarget => 纹理兼任鼠标交互; LayoutElement.ignoreLayout:布局尺寸调整; 文本显示(color.a = (active ? 1 : 0))
                        {
                            // 添加样式组件(LayoutElement),布局重建阶段父级的布局组件会读取子级的样式值,重写子级的RectTransform
                            // 渲染阶段,渲染器会读取RectTransform的值,在指定位置绘制指定尺寸的内容
                            按键文本.GetOrAddComponent<LayoutElement>().preferredWidth = 235;   // 左键235,右键210
                            var TextComponent = 按键文本 as TextMeshProUGUI;

                            向UiComponentRenderer中添加新UI元素(TextComponent, 按键管理.KeyRenderer);
                            向UiComponentRenderer中添加新UI元素(TextComponent, 按键管理.ButtonRenderer);
                        }
                        {
                            var 样式 = 按键图标.GetOrAddComponent<LayoutElement>();
                            样式.minWidth = 样式.preferredWidth = 40;   // 左键40,右键28
                            样式.minHeight = 样式.preferredHeight = 30; // 左键30,右键25

                            向UiComponentRenderer中添加新UI元素(按键图标, 按键管理.KeyRenderer);
                            向UiComponentRenderer中添加新UI元素(按键图标, 按键管理.ButtonRenderer);
                        }
                        {
                            // 爷爷级读取父级的样式组件(LayoutElement)的值,重写父级的RectTransform
                            按键布局.GetOrAddComponent<LayoutElement>().preferredHeight = 38;
                            // 按键布局.GetOrAddComponent<LayoutElement>().preferredWidth = 225;   // 左键不用设置,右键=225
                            // 启用父级布局组件的子级RectTransform重写功能,否则子级中添加的样式组件(LayoutElement)是无效的
                            按键布局.childControlHeight = 按键布局.childControlWidth = true;
                            按键布局.spacing = -10;
                            按键布局.SetEnable(true);
                        }
                    }

                    var 鼠标按住提示 = tooltip.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(2).GetComponent<TMP_Text>();
                    汉化模块工具.修改字体尺寸(鼠标按住提示, 24);
                    鼠标按住提示.font = 前置_资源加载器.单例.当前TMP字体;

                    Log.LogMessage($"成功修改工具及鼠标左键提示字体尺寸");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<Stationpedia>(out var 百科))
            {
                foreach (Stationpedia stationpedia in 百科)
                {
                    Traverse.Create(stationpedia).Field("_searchRegex").SetValue(new Regex("[^a-zA-Z0-9-\u4e00-\u9fa5]+", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace));
                    Log.LogMessage($"成功修改F1百科的搜索正则表达式以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<SPDAFoundIn>(out var 百科脱气产物条目))
            {
                foreach (SPDAFoundIn sPDAFoundIn in 百科脱气产物条目)
                {
                    sPDAFoundIn.ItemFound.overflowMode = TextOverflowModes.Overflow;
                    sPDAFoundIn.QuantityofItem.overflowMode = TextOverflowModes.Overflow;
                    Log.LogMessage($"成功修改F1百科的脱气产物条目以正确显示中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllGameObject("SelectComparison", out var 百科相变图当前对比流体))
            {
                foreach (var itemLabel in 百科相变图当前对比流体)
                {
                    var __ = itemLabel.transform.GetChild(1);
                    if (__ && __.name == "Label")
                    {
                        var component = __.GetComponent<TMP_Text>();
                        component.font = 前置_资源加载器.单例.当前TMP字体;
                    }
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<PhaseChangeDiagram>(out var 百科相变图))
            {
                foreach (PhaseChangeDiagram phaseChangeDiagram in 百科相变图)
                {
                    var 温度 = (TMP_Text)Traverse.Create(phaseChangeDiagram).Field("temperature").GetValue();
                    温度.font = 前置_资源加载器.单例.当前TMP字体;

                    var 饱和蒸汽压 = (TMP_Text)Traverse.Create(phaseChangeDiagram).Field("pressure").GetValue();
                    饱和蒸汽压.font = 前置_资源加载器.单例.当前TMP字体;
                }
            }

            Log.LogMessage($"成功修改F1百科相变图以正确显示中文");

            if (前置_资源加载器.单例.TryGetAllComponent<StatusIcon>(out var 玩家状态报警图标))
            {
                foreach (StatusIcon statusIcon in 玩家状态报警图标)
                {
                    var 危机值 = statusIcon.transform.Find("Panel");
                    var 危机类型 = statusIcon.transform.Find("SuitText");

                    if (危机值)
                    {
                        foreach (var component in 危机值.transform.GetComponentsInChildren<TMP_Text>(includeInactive: true))
                        { 汉化模块工具.修改字体尺寸(component, 26); }
                    }
                    if (危机类型)
                    {
                        foreach (var component in 危机类型.transform.GetComponentsInChildren<TMP_Text>(includeInactive: true))
                        { 汉化模块工具.修改字体尺寸(component, 21); }
                    }

                    Log.LogMessage($"成功修改玩家状态报警图标字体尺寸");
                }

            }

            if (前置_资源加载器.单例.TryGetAllComponent<InGameMenu>(out var 保存游戏面板))
            {
                foreach (InGameMenu 面板 in 保存游戏面板)
                {
                    var 保存游戏 = (TextMeshProUGUI)Traverse.Create(面板).Field("_saveButtonText").GetValue();
                    保存游戏.font = 前置_资源加载器.单例.当前TMP字体;
                    保存游戏.text = "保存游戏";
                    var 另存为 = (TextMeshProUGUI)Traverse.Create(面板).Field("_saveAsButtonText").GetValue();
                    另存为.font = 前置_资源加载器.单例.当前TMP字体;

                    Log.LogMessage($"成功修改保存游戏面板以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<MainMenu>(out var 打开角色捏脸场景按钮1))
            {
                foreach (MainMenu mainMenu in 打开角色捏脸场景按钮1)
                {
                    var 按钮 = (Button)Traverse.Create(mainMenu).Field("_appearanceButton").GetValue();
                    按钮.onClick.AddListener(() => { 单例.StartCoroutine(单例.等待角色捏脸场景加载完成后进行汉化修改()); });
                    Log.LogMessage($"成功修改角色捏脸场景以支持中文.1");
                    // 此场景退出时自动销毁,必须每次修改,除非直接修改场景源
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<InGameMenu>(out var 打开角色捏脸场景按钮2))
            {
                foreach (InGameMenu inGameMenu in 打开角色捏脸场景按钮2)
                {
                    var 按钮 = (Button)Traverse.Create(inGameMenu).Field("_changeAppearanceButton").GetValue();
                    按钮.onClick.AddListener(() => { 单例.StartCoroutine(单例.等待角色捏脸场景加载完成后进行汉化修改()); });
                    Log.LogMessage($"成功修改角色捏脸场景以支持中文.2");
                    // 此场景退出时自动销毁,必须每次修改,除非直接修改场景源
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<PasswordWindow>(out var 联机输入密码面板))
            {
                foreach (PasswordWindow passwordWindow in 联机输入密码面板)
                {
                    var 标题 = (TextMeshProUGUI)Traverse.Create(passwordWindow).Field("_titleText").GetValue();
                    标题.font = 前置_资源加载器.单例.当前TMP字体;
                    Log.LogMessage($"成功修改联机输入密码面板以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<HelperHintsTextController>(out var 游玩引导面板))
            {
                foreach (HelperHintsTextController helper in 游玩引导面板)
                {
                    汉化模块工具.修改字体尺寸(helper.TextMesh, 23);

                    var __ = helper.transform.root.Find("StationpediaHint");
                    if (__)
                    {
                        var F1按键提示 = __.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
                        汉化模块工具.修改字体尺寸(F1按键提示, 28);
                    }

                    var F2按键提示 = helper.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<TMP_Text>();
                    汉化模块工具.修改字体尺寸(F2按键提示, 26);

                    var 引导标题栏 = helper.transform.GetChild(0).GetChild(1);

                    var 帮助提示 = 引导标题栏.GetChild(0).GetComponent<TMP_Text>();
                    汉化模块工具.修改字体尺寸(帮助提示, 22);
                    帮助提示.enableWordWrapping = false;

                    var 自动展开 = 引导标题栏.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
                    汉化模块工具.修改字体尺寸(自动展开, 22);
                    自动展开.enableWordWrapping = false;

                    var 显示完成的项目 = 引导标题栏.GetChild(2).GetChild(0).GetComponent<TMP_Text>();
                    汉化模块工具.修改字体尺寸(显示完成的项目, 22);
                    显示完成的项目.enableWordWrapping = false;

                    var 屏蔽完成的项目 = 引导标题栏.GetChild(3).GetChild(0).GetComponent<TMP_Text>();
                    汉化模块工具.修改字体尺寸(屏蔽完成的项目, 22);
                    屏蔽完成的项目.enableWordWrapping = false;

                    Log.LogMessage($"成功修改游玩引导面板字体尺寸");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<MainMenu>(out var 选择起始状态面板_开局物资面板))
            {
                foreach (MainMenu mainMenu in 选择起始状态面板_开局物资面板)
                {
                    var 开局物资面板 = mainMenu.StartConditionInfo;
                    var 副标题 = 开局物资面板.SubTitlePrefab;
                    副标题.font = 前置_资源加载器.单例.当前TMP字体;

                    var 主标题 = 开局物资面板.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                    主标题.font = 前置_资源加载器.单例.当前TMP字体;
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<StartConditionMenu>(out var 选择起始状态面板))
            {
                foreach (StartConditionMenu startConditionMenu in 选择起始状态面板)
                {
                    var _worldNameInput = startConditionMenu._worldNameInput;
                    _worldNameInput.fontAsset = 前置_资源加载器.单例.当前TMP字体;
                    _worldNameInput.textComponent.font = 前置_资源加载器.单例.当前TMP字体;
                    ((TMP_Text)_worldNameInput.placeholder).font = 前置_资源加载器.单例.当前TMP字体;

                    var 物资难度选择 = startConditionMenu.transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<TMP_Text>();
                    物资难度选择.text = "选择条件";

                    var 生存难度选择 = startConditionMenu.transform.parent.GetChild(6).GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>();
                    生存难度选择.text = "选择难度";
                }
            }

            Log.LogMessage($"成功修改选择起始状态面板字体以支持中文");

            if (前置_资源加载器.单例.TryGetAllComponent<SavePanel>(out var 保存世界面板))
            {
                foreach (SavePanel savePanel in 保存世界面板)
                {
                    var _worldSavePrefab = (WorldSave)Traverse.Create(savePanel).Field("_worldSavePrefab").GetValue();
                    _worldSavePrefab.Filename.font = 前置_资源加载器.单例.当前TMP字体;
                    _worldSavePrefab.Date.font = 前置_资源加载器.单例.当前TMP字体;

                    var _saveNameInput = (TMP_InputField)Traverse.Create(savePanel).Field("_saveNameInput").GetValue();
                    _saveNameInput.fontAsset = 前置_资源加载器.单例.当前TMP字体;
                    _saveNameInput.textComponent.font = 前置_资源加载器.单例.当前TMP字体;
                    ((TMP_Text)_saveNameInput.placeholder).font = 前置_资源加载器.单例.当前TMP字体;

                    Log.LogMessage($"成功修改保存世界面板字体以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<LoadGamePage>(out var 载入游戏面板))
            {
                foreach (LoadGamePage loadGamePage in 载入游戏面板)
                {
                    var _searchField = (TMP_InputField)Traverse.Create(loadGamePage).Field("_searchField").GetValue();
                    _searchField.fontAsset = 前置_资源加载器.单例.当前TMP字体;
                    _searchField.textComponent.font = 前置_资源加载器.单例.当前TMP字体;
                    ((TMP_Text)_searchField.placeholder).font = 前置_资源加载器.单例.当前TMP字体;

                    var _worldItemPrefab = (WorldItem)Traverse.Create(loadGamePage).Field("_worldItemPrefab").GetValue();
                    var _titleText = (TextMeshProUGUI)Traverse.Create(_worldItemPrefab).Field("_titleText").GetValue();
                    var _infoText = (TextMeshProUGUI)Traverse.Create(_worldItemPrefab).Field("_infoText").GetValue();
                    _titleText.font = 前置_资源加载器.单例.当前TMP字体;
                    _infoText.font = 前置_资源加载器.单例.当前TMP字体;

                    var _workshopSaveItemPrefab = (WorkshopSaveItem)Traverse.Create(loadGamePage).Field("_workshopSaveItemPrefab").GetValue();
                    var 名称1 = (TextMeshProUGUI)Traverse.Create(_workshopSaveItemPrefab).Field("_nameText").GetValue();
                    var 信息1 = (TextMeshProUGUI)Traverse.Create(_workshopSaveItemPrefab).Field("_infoText").GetValue();
                    var 版本1 = (TextMeshProUGUI)Traverse.Create(_workshopSaveItemPrefab).Field("_versionText").GetValue();
                    名称1.font = 前置_资源加载器.单例.当前TMP字体;
                    信息1.font = 前置_资源加载器.单例.当前TMP字体;
                    版本1.font = 前置_资源加载器.单例.当前TMP字体;

                    var _saveItemPrefab = (SaveItem)Traverse.Create(_worldItemPrefab).Field("_saveItemPrefab").GetValue();
                    var 名称2 = (TextMeshProUGUI)Traverse.Create(_saveItemPrefab).Field("_nameText").GetValue();
                    var 信息2 = (TextMeshProUGUI)Traverse.Create(_saveItemPrefab).Field("_infoText").GetValue();
                    var 版本2 = (TextMeshProUGUI)Traverse.Create(_saveItemPrefab).Field("_versionText").GetValue();
                    名称2.font = 前置_资源加载器.单例.当前TMP字体;
                    信息2.font = 前置_资源加载器.单例.当前TMP字体;
                    版本2.font = 前置_资源加载器.单例.当前TMP字体;

                    var 发布存档 = 版本2.transform.parent.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                    发布存档.enableWordWrapping = false;

                    Log.LogMessage($"成功修改载入游戏面板字体以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllGameObject("ContactItemNew", out var 贸易主板面板商人条目))
            {
                foreach (var 条目 in 贸易主板面板商人条目)
                {
                    var 商人名称 = 条目.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
                    商人名称.GetComponent<LayoutElement>().preferredWidth = 20000;

                    var 通讯信息 = 条目.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                    通讯信息.enableWordWrapping = false;

                    Log.LogMessage($"成功修改贸易主板面板商人条目字体以正确显示中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<SorterMotherboard>(out var 分拣主板))
            {
                foreach (SorterMotherboard sorterMotherboard in 分拣主板)
                {
                    var 分拣机面板 = sorterMotherboard.ScreenSorterPrefab;
                    var 分拣机名称 = 分拣机面板.Title;
                    分拣机名称.font = 前置_资源加载器.单例.当前Font字体;
                    var 白名单 = 分拣机面板.ButtonAddFilter.transform.parent.GetComponent<Text>();
                    白名单.font = 前置_资源加载器.单例.当前Font字体;
                    汉化模块工具.修改字体尺寸(白名单, 26);
                    白名单.text = "分拣名单";

                    var 分拣机白名单面板 = sorterMotherboard.ScreenFilterPrefab;
                    var 下拉列表 = 分拣机白名单面板.TypeList;
                    var 当前 = 下拉列表.captionText;
                    当前.font = 前置_资源加载器.单例.当前Font字体;
                    汉化模块工具.修改字体尺寸(当前, 22);
                    当前.verticalOverflow = VerticalWrapMode.Overflow;
                    当前.horizontalOverflow = HorizontalWrapMode.Overflow;

                    var 条目克隆母体 = 下拉列表.itemText;
                    条目克隆母体.font = 前置_资源加载器.单例.当前Font字体;
                    汉化模块工具.修改字体尺寸(条目克隆母体, 22);
                    条目克隆母体.verticalOverflow = VerticalWrapMode.Overflow;
                    条目克隆母体.horizontalOverflow = HorizontalWrapMode.Overflow;

                    Log.LogMessage($"成功修改分拣主板面板字体以正确显示中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<InputPrefabs>(out var 选择配方面板))
            {
                foreach (InputPrefabs inputPrefabs in 选择配方面板)
                {
                    var 面板标题 = inputPrefabs.TitleText;
                    面板标题.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(面板标题, 23);
                    面板标题.overflowMode = TextOverflowModes.Overflow;

                    var 取消按钮 = inputPrefabs.transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>();
                    取消按钮.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(取消按钮, 23);

                    var 搜索栏 = inputPrefabs.SearchBar;
                    搜索栏.fontAsset = 前置_资源加载器.单例.当前TMP字体;

                    var 输入框输入显示 = 搜索栏.textComponent;
                    输入框输入显示.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(输入框输入显示, 22);

                    var 输入框淡色提示 = (TMP_Text)搜索栏.placeholder;
                    输入框淡色提示.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(输入框淡色提示, 22);

                    var 配方大类标题 = inputPrefabs.ControlGroupPrefab.Title;
                    配方大类标题.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(配方大类标题, 29);

                    var 配方描述 = inputPrefabs.PrefabReference.Text;
                    配方描述.font = 前置_资源加载器.单例.当前TMP字体;
                    汉化模块工具.修改字体尺寸(配方描述, 27);

                    Log.LogMessage($"成功修改选择配方面板字体及尺寸以正确显示中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<PlayerListItem>(out var TAB键联机面板玩家名单))
            {
                foreach (PlayerListItem playerListItem in TAB键联机面板玩家名单)
                {
                    var _usernameText = (TextMeshProUGUI)Traverse.Create(playerListItem).Field("_usernameText").GetValue();
                    _usernameText.font = 前置_资源加载器.单例.当前TMP字体;
                    Log.LogMessage($"成功修改TAB键联机面板玩家名单字体以正确显示中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<DeepMinerCartridge>(out var 功能卡深层矿物面板))
            {
                foreach (DeepMinerCartridge deepMinerCartridge in 功能卡深层矿物面板)
                {
                    var 面板标题 = deepMinerCartridge.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                    面板标题.font = 前置_资源加载器.单例.当前TMP字体;
                    面板标题.text = "深层矿物扫描仪";

                    var 成分标题 = deepMinerCartridge.transform.GetChild(0).GetChild(3).GetComponent<TMP_Text>();
                    成分标题.font = 前置_资源加载器.单例.当前TMP字体;
                    成分标题.text = "矿物成分";

                    deepMinerCartridge.RegionText.font = 前置_资源加载器.单例.当前TMP字体;
                    deepMinerCartridge.InfoText.font = 前置_资源加载器.单例.当前TMP字体;

                    Log.LogMessage($"成功修改功能卡深层矿物面板字体以支持中文");
                }
            }


            {
                var __ = Traverse.Create(GameStrings.Activate);
                __.Field("_baseString").SetValue("激活");
                __.Field("_overrideString").SetValue("激活");
            }
            {
                var __ = Traverse.Create(GameStrings.Deactivate);
                __.Field("_baseString").SetValue("失活");
                __.Field("_overrideString").SetValue("失活");
            }
            Log.LogMessage($"成功修改高级平板电脑激活与失活词条");

            // {
            //     var __ = Traverse.Create(GameStrings.IceCrusherState);
            //     __.Field("_baseString").SetValue("碎冰机状态");
            //     __.Field("_overrideString").SetValue("碎冰机状态");
            // }

            // {
            //     var __ = Traverse.Create(GameStrings.InternalAtmosphere);
            //     __.Field("_baseString").SetValue("内部环境");
            //     __.Field("_overrideString").SetValue("内部环境");
            // }

            // {
            //     var __ = Traverse.Create(GameStrings.IceCrusherSetting);
            //     __.Field("_baseString").SetValue("当温度低于目标温度 {0} 将启动电加热");
            //     __.Field("_overrideString").SetValue("当温度低于目标温度 {0} 将启动电加热");
            // }

            // {
            //     var __ = Traverse.Create(GameStrings.IceCrusherHeater);
            //     __.Field("_baseString").SetValue("碎冰机电加热器");
            //     __.Field("_overrideString").SetValue("碎冰机电加热器");
            // }

            // {
            //     var __ = Traverse.Create(GameStrings.IceCrusherHeating);
            //     __.Field("_baseString").SetValue("加热到目标温度 {0} 时操作变慢");
            //     __.Field("_overrideString").SetValue("加热到目标温度 {0} 时操作变慢");
            // }

            // {
            //     var __ = Traverse.Create(GameStrings.IceCrusherIndicatorHeat);
            //     __.Field("_baseString").SetValue("加热");
            //     __.Field("_overrideString").SetValue("加热");
            // }

            // {
            //     var __ = Traverse.Create(GameStrings.IceCrusherIndicatorIdle);
            //     __.Field("_baseString").SetValue("空闲");
            //     __.Field("_overrideString").SetValue("空闲");
            // }

            // {
            //     var __ = Traverse.Create(GameStrings.IceCrusherAtmosFull);
            //     __.Field("_baseString").SetValue("内部容器已充满");
            //     __.Field("_overrideString").SetValue("内部容器已充满");
            // }

            // Log.LogMessage($"成功修改碎冰机提示面板词条");

            {
                var __ = Traverse.Create(GameStrings.RegionName);
                __.Field("_baseString").SetValue("地区: {0}");
                __.Field("_overrideString").SetValue("地区: {0}");

                if (前置_资源加载器.单例.TryGetAllComponent<Tracker>(out var 功能卡_定位面板))
                {
                    foreach (Tracker tracker in 功能卡_定位面板)
                    {
                        tracker.RegionText.font = 前置_资源加载器.单例.当前TMP字体;
                    }
                }

                Log.LogMessage($"成功修改功能卡(定位)面板字体以支持中文");
            }

            if (前置_资源加载器.单例.TryGetAllComponent<ConfigCartridge>(out var 功能卡_设备配置面板))
            {
                foreach (ConfigCartridge configCartridge in 功能卡_设备配置面板)
                {
                    var 面板标题 = configCartridge.transform.GetChild(0).GetChild(0).GetComponent<Text>();
                    面板标题.text = "设备配置";
                    面板标题.font = 前置_资源加载器.单例.当前Font字体;

                    configCartridge.SelectedTitle.font = 前置_资源加载器.单例.当前TMP字体;

                    Log.LogMessage($"成功修改功能卡_设备配置面板字体以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<MapMotherboardPanel>(out var 主板_地图面板))
            {
                foreach (MapMotherboardPanel mapMotherboardPanel in 主板_地图面板)
                {
                    // var 可用扫描设备条目 = (MapMotherboardPanelScannerListItem)Traverse.Create(mapMotherboardPanel).Field("_scannerListItemPrefab").GetValue();
                    // var 条目名称 = (TextMeshProUGUI)Traverse.Create(可用扫描设备条目).Field("_textMesh").GetValue();
                    // 条目名称.font = 前置_资源加载器.单例.当前TMP字体;
                    // Log.LogMessage($"成功修改主板_地图面板字体以支持中文");
                }
            }


            if (前置_资源加载器.单例.TryGetAllComponent<WorkshopMenu>(out var 创意工坊面板))
            {
                foreach (WorkshopMenu workshopMenu in 创意工坊面板)
                {
                    workshopMenu.TitleText.font = 前置_资源加载器.单例.当前TMP字体;
                    workshopMenu.TitleText.overflowMode = TextOverflowModes.Overflow;
                    Log.LogMessage($"成功修改创意工坊模组标题以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<InputSourceCode>(out var IC10代码编辑面板))
            {
                foreach (InputSourceCode inputSourceCode in IC10代码编辑面板)
                {
                    inputSourceCode.TitleText.font = 前置_资源加载器.单例.当前TMP字体;
                    inputSourceCode.SizeText.font = 前置_资源加载器.单例.当前TMP字体;
                    inputSourceCode.InputType.font = 前置_资源加载器.单例.当前TMP字体;
                    inputSourceCode.TitleText.text = "IC10代码编辑器";
                    Log.LogMessage($"成功修改IC10代码编辑面板以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllComponent<ProgrammableChipMotherboard>(out var 主板_IC编辑器面板))
            {
                foreach (ProgrammableChipMotherboard programmableChipMotherboard in 主板_IC编辑器面板)
                {
                    var 选择设备下拉列表 = (ButtonDropdown)Traverse.Create(programmableChipMotherboard).Field("_dropdown").GetValue();
                    var _dropdown = Traverse.Create(选择设备下拉列表);
                    var 当前 = (TextMeshProUGUI)_dropdown.Field("_selectedTextMesh").GetValue();
                    当前.font = 前置_资源加载器.单例.当前TMP字体;
                    var 条目克隆母体 = (ButtonDropdownItem)_dropdown.Field("_buttonPrefab").GetValue();
                    var _textMesh = (TextMeshProUGUI)Traverse.Create(条目克隆母体).Field("_textMesh").GetValue();
                    _textMesh.font = 前置_资源加载器.单例.当前TMP字体;
                    Log.LogMessage($"成功修改主板_IC编辑器面板以支持中文");
                }
            }

            if (前置_资源加载器.单例.TryGetAllGameObject("PanelNormal", out var Led控制台面板))
            {
                foreach (var __ in Led控制台面板)
                {
                    var displayName = __.transform.Find("DisplayName");
                    if (displayName)
                    {
                        var Led控制台面板标题 = displayName.GetComponent<UnityEngine.UI.Text>();
                        if (Led控制台面板标题)
                        {
                            Led控制台面板标题.font = 前置_资源加载器.单例.当前Font字体;
                            Led控制台面板标题.color = new(0.7f, 0.7f, 0.7f, 1);
                            Log.LogMessage($"成功修改Led控制台面板标题以支持中文 {__.transform.parent}");
                        }
                    }
                }
            }

            修改键位配置选项卡以支持中文();
            KeyManager.OnControlsChanged += 修改键位配置选项卡以支持中文;

            static void 修改键位配置选项卡以支持中文()
            {
                foreach (var 键位配置选项卡 in ControlsGroup.AllControlGroups)
                {
                    var 键位分组布局 = 键位配置选项卡.Transform.GetComponent<ControlGroupItem>();

                    键位分组布局.Title.font = 前置_资源加载器.单例.当前TMP字体;
                    switch (键位分组布局.Title.text)
                    {
                        case "General": 键位分组布局.Title.text = "通用"; break;
                        case "Interface": 键位分组布局.Title.text = "界面"; break;
                        case "Movement": 键位分组布局.Title.text = "移动"; break;
                        case "Inventory": 键位分组布局.Title.text = "物品操作"; break;
                        case "Interaction": 键位分组布局.Title.text = "装备互动"; break;
                        case "Construction": 键位分组布局.Title.text = "建造"; break;
                        case "Creative": 键位分组布局.Title.text = "创造"; break;
                        case "Camera": 键位分组布局.Title.text = "视角"; break;
                        case "KuaiJie": 键位分组布局.Title.text = "快捷轮盘菜单"; break;
                        case "KuangWu": 键位分组布局.Title.text = "矿物扫描眼镜"; break;
                        case "FangJian": 键位分组布局.Title.text = "房间闭合检测"; break;
                        case "Zoop": 键位分组布局.Title.text = "Zoop的批量拆装"; break;
                    }

                    // 单击按钮变更键位
                    foreach (var v in 键位配置选项卡.Transform.GetComponentsInChildren<ControlsAssignment>(includeInactive: true))
                    {
                        v.Name.font = 前置_资源加载器.单例.当前TMP字体;
                        switch (v.Name.text)
                        {
                            case "Primary Action": v.Name.text = "主要交互"; break;
                            case "Secondary Action": v.Name.text = "次要交互"; break;
                            case "Night Vision": v.Name.text = "夜视仪/开关"; break;
                            case "Quantity Modifier": v.Name.text = "多功能辅助键"; break;
                            case "Emote Wave": v.Name.text = "打招呼动作"; break;

                            case "Show Score Board": v.Name.text = "计分板窗口"; break;
                            case "Help": v.Name.text = "百科窗口"; break;
                            case "Toggle U I Visibility": v.Name.text = "切换UI可见性"; break;
                            case "Toggle Helper Hints": v.Name.text = "切换教程提示可见性"; break;
                            case "Toggle Console": v.Name.text = "控制台窗口"; break;
                            case "Toggle Info": v.Name.text = "连接状态窗口"; break;
                            case "Screen Shot": v.Name.text = "截图"; break;
                            case "Chatting": v.Name.text = "聊天"; break;
                            case "Cancel": v.Name.text = "返回"; break;
                            case "Hide All Windows": v.Name.text = "关闭所有槽位面板"; break;
                            case "Ping Highlight": v.Name.text = "网络延迟窗口"; break;

                            case "Ascend": v.Name.text = "跳跃/上升"; break;
                            case "Descend": v.Name.text = "下降"; break;
                            case "Left": v.Name.text = "左移"; break;
                            case "Right": v.Name.text = "右移"; break;
                            case "Forward": v.Name.text = "前进"; break;
                            case "Backward": v.Name.text = "后退"; break;
                            case "Grab": v.Name.text = "抓住"; break;

                            case "Walk": v.Name.text = "切换走/跑"; break;
                            case "Crouch": v.Name.text = "切换蹲/站"; break;

                            case "Inventory Select": v.Name.text = "交换焦点槽或单击焦点控件"; break;
                            case "Move All Of Type": v.Name.text = "转移所有同类物品"; break;
                            case "Move All": v.Name.text = "转移所有物品"; break;

                            case "Helmet Slot": v.Name.text = "头盔槽"; break;
                            case "Glasses Slot": v.Name.text = "眼镜槽"; break;
                            case "Suit Slot": v.Name.text = "外套槽"; break;
                            case "Back Slot": v.Name.text = "背包槽"; break;
                            case "Uniform Slot": v.Name.text = "制服槽"; break;
                            case "Tool Belt Slot": v.Name.text = "腰带槽"; break;
                            case "Active Hand Slot": v.Name.text = "活动手槽"; break;

                            case "Drop": v.Name.text = "丢弃"; break;
                            case "Open Seat Screen": v.Name.text = "开启座舱屏幕"; break;
                            case "Smart Stow": v.Name.text = "收纳物品到空白槽"; break;
                            case "Swap Hands": v.Name.text = "切换活动手"; break;
                            case "Precision Place": v.Name.text = "精确放置"; break;
                            case "Mouse Control": v.Name.text = "呼出鼠标"; break;
                            case "Mouse Inspect": v.Name.text = "检索百科条目"; break;

                            case "Jetpack": v.Name.text = "喷气背包/开关"; break;
                            case "Internals": v.Name.text = "面罩/开关"; break;
                            case "Toggle Hand Power": v.Name.text = "活动手槽物品电源/开关"; break;
                            case "Toggle Light": v.Name.text = "头灯/开关"; break;
                            case "Suit Pressure Increase": v.Name.text = "增加宇航服压力"; break;
                            case "Suit Pressure Decrease": v.Name.text = "降低宇航服压力"; break;
                            case "Suit Temperature Increase": v.Name.text = "增加宇航服温度"; break;
                            case "Suit Temperature Decrease": v.Name.text = "降低宇航服温度"; break;
                            case "Jetpack Thrust Increase": v.Name.text = "增加喷气背包推力"; break;
                            case "Jetpack Thrust Decrease": v.Name.text = "降低喷气背包推力"; break;
                            case "Jetpack Toggle Stabilizer": v.Name.text = "喷气背包悬停/开关"; break;

                            case "Rotate Left": v.Name.text = "向左旋转"; break;
                            case "Rotate Right": v.Name.text = "向右旋转"; break;
                            case "Rotate Up": v.Name.text = "向上旋转"; break;
                            case "Rotate Down": v.Name.text = "向下旋转"; break;
                            case "Rotate Roll Left": v.Name.text = "顺时针旋转"; break;
                            case "Rotate Roll Right": v.Name.text = "逆时针旋转"; break;

                            case "Show Dynamic Panel": v.Name.text = "选择生成物品窗口"; break;
                            case "Previous Item": v.Name.text = "切换上个控件或槽为焦点"; break;
                            case "Next Item": v.Name.text = "切换下个控件或槽为焦点"; break;
                            case "Spawn Item": v.Name.text = "生成物品"; break;
                            case "Instant Stop": v.Name.text = "急停"; break;

                            case "Fo V Up": v.Name.text = "增加Fov"; break;
                            case "Fo V Down": v.Name.text = "减少Fov"; break;
                            case "Fov Reset": v.Name.text = "重置Fov"; break;
                            case "Third Person Control": v.Name.text = "第三人称控制"; break;

                            //  在ControlsAssignment.Created函数中, 执行了this.Name.text = keyItem.Name.ToProper(true); 会导致Name中间自动插入空格
                            case "Kuai Jie Lun Pan": v.Name.text = "快捷轮盘菜单/开关"; break;
                            case "Kuang Wu Sao Miao Yan Jing": v.Name.text = "矿物扫描眼镜/开关"; break;
                            case "Fang Jian Bi He Jian Ce": v.Name.text = "房间闭合检测/开关"; break;

                            case "Zoop  Hold": v.Name.text = "观察模式/按住开启"; break;
                            case "Zoop  Switch": v.Name.text = "批量放置/开关"; break;
                            case "Zoop  Add  Waypoint": v.Name.text = "添加路径点"; break;
                            case "Zoop  Remove  Last  Waypoint": v.Name.text = "回退到上一个路径点"; break;
                            case "Zoop  Allow  More  Long  Pieces": v.Name.text = "切换并使用下一个长度的变体"; break;
                            case "Zoop  Restrict  Long  Pieces": v.Name.text = "回退到上一个长度的变体"; break;
                            case "Zoop  Bulk  Deconstruct": v.Name.text = "批量拆卸/开关"; break;
                        }
                    }

                    // 单击下拉列表变更相关配置
                    foreach (var v in 键位配置选项卡.Transform.GetComponentsInChildren<ControllerAxisItem>(includeInactive: true))
                    {
                        v.AssignmentText.font = 前置_资源加载器.单例.当前TMP字体;
                        switch (v.AssignmentText.text)
                        {
                            case "Strafe": v.AssignmentText.text = "手柄左右移动杆"; break;
                            case "Turn": v.AssignmentText.text = "手柄旋转杆"; break;
                            case "Forward": v.AssignmentText.text = "手柄前后移动杆"; break;
                            case "Vertical": v.AssignmentText.text = "手柄上下移动杆"; break;
                        }
                    }
                }

                Log.LogMessage($"成功修改键位配置选项卡以支持中文");
            }

            {
                {
                    var __ = Traverse.Create(GameStrings.FermenterItemsLeftToFerment);
                    __.Field("_baseString").SetValue("正在发酵中 <color=yellow>{2}</color> × <color=green>{0} {1}%</color>");
                    __.Field("_overrideString").SetValue("正在发酵中 <color=yellow>{2}</color> × <color=green>{0} {1}%</color>");
                }
                {
                    var __ = Traverse.Create(GameStrings.FermenterInputIsEmpty);
                    __.Field("_baseString").SetValue("输入口没有可发酵的物品投入");
                    __.Field("_overrideString").SetValue("输入口没有可发酵的物品投入");
                }
                {
                    var __ = Traverse.Create(GameStrings.FermenterWillProduce);
                    __.Field("_baseString").SetValue("将生成 <color=purple>{0}</color>.");
                    __.Field("_overrideString").SetValue("将生成 <color=purple>{0}</color>.");
                }

                Log.LogMessage($"成功修改发酵罐面板硬编码以支持中文");

                if (前置_资源加载器.单例.TryGetAllComponent<AdvancedAirlockControl>(out var 高级气闸控制电路板))
                {
                    foreach (AdvancedAirlockControl 当前 in 高级气闸控制电路板)
                    {
                        当前.ButtonTextFirst.font = 前置_资源加载器.单例.当前Font字体;
                        当前.ButtonTextFirst.fontSize = 26;
                        当前.ButtonTextFirst.fontStyle = FontStyle.Normal;
                        当前.ButtonTextSecond.font = 前置_资源加载器.单例.当前Font字体;
                        当前.ButtonPressureExternalText.font = 前置_资源加载器.单例.当前Font字体;
                        当前.ButtonPressureInternalText.font = 前置_资源加载器.单例.当前Font字体;
                    }
                }

                Log.LogMessage($"成功修改高级气闸控制电路板字体以支持中文");

                if (前置_资源加载器.单例.TryGetAllComponent<InputWindow>(out var 输入窗口))
                {
                    foreach (InputWindow 当前 in 输入窗口)
                    {
                        当前.TitleText.font = 前置_资源加载器.单例.当前TMP字体;
                        当前.TitleText.enableWordWrapping = true;
                        当前.TitleText.overflowMode = TextOverflowModes.Overflow;
                    }
                }

                Log.LogMessage($"成功修改输入窗口字体以支持中文");

                {
                    var __ = Traverse.Create(GameStrings.LifeSuspendedHeader);
                    __.Field("_baseString").SetValue("睡眠中");
                    __.Field("_overrideString").SetValue("睡眠中");
                }

                {
                    var __ = Traverse.Create(GameStrings.LifeSuspendedDescription);
                    __.Field("_baseString").SetValue("新陈代谢暂停了");
                    __.Field("_overrideString").SetValue("新陈代谢暂停了");
                }

                {
                    var __ = Traverse.Create(GameStrings.NoNeedToEat);
                    __.Field("_baseString").SetValue("<color=green>饱食</color>已停止降低");
                    __.Field("_overrideString").SetValue("<color=green>饱食</color>已停止降低");
                }

                {
                    var __ = Traverse.Create(GameStrings.NoNeedToDrink);
                    __.Field("_baseString").SetValue("<color=green>水分</color>已停止降低");
                    __.Field("_overrideString").SetValue("<color=green>水分</color>已停止降低");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonStartLink);
                    __.Field("_baseString").SetValue("从这里开始架设输电线路");
                    __.Field("_overrideString").SetValue("从这里开始架设输电线路");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonCantConnect);
                    __.Field("_baseString").SetValue("这里不允许架设输电线路");
                    __.Field("_overrideString").SetValue("这里不允许架设输电线路");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonCompleteLink);
                    __.Field("_baseString").SetValue("在这里结束架设, 预计消耗 {0} 个电缆");
                    __.Field("_overrideString").SetValue("在这里结束架设, 预计消耗 {0} 个电缆");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonTerminusNeedsPylon);
                    __.Field("_baseString").SetValue("转接口只允许连接到输电线路铁塔上");
                    __.Field("_overrideString").SetValue("转接口只允许连接到输电线路铁塔上");
                }

                {
                    var __ = Traverse.Create(GameStrings.ConnectionLimitReached);
                    __.Field("_baseString").SetValue("这里已经存在输电线路, 无法架设");
                    __.Field("_overrideString").SetValue("这里已经存在输电线路, 无法架设");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonUnlink);
                    __.Field("_baseString").SetValue("拆除所有连接到这里的输电线");
                    __.Field("_overrideString").SetValue("拆除所有连接到这里的输电线");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonNeedCablesToLink);
                    __.Field("_baseString").SetValue("缺少电缆, 无法架设, 预计需要 {0} 个电缆");
                    __.Field("_overrideString").SetValue("缺少电缆, 无法架设, 预计需要 {0} 个电缆");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonNoConnectionsToUnlink);
                    __.Field("_baseString").SetValue("这里没有输电线, 无法拆除");
                    __.Field("_overrideString").SetValue("这里没有输电线, 无法拆除");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonStartNodeTooFar);
                    __.Field("_baseString").SetValue("无法在这里结束架设, 输电线太长了");
                    __.Field("_overrideString").SetValue("无法在这里结束架设, 输电线太长了");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonPathObstructed);
                    __.Field("_baseString").SetValue("架设路径上存在障碍物, 无法架设");
                    __.Field("_overrideString").SetValue("架设路径上存在障碍物, 无法架设");
                }

                {
                    var __ = Traverse.Create(GameStrings.PylonLink);
                    __.Field("_baseString").SetValue("架设输电线");
                    __.Field("_overrideString").SetValue("架设输电线");
                }
            }

            static void 向UiComponentRenderer中添加新UI元素(ILayoutElement New, UiComponentRenderer UI层级)
            {
                switch (New)
                {
                    case TextMeshProUGUI 文本组件:
                        {
                            if (UI层级.TextComponents == null)
                            {
                                UI层级.TextComponents = [文本组件];
                            }
                            else if (!UI层级.TextComponents.Any(d => d == 文本组件))
                            {
                                Array.Resize(ref UI层级.TextComponents, UI层级.TextComponents.Length + 1);
                                UI层级.TextComponents[UI层级.TextComponents.Length - 1] = 文本组件;   // 添加新元素 
                            }
                            break;
                        }
                    case Image 纹理组件:
                        {
                            if (UI层级.ImageComponents == null)
                            {
                                UI层级.ImageComponents = [纹理组件];
                            }
                            else if (!UI层级.ImageComponents.Any(d => d == 纹理组件))
                            {
                                Array.Resize(ref UI层级.ImageComponents, UI层级.ImageComponents.Length + 1);
                                UI层级.ImageComponents[UI层级.ImageComponents.Length - 1] = 纹理组件;   // 添加新元素 
                            }
                            break;
                        }
                }
            }
        }
        private IEnumerator 等待角色捏脸场景加载完成后进行汉化修改()
        {
            // 由于角色捏脸场景进入时创建,离开后立刻销毁,没有复用,因此每次场景创建后都需要修改一次
            // 除非有办法在场景未加载前修改原始数据,母体变了,自然实例化的结果跟着变

            var 等待场景么 = true; Scene sceneAt = default;
            while (等待场景么)
            {
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    sceneAt = SceneManager.GetSceneAt(i);
                    if (sceneAt.name == "CharacterCustomisation" && sceneAt.isLoaded) { 等待场景么 = false; }
                }
                if (等待场景么) { yield return null; } // 等待下一帧
            }

            var rootObjects = sceneAt.GetRootGameObjects();
            foreach (var __ in rootObjects)
            {
                var 捏脸面板 = __.GetComponent<CharacterCreationPanel>();
                if (捏脸面板)
                {
                    var 胡子颜色调节 = (UIListIterator)Traverse.Create(捏脸面板).Field("_facialHairColourIterator").GetValue();
                    var 调节项名称 = 胡子颜色调节.GetComponent<TMP_Text>();
                    调节项名称.font = 前置_资源加载器.单例.当前TMP字体;
                    调节项名称.text = "胡子颜色";
                    // Log.LogMessage($"成功修改角色捏脸面板胡子颜色调节以支持中文");

                    var 试衣换装面板 = 捏脸面板.transform.GetChild(3);
                    var 标题 = 试衣换装面板.GetChild(0).GetComponent<TMP_Text>();
                    标题.font = 前置_资源加载器.单例.当前TMP字体;
                    标题.text = "试衣换装";

                    var 身体 = 试衣换装面板.GetChild(1).GetComponent<TMP_Text>();
                    身体.font = 前置_资源加载器.单例.当前TMP字体;
                    身体.text = "身体";

                    var 太空服 = 试衣换装面板.GetChild(2).GetChild(0).GetComponent<TMP_Text>();
                    太空服.font = 前置_资源加载器.单例.当前TMP字体;
                    太空服.text = "太空服";

                    var 运动服 = 试衣换装面板.GetChild(3).GetChild(0).GetComponent<TMP_Text>();
                    运动服.font = 前置_资源加载器.单例.当前TMP字体;
                    运动服.text = "运动服";

                    var 陆战队服 = 试衣换装面板.GetChild(4).GetChild(0).GetComponent<TMP_Text>();
                    陆战队服.font = 前置_资源加载器.单例.当前TMP字体;
                    陆战队服.text = "陆战队服";

                    var 头部 = 试衣换装面板.GetChild(5).GetComponent<TMP_Text>();
                    头部.font = 前置_资源加载器.单例.当前TMP字体;
                    头部.text = "头部";

                    var 太空头盔 = 试衣换装面板.GetChild(6).GetChild(0).GetComponent<TMP_Text>();
                    太空头盔.font = 前置_资源加载器.单例.当前TMP字体;
                    太空头盔.text = "太空头盔";

                    var 棒球帽 = 试衣换装面板.GetChild(7).GetChild(0).GetComponent<TMP_Text>();
                    棒球帽.font = 前置_资源加载器.单例.当前TMP字体;
                    棒球帽.text = "棒球帽";

                    var 陆战队头盔 = 试衣换装面板.GetChild(8).GetChild(0).GetComponent<TMP_Text>();
                    陆战队头盔.font = 前置_资源加载器.单例.当前TMP字体;
                    陆战队头盔.text = "陆战队头盔";

                    var 表情 = 试衣换装面板.GetChild(9).GetComponent<TMP_Text>();
                    表情.font = 前置_资源加载器.单例.当前TMP字体;
                    表情.text = "表情";

                    var 表情下拉列表 = 试衣换装面板.GetChild(10).GetComponent<TMP_Dropdown>();

                    // 修改下拉列表字体和文本
                    foreach (var 文本_纹理 in 表情下拉列表.options)
                    {
                        switch (文本_纹理.text)
                        {
                            case "None": 文本_纹理.text = "正常"; break;
                            case "Happy": 文本_纹理.text = "快乐"; break;
                            case "Angry": 文本_纹理.text = "生气"; break;
                            case "Surprised": 文本_纹理.text = "惊讶"; break;
                        }
                    }

                    var 当前 = 表情下拉列表.captionText;
                    当前.font = 前置_资源加载器.单例.当前TMP字体;
                    var 当前编号 = 表情下拉列表.value;
                    当前.text = 表情下拉列表.options[当前编号].text;

                    var 条目克隆母体 = 表情下拉列表.itemText;
                    条目克隆母体.font = 前置_资源加载器.单例.当前TMP字体;
                }
            }
        }


        public static IntPtr QuantityFontMinSize;
        public static IntPtr QuantityFontMaxSize;
        public static IntPtr QuantitySizeDeltaX;
        public static IntPtr QuantitySizeDeltaY;
        public static readonly Func<IntPtr> QuantityFontMinSizeRefGetter = 汉化模块工具.CreateRefGetter(type: typeof(SlotDisplayButton), fieldName: "QuantityFontMinSize");
        public static readonly Func<IntPtr> QuantityFontMaxSizeRefGetter = 汉化模块工具.CreateRefGetter(type: typeof(SlotDisplayButton), fieldName: "QuantityFontMaxSize");
        public static readonly Func<IntPtr> QuantitySizeDeltaXRefGetter = 汉化模块工具.CreateRefGetter(type: typeof(SlotDisplayButton), fieldName: "QuantitySizeDeltaX");
        public static readonly Func<IntPtr> QuantitySizeDeltaYRefGetter = 汉化模块工具.CreateRefGetter(type: typeof(SlotDisplayButton), fieldName: "QuantitySizeDeltaY");

        private void 关闭字体警报()
        {
            try // 字体缺少某字时会刷红字,很烦,这个字段就是关闭TMP警报日志
            { Traverse.Create(TMP_Settings.instance).Field("m_warningsDisabled").SetValue(true); }
            catch (Exception e) { Log.LogDebug($"关闭字体警报失败 {e.Message}"); }
            Log.LogMessage($"成功关闭字体警报");
        }

        private static void 修改玩家状态栏布局宽度(Transform obj)
        {
            if (obj)
            {
                foreach (var layout in obj.GetComponentsInChildren<LayoutElement>(includeInactive: true))
                {
                    layout.minWidth = 220;
                    layout.preferredWidth = 220;
                }
            }
        }
        private static void 修改玩家状态栏压力表宽度(GameObject obj, int 宽度 = 190)
        {
            if (obj)
            {
                var BGRect = obj.GetComponent<RectTransform>();
                BGRect.sizeDelta = new Vector2(宽度, BGRect.rect.height);
            }
        }
        private static TMP_Text 修改玩家状态栏字体尺寸(Transform obj, string 路径, int 字体尺寸 = 23, int 文本偏移 = 0, TextAlignmentOptions 对齐 = default)
        {
            obj = string.IsNullOrEmpty(路径) ? obj : obj?.Find(路径);
            if (obj == null) { return null; }
            var component = obj.GetComponent<TMP_Text>();
            汉化模块工具.修改字体尺寸(component, 字体尺寸);
            if (对齐 != default)
            { component.alignment = 对齐; }
            if (文本偏移 != 0)
            { component.rectTransform.anchoredPosition = new Vector2(文本偏移, component.rectTransform.anchoredPosition.y); }
            return component;
        }
    }


    public class 修改ImGui绘制字体指令
    {
        private static readonly MethodInfo getFont = AccessTools.Method(typeof(ImguiHelper), "GetFont", [typeof(int)]);
        private static readonly MethodInfo pushFont = AccessTools.Method(typeof(ImGui), "PushFont", [typeof(ImFontPtr)]);
        private static readonly MethodInfo popFont = AccessTools.Method(typeof(ImGui), "PopFont", Type.EmptyTypes);
        public static readonly CodeInstruction[] 添加新字体指针并应用 = [new CodeInstruction(OpCodes.Ldc_I4_2),
            new CodeInstruction(OpCodes.Call, getFont),new CodeInstruction(OpCodes.Call, pushFont)];
        public static readonly CodeInstruction 退回旧字体指针并应用 = new CodeInstruction(OpCodes.Call, popFont);
    }

    [HarmonyPatch(typeof(DeepMinerCartridge), nameof(DeepMinerCartridge.Redraw))]
    public class 修改功能卡深层矿物面板硬编码文本
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改功能卡深层矿物面板硬编码文本以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            for (var i = 0; i < IL指令序列.Count; i++)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string 内容)
                    {
                        switch (内容)
                        {
                            case "Region: ": IL指令序列[i].operand = "地区: "; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }

    // [HarmonyPatch(typeof(TraderContact), nameof(TraderContact.LoadStationContacts), [typeof(IEnumerable<StationContactData>)])]
    // public class TraderContacts_LoadStationContacts_Patch
    // {
    //     public static void Prefix(ref IEnumerable<StationContactData> contacts)
    //     {
    //         汉化模块.Log.LogMessage($"成功修改贸易商名称加上行业_前置");
    //         var 新 = contacts.ToArray();
    //         for (var i = 0; i < 新.Length; i++)
    //             新[i].ContactName = 新[i].ContactName.词条匹配();
    //         contacts = 新;
    //     }
    //     public static void Postfix()
    //     {
    //         汉化模块.Log.LogMessage($"成功修改贸易商名称加上行业_后置");
    //         foreach (var obj in TraderData.AllTraderData)
    //             for (var i = 0; i < obj.Names.Count; i++)
    //                 obj.Names[i].Value = obj.Names[i].Value.词条匹配();
    //     }
    // }

    [HarmonyPatch(typeof(WorkshopMenu), "RefreshButtons")]
    public class 修改创意工坊发布与取消订阅字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改创意工坊发布与取消订阅字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            for (var i = 0; i < IL指令序列.Count; i++)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string 内容)
                    {
                        switch (内容)
                        {
                            case "Publish": IL指令序列[i].operand = "发布到工坊"; break;
                            case "Unsubscribe": IL指令序列[i].operand = "取消订阅"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }


    [HarmonyPatch(typeof(ImGuiTerrainEditorTool), "DrawMeshBrowser")]
    public class 修改ImGui地形编辑工具DrawMeshBrowser字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改ImGui地形编辑工具DrawMeshBrowser字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            for (var i = 0; i < IL指令序列.Count; i++)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string 内容)
                    {
                        switch (内容)
                        {
                            case "Selected File: ": IL指令序列[i].operand = "选定的文件: "; break;
                            case "Scale": IL指令序列[i].operand = "比例"; break;
                            case "Import scale": IL指令序列[i].operand = "导入比例"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }

    [HarmonyPatch(typeof(ImGuiTerrainEditorTool), "DrawMain")]
    public class 修改ImGui地形编辑工具DrawMain字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改ImGui地形编辑工具DrawMain字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            for (var i = 0; i < IL指令序列.Count; i++)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string 内容)
                    {
                        switch (内容)
                        {
                            case "Mode": IL指令序列[i].operand = "模式"; break;
                            case "Position": IL指令序列[i].operand = "位置"; break;
                            case "###PositionDistance": IL指令序列[i].operand = "###位置距离"; break;
                            case "Texture1": IL指令序列[i].operand = "纹理1"; break;
                            case "Texture2": IL指令序列[i].operand = "纹理2"; break;
                            case "MacroTexture": IL指令序列[i].operand = "宏观纹理"; break;
                            case "Pin": IL指令序列[i].operand = "钳住"; break;
                            case "Continuous": IL指令序列[i].operand = "连续的"; break;
                            case "Undo": IL指令序列[i].operand = "撤销"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }


    [HarmonyPatch(typeof(ImGuiTerrainEditorTool), nameof(ImGuiTerrainEditorTool.Draw))]
    public class 修改ImGui地形编辑工具字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改ImGui地形编辑工具字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            return IL指令序列;
        }
    }


    [HarmonyPatch(typeof(ImGuiWindowManager), nameof(ImGuiWindowManager.Draw))]
    public class 修改ImGui窗口管理器字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改ImGui窗口管理器字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            return IL指令序列;
        }
    }


    [HarmonyPatch(typeof(WorldSettingToolsImguiWindow), nameof(WorldSettingToolsImguiWindow.Draw))]
    public class 修改世界设置工具Imgui窗口字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改世界设置工具Imgui窗口字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            for (var i = 0; i < IL指令序列.Count; i++)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string 内容)
                    {
                        switch (内容)
                        {
                            case "Populate": IL指令序列[i].operand = "填充"; break;
                            case "WorldId": IL指令序列[i].operand = "世界ID"; break;
                            case "ExportWorldSetting": IL指令序列[i].operand = "导出世界设置"; break;
                            case "ExportGHGGraphs": IL指令序列[i].operand = "导出温室气体图表"; break;
                            case "Apply": IL指令序列[i].operand = "应用"; break;

                            case "Tabs": IL指令序列[i].operand = "标签"; break;
                            case "GlobalTemperature": IL指令序列[i].operand = "全球温度"; break;
                            case "Average Global Temperature: ": IL指令序列[i].operand = "全球平均温度: "; break;
                            case "data": IL指令序列[i].operand = "数据"; break;

                            case "Data": IL指令序列[i].operand = "数据"; break;
                            case "Aggregate Temperature: ": IL指令序列[i].operand = "聚合温度: "; break;
                            case "Solar Angle: ": IL指令序列[i].operand = "太阳角: "; break;
                            case "Solar Radiation Offset: ": IL指令序列[i].operand = "太阳辐射抵消: "; break;
                            case "GHG Offset: ": IL指令序列[i].operand = "温室气体抵消: "; break;
                            case "Density Offset: ": IL指令序列[i].operand = "密度偏移: "; break;

                            case "Latent Offset: ": IL指令序列[i].operand = "潜在偏移: "; break;
                            case "External Energy Input Offset: ": IL指令序列[i].operand = "外部能量输入补偿: "; break;
                            case "Global GasMix": IL指令序列[i].operand = "全球气体混合"; break;
                            case "Volume": IL指令序列[i].operand = "体积"; break;
                            case "Ghg Index Graphs": IL指令序列[i].operand = "温室气体指数图"; break;
                            case "GHG Info": IL指令序列[i].operand = "温室气体信息"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }




    // [HarmonyPatch(typeof(GameInfoWindow), nameof(GameInfoWindow.Draw))]
    // public class 修改游戏信息窗口字体
    // {
    //     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         汉化模块.Log.LogMessage($"成功修改游戏信息窗口字体以支持中文");
    //         var IL指令序列 = new List<CodeInstruction>(instructions);

    //         // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
    //         IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

    //         // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
    //         for (int i = IL指令序列.Count - 1; i >= 0; i--)
    //         {
    //             if (IL指令序列[i].opcode == OpCodes.Ret)
    //             {
    //                 IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
    //             }
    //         }

    //         for (var i = 0; i < IL指令序列.Count; i++)
    //         {
    //             if (IL指令序列[i].opcode == OpCodes.Ldstr)
    //             {
    //                 if (IL指令序列[i].operand is string 内容)
    //                 {
    //                     switch (内容)
    //                     {
    //                         case "Game Info": IL指令序列[i].operand = "游戏信息"; break;
    //                         case "Tabs": IL指令序列[i].operand = "标签"; break;
    //                         case "Network Traffic": IL指令序列[i].operand = "网络流量"; break;
    //                         case "data": IL指令序列[i].operand = "data"; break;
    //                         case "NetworkData": IL指令序列[i].operand = "网络数据"; break;
    //                         case "Traffic": IL指令序列[i].operand = "交通"; break;
    //                         case "Incoming:": IL指令序列[i].operand = "来信:"; break;
    //                         case "Incoming Queue:": IL指令序列[i].operand = "待处理队列:"; break;
    //                         case "Outgoing:": IL指令序列[i].operand = "发出:"; break;
    //                         case "Outgoing Queue:": IL指令序列[i].operand = "发出队列:"; break;
    //                         case "State Immediate": IL指令序列[i].operand = "立即状态"; break;
    //                         case "Size:": IL指令序列[i].operand = "尺寸:"; break;
    //                         case "Fragments:": IL指令序列[i].operand = "片段:"; break;
    //                         case "Max Size:": IL指令序列[i].operand = "最大尺寸:"; break;
    //                         case "Max Fragments:": IL指令序列[i].operand = "最大片段:"; break;
    //                         case "Fragment Size:": IL指令序列[i].operand = "片段大小:"; break;
    //                         case "Max Fragment Size:": IL指令序列[i].operand = "最大片段大小:"; break;
    //                         case "Clear Graph": IL指令序列[i].operand = "清空图表"; break;
    //                     }
    //                 }
    //             }
    //         }

    //         return IL指令序列;
    //     }
    // }



    [HarmonyPatch(typeof(ThreadProfiler), nameof(ThreadProfiler.DrawBatchInfo))]
    public class 修改线程分析器字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改线程分析器体字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            return IL指令序列;
        }
    }



    [HarmonyPatch(typeof(RegionManager), nameof(RegionManager.DrawDebug))]
    public class 修改区域管理器字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改区域管理器字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            for (var i = 0; i < IL指令序列.Count; i++)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string 内容)
                    {
                        switch (内容)
                        {
                            case "Set: ": IL指令序列[i].operand = "设置: "; break;
                            case " Region: ": IL指令序列[i].operand = " 区域: "; break;
                            case "No regions found": IL指令序列[i].operand = "未找到区域"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }


    [HarmonyPatch(typeof(OrbitalSimulation), nameof(OrbitalSimulation.Draw))]
    public class 修改轨道模拟字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改轨道模拟字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            for (var i = 0; i < IL指令序列.Count; i++)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string 内容)
                    {
                        switch (内容)
                        {
                            case "IsEclipse: {0}": IL指令序列[i].operand = "是日食: {0}"; break;
                            case "IsRetrogradeRotation: {0}": IL指令序列[i].operand = "逆行旋转: {0}"; break;
                            case "Latitude: {0:F1}": IL指令序列[i].operand = "纬度: {0:F1}"; break;
                            case "Longitude: {0:F1}": IL指令序列[i].operand = "经度: {0:F1}"; break;
                            case "TimeOfDay: {0:F2}": IL指令序列[i].operand = "一天中的时间: {0:F2}"; break;
                            case "AtmosphereTimeOfDay: {0:F2}": IL指令序列[i].operand = "季节天数: {0:F2}"; break;
                            case "Timescale: {0:F6} (standard: {1:F6})": IL指令序列[i].operand = "时间尺度: {0:F6} (标准: {1:F6})"; break;
                            case "DistanceFromSun: {0:F3} AU": IL指令序列[i].operand = "距离太阳的距离: {0:F3} AU"; break;
                            case "SolarIrradiance: {0:F3} W/m2": IL指令序列[i].operand = "太阳辐射: {0:F3} W/m2"; break;
                            case "RotationSpeed: {0:F3}": IL指令序列[i].operand = "旋转速度: {0:F3}"; break;
                            case "SkyboxScale: {0:F4}": IL指令序列[i].operand = "天空盒比例: {0:F4}"; break;
                            case "BodyScale: {0:F1}": IL指令序列[i].operand = "身体比例: {0:F1}"; break;
                            case "SiderealDay: {0}": IL指令序列[i].operand = "恒星日: {0}"; break;
                            case "SiderealYear: {0}": IL指令序列[i].operand = "恒星年: {0}"; break;
                            case "NewTimeScale: {0}": IL指令序列[i].operand = "新时间尺度: {0}"; break;
                            case "DaysPast: {0}": IL指令序列[i].operand = "过去的天数: {0}"; break;
                            case "CurrentAngle: {0:F1}°": IL指令序列[i].operand = "当前角度: {0:F1}°"; break;
                            case "SolarProgressionPerDay: {0:F1}°": IL指令序列[i].operand = "每日太阳能进展: {0:F1}°"; break;
                            case "AccumulatedAngle: {0:F1}°": IL指令序列[i].operand = "累积角: {0:F1}°"; break;
                            case "TrueAnomaly: {0:F1}°": IL指令序列[i].operand = "真近点角: {0:F1}°"; break;
                            case "WrappedTrueAnomaly: {0:F1}°": IL指令序列[i].operand = "包装真近点角: {0:F1}°"; break;
                            case "TotalRealTimeSeconds: {0}": IL指令序列[i].operand = "总实时秒数: {0}"; break;
                            case "SimulationTimeSeconds: {0}": IL指令序列[i].operand = "模拟时间(秒): {0}"; break;
                            case "OffsetTime: ": IL指令序列[i].operand = "偏移时间: "; break;
                            case "AdjustdOffsetTime: {0}": IL指令序列[i].operand = "调整后的偏移时间: {0}"; break;
                            case "DayAngle: {0:F1}°": IL指令序列[i].operand = "日角: {0:F1}°"; break;
                            case "DayCount: {0}": IL指令序列[i].operand = "天数: {0}"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }


    [HarmonyPatch(typeof(ImguiHelper), nameof(ImguiHelper.DrawText), [typeof(string), typeof(Vector2), typeof(string), typeof(Vector4)])]
    public class 修改ImguiHelper字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改ImguiHelper字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            return IL指令序列;
        }
    }


    [HarmonyPatch(typeof(ImGuiLoadingScreen), "DrawProgressBar")]
    public class 修改加载屏幕进度条字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改加载屏幕进度条字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 为了防止BUG,在每帧开头将字体图集指针指向中文字体
            IL指令序列.InsertRange(0, 修改ImGui绘制字体指令.添加新字体指针并应用);

            // 在每帧结束用pop将字体图集指针弹出,防止栈溢出
            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ret)
                {
                    IL指令序列.Insert(i, 修改ImGui绘制字体指令.退回旧字体指针并应用);
                }
            }

            return IL指令序列;
        }
    }

    [HarmonyPatch(typeof(ConsoleWindow), nameof(ConsoleWindow.Draw))]
    public class 修改F3控制台字体
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 见<HarmonyPatch原理与IL指令序列.cs>文件,里面描述了本补丁的原理
            // 官方在新版中制作了ImGui的中文字体图集,在Localization.PushFont中也应用该字体,以下见详情
            // if (Localization.CurrentLanguage == LanguageCode.CN)
            // { ImGui.PushFont(ImguiHelper.GetFont(2)); }
            // 但是官方并不是在所有地方都应用了该字体,比如ConsoleWindow.Draw方法中就没有应用,因此需要手动补丁
            汉化模块.Log.LogMessage($"成功修改F3控制台字体以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);
            IL指令序列[1].opcode = OpCodes.Ldc_I4_2;

            return IL指令序列;
        }
    }

    [HarmonyPatch(typeof(MiningDrill), nameof(MiningDrill.ModeStrings), MethodType.Getter)]
    public class 修改手持电动矿机模式词条
    {
        public static string[] 模式词条表 = ["挖掘模式", "找平模式"];

        [HarmonyPrefix]
        public static void 修改按钮上的模式词条(ref string[] __result)
        {
            Traverse.Create(typeof(MiningDrill)).Field("_modeStrings").SetValue(模式词条表);
            汉化模块.Log.LogMessage($"成功修改手持电动矿机模式词条");
            汉化模块.补丁.Unpatch(typeof(MiningDrill).GetMethod("get_ModeStrings", BindingFlags.Instance | BindingFlags.Public), HarmonyPatchType.Prefix, 汉化模块.补丁.Id);
            汉化模块.Log.LogMessage($"成功卸载补丁=>修改手持电动矿机模式词条");
        }
    }

    [HarmonyPatch(typeof(PneumaticMiningDrill), nameof(PneumaticMiningDrill.ModeStrings), MethodType.Getter)]
    public class 修改手持气动矿机模式词条
    {
        [HarmonyPrefix]
        public static void 修改按钮上的模式词条(ref string[] __result)
        {
            Traverse.Create(typeof(PneumaticMiningDrill)).Field("_modeStrings").SetValue(修改手持电动矿机模式词条.模式词条表);
            汉化模块.Log.LogMessage($"成功修改手持气动矿机模式词条");
            汉化模块.补丁.Unpatch(typeof(PneumaticMiningDrill).GetMethod("get_ModeStrings", BindingFlags.Instance | BindingFlags.Public), HarmonyPatchType.Prefix, 汉化模块.补丁.Id);
            汉化模块.Log.LogMessage($"成功卸载补丁=>修改手持气动矿机模式词条");
        }
    }

    // [HarmonyPatch(typeof(Jetpack), nameof(Jetpack.GetContextualName), typeof(Interactable))]
    // public class 修改喷气背包自动悬停词条
    // {
    //     [HarmonyPrefix]
    //     public static void 修改按钮上的自动悬停词条(ref string __result, Jetpack __instance, Interactable interactable)
    //     {
    //         var __ = Traverse.Create(GameStrings.JetpackStabilizerState);
    //         __.Field("_baseString").SetValue("自动悬停 {0}");
    //         __.Field("_overrideString").SetValue("自动悬停 {0}");
    //         汉化模块.Log.LogMessage($"成功修改喷气背包自动悬停词条");
    //         汉化模块.补丁.Unpatch(typeof(Jetpack).GetMethod(nameof(Jetpack.GetContextualName), BindingFlags.Instance | BindingFlags.Public), HarmonyPatchType.Prefix, 汉化模块.补丁.Id);
    //         汉化模块.Log.LogMessage($"成功卸载补丁=>修改喷气背包自动悬停词条");
    //     }
    // }

    [HarmonyPatch(typeof(SettingItem), nameof(SettingItem.Setup))]
    public class 修改游戏设置中的HUD缩放下限
    {
        [HarmonyPrefix]
        public static void 修改HUD缩放按钮(SettingItem __instance)
        {
            // 汉化模块.Log.LogMessage("调用=>修改游戏设置中的HUD缩放下限");
            var HUD滑动条 = __instance.Selectable as Slider;
            if (HUD滑动条)
            {
                if (HUD滑动条.minValue == 25)
                {
                    HUD滑动条.minValue = 1;
                    汉化模块.Log.LogMessage("成功修改游戏设置中的HUD缩放下限");
                    汉化模块.补丁.Unpatch(typeof(SettingItem).GetMethod(nameof(SettingItem.Setup), BindingFlags.Instance | BindingFlags.Public), HarmonyPatchType.Prefix, 汉化模块.补丁.Id);
                    汉化模块.Log.LogMessage($"成功卸载补丁=>修改游戏设置中的HUD缩放下限");
                }
            }
        }
    }

    [HarmonyPatch(typeof(InventoryWindow), nameof(InventoryWindow.SetVisible))]
    public class 修复背包垂直布局间距不一致
    {
        // 背包窗口不这样做,最底下会空一格,且间距不一样
        public static void Prefix(ref InventoryWindow __instance, bool isVisble)
        {
            __instance.RectTransform.gameObject.SetActive(isVisble);
        }
    }

    public static class 汉化模块工具
    {
        public static void 修改字体尺寸(TMP_Text component, float fontSize)
        {
            if (component == null)
            {
                汉化模块.Log.LogDebug($"TMP_Text是空引用,无法修改字体尺寸");
                return;
            }

            if (component.fontSize < fontSize)
            {
                component.fontSize = fontSize;
                component.fontSizeMin = fontSize;
            }
            if (component.fontSizeMax < fontSize) { component.fontSizeMax = fontSize; }
        }

        public static void 修改字体尺寸(Text component, int fontSize)
        {
            if (component == null)
            {
                汉化模块.Log.LogDebug($"Text是空引用,无法修改字体尺寸");
                return;
            }

            if (component.fontSize < fontSize)
            {
                component.fontSize = fontSize;
                component.resizeTextMinSize = fontSize;
            }
            if (component.resizeTextMaxSize < fontSize) { component.resizeTextMaxSize = fontSize; }
        }

        public static Func<IntPtr> CreateRefGetter(Type type, string fieldName)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            var fi = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (fi == null) { throw new MissingFieldException(type.FullName, fieldName); }

            var dm = new DynamicMethod(
                name: "GetRef_" + type.FullName + "_" + fieldName,
                returnType: typeof(IntPtr),
                parameterTypes: Type.EmptyTypes,
                m: fi.Module,
                skipVisibility: true);

            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldsflda, fi);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Ret);

            return (Func<IntPtr>)dm.CreateDelegate(typeof(Func<IntPtr>));
        }


        // public static string 词条匹配(this string 源)
        // {
        //     string 结果 = null;

        //     switch (源)
        //     {
        //         case "More Ore Less": 结果 = "MoreOreLess-<size=88%>矿石</size>"; break;
        //         case "Asteroid Assayers": 结果 = "AsteroidAssayers-<size=88%>矿石</size>"; break;
        //         case "Cosmic Crush": 结果 = "CosmicCrush-<size=88%>矿石</size>"; break;
        //         case "Galactic Gravels": 结果 = "GalacticGravels-<size=88%>矿石</size>"; break;
        //         case "Nebula Nuggets": 结果 = "NebulaNuggets-<size=88%>矿石</size>"; break;
        //         case "Orbit Ore Oasis": 结果 = "OrbitOreOasis-<size=88%>矿石</size>"; break;
        //         case "Stellar Stone Supply": 结果 = "StellarStoneSupply-<size=88%>矿石</size>"; break;
        //         case "Interstellar Excavators": 结果 = "InterstellarExcavators-<size=88%>矿石</size>"; break;
        //         case "Void Vein Vendors": 结果 = "VoidVeinVendors-<size=88%>矿石</size>"; break;
        //         case "Planetary Pebbles": 结果 = "PlanetaryPebbles-<size=88%>矿石</size>"; break;

        //         case "All Alloys": 结果 = "AllAlloys-<size=88%>合金</size>"; break;
        //         case "Metal Mavens": 结果 = "MetalMavens-<size=88%>合金</size>"; break;
        //         case "AstroAlloy Emporium": 结果 = "AstroAlloyEmporium-<size=88%>合金</size>"; break;
        //         case "Cosmic Forge": 结果 = "CosmicForge-<size=88%>合金</size>"; break;
        //         case "Galactic Metallurgy": 结果 = "GalacticMetallurgy-<size=88%>合金</size>"; break;
        //         case "OrbitOre Outfitters": 结果 = "OrbitOreOutfitters-<size=88%>合金</size>"; break;
        //         case "Stellar Smelter": 结果 = "StellarSmelter-<size=88%>合金</size>"; break;
        //         case "Interstellar Ingots": 结果 = "InterstellarIngots-<size=88%>合金</size>"; break;
        //         case "Nebula Nucleus": 结果 = "NebulaNucleus-<size=88%>合金</size>"; break;
        //         case "Space Alloy Specialists": 结果 = "SpaceAlloySpecialists-<size=88%>合金</size>"; break;
        //         case "Star Smelter": 结果 = "StarSmelter-<size=88%>合金</size>"; break;

        //         case "Starlight Suppers": 结果 = "StarlightSuppers-<size=88%>食品</size>"; break;
        //         case "Galactic Groceries": 结果 = "GalacticGroceries-<size=88%>食品</size>"; break;
        //         case "Orbiting Organics": 结果 = "OrbitingOrganics-<size=88%>食品</size>"; break;
        //         case "Cosmic Cuisine": 结果 = "CosmicCuisine-<size=88%>食品</size>"; break;
        //         case "Asteroid Eats": 结果 = "AsteroidEats-<size=88%>食品</size>"; break;
        //         case "Nebula Nibbles": 结果 = "NebulaNibbles-<size=88%>食品</size>"; break;
        //         case "Stellar Snacks": 结果 = "StellarSnacks-<size=88%>食品</size>"; break;
        //         case "Interstellar Ingredients": 结果 = "InterstellarIngredients-<size=88%>食品</size>"; break;
        //         case "Space Spices": 结果 = "SpaceSpices-<size=88%>食品</size>"; break;
        //         case "Void Vegetables": 结果 = "VoidVegetables-<size=88%>食品</size>"; break;
        //         case "Planetary Produce": 结果 = "PlanetaryProduce-<size=88%>食品</size>"; break;

        //         case "Green Futures": 结果 = "GreenFutures-<size=88%>水培</size>"; break;
        //         case "AstroAgronomics": 结果 = "AstroAgronomics-<size=88%>水培</size>"; break;
        //         case "Stellar Sprouts": 结果 = "StellarSprouts-<size=88%>水培</size>"; break;
        //         case "HydroHarvest Haven": 结果 = "HydroHarvestHaven-<size=88%>水培</size>"; break;
        //         case "Orbiting Orchards": 结果 = "OrbitingOrchards-<size=88%>水培</size>"; break;
        //         case "Galactic Growers": 结果 = "GalacticGrowers-<size=88%>水培</size>"; break;
        //         case "CosmoCrop Connect": 结果 = "CosmoCropConnect-<size=88%>水培</size>"; break;
        //         case "Space Sprout Suppliers": 结果 = "SpaceSproutSuppliers-<size=88%>水培</size>"; break;
        //         case "Nebula Nurturers": 结果 = "NebulaNurturers-<size=88%>水培</size>"; break;
        //         case "Star Seedlings": 结果 = "StarSeedlings-<size=88%>水培</size>"; break;
        //         case "EcoSphere Essentials": 结果 = "EcoSphereEssentials-<size=88%>水培</size>"; break;
        //         case "Interstellar Irrigation": 结果 = "InterstellarIrrigation-<size=88%>水培</size>"; break;

        //         case "GasForLess": 结果 = "GasForLess-<size=88%>气体</size>"; break;
        //         case "AstroAether": 结果 = "AstroAether-<size=88%>气体</size>"; break;
        //         case "Cosmic Clouds": 结果 = "CosmicClouds-<size=88%>气体</size>"; break;
        //         case "Nebula Nectars": 结果 = "NebulaNectars-<size=88%>气体</size>|<size=88%>液体</size>"; break;
        //         case "Orbiting Oxygens": 结果 = "OrbitingOxygens-<size=88%>气体</size>"; break;
        //         case "Galactic Gases": 结果 = "GalacticGases-<size=88%>气体</size>"; break;
        //         case "Stellar Steam": 结果 = "StellarSteam-<size=88%>气体</size>"; break;
        //         case "Interstellar Inhalants": 结果 = "InterstellarInhalants-<size=88%>气体</size>"; break;
        //         case "Void Vapors": 结果 = "VoidVapors-<size=88%>气体</size>"; break;
        //         case "Space Gas Station": 结果 = "SpaceGasStation-<size=88%>气体</size>"; break;

        //         case "Build INC": 结果 = "Build INC-建材"; break;

        //         case "Payless Liquids": 结果 = "PaylessLiquids-<size=88%>液体</size>"; break;
        //         case "Frosty Barrels": 结果 = "FrostyBarrels-<size=88%>液体</size>"; break;
        //         case "Cosmic Concoctions": 结果 = "CosmicConcoctions-<size=88%>液体</size>"; break;
        //         case "Galactic Gush": 结果 = "GalacticGush-<size=88%>液体</size>"; break;
        //         case "Orbital Oceans": 结果 = "OrbitalOceans-<size=88%>液体</size>"; break;
        //         case "Stellar Streams": 结果 = "StellarStreams-<size=88%>液体</size>"; break;
        //         case "Interstellar Icicles": 结果 = "InterstellarIcicles-<size=88%>液体</size>"; break;
        //         case "Void Vessels": 结果 = "VoidVessels-<size=88%>液体</size>"; break;
        //         case "Space Springs": 结果 = "SpaceSprings-<size=88%>液体</size>"; break;
        //         case "Star Sippers": 结果 = "StarSippers-<size=88%>液体</size>"; break;

        //         case "Cosmic Tools &amp; More": 结果 = "CosmicTools&amp;More-<size=88%>成品</size>"; break;
        //         case "Galactic Gearworks": 结果 = "GalacticGearworks-<size=88%>成品</size>"; break;
        //         case "Stellar Supplies": 结果 = "StellarSupplies-<size=88%>成品</size>"; break;
        //         case "OrbitOps Hardware": 结果 = "OrbitOpsHardware-<size=88%>成品</size>"; break;
        //         case "Interstellar Implements": 结果 = "InterstellarImplements-<size=88%>成品</size>"; break;
        //         case "Void Ventures": 结果 = "VoidVentures-<size=88%>成品</size>"; break;
        //         case "Asteroid Artisans": 结果 = "AsteroidArtisans-<size=88%>成品</size>"; break;
        //         case "Space Spanners": 结果 = "SpaceSpanners-<size=88%>成品</size>"; break;
        //         case "Meteor Mechanics": 结果 = "MeteorMechanics-<size=88%>成品</size>|<size=88%>家电</size>"; break;

        //         case "AstroMart": 结果 = "AstroMart-<size=88%>耗材</size>"; break;
        //         case "Cosmo's Convenience": 结果 = "Cosmo'sConvenience-<size=88%>耗材</size>"; break;
        //         case "StarStop": 结果 = "StarStop-<size=88%>耗材</size>"; break;
        //         case "Nebula Necessities": 结果 = "NebulaNecessities-<size=88%>耗材</size>"; break;
        //         case "Interstellar Essentials": 结果 = "InterstellarEssentials-<size=88%>耗材</size>"; break;
        //         case "Void Vending": 结果 = "VoidVending-<size=88%>耗材</size>"; break;
        //         case "Meteor Munchies": 结果 = "MeteorMunchies-<size=88%>耗材</size>"; break;

        //         case "Galactic Gadgets": 结果 = "GalacticGadgets-<size=88%>家电</size>"; break;
        //         case "Orbitron Appliances": 结果 = "OrbitronAppliances-<size=88%>家电</size>"; break;
        //         case "Stellar Systems Store": 结果 = "StellarSystemsStore-<size=88%>家电</size>"; break;
        //         case "Space Savvy Solutions": 结果 = "SpaceSavvySolutions-<size=88%>家电</size>"; break;
        //         case "Interstellar Innovations": 结果 = "InterstellarInnovations-<size=88%>家电</size>"; break;
        //         case "Void Visions": 结果 = "VoidVisions-<size=88%>家电</size>"; break;
        //         case "Asteroid Appliances": 结果 = "AsteroidAppliances-<size=88%>家电</size>"; break;
        //     }

        //     return 结果 ?? 源;
        // }

    }

    [HarmonyPatch(typeof(SpawnGas), nameof(SpawnGas.ToString))]
    public class 修复发酵罐面板硬编码以支持中文
    {
        [HarmonyPrefix]
        public static bool __(ref string __result, SpawnGas __instance)
        {
            var _ = string.Empty;
            switch (__instance.Name)
            {
                case "Oxygen": _ = "氧气"; break;
                case "Nitrogen": _ = "氮气"; break;
                case "CarbonDioxide": _ = "二氧化碳"; break;
                case "Methane": _ = "甲烷"; break;
                case "Pollutant": _ = "污染物"; break;
                case "Water": _ = "水"; break;
                case "NitrousOxide": _ = "一氧化二氮"; break;
                case "LiquidNitrogen": _ = "液氮"; break;
                case "LiquidOxygen": _ = "液氧"; break;
                case "LiquidMethane": _ = "液态甲烷"; break;
                case "Steam": _ = "水蒸汽"; break;
                case "LiquidCarbonDioxide": _ = "液态二氧化碳"; break;
                case "LiquidPollutant": _ = "液态污染物"; break;
                case "LiquidNitrousOxide": _ = "液态一氧化二氮"; break;
                case "Hydrogen": _ = "氢气"; break;
                case "LiquidHydrogen": _ = "液氢"; break;
                case "PollutedWater": _ = "污水"; break;
                case "Hydrazine": _ = "联氨"; break;
                case "LiquidHydrazine": _ = "液态联氨"; break;
                case "LiquidAlcohol": _ = "液态酒精"; break;
                case "Helium": _ = "氦气"; break;
                case "LiquidSodiumChloride": _ = "液态氯化钠"; break;
                case "Silanol": _ = "硅醇"; break;
                case "LiquidSilanol": _ = "液态硅醇"; break;
                case "HydrochloricAcid": _ = "盐酸"; break;
                case "LiquidHydrochloricAcid": _ = "液态盐酸"; break;
                case "Ozone": _ = "臭氧"; break;
                case "LiquidOzone": _ = "液态臭氧"; break;
                case "Air": _ = "空气"; break;
                case "Fuel": _ = "燃料"; break;
                default:
                    return true;
            }

            __result = $"<color=yellow>{__instance.Quantity} mol</color> x <link=Gas{__instance.Type}><color=#44AD83>{_}</color></link>";
            return false;
        }
    }

    [HarmonyPatch(typeof(AdvancedAirlockControl), nameof(AdvancedAirlockControl.RefreshScreen))]
    public class 修改高级气闸控制电路板硬编码
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改高级气闸控制电路板硬编码以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string str)
                    {
                        switch (str)
                        {
                            case "ERROR": IL指令序列[i].operand = "存在错误"; break;
                            case "IN CONFIG": IL指令序列[i].operand = "在配置界面中"; break;
                            case "CANCEL": IL指令序列[i].operand = "强行取消"; break;
                            case "PRESSURIZE": IL指令序列[i].operand = "加压过程"; break;
                            case "CYCLE": IL指令序列[i].operand = "执行通过程序"; break;
                            case "TO EXTERIOR": IL指令序列[i].operand = "目标: 外舱"; break;
                            case "DEPRESSURIZE": IL指令序列[i].operand = "减压过程"; break;
                            case "TO INTERIOR": IL指令序列[i].operand = "目标: 内舱"; break;
                            case "External:\n<b>{0}</b>": IL指令序列[i].operand = "目标是外舱时加压至:\n<b>{0}</b>"; break;
                            case "Internal:\n<b>{0}</b>": IL指令序列[i].operand = "目标是内舱时加压至:\n<b>{0}</b>"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }

    [HarmonyPatch(typeof(AdvancedAirlockControl), nameof(AdvancedAirlockControl.DeviceListChangeTask))]
    public class 修改高级气闸控制电路板硬编码2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改高级气闸控制电路板硬编码2以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            for (int i = IL指令序列.Count - 1; i >= 0; i--)
            {
                if (IL指令序列[i].opcode == OpCodes.Ldstr)
                {
                    if (IL指令序列[i].operand is string str)
                    {
                        switch (str)
                        {
                            case "INTERIOR": IL指令序列[i].operand = "内舱"; break;
                            case "EXTERIOR": IL指令序列[i].operand = "外舱"; break;
                            case "><b>LIGHT</b></color>": IL指令序列[i].operand = "><b>气闸灯光</b></color>"; break;
                            case "><b>SENSOR</b></color>": IL指令序列[i].operand = "><b>气闸传感器</b></color>"; break;
                        }
                    }
                }
            }

            return IL指令序列;
        }
    }

    [HarmonyPatch(typeof(Stationpedia), "StartSearchCountdown")]
    public class 修改F1百科搜索栏启动搜索最少字数
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            汉化模块.Log.LogMessage($"成功修改F1百科搜索栏启动搜索最少字数以支持中文");
            var IL指令序列 = new List<CodeInstruction>(instructions);

            // 修改启动搜索最少字数3为1
            if (IL指令序列[7].opcode == OpCodes.Ldc_I4_3)
            {
                IL指令序列[7].opcode = OpCodes.Ldc_I4_1;
            }

            return IL指令序列;
        }
    }

    [HarmonyPatch(typeof(CopyToClipboardHandler), nameof(CopyToClipboardHandler.OnPointerClick))]
    public class F1百科复制到剪切板时尝试切换创造模式的当前生成物品
    {
        [HarmonyPostfix]
        public static void 执行()
        {
            if (GameManager.IsTutorial || WorldManager.Instance.GameMode != GameMode.Creative) { return; }

            var 剪切板 = GameManager.Clipboard;
            if (string.IsNullOrEmpty(剪切板) || string.IsNullOrWhiteSpace(剪切板)) { return; }

            Thing 预制体;
            bool 是否可生成 = false;

            if (int.TryParse(剪切板, out var 预设哈希) && Prefab.TryFind(预设哈希, out 预制体))
            {
                是否可生成 = true;
                汉化模块.Log.LogMessage($"剪切板内容是预设哈希: {预设哈希}, 已切换当前生成物品: {预制体.DisplayName}");
            }
            else if (Prefab.TryFind(剪切板, out 预制体))
            {
                是否可生成 = true;
                汉化模块.Log.LogMessage($"剪切板内容是预设名称: {剪切板}, 已切换当前生成物品: {预制体.DisplayName}");
            }

            if (是否可生成 && 预制体 != null && 在创造模式下该物品是否可按F9键生成(预制体)) { InventoryManager.SpawnPrefab = 预制体; }
        }

        public static readonly Func<Thing, bool> 在创造模式下该物品是否可按F9键生成 = static (d) => d is DynamicThing && !d.CompareTag("NotSpawnable");
    }

    [HarmonyPatch(typeof(HelpLinkHandler), nameof(HelpLinkHandler.OnPointerClick))]
    public class F1百科复制到剪切板时尝试切换创造模式的当前生成物品2
    {
        [HarmonyPostfix]
        public static void 执行() { F1百科复制到剪切板时尝试切换创造模式的当前生成物品.执行(); }
    }
}