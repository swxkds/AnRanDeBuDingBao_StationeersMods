using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class LogicBatchReaderUI : 绘制单元基类, I文本, I简易UI
    {


        public LogicBatchReaderUI() : base() { }
        public Type ThingType() => typeof(LogicBatchReader);
        public string ThingString(Thing thing)
        {
            var obj = thing as LogicBatchReader;

            return $"{Utils.GetDisplayName(obj)}->{obj.BatchMethod} {obj.LogicType} {obj.CurrentPrefabHash} {obj.InputNetwork1DevicesSorted}";
        }
        protected override object事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么, Thing thing)
        {
            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<object事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            var name = UI面板表格构造工具.构造TMP(layoutRect, 词条库类.name, 世界坐标系么);
            事件容器.添加事件((th) => name.text = $"{Utils.GetDisplayName(th)}");

            // Log.Debug(()=>"批量读取器的构造简易UI绘制单元");

            // TODO:使用标记语言解析器分析参数,根据参数构造具体的UI
            if (thing.DisplayName.Contains("BatchRatio"))
            { 构造可变绘制单元_电池(layoutRect, 世界坐标系么, thing, 事件容器); }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        protected override object事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么, Thing thing) => null;
        private void 构造可变绘制单元_电池(RectTransform parentRect, bool 世界坐标系么, Thing thing, object事件容器 事件容器)
        {
            //     var 日志面板消息调度器 = 构造日志UI回调(new 日志面板消息调度器类(), thing as LogicBatchReader);
            //     var 弹窗面板消息调度器 = new 弹窗面板特效管理器();

            //     var list = UI面板表格构造工具.构造单单元格水平序列(parentRect, new string[] { 词条库类.ratio, 词条库类.储电 }, 世界坐标系么, UI面板表格构造工具.默认valueBkgdColor);
            //     (TextMeshProUGUI Tmp, RawImage Bkgd) 储电比例 = list[0];
            //     事件容器.添加事件((自定义泛型事件容器类<object>.函数对象容器)((th) =>
            //             {
            //                 var obj = th as LogicBatchReader;
            //                 if (obj.Powered && obj.Error == 0)
            //                 {
            //                     // 批量读取器,分别是统计方式,逻辑参数,读取什么物体,数据口相临节点树
            //                     var ratio = Math.Round(100 * Device.BatchRead(obj.BatchMethod, obj.LogicType, obj.CurrentPrefabHash, obj.InputNetwork1DevicesSorted));
            //                     储电比例.Tmp.text = $"{ratio}%";

            //                     if (ratio <= 20)
            //                     {
            //                         储电比例.Bkgd.color = new Color(0.5f, 0, 0, 0.4f);
            //                     }
            //                     else
            //                     {
            //                         储电比例.Bkgd.color = new Color(0, 0, 0, 0f);
            //                     }

            //                     if (ratio <= 20) { 日志面板消息调度器.构造一条日志消息(); }
            //                     else if (ratio > 21) { 日志面板消息调度器.ResetCount(); }

            //                     if (ratio <= 10) { 弹窗面板消息调度器.播放($"{Utils.GetDisplayName(obj)}检测到电量即将耗尽"); }
            //                     else if (ratio > 11) { 弹窗面板消息调度器.复用初始化(); }
            //                 }
            //                 else { 储电比例.Tmp.text = "OFF"; }
            //             }));

            //     (TextMeshProUGUI Tmp, RawImage Bkgd) 储电sum = list[1];
            //     事件容器.添加事件((th) =>
            //     {
            //         var obj = th as LogicBatchReader;
            //         if (obj.Powered && obj.Error == 0)
            //         { 储电sum.Tmp.text = 换算工具.PowerToString(Device.BatchRead(LogicBatchMethod.Sum, LogicType.Charge, obj.CurrentPrefabHash, obj.InputNetwork1DevicesSorted)); }
            //         else { 储电sum.Tmp.text = ""; }
            //     });

            // Log.Debug(()=>"构造可变绘制单元_电池");
        }
        // private 日志面板消息调度器类 构造日志UI回调(日志面板消息调度器类 日志面板消息调度器, LogicBatchReader thing)
        // {
        //     // 日志UI在Update中根据时长自动销毁
        //     日志面板消息调度器.构造UI回调 = (日志内容区域_parentRect) =>
        //     {
        //         var 标记语言解析器 = new 标记语言解析器类();
        //         var 事件容器 = 日志内容区域_parentRect.gameObject.AddComponent<object事件容器>();
        //         var 消息 = UI面板表格构造工具.构造TMP(日志内容区域_parentRect, 词条库类.消息, 世界坐标系么: false);
        //         // 这里的d只是为了通过语法检测,实际上不用传参,只使用捕获的thing的参数
        //         事件容器.添加事件((d) =>
        //         {
        //             var ratio = Math.Round(100 * Device.BatchRead(thing.BatchMethod, thing.LogicType, thing.CurrentPrefabHash, thing.InputNetwork1DevicesSorted));
        //             var charge = 换算工具.PowerToString(Device.BatchRead(LogicBatchMethod.Sum, LogicType.Charge, thing.CurrentPrefabHash, thing.InputNetwork1DevicesSorted));
        //             消息.text = $"警报 charge-> {ratio}% {charge}";
        //         });

        //         LayoutRebuilder.ForceRebuildLayoutImmediate(日志内容区域_parentRect);
        //         Utils.唤醒节点(日志内容区域_parentRect);
        //         return 事件容器.gameObject;
        //     };
        //     return 日志面板消息调度器;
        // }
    }
}