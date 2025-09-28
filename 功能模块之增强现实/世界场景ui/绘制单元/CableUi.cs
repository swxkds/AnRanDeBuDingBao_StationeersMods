using System;
using System.Collections.Generic;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using HarmonyLib;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class CableUI : 绘制单元基类<CableUI.Cable事件容器, CableUI.Cable数据库.条目单元>, I文本, I完整UI
    {
        
       
        public class Cable事件容器 : 自定义泛型事件容器类<Cable数据库.条目单元> { }
        public CableUI() : base( new Cable数据库()) { }
        public Type ThingType() => typeof(Cable);
        public string ThingString(Thing thing)
        {
            var obj = thing as Cable;
            var net = obj.CableNetwork;
            var Channels = GetChannels(net);
            string 线路寄存器 = null;
            for (var i = 0; i < Channels.Length; i++)
            { 线路寄存器 += $"Channel{i} = {Channels[i]}\n"; }
            return $"{net.DisplayName}\n{词条库类.用电}: {换算工具.PowerToString(net.CurrentLoad)}\n{词条库类.供电}: {换算工具.PowerToString(net.PotentialLoad)}\n{线路寄存器}";
        }
        internal static double[] GetChannels(CableNetwork net) => Traverse.Create(net).Field("_channels").GetValue() as double[];
        protected override Cable事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么) => 构造完整UI绘制单元(parentRect, 世界坐标系么);
        protected override Cable事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么)
        {
            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<Cable事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            (TextMeshProUGUI tmp, RawImage bkgd) name = UI面板表格构造工具.构造单元格(layoutRect, 词条库类.name, 1, 世界坐标系么, UI面板表格构造工具.默认titleBkgdColor);
            事件容器.添加事件((d) => name.tmp.text = d.name.Current);

            var 表 = new List<双单元格水平序列类>(20);

            List<双单元格水平序列类> 供用电 = UI面板表格构造工具.构造双单元格水平序列(layoutRect, new[] { 词条库类.供电, 词条库类.用电 }, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            事件容器.添加事件((d) => 供用电[0].valueTmp.text = d.供电.Current);
            事件容器.添加事件((d) => 供用电[1].valueTmp.text = d.用电.Current);
            表.AddRange(供用电);

            for (var i = 0; i < 4; i++)
            {
                // |0-1|2-3|4-5|6-7|
                var n = i * 2;
                var m = n + 1;

                var rList = UI面板表格构造工具.构造双单元格水平序列(layoutRect, new string[] { $"Channel{n}", $"Channel{m}" }, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
                事件容器.添加事件((d) => rList[0].valueTmp.text = 换算工具.DoubleToString(d.Channels[n].Current));
                事件容器.添加事件((d) => rList[1].valueTmp.text = 换算工具.DoubleToString(d.Channels[m].Current));

                表.AddRange(rList);
            }

            表.ForEach((v) =>
            {
                v.nameTmp.fontStyle = v.valueTmp.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
                v.nameTmp.color = v.valueTmp.color = UI面板表格构造工具.默认textColor;
            });

            // Log.Debug(() => $"{string.Join(",", 表.Select(t => t.nameTmp.text))}");

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        public class Cable数据库 : 数据库基类
        {
            public class 条目单元 : 条目单元基类
            {
                public readonly 条目类<string> 供电;
                public readonly 条目类<string> 用电;
                public readonly 条目类<double>[] Channels = new 条目类<double>[8];
                public 条目单元(节点类 节点, string typeName) : base(节点, typeName)
                {
                    var 备份周期 = 0.3f;
                    var 备份保留时长 = 120;
                    var 缓冲区尺寸 = Mathf.RoundToInt(备份保留时长 / 备份周期);
                    供电 = 节点.Add($"供电", new 条目类<string>(new string[缓冲区尺寸], 备份周期));
                    用电 = 节点.Add($"用电", new 条目类<string>(new string[缓冲区尺寸], 备份周期));
                    for (var i = 0; i < 8; i++)
                    { Channels[i] = 节点.Add($"Channel{i}", new 条目类<double>(new double[缓冲区尺寸], 备份周期)); }
                }
            }
            protected override 条目单元 GetOrAdd(Thing thing)
            {
                var thingId = Utils.GetReferenceId(thing);
                return 条目单元表.GetOrAdd(thingId, (_) =>
                {
                    var 节点 = 父节点.GetOrAdd(thingId, () => new 节点类());
                    return new 条目单元(节点, thing.GetType().Name);
                });
            }
            public override 条目单元 更新数据(Thing thing, string 自定义消息)
            {
                var obj = thing as Cable;
                if (obj == null) { return null; }
                var 最新时间 = Time.time;
                var 条目单元 = GetOrAdd(thing);
                条目单元.name.输入数据(Utils.GetDisplayName(obj), 最新时间);
                if (自定义消息 != null) { 条目单元.自定义消息.输入数据(自定义消息, 最新时间); }
                if (obj != null)
                {
                    var net = obj.CableNetwork;
                    条目单元.供电.输入数据(换算工具.PowerToString(net.PotentialLoad), 最新时间);
                    条目单元.用电.输入数据(换算工具.PowerToString(net.CurrentLoad), 最新时间);
                    var Channels = GetChannels(net);
                    for (var i = 0; i < Channels.Length; i++)
                    { 条目单元.Channels[i].输入数据(Channels[i], 最新时间); }
                }
                return 条目单元;
            }
        }
    }
}