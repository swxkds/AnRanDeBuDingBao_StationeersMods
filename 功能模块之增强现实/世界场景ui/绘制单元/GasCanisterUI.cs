using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class GasCanisterUI : 绘制单元基类<GasCanisterUI.气瓶事件容器, GasCanisterUI.气瓶数据库类.条目单元>, I文本, I完整UI
    {
        
       
        public class 气瓶事件容器 : 自定义泛型事件容器类<气瓶数据库类.条目单元> { }
        public GasCanisterUI() : base( new 气瓶数据库类()) { }
        public Type ThingType() => typeof(GasCanister);
        public string ThingString(Thing thing)
        {
            var obj = thing as GasCanister;
            var 当前气压 = obj.Pressure.ToDouble();
            var 最大耐压 = obj.MaxPressure.ToDouble();
            var 气体温度 = obj.InternalAtmosphere.Temperature.ToDouble();
            return $"{obj.DisplayName}\n当前气压:{当前气压}kpa / 最大耐压{最大耐压}kpa / 气体温度{气体温度}k";
        }
        protected override 气瓶事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么) => 构造完整UI绘制单元(parentRect, 世界坐标系么);
        protected override 气瓶事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么)
        {
            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<气瓶事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            (TextMeshProUGUI Tmp, RawImage Bkgd) name = UI面板表格构造工具.构造单元格(layoutRect, 词条库类.name, 1, 世界坐标系么);
            事件容器.添加事件((d) => name.Tmp.text = d.name.Current);

            var 当前气压 = UI面板表格构造工具.构造双单元格(layoutRect, 词条库类.当前气压, 1, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            事件容器.添加事件((d) => 当前气压.valueTmp.text = $"{d.当前气压.Current}");

            var 最大耐压 = UI面板表格构造工具.构造双单元格(layoutRect, 词条库类.最大耐压, 1, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            事件容器.添加事件((d) => 最大耐压.valueTmp.text = $"{d.最大耐压}");

            var 气体温度 = UI面板表格构造工具.构造双单元格(layoutRect, 词条库类.气体温度, 1, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            事件容器.添加事件((d) => 气体温度.valueTmp.text = $"{d.气体温度.Current}");

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        public class 气瓶数据库类 : 数据库基类
        {
            public class 条目单元 : 条目单元基类
            {
                public readonly 条目类<double> 当前气压;
                public readonly 条目类<double> 气体温度;
                public readonly double 最大耐压;
                public 条目单元(节点类 节点, string typeName, double 最大耐压) : base(节点, typeName)
                {
                    var 备份周期 = 0.5f;
                    var 备份保留时长 = 120;
                    var 缓冲区尺寸 = Mathf.RoundToInt(备份保留时长 / 备份周期);
                    当前气压 = 节点.Add("当前气压", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    气体温度 = 节点.Add("气体温度", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    this.最大耐压 = 最大耐压;
                }
            }
            protected override 条目单元 GetOrAdd(Thing thing)
            {
                var thingId = Utils.GetReferenceId(thing);
                return 条目单元表.GetOrAdd(thingId, (_) =>
                {
                    var 节点 = 父节点.GetOrAdd(thingId, () => new 节点类());
                    return new 条目单元(节点, thing.GetType().Name, (thing as GasCanister).MaxPressure.ToDouble());
                });
            }
            public override 条目单元 更新数据(Thing thing, string 自定义消息)
            {
                var obj = thing as GasCanister;
                if (obj == null) { return null; }
                var 最新时间 = Time.time;
                var 条目单元 = GetOrAdd(obj);
                if (自定义消息 != null) { 条目单元.自定义消息.输入数据(自定义消息, 最新时间); }
                条目单元.name.输入数据(Utils.GetDisplayName(thing), 最新时间);
                条目单元.当前气压.输入数据(obj.Pressure.ToDouble(), 最新时间);
                条目单元.气体温度.输入数据(obj.InternalAtmosphere.Temperature.ToDouble(), 最新时间);
                return 条目单元;
            }
        }

    }
}
