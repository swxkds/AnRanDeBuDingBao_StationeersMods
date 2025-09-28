using Assets.Scripts;
using Assets.Scripts.Objects;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.utils
{
    public interface IEnumerable重写接口 : IEnumerable
    {
        // 请在继承此接口的类中重写IEnumerable方法
        IEnumerable重写接口 Parent { get; }
    }
    public static class 节点树批处理类
    {
        public delegate IEnumerable IEnumerable重写接口_事件容器(object node);
        public delegate void 节点树批处理类_事件容器(object node);
        public static void 销毁节点树_含本级(this IEnumerable重写接口 根节点)
        {
            if (根节点 == null)
                return;
            根节点.节点树批处理(
                (o) => o as IEnumerable重写接口,
                (o) =>
                {
                    switch (o)
                    {
                        case GameObject obj:
                            Utils.销毁节点(obj);
                            break;
                        case Component obj:
                            Utils.销毁节点(obj);
                            break;
                    }
                });
        }
        public static void 睡眠(this IEnumerable重写接口 根节点)
        {
            if (根节点 == null)
                return;
            根节点.节点树批处理(
                (o) => o as IEnumerable重写接口,
                (o) =>
                {
                    switch (o)
                    {
                        case GameObject obj:
                            Utils.休眠节点(obj);
                            break;
                        case Component obj:
                            Utils.休眠节点(obj);
                            break;
                    }
                });
        }
        public static void 工作(this IEnumerable重写接口 根节点)
        {
            if (根节点 == null)
                return;
            根节点.节点树批处理(
                (o) => o as IEnumerable重写接口,
                (o) =>
                {
                    switch (o)
                    {
                        case GameObject obj:
                            Utils.唤醒节点(obj);
                            break;
                        case Component obj:
                            Utils.唤醒节点(obj);
                            break;
                    }
                });
        }
        private static void 节点树批处理(this IEnumerable 根节点, IEnumerable重写接口_事件容器 重写枚举器了么回调, 节点树批处理类_事件容器 节点处理回调)
        {
            // 将所有已经处理的节点添加到此表中
            var 已处理表 = new HashSet<object>();
            // 将发现的节点添加到待处理表,然后继续搜索
            var 待处理表 = new HashSet<object>();
            // 根据先入后出,父级节点始终后处理,最基层的节点先处理,当节点树还是分支时,不断更新基层节点,当一条分支到头,则取出基层节点并处理
            var 节点栈 = new Stack();

            节点栈.Push(根节点);
            int 穷举计数 = 0;
            int 处理计数 = 0;

            while (节点栈.Count > 0)
            {
                if (穷举计数++ > 500_000)
                    throw new InvalidOperationException($"节点树批处理失败,错误信息->{根节点} 找到节点数: {穷举计数},陷入死循环");

                var node = 节点栈.Peek();
                if (node == null || 已处理表.Contains(node))
                {
                    // 存在节点交叉引用的情况,因此已处理表确保不会对同一节点处理多次
                    节点栈.Pop();
                    continue;
                }

                if (待处理表.Contains(node))
                {
                    // 在一条节点分支结束搜索时,从最基层开始依次处理
                    节点栈.Pop();
                    已处理表.Add(node);
                    节点处理回调(node);
                    处理计数++;
                }
                else
                {
                    // 将发现的节点添加到待处理表,然后判断该节点是否存在分支
                    // 通过深度搜索node的类型指针指向的类元数据实例中的继承链表,来判断是否继承了IEnumerable重写接口
                    // 重写枚举器了么回调(node):返回null 或 node ; null的类型指针是0,0直接返回false

                    待处理表.Add(node);
                    if (重写枚举器了么回调(node) is IEnumerable Enumerable)
                    {
                        var enumerator = Enumerable.GetEnumerator();
                        if (enumerator != null)
                        {
                            while (enumerator.MoveNext())
                            {
                                var current = enumerator.Current;
                                if (current == null || 待处理表.Contains(current))
                                    continue;
                                节点栈.Push(current);
                            }
                        }
                    }
                }
            }
            // Log.Debug(() => $"已处理计数={处理计数}, 处理方法={节点处理回调}");
        }
    }
    public class Utils
    {
        private static readonly 标记语言解析器类 标记语言解析器 = new 标记语言解析器类();
        public static string 打印节点树信息_深度搜索(GameObject root, string ident = "")
        {
            string result = ident + "---" + root + "\n";
            result += 打印节点树信息_深度搜索_内部递归(root, ident);
            return result;
        }
        private static string 打印节点树信息_深度搜索_内部递归(GameObject root, string ident = "")
        {
            string result = "";
            result += 打印节点树信息_本级(root, ident);
            for (var i = 0; i < root.transform.childCount; i++)
            {
                var child = root.transform.GetChild(i).gameObject;
                var childIdent = ident + "\t";
                result += childIdent + "---" + child + "\n";
                result += 打印节点树信息_深度搜索_内部递归(child, childIdent);
            }
            return result;
        }
        public static string 打印节点树信息_本级(GameObject root, string ident = "")
        {
            string result = "";
            foreach (var c in root.GetComponents<Component>())
            {
                result += ident + c.GetType() + "\n";
            }
            return result;
        }
        public static T 构造节点<T>() where T : Component => 构造节点<T>((Transform)null);
        public static T 构造节点<T>(GameObject parent) where T : Component => 构造节点<T>(parent ? parent.transform : null);
        public static T 构造节点<T>(Component parent) where T : Component => 构造节点<T>(parent ? parent.transform : null);
        public static T 构造节点<T>(Transform parent) where T : Component
        {   
            var 节点 = new GameObject().AddComponent<T>();
            节点.name = $"{typeof(T)}";
            if (parent != null) { 节点.transform.SetParent(parent, false); }
            return 节点;
        }
        public static GameObject 构造节点(GameObject parent)
        {
            var 节点 = new GameObject();
            if (parent != null) { 节点.transform.SetParent(parent.transform, false); }
            return 节点;
        }
        public static void 销毁节点(Component obj)
        {
            if (obj != null) { 销毁节点(obj.gameObject); }
        }
        public static void 销毁节点(GameObject obj)
        {
            if (obj != null) { UnityEngine.Object.Destroy(obj); }
        }
#pragma warning disable CS0618
        public static void 唤醒节点(Component obj) => 唤醒节点(obj ? obj.gameObject : null);
        public static void 唤醒节点(GameObject obj)
        {
            if (obj != null) { obj.SetActiveRecursively(true); }
        }
        public static void 休眠节点(Component obj) => 休眠节点(obj ? obj.gameObject : null);
        public static void 休眠节点(GameObject obj)
        {
            if (obj != null) { obj.SetActiveRecursively(false); }
        }
        public static void 销毁子级节点(Transform obj)
        {
            if (obj == null) { return; }
            foreach (Transform child in obj) { 销毁节点(child.gameObject); }
        }
        public static void 休眠子级节点(Transform obj)
        {
            if (obj == null) { return; }
            foreach (Transform child in obj) { child.gameObject.SetActive(false); }
        }
        public static string GetReferenceId(Thing thing) { return thing == null ? "" : thing.ReferenceId.ToString(); }
        public static string GetDisplayName(object obj)
        {
            if (obj == null) { return null; }
            if (obj is Thing th) { return GetDisplayName(th); }
            return obj.ToString();
        }
        public static string GetDisplayName(Thing thing) => GetDisplayName(thing.DisplayName);
        public static string GetDisplayName(String str) => 标记语言解析器.从名称中移除标记字符(str);
        public static VerticalLayoutGroup 构造VL(Component parent) => 构造VL(parent?.gameObject);   // 布局的父级可以传空
        public static VerticalLayoutGroup 构造VL(GameObject parent) => (VerticalLayoutGroup)VlHl_Init(构造节点<VerticalLayoutGroup>(parent));
        public static HorizontalLayoutGroup 构造HL(Component parent) => 构造HL(parent?.gameObject); // 布局的父级可以传空
        public static HorizontalLayoutGroup 构造HL(GameObject parent) => (HorizontalLayoutGroup)VlHl_Init(构造节点<HorizontalLayoutGroup>(parent));
        public static TextMeshProUGUI 构造TMP(Component parent) => 构造TMP(parent.gameObject);
        public static TextMeshProUGUI 构造TMP(GameObject parent) => TMPInit(构造节点<TextMeshProUGUI>(parent));
        public static HorizontalOrVerticalLayoutGroup VlHl_Init(HorizontalOrVerticalLayoutGroup layout)
        {
            // 每次布局相当于一次深度搜索,上级需要根据自身的区域和起始坐标信息配置子级的区域和子级的起始坐标信息
            layout.childAlignment = TextAnchor.UpperLeft;   // 用过排版软件的对齐功能就明白了
            layout.spacing = 0;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = false;               // 例:有两个TMP子级,A字数=2,B字数=3,而上级宽为15,以下是简易比例计算,实际还包括字间距,字体不同字宽不同等等
            layout.childControlHeight = false;              //    则15*[A字数*字宽/(A字数*字宽+B字数*字宽)]+15*[B字数*字宽/(A字数*字宽+B字数*字宽)]=15
            layout.childForceExpandWidth = false;           // 例:有两个子级,区域宽分别为2和3,而上级宽为15
            layout.childForceExpandHeight = false;          //    则15-2-3=10, 2+10*[2/(2+3)]+3+10*[3/(2+3)]=15
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
            return layout;
        }
        private static TextMeshProUGUI TMPInit(TextMeshProUGUI tmp)
        {
            tmp.alignment = TextAlignmentOptions.TopLeft;   // tmp.rectTransform.anchorMin和anchorMax设置,区域与父级区域左上对齐
            tmp.rectTransform.pivot = Vector2.up;           // TMP组件的文本变化时,区域也会相应缩放,锚定左上角不动,让布局区域向右向下扩展
            tmp.lineSpacing = 0;                            // 字符之间的行间距  
            tmp.characterSpacing = 0;                       // 字符之间的列间距  
            tmp.margin = Vector4.zero;                      // 区域内间距预留多少后是实际文本绘制区域,分别是距左,距上,距右,距下
            tmp.text = string.Empty;
            tmp.font = null;                                // unity引擎Localization插件,这个静态引用保存了当前语言所使用的字体
            tmp.fontSize = 0;                               // 字体尺寸请尽量设置成TMP字体原始大小的整数倍,确保缩放采样时最外围能对齐,而不是因为四舍五入导致采样像素错位导致锯齿,可通过打印字体信息获取原始大小
            tmp.color = UI面板表格构造工具.默认textColor;        // 单字的透明度指的是对RGB分量的修饰
            tmp.alpha = 1;                                  // 文本的透明度指的是文本所在图层与其它图层混叠时的透明系数
            tmp.richText = true;                            // 是否启用富文本语言解析器
            tmp.maskable = true;                            // 是否可被遮罩
            tmp.overflowMode = TextOverflowModes.Truncate;  // 如果文本绘制和区域冲突了,以区域为准,超出部分截断 注:区域仅仅是提供绘制的起始坐标 例:A的超始坐标是(2,3),A区域宽高是(3,3),垂直分布,则B起始坐标是(2,6)
            tmp.enableWordWrapping = true;                  // 是否可自动换行
            tmp.fontStyle = FontStyles.Normal;              // 字符的描边,unity引擎对字符描边的几种模板化的配置
            tmp.outlineWidth = 0;                           // 字符的描边,自定义描边配置
            return tmp;
        }
        public static void 捕获父级区域宽高(RectTransform childRect) => 捕获父级区域宽高(childRect, Vector2.zero, Vector2.one);
        public static void 捕获父级区域宽高(RectTransform childRect, Vector2 捕获点A, Vector2 捕获点B)
        {
            // 捕获父级两个点构成的矩形区域宽高赋给子级区域宽高,并将父级的左下角超始坐标赋给子级作起始坐标
            // 例:(0.3f,1)指的是 父级区域宽*0.3,父级区域高*1处的点
            childRect.anchorMin = 捕获点A;
            childRect.anchorMax = 捕获点B;
            childRect.sizeDelta = Vector2.zero;          // 捕获到的区域宽高额外增减
            childRect.anchoredPosition = Vector2.zero;   // 子级的起始坐标额外位移
            childRect.offsetMin = Vector2.zero;          // 子级的区域往左往下额外增减
            childRect.offsetMax = Vector2.zero;          // 子级的区域往右往上额外增减
        }
        public static Rect 获取区域左下角在屏幕坐标系的精确坐标(RectTransform rectTransform)
        {
            // 面板区域的几何中心点距离屏幕左下角的距离,屏幕左下角由驱动程序自动提供,不需要手写;Vector2.zero:对结果进行加0;
            // 注:是以面板为平面坐标系原点来计算距离,即屏幕左下角在面板的左边是负距离X,在下边是负距离Y,在右边是正距离X,在上边是正距离Y

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Vector2.zero, null, out var 精确坐标);
            精确坐标.x = Math.Abs(精确坐标.x) - rectTransform.rect.width / 2;    // 面板区域左下角的精确屏幕X坐标,也可以在Vector2.zero那里写偏移量
            精确坐标.y = Math.Abs(精确坐标.y) - rectTransform.rect.height / 2;   // 面板区域左下角的精确屏幕Y坐标,也可以在Vector2.zero那里写偏移量

            if (精确坐标.x < 0 || 精确坐标.y < 0 || 精确坐标.x > Screen.width || 精确坐标.y > Screen.height)
            {
                // 正常来说,区域都是在屏幕内部,最极限的情况也是区域的左下角贴着屏幕左下角,在修饰过后出现负数说明超出屏幕范围
                // x/y大于屏幕比如1920/1080时说明超出屏幕
                return Rect.zero;
            }

            return new Rect(精确坐标, rectTransform.rect.size);
        }
        public static void 相对偏移调整区域宽高_注意锚点位置(RectTransform rectTransform, Vector2 调整量)
        {
            // TODO:layout.padding是在分配子级起始坐标时的偏移量,不改变父级区域的宽高与起始坐标
            //      但是在布局时调整了padding,常常忘记了调整了什么尺寸,跟父级区域对不上,还要查代码
            //      因此用offset来调整是比较好用的方法,这个调整的是父级的区域和超始坐标,这样子级只需要捕获父级宽高即可
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x + 调整量.x, rectTransform.offsetMax.y + 调整量.y);
        }
    }
}
