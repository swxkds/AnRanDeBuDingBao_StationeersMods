using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.utils;
using HarmonyLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets.Scripts.Objects.Items;
using System.Text;

namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class ProgrammableChipUI : 绘制单元基类<ProgrammableChipUI.IC事件容器, ProgrammableChipUI.IC数据库类.条目单元>, I文本, I完整UI
    {
        
       
        public class IC事件容器 : 自定义泛型事件容器类<IC数据库类.条目单元> { }
        StringBuilder str = new StringBuilder();
        public ProgrammableChipUI() : base( new IC数据库类()) { }
        public Type ThingType() => typeof(ProgrammableChip);
        public string ThingString(Thing thing)
        {
            var chip = thing as ProgrammableChip;
            str.Clear();
            var registers = GetRegisters(chip);
            for (var i = 0; i < 16; i++)
            {
                var v = registers[i];
                str.Append($"\nR{i}={换算工具.DoubleToString(v)}");
            }
            var stacks = GetStacks(chip);
            for (var i = 0; i < stacks.Length; i++)
            {
                var v = stacks[i];
                if (v != 0) { str.Append($"\nStack[{i + 1}]={v}"); }
            }
            return $"{chip.DisplayName}\n<color=green><b>DB={换算工具.DoubleToString(GetSetting(chip))}\nSP={registers[16]}\nRA={registers[17]}{str}</b></color>";
        }
        protected override IC事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么) => 构造完整UI绘制单元(parentRect, 世界坐标系么);
        protected override IC事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么)
        {
            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<IC事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            (TextMeshProUGUI tmp, RawImage bkgd) name = UI面板表格构造工具.构造单元格(layoutRect, 词条库类.name, 1, 世界坐标系么, UI面板表格构造工具.默认titleBkgdColor);
            事件容器.添加事件((d) => name.tmp.text = d.name.Current);

            // var 休眠组件表 = new List<休眠组件类>(20);
            var 表 = new List<双单元格水平序列类>(20);

            {
                var hl = UI面板表格构造工具.构造HL(layoutRect);
                var hlRect = hl.GetComponent<RectTransform>();
                hlRect.sizeDelta = new Vector2(layoutRect.rect.width, 0);
                hl.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;
                hlRect.gameObject.AddComponent<RectMask2D>();

                (TextMeshProUGUI Tmp, RawImage Bkgd) spName = UI面板表格构造工具.构造单元格(hlRect, 词条库类.sp, 0.165f, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor);
                (TextMeshProUGUI Tmp, RawImage Bkgd) spValue = UI面板表格构造工具.构造单元格(hlRect, 词条库类.setting, 0.835f, 世界坐标系么, UI面板表格构造工具.默认valueBkgdColor);
                事件容器.添加事件((d) =>
                {
                    spValue.Tmp.text = 换算工具.DoubleToString(d.sp.Current);
                    var alpha = (10 - Mathf.Clamp(d.sp.ChangeAge, 0, 10)) / 40f;
                    spValue.Bkgd.color = new Color(0.1f, 0.5f, 0.1f, alpha);
                });

                表.Add(new 双单元格水平序列类(hlRect, spName.Tmp, spName.Bkgd, spValue.Tmp, spValue.Bkgd));
            }

            // db 和 ra 寄存器 并列   注:sp指针不显示
            var dbRaList = UI面板表格构造工具.构造双单元格水平序列(layoutRect, new string[] { 词条库类.db, 词条库类.ra }, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);
            事件容器.添加事件((d) =>
            {
                dbRaList[0].valueTmp.text = 换算工具.DoubleToString(d.db.Current);
                var alpha = (10 - Mathf.Clamp(d.db.ChangeAge, 0, 10)) / 40f;
                dbRaList[0].valueBkgd.color = new Color(0.1f, 0.5f, 0.1f, alpha);
            });
            添加Register事件(事件容器, dbRaList[1], 编号: 17);

            表.AddRange(dbRaList);

            for (var i = 0; i < 8; i++)
            {
                // |0-1|2-3|4-5|6-7|8-9|10-11|12-13|14-15|
                var n = i * 2;
                var m = n + 1;

                var rList = UI面板表格构造工具.构造双单元格水平序列(layoutRect, new string[] { $"R{n}", $"R{m}" }, 世界坐标系么, UI面板表格构造工具.默认nameBkgdColor, UI面板表格构造工具.默认valueBkgdColor);

                添加Register事件(事件容器, rList[0], 编号: n);
                添加Register事件(事件容器, rList[1], 编号: m);

                // var rnm = rList[0].rootLayout.transform.parent.gameObject.AddComponent<休眠组件类>();
                // 休眠组件表.Add(rnm);

                表.AddRange(rList);
            }

            表.ForEach((v) =>
            {
                v.nameTmp.fontStyle = v.valueTmp.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
                v.nameTmp.color = v.valueTmp.color = UI面板表格构造工具.默认textColor;
            });

            // Log.Debug(() => $"{string.Join(",", 表.Select(t => t.nameTmp.text))}");

            // 事件容器.添加事件((d) =>
            // {
            //     休眠组件表.ForEach(v => v.休眠());
            //     for (var i = 0; i < 8; i++)
            //     {
            //         var n = i * 2;
            //         var m = n + 1;
            //         // 如果寄存器值是0,且距上一次更新时长超过20秒,隐藏这一行
            //         if (d.r[n].Current != 0 || d.r[m].Current != 0 || d.r[n].ChangeAge < 20 || d.r[m].ChangeAge < 20)
            //         { 休眠组件表[i].唤醒(); }
            //     }
            // });

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        private void 添加Register事件(IC事件容器 事件容器, 双单元格水平序列类 表格_条目单元, int 编号) => 事件容器.添加事件((d) =>
        {
            条目类<double> data;
            switch (编号)
            {
                case 16: data = d.sp; break;
                case 17: data = d.ra; break;
                default: data = d.r[编号]; break;
            }
            表格_条目单元.valueTmp.text = 换算工具.DoubleToString(data.Current);
            var alpha = (10 - Mathf.Clamp(data.ChangeAge, 0, 10)) / 40f;
            表格_条目单元.valueBkgd.color = new Color(0.1f, 0.5f, 0.1f, alpha);
        });
        internal static double[] GetRegisters(ProgrammableChip chip) => Traverse.Create(chip).Field("_Registers").GetValue() as double[];
        internal static double[] GetStacks(ProgrammableChip chip) => Traverse.Create(chip).Field("_Stack").GetValue() as double[];
        internal static double GetSetting(ProgrammableChip chip)
        {
            var housing = Traverse.Create(chip).Property("CircuitHousing").GetValue();
            double db = 0;
            switch (housing)
            {
                case LogicUnitBase o: db = o.Setting; break;
                case AirConditioner o: db = o.GoalTemperature.ToDouble(); break;
            }
            return db;
        }
        public class IC数据库类 : 数据库基类
        {
            public class 条目单元 : 条目单元基类
            {
                public readonly 条目类<double> db;
                public readonly 条目类<double> ra;
                public readonly 条目类<double> sp;
                public readonly 条目类<double>[] r = new 条目类<double>[16];
                public 条目单元(节点类 节点, string typeName) : base(节点, typeName)
                {
                    var 备份周期 = 0.1f;
                    var 备份保留时长 = 120;
                    var 缓冲区尺寸 = Mathf.RoundToInt(备份保留时长 / 备份周期);
                    db = 节点.Add("db", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    ra = 节点.Add("ra", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    sp = 节点.Add("sp", new 条目类<double>(new double[缓冲区尺寸], 备份周期));
                    for (var i = 0; i < 16; i++)
                    { r[i] = 节点.Add($"r{i}", new 条目类<double>(new double[缓冲区尺寸], 备份周期)); }
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
                var chip = thing as ProgrammableChip;
                if (chip == null) { return null; }
                var 最新时间 = Time.time;
                var 条目单元 = GetOrAdd(thing);
                条目单元.name.输入数据(Utils.GetDisplayName(chip), 最新时间);
                if (自定义消息 != null) { 条目单元.自定义消息.输入数据(自定义消息, 最新时间); }
                条目单元.db.输入数据(GetSetting(chip), 最新时间);
                if (chip != null)
                {
                    var registers = GetRegisters(chip);
                    条目单元.sp.输入数据(registers[16], 最新时间);
                    条目单元.ra.输入数据(registers[17], 最新时间);
                    for (var i = 0; i < 16; i++)
                    { 条目单元.r[i].输入数据(registers[i], 最新时间); }
                }
                return 条目单元;
            }
        }
    }
}
