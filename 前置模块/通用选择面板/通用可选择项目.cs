using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using Reagents;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 通用可选择项目
    {
        public enum 数据解包标志
        {
            未知,
            试剂参数, 逻辑参数, 插槽参数, 插槽编号, 试剂模式, 统计模式,
            基础数学运算符, 高等数学运算符, 比较运算符, 逻辑门运算符, 最大最小值运算符,
            物联网信号模式, 物联网已上线设备, 有线网已上线设备,
            内部储物, 贸易商船, 厨电配方
        }

        public enum ComparisonOperation { Greater = 1, Less }
        public enum 信号模式 { 直连, 桥接 }
        public 数据解包标志 解包标志;
        public Thing 主物体;
        public Interactable 控件;
        public Thing 链接物体;
        public TraderContact 链接贸易商船;
        public (ReagentMixture 配方, Item 成品) 链接厨电配方;
        public object 物联网已上线设备表或内部储物表或试剂引用;
        public Dictionary<string, int> 操作数;
        public 通用可选择项目() { this.操作数 = new Dictionary<string, int>(); }
        public 通用可选择项目(数据解包标志 解包标志, Thing 主物体, Interactable 控件, Thing 链接物体, TraderContact 链接贸易商船, (ReagentMixture 配方, Item 成品) 链接厨电配方, object 物联网已上线设备表或试剂引用, params (string, int)[] 操作数) : this()
        {
            this.解包标志 = 解包标志;
            this.主物体 = 主物体;
            this.控件 = 控件;
            this.链接物体 = 链接物体;
            this.链接贸易商船 = 链接贸易商船;
            this.链接厨电配方 = 链接厨电配方;
            this.物联网已上线设备表或内部储物表或试剂引用 = 物联网已上线设备表或试剂引用;
            if (操作数 != null) { foreach (var __ in 操作数) { this.操作数.Add(__.Item1, __.Item2); } }
        }
        public static void 移动赋值(ref 通用可选择项目 源, ref 通用可选择项目 目标)
        {
            目标.解包标志 = 源.解包标志;
            目标.主物体 = 源.主物体;
            目标.控件 = 源.控件;
            目标.链接物体 = 源.链接物体;
            目标.链接贸易商船 = 源.链接贸易商船;
            目标.链接厨电配方 = 源.链接厨电配方;
            目标.物联网已上线设备表或内部储物表或试剂引用 = 源.物联网已上线设备表或内部储物表或试剂引用;
            目标.操作数.Clear();
            目标.操作数.AddRange(源.操作数);
        }
        public static 通用可选择项目 拷贝构造(ref 通用可选择项目 源)
        {
            var 目标 = new 通用可选择项目(源.解包标志, 源.主物体, 源.控件, 源.链接物体, 源.链接贸易商船, 源.链接厨电配方, 源.物联网已上线设备表或内部储物表或试剂引用);
            目标.操作数.Clear();
            目标.操作数.AddRange(源.操作数);
            return 目标;
        }
    }
}