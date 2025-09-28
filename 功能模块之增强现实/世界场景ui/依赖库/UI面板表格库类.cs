using System.Collections.Generic;
using System.Linq;

using meanran_xuexi_mods_xiaoyouhua.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui.presenter
{
    public class UI面板表格构造工具
    {
        public static readonly Color 默认textColor = new Color(1, 1, 1, 0.5f);
        public static readonly Color 默认titleBkgdColor = new Color(0.5f, 0, 0.5f, 0.4f);
        public static readonly Color 默认nameBkgdColor = new Color(0, 0.5f, 0.5f, 0.4f);
        public static readonly Color 默认valueBkgdColor = new Color(0, 0, 0.5f, 0.4f);
        public static readonly Color 默认面板BkgdColor = new Color(0, 0, 0, 0.6f);
        public static readonly Vector4 TMP内缩_屏幕 = new Vector4(3, 3, 3, 3);  // TextMeshProUGUI实际文本像素区域相对于背景区域的内缩距离
        public static readonly Vector4 TMP内缩_世界 = new Vector4(0.006f, 0.006f, 0.006f, 0.006f);  // 同上,但是世界坐标是仿真的,比如人物也只有1.5左右高,由unity引擎转换成屏幕坐标
        public static readonly float UI间距_屏幕 = 3;   // 两个背景区域(比如TextMeshProUGUI的背景区域)之间的留空距离,任意水平布局和垂直布局设置
        public static readonly float UI间距_世界 = 0.006f;
        public static readonly RectOffset root内缩_屏幕 = new RectOffset(3, 3, 3, 3);    // 整个表格的内容区域相对于表格背景区域的内缩距离,由表格根垂直布局设置
        public static readonly RectOffset root内缩_世界 = new RectOffset(0, 0, 0, 0);   // 只能先放大再缩放整数位,否则不可以内偏移小数

        public static void Add_ContentSizeFitter(Component obj)
        {
            var fitter = obj.GetOrAddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        public static VerticalLayoutGroup 构造VL(RectTransform parentRect)
        {
            var vl = Utils.构造VL(parentRect);
            Add_ContentSizeFitter(vl);
            vl.GetComponent<RectTransform>().sizeDelta = new Vector2(parentRect.rect.width, 0);
            return vl;
        }
        public static HorizontalLayoutGroup 构造HL(RectTransform parentRect)
        {
            var hl = Utils.构造HL(parentRect);
            Add_ContentSizeFitter(hl);
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childControlHeight = true;
            var hlRect = hl.GetComponent<RectTransform>();
            hlRect.sizeDelta = Vector2.zero;
            return hl;
        }

        public static void 应用面板字体定义(TextMeshProUGUI tmp, bool 世界坐标系么 = false, int size = 19)
        {
            tmp.font = 前置_资源加载器.单例.当前TMP字体;
            tmp.fontSize = 世界坐标系么 ? 0.05f : size;
            tmp.lineSpacing = 世界坐标系么 ? -30 : 0;
        }
        public static TextMeshProUGUI 构造TMP(RectTransform parentRect, string 绘制文本, bool 世界坐标系么)
        {
            var tmp = Utils.构造TMP(parentRect);
            Add_ContentSizeFitter(tmp);
            tmp.rectTransform.sizeDelta = new Vector2(parentRect.rect.width, 0);
            应用面板字体定义(tmp, 世界坐标系么);
            tmp.margin = 世界坐标系么 ? TMP内缩_世界 : TMP内缩_屏幕;
            tmp.text = 绘制文本;
            return tmp;
        }
        public static (TextMeshProUGUI, RawImage) 构造单元格(RectTransform parentRect, string 绘制文本, float WidthRatio, bool 世界坐标系么, Color bkgdColor = default)
        {
            var hl = 构造HL(parentRect);
            var hlRect = hl.GetComponent<RectTransform>();
            hlRect.sizeDelta = new Vector2(parentRect.rect.width * WidthRatio, 0);

            RawImage bkgd = null;
            if (bkgdColor != null)
            {
                bkgd = hlRect.gameObject.AddComponent<RawImage>();
                bkgd.raycastTarget = false;     // 不处理射线事件
                bkgd.color = bkgdColor;
            }
            var tmp = 构造TMP(hlRect, 绘制文本, 世界坐标系么);
            return (tmp, bkgd);
        }
        public static 双单元格水平序列类 构造双单元格(RectTransform parentRect, string 条目名, int 并列数, bool 世界坐标系么, Color nameBkgdColor = default, Color valueBkgdColor = default)
        {
            var hl = 构造HL(parentRect);
            var hlRect = hl.GetComponent<RectTransform>();
            hlRect.sizeDelta = new Vector2(parentRect.rect.width / 并列数, 0);
            hl.spacing = 世界坐标系么 ? UI间距_世界 : UI间距_屏幕;
            hlRect.gameObject.AddComponent<RectMask2D>();

            var 宽 = 0.33f;
            var (nameTmp, nameBkgd) = 构造单元格(hlRect, 条目名, 宽, 世界坐标系么, nameBkgdColor);
            var (valueTmp, valueBkgd) = 构造单元格(hlRect, 词条库类.setting, 1 - 宽, 世界坐标系么, valueBkgdColor);
            return new 双单元格水平序列类(hlRect, nameTmp, nameBkgd, valueTmp, valueBkgd);
        }
        public static List<双单元格水平序列类> 构造双单元格水平序列(RectTransform parentRect, string[] 条目名表, bool 世界坐标系么, Color nameBkgdColor = default, Color valueBkgdColor = default)
        {
            var hl = 构造HL(parentRect);
            var hlRect = hl.GetComponent<RectTransform>();
            hlRect.sizeDelta = new Vector2(parentRect.rect.width, 0);
            hl.spacing = 世界坐标系么 ? UI间距_世界 : UI间距_屏幕;
            hlRect.gameObject.AddComponent<RectMask2D>();

            var 并列数 = 条目名表.Count();
            List<双单元格水平序列类> 双单元格水平序列 = new List<双单元格水平序列类>(并列数);
            for (var i = 0; i < 并列数; i++)
            { 双单元格水平序列.Add(构造双单元格(hlRect, 条目名表[i], 并列数, 世界坐标系么, nameBkgdColor, valueBkgdColor)); }
            return 双单元格水平序列;
        }
        public static List<(TextMeshProUGUI, RawImage)> 构造单单元格水平序列(RectTransform parentRect, string[] 条目名表, bool 世界坐标系么, Color bkgdColor = default)
        {
            var hl = 构造HL(parentRect);
            var hlRect = hl.GetComponent<RectTransform>();
            hlRect.sizeDelta = new Vector2(parentRect.rect.width, 0);
            hl.spacing = 世界坐标系么 ? UI间距_世界 : UI间距_屏幕;
            hlRect.gameObject.AddComponent<RectMask2D>();

            var 并列数 = 条目名表.Count();
            List<(TextMeshProUGUI, RawImage)> 单单元格水平序列list = new List<(TextMeshProUGUI, RawImage)>(并列数);

            for (var i = 0; i < 并列数; i++)
            {
                float WidthRatio = 1f / 并列数;
                单单元格水平序列list.Add(构造单元格(hlRect, 条目名表[i], WidthRatio, 世界坐标系么, bkgdColor));
                // Log.Debug(() => $"单单元格水平序列->{条目名表[i]} {WidthRatio} 比例运算一定要强转为浮点数,避免整数除法舍入小数");
            }
            return 单单元格水平序列list;
        }
    }
    public class 双单元格水平序列类
    {
        public RectTransform rootLayout;    // 表格单元根节点
        public TextMeshProUGUI nameTmp;     // 表格单元-左边条目名绘制组件
        public RawImage nameBkgd;           // 表格单元-左边条目名背景色
        public TextMeshProUGUI valueTmp;    // 表格单元-右边条目值绘制组件
        public RawImage valueBkgd;          // 表格单元-右边条目名背景色
        public 双单元格水平序列类(RectTransform rootLayout, TextMeshProUGUI name, RawImage nameBkgd, TextMeshProUGUI value, RawImage valueBkgd)
        {
            this.rootLayout = rootLayout;
            nameTmp = name;
            this.nameBkgd = nameBkgd;
            valueTmp = value;
            this.valueBkgd = valueBkgd;
        }
    }
}