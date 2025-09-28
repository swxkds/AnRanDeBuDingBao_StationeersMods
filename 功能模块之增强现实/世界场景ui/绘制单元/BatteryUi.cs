using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class BatteryUI : 绘制单元基类<BatteryUI.电池事件容器, BatteryUI.电池数据库类.条目单元>, I文本, I简易UI, I完整UI
    {
        public class 电池事件容器 : 自定义泛型事件容器类<电池数据库类.条目单元> { }
        public BatteryUI() : base(new 电池数据库类()) { }
        public Type ThingType() => typeof(Battery);
        public string ThingString(Thing thing)
        {
            var obj = thing as Battery;
            var 储电量 = obj.GetLogicValue(LogicType.Charge);
            var 最大储电 = obj.GetLogicValue(LogicType.Maximum);
            return $"{obj.DisplayName} {换算工具.PowerToString(储电量)} / {换算工具.PowerToString(最大储电)}";
        }
        protected override 电池事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么)
        {
            // Log.Debug(() => $"测试循环,测试信息->构造基础面板UI");
            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<电池事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            (TextMeshProUGUI Tmp, RawImage Bkgd) name = UI面板表格构造工具.构造单元格(layoutRect, 词条库类.name, 1, 世界坐标系么);
            事件容器.添加事件((d) => name.Tmp.text = d.name.Current);
            var list = UI面板表格构造工具.构造单单元格水平序列(layoutRect, new string[] { 词条库类.ratio, 词条库类.储电, 词条库类.delta }, 世界坐标系么, UI面板表格构造工具.默认valueBkgdColor);
            添加事件_比率(事件容器, list[0], list[1]);
            添加事件_增量(事件容器, list[2]);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        protected override 电池事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么)
        {
            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<电池事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            (TextMeshProUGUI Tmp, RawImage Bkgd) name = UI面板表格构造工具.构造单元格(layoutRect, 词条库类.name, 1, 世界坐标系么);
            事件容器.添加事件((d) => name.Tmp.text = d.name.Current);

            var list1 = UI面板表格构造工具.构造单单元格水平序列(layoutRect, new string[] { 词条库类.ratio, 词条库类.储电 }, 世界坐标系么, UI面板表格构造工具.默认valueBkgdColor);
            添加事件_比率(事件容器, list1[0], list1[1]);

            var 增量 = UI面板表格构造工具.构造双单元格(layoutRect, 词条库类.delta, 1, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            添加事件_增量(事件容器, (增量.valueTmp, 增量.valueBkgd));

            var 供电 = UI面板表格构造工具.构造双单元格(layoutRect, 词条库类.供电, 1, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            事件容器.添加事件((d) => 供电.valueTmp.text = 换算工具.PowerToString(d.供电量.Current));

            var 用电 = UI面板表格构造工具.构造双单元格(layoutRect, 词条库类.用电, 1, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            事件容器.添加事件((d) => 用电.valueTmp.text = 换算工具.PowerToString(d.用电量.Current));

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        private void 添加事件_比率(电池事件容器 事件容器, (TextMeshProUGUI Tmp, RawImage Bkgd) 电量百分比, (TextMeshProUGUI Tmp, RawImage Bkgd) 电池储电量) => 事件容器.添加事件((d) =>
        {
            var 储电量 = d.储电量.Current;
            var 比率 = Math.Round(储电量 / d.最大储电, 0);
            电量百分比.Bkgd.color = Color.Lerp(new Color(1, 0, 0, 0.4f), new Color(0, 0, 1, 0.4f), (float)比率);
            电量百分比.Tmp.text = $"{比率 * 100}%";
            电池储电量.Tmp.text = 换算工具.PowerToString(储电量);
        });
        private void 添加事件_增量(电池事件容器 事件容器, (TextMeshProUGUI Tmp, RawImage Bkgd) 电量增量) => 事件容器.添加事件((d) =>
        {
            var (meta, value, _) = d.储电量.Current_含Meta;
            var (oldmeta, oldvalue, _) = d.储电量.GetOldData_含Meta(10);

            // 用可空类型包装Meta是为了判断环形缓冲区某数据位是否写入过
            if (meta != null && oldmeta != null)
            {
                var deltaValue = value - oldvalue;
                var deltaTime = Mathf.Clamp(((Meta)meta).更新数据时间 - ((Meta)oldmeta).更新数据时间, 1, 1000);
                var 增量 = deltaValue / deltaTime;
                电量增量.Tmp.text = $"\u0394={换算工具.PowerToString(增量)}/s";
                电量增量.Bkgd.color = Color.Lerp(new Color(1, 0, 0, 0.4f), new Color(0, 0, 1, 0.4f), Mathf.Clamp01((float)增量 / 500));
            }
            else { 电量增量.Tmp.text = "∞"; }
        });
        public class 电池数据库类 : 数据库基类
        {
            public class 条目单元 : 条目单元基类
            {
                public readonly 条目类<double> 储电量;
                public readonly 条目类<double> 用电量;
                public readonly 条目类<double> 供电量;
                public readonly double 最大储电;
                public 条目单元(节点类 节点, string typeName, double chargeMax) : base(节点, typeName)
                {
                    var 备份周期 = 0.5f;
                    var 备份保留时长 = 120;
                    var 缓冲区尺寸 = Mathf.RoundToInt(备份保留时长 / 备份周期);
                    储电量 = 节点.Add("储电量", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    用电量 = 节点.Add("用电量", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    供电量 = 节点.Add("供电量", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    最大储电 = chargeMax;
                }
            }
            protected override 条目单元 GetOrAdd(Thing thing)
            {
                var thingId = Utils.GetReferenceId(thing);
                return 条目单元表.GetOrAdd(thingId, (_) =>
                {
                    var 节点 = 父节点.GetOrAdd(thingId, () => new 节点类());
                    return new 条目单元(节点, thing.GetType().Name, (thing as Battery).GetLogicValue(LogicType.Maximum));
                });
            }
            public override 条目单元 更新数据(Thing thing, string 自定义消息)
            {
                var obj = thing as Battery;
                if (obj == null) { return null; }
                var 最新时间 = Time.time;
                var 条目单元 = GetOrAdd(obj);
                if (自定义消息 != null) { 条目单元.自定义消息.输入数据(自定义消息, 最新时间); }
                条目单元.name.输入数据(Utils.GetDisplayName(thing), 最新时间);
                条目单元.储电量.输入数据(obj.GetLogicValue(LogicType.Charge), 最新时间);
                条目单元.用电量.输入数据(obj.GetLogicValue(LogicType.PowerActual), 最新时间);
                条目单元.供电量.输入数据(obj.GetLogicValue(LogicType.PowerPotential), 最新时间);
                return 条目单元;
            }
        }

    }
}
