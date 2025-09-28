using Assets.Scripts.Objects;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;
using meanran_xuexi_mods_xiaoyouhua.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui
{
    public class 面板管理器 : IEnumerable重写接口
    {
        public void OnDestroy()
        {

        }
        IEnumerable重写接口 IEnumerable重写接口.Parent => null;     // UI面板管理器类是根节点,没有父级
        IEnumerator IEnumerable.GetEnumerator() => components.GetEnumerator();
        private readonly List<Component> components = new List<Component>();
        private readonly 右光标焦点面板类 右边_视线处的可交互物体面板;
        private readonly 实体面板类 固定实体面板;
        internal readonly 弹窗面板 弹窗面板;
        private readonly ThingsUi thingsUi;
        private Thing 当前显示 = null;
        public 面板管理器()
        {
            thingsUi = new ThingsUi();
            // 这些主面板节点由UI状态管理器负责递归操作进行唤醒与休眠
            var 面板节点管理器 = 构造面板节点(demoMode: false);
            components.Add(面板节点管理器.root);

            右边_视线处的可交互物体面板 = 右光标焦点面板类.构造函数(面板节点管理器.右面板Rect, thingsUi);
            components.Add(右边_视线处的可交互物体面板);

            固定实体面板 = 实体面板类.构造函数(thingsUi);
            components.Add(固定实体面板);

            弹窗面板 = 弹窗面板.构造函数();
            components.Add(弹窗面板);
        }
        private class 面板节点管理器类
        {
            public Canvas root;
            public RectTransform 左面板Rect;
            public RectTransform 右面板Rect;
            public RectTransform 左上观察Rect;
            public RectTransform 左下日志Rect;
            public 面板节点管理器类(Canvas root, RectTransform 左面板Rect, RectTransform 右面板Rect, RectTransform 左上观察Rect, RectTransform 左下日志Rect)
            {
                this.root = root;
                this.左面板Rect = 左面板Rect;
                this.右面板Rect = 右面板Rect;
                this.左上观察Rect = 左上观察Rect;
                this.左下日志Rect = 左下日志Rect;
            }
        }
        private 面板节点管理器类 构造面板节点(bool demoMode)
        {
            var root = Utils.构造节点<Canvas>(HUD抬头显示器.单例.transform);
            root.renderMode = RenderMode.ScreenSpaceOverlay;
            root.pixelPerfect = true;
            root.scaleFactor = 1;

            var 面板区域 = new UI面板区域定义();
            var 中面板宽 = Screen.width - 面板区域.左面板宽 - 面板区域.右面板宽 - 面板区域.屏幕左间距 - 面板区域.屏幕右间距;
            var 面板高 = Screen.height - 面板区域.屏幕上间距 - 面板区域.屏幕下间距;

            var hl = Utils.构造HL(root);
            var hlRect = hl.GetOrAddComponent<RectTransform>();
            Utils.捕获父级区域宽高(hlRect);
            hl.padding = new RectOffset(面板区域.屏幕左间距, 0, 面板区域.屏幕上间距, 0);

            var 左面板节点 = Utils.构造VL(hl);
            var 左面板Rect = 左面板节点.GetOrAddComponent<RectTransform>();
            左面板Rect.sizeDelta = new Vector2(面板区域.左面板宽, 500);
            var (左上观察Rect, 左下日志Rect) = 构造左面板子节点(左面板Rect);
            左上观察Rect.gameObject.AddComponent<RawImage>().color = new Color(0, 1, 1, 0.02f);
            左下日志Rect.gameObject.AddComponent<RawImage>().color = new Color(1, 1, 0, 0.02f);

            // 滚动事件处理器
            var 滚动 = Utils.构造节点<ScrollRect>(左上观察Rect);
            var 事件Rect = 滚动.GetOrAddComponent<RectTransform>();
            事件Rect.sizeDelta = 左上观察Rect.rect.size;
            var 内容Rect = UI面板表格构造工具.构造VL(事件Rect).GetOrAddComponent<RectTransform>();
            内容Rect.pivot = new Vector2(0.5f, 1);
            // 鼠标位移消息处理器,比如单击后变成单击状态,单击状态下位移,生成滚动事件
            滚动.GetOrAddComponent<GraphicRaycaster>();

            // 渲染时,UI组件在根内存位图中以分配给各自的起始坐标开始绘制各自的内容
            // 根内存位图绘制的第一个像素点坐标=事件区域的左上角坐标(左上角对齐)
            // 滚动组件貌似自带遮罩功能-> 当绘制坐标X或者Y大于区域最大坐标时,终止此列或者此行绘制
            // UI绘制时,双循环遍历像素表-> for (var 行 = 起始y坐标; 行 < 内容高; 行++) { for ( var 列 = 超始x坐标; 列 < 内容宽; 列++) { 绘制[行][列]; } }
            // 事件区域触发了拖拽事件后,对超始X坐标/y坐标进行相应变动
            滚动.content = 内容Rect;    // 提供根内存位图,即像素表
            滚动.viewport = 事件Rect;   // 事件触发坐标区域,根据连续光标消息中坐标的变化量,得出是修改起始Y坐标还是X坐标
            滚动.vertical = true;       // 允许修改起始Y坐标,即可以上下滚动
            滚动.horizontal = false;    // 禁用修改起始X坐标,即禁用左右滚动

            if (demoMode)
            {
                for (var i = 0; i < 11; i++)
                {
                    UI面板表格构造工具.构造TMP(左上观察Rect, $"观察区域 {i} Watch window {i} Text {i} Text {i}\naaaa bbbb", 世界坐标系么: false);
                    UI面板表格构造工具.构造TMP(左下日志Rect, $"日志区域 {i} Log   window {i} Text {i} Text {i}\naaaa bbbb", 世界坐标系么: false);
                }
            }

            var 中面板节点 = Utils.构造VL(hl);
            var 中面板Rect = 中面板节点.GetOrAddComponent<RectTransform>();
            中面板Rect.sizeDelta = new Vector2(中面板宽, 面板高);
            // 中面板Rect.gameObject.AddComponent<RectMask2D>();

            var bkgd2 = 中面板节点.gameObject.AddComponent<RawImage>();
            bkgd2.color = demoMode ? new Color(0, 1, 0, 0.1f) : new Color(0, 0, 0, 0f);

            var 右面板节点 = Utils.构造VL(hl);
            var 右面板Rect = 右面板节点.GetOrAddComponent<RectTransform>();
            右面板Rect.sizeDelta = new Vector2(面板区域.右面板宽, 面板高);
            // 右面板Rect.gameObject.AddComponent<RectMask2D>();

            var bkgd3 = 右面板节点.gameObject.AddComponent<RawImage>();
            bkgd3.color = demoMode ? new Color(0, 0, 1, 0.1f) : UI面板表格构造工具.默认面板BkgdColor;

            if (demoMode)
            {
                for (var i = 0; i < 37; i++)
                { UI面板表格构造工具.构造TMP(右面板Rect, $"Text {i} Text {i} Text {i} Text {i} Text {i}\naaaa bbbb", 世界坐标系么: false); }
            }

            // Log.Debug(() => $"面板根节点 {canvas.gameObject}:\n{Utils.打印节点树信息_深度搜索(canvas.gameObject)}");

            return new 面板节点管理器类(root, 左面板Rect, 右面板Rect, 内容Rect, 左下日志Rect);
        }
        private (RectTransform, RectTransform) 构造左面板子节点(RectTransform parentRect)
        {
            var 左下日志节点高 = 198;
            var 左上观察节点高 = parentRect.rect.height - 左下日志节点高;

            var 左上观察节点 = Utils.构造VL(parentRect);
            var 左上观察Rect = 左上观察节点.GetOrAddComponent<RectTransform>();
            左上观察Rect.sizeDelta = new Vector2(parentRect.rect.width, 左上观察节点高);
            左上观察Rect.gameObject.AddComponent<RectMask2D>();

            var 左下日志节点 = Utils.构造VL(parentRect);
            var 左下日志Rect = 左下日志节点.GetOrAddComponent<RectTransform>();
            左下日志Rect.sizeDelta = new Vector2(parentRect.rect.width, 左下日志节点高);
            左下日志节点.childAlignment = TextAnchor.LowerLeft;
            左下日志节点.gameObject.AddComponent<RectMask2D>();

            return (左上观察Rect, 左下日志Rect);
        }

        internal void 显示视线处的可交互物体(交互消息 交互消息)
        {
            var __ = 交互消息.交互物体;
            if (当前显示 != __)
            {
                当前显示 = __;
                if (当前显示 != null) { 右边_视线处的可交互物体面板.工作(当前显示); }
                else { 右边_视线处的可交互物体面板.睡眠(); }
            }
        }
    }

    public class 换算工具
    {
        // 将数值转换成保留3位小数的字符串供UI面板使用
        public static string DoubleToString(double d) => Math.Round(d, 3).ToString();

        // 将电量转换成保留2位小数的且根据数值匹配MW/KW/W的字符串供UI面板使用
        public static string PowerToString(double d)
        {
            if (double.IsNaN(d))
                return "NaN";

            var abs = Math.Abs(d);
            if (abs > 900_000)
            {
                return $"{Math.Round(d / 1_000_000f, 2)}MW";
            }
            else if (abs > 900)
            {
                return $"{Math.Round(d / 1_000f, 2)}kW";
            }
            else
            {
                return $"{Math.Round(d, 2)}W";
            }
        }
    }

    public class UI面板区域定义
    {
        private static int 适应性调整宽(int i) => (int)Mathf.RoundToInt(i * Screen.width / 1920f);
        private static int 适应性调整高(int i) => (int)Mathf.RoundToInt(i * Screen.height / 1080f);
        public int 左面板宽 = 适应性调整宽(350);
        public int 右面板宽 = 适应性调整宽(450);
        public int 屏幕左间距 = 适应性调整宽(100);
        public int 屏幕右间距 = 适应性调整宽(100);
        public int 屏幕上间距 = 适应性调整高(100);
        public int 屏幕下间距 = 适应性调整高(500);
    }
}
