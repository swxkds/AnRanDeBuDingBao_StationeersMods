using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class LogicUnitBaseUI : 绘制单元基类, I文本, I简易UI, I完整UI, I派生
    {
        
       
        public LogicUnitBaseUI() : base() { }
        public bool 派生么(Thing thing) => typeof(LogicUnitBase).IsAssignableFrom(thing.GetType());
        public Type ThingType() => typeof(LogicUnitBase);
        public string ThingString(Thing thing)
        {
            var obj = thing as LogicUnitBase;
            if (obj.Powered && obj.Error == 0) { return $"{obj.DisplayName} {换算工具.DoubleToString(obj.Setting)}"; }
            else { return $"{obj.DisplayName} OFF"; }
        }
        protected override object事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么, Thing thing)
        {
            // Log.Debug(() => $"检测到{thing.name}");

            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<object事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            var name = UI面板表格构造工具.构造TMP(layoutRect, 词条库类.name, 世界坐标系么);
            事件容器.添加事件((th) => name.text = Utils.GetDisplayName(th));

            (TextMeshProUGUI Tmp, RawImage Bkgd) setting = UI面板表格构造工具.构造单元格(layoutRect, 词条库类.setting, 1, 世界坐标系么);
            事件容器.添加事件((th) =>
            {
                var obj = th as LogicUnitBase;
                if (obj.Powered && obj.Error == 0) { setting.Tmp.text = 换算工具.DoubleToString(obj.Setting); }
                else { setting.Tmp.text = "OFF"; }
            });

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        protected override object事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么, Thing thing) => 构造简易UI绘制单元(parentRect, 世界坐标系么, thing);
    }
}
