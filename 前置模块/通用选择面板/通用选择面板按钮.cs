using System;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.UI;
using Reagents;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 通用选择面板按钮 : UserInterfaceAnimated, IScreenSpaceTooltip
    {
        public 通用可选择项目 项目信息;
        public virtual string DisplayName
        {
            get
            {
                switch (项目信息.解包标志)
                {
                    case 通用可选择项目.数据解包标志.物联网已上线设备:
                    case 通用可选择项目.数据解包标志.内部储物:
                        return 项目信息.链接物体.DisplayName;
                    case 通用可选择项目.数据解包标志.贸易商船:
                        return 项目信息.链接贸易商船.DisplayName;
                    case 通用可选择项目.数据解包标志.厨电配方:
                        return 项目信息.链接厨电配方.成品.DisplayName;
                }

                return 右侧文本.text;
            }
        }
        public bool TooltipIsVisible => IsVisible;
        public Image 左侧缩略图;
        public TextMeshProUGUI 右侧文本;
        public Button 按钮;
        public virtual void 按钮点击事件()
        {
            // 在构造按钮时,将这个方法赋给按钮的onClick;
            // 左侧缩略图和右侧文本和按钮和可选择项目都在同一个布局,可选择项目相当于是对外暴露的API(管理纹理与文本、按钮事件)
            通用选择面板.当前选择项目 = this;
            通用选择面板.按钮点击事件();
        }
        public void 构造初始化()
        {
            GameObject = gameObject;        // 原版游戏有些函数依赖GameObject
            按钮.onClick.RemoveAllListeners();
            按钮.onClick.AddListener(按钮点击事件);
        }
        public void 复用初始化(通用可选择项目 注册项目)
        {
            if (项目信息 == null) { 项目信息 = 通用可选择项目.拷贝构造(ref 注册项目); }
            else { 通用可选择项目.移动赋值(ref 注册项目, ref 项目信息); }

            switch (项目信息.解包标志)
            {
                case 通用可选择项目.数据解包标志.物联网已上线设备:
                    {
                        // 每次显示前,更新右侧文本为该物体被贴标机修改过的最新DisplayName
                        左侧缩略图.sprite = 项目信息.链接物体.Thumbnail;
                        右侧文本.text = 项目信息.链接物体.ToTooltip() + "\n" + Localization.GetSlotTooltip(Slot.Class.None);
                        break;
                    }
                case 通用可选择项目.数据解包标志.内部储物:
                    {
                        // 每次显示前,更新右侧文本为该物体被贴标机修改过的最新DisplayName
                        左侧缩略图.sprite = 项目信息.链接物体.Thumbnail;
                        var 数量 = (!(项目信息.链接物体 is Stackable { Quantity: > 1 } 可堆垛物)) ? 1 : 可堆垛物.Quantity;
                        const string 数量颜色 = "#FFED29";
                        右侧文本.text = $"{项目信息.链接物体.ToTooltip()} x<color={数量颜色}>{数量}</color>";
                        break;
                    }
                case 通用可选择项目.数据解包标志.贸易商船:
                    {
                        左侧缩略图.sprite = 项目信息.主物体.Thumbnail;  // 当前选择的卫星天线是主物体
                        const string 颜色 = "#FFED29";
                        右侧文本.text = $"<color=green>{项目信息.链接贸易商船.DisplayName}</color> <color={颜色}>{项目信息.链接贸易商船.Angle.ToString(null, null)}</color>";
                        break;
                    }
                case 通用可选择项目.数据解包标志.厨电配方:
                    {
                        左侧缩略图.sprite = 项目信息.链接厨电配方.成品.Thumbnail;
                        右侧文本.text = 项目信息.链接厨电配方.成品.ToTooltip() + "\n" + Localization.GetSlotTooltip(Slot.Class.None);
                        break;
                    }
                default:
                    {
                        左侧缩略图.sprite = 项目信息.链接物体 ? 项目信息.链接物体.Thumbnail : 项目信息.主物体.Thumbnail;

                        var __ = string.Empty;
                        switch (项目信息.解包标志)
                        {
                            case 通用可选择项目.数据解包标志.试剂参数:
                                右侧文本.text = ((Reagent)项目信息.物联网已上线设备表或内部储物表或试剂引用).DisplayName + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.逻辑参数:
                                __ = Enum.GetName(typeof(LogicType), 项目信息.操作数["参数"]);
                                右侧文本.text = __ + "\n<color=yellow>" + Localization.GetInterface(UnityEngine.Animator.StringToHash($"LogicType{__}")) + "</color>"; break;
                            case 通用可选择项目.数据解包标志.插槽参数:
                                __ = EnumCollections.LogicSlotTypes.GetName((LogicSlotType)项目信息.操作数["参数"], false);
                                右侧文本.text = __ + "\n<color=yellow>" + Localization.GetInterface(UnityEngine.Animator.StringToHash($"LogicSlotType{__}")) + "</color>"; break;
                            case 通用可选择项目.数据解包标志.插槽编号:
                                右侧文本.text = 项目信息.链接物体.GetSlot(项目信息.操作数["插槽编号"]).DisplayName + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.试剂模式:
                                右侧文本.text = Enum.GetName(typeof(LogicReagentMode), 项目信息.操作数["参数"]) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.统计模式:
                                右侧文本.text = Enum.GetName(typeof(LogicBatchMethod), 项目信息.操作数["参数"]) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.基础数学运算符:
                                右侧文本.text = LogicMath.EnumOperators.GetName((MathOperators)项目信息.操作数["参数"], false) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.高等数学运算符:
                                右侧文本.text = LogicMathUnary.EnumOperators.GetNameFromValue(项目信息.操作数["参数"], false) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.比较运算符:
                                右侧文本.text = Enum.GetName(typeof(ConditionOperation), 项目信息.操作数["参数"]) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.逻辑门运算符:
                                右侧文本.text = Enum.GetName(typeof(GateOperators), 项目信息.操作数["参数"]) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.最大最小值运算符:
                                右侧文本.text = Enum.GetName(typeof(通用可选择项目.ComparisonOperation), 项目信息.操作数["参数"]) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                            case 通用可选择项目.数据解包标志.物联网信号模式:
                                右侧文本.text = Enum.GetName(typeof(通用可选择项目.信号模式), 项目信息.操作数["参数"]) + "\n" + Localization.GetSlotTooltip(Slot.Class.None); break;
                        }

                        break;
                    }
            }
        }
        public virtual string 交互提示面板内容()
        {
            switch (项目信息.解包标志)
            {
                case 通用可选择项目.数据解包标志.内部储物:
                    return 项目信息.链接物体.GetExtendedText().ToString();      // DisplayName显示物品名称, 在此处尝试显示该物品的槽位中有哪些子物品
                case 通用可选择项目.数据解包标志.厨电配方:
                    return 项目信息.链接厨电配方.配方.ToString();                // DisplayName显示成品名称, 在此处尝试显示该配方的详细信息
            }

            return "模组制作真好玩";
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            // 此函数由Unity引擎自动调用,当光标进入感应区域时调用
            base.OnPointerEnter(eventData);
            PanelToolTip.Instance.SetUpTooltip(DisplayName, 交互提示面板内容(), this);
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

}