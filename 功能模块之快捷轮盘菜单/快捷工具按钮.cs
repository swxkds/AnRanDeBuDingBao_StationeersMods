using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Objects;
using TMPro;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 快捷工具按钮 : 通用选择面板按钮
    {
        public Slot 源槽位;
        public Item 源槽位当前物品;
        public override string DisplayName => 源槽位当前物品?.DisplayName;
        public override void 按钮点击事件()
        {
            // 在构造按钮时,将这个方法赋给按钮的onClick;
            var (活动手, 空闲手) = 通用工具.获取活动手槽位和空闲手槽位();
            var 活动手配方 = 通用工具.取出物品数据包.创建取出数据包(源槽位当前物品, 1);

            var 手槽物品 = 活动手.Get<Item>();
            var 取出方式 = 通用工具.取出方式检查(手槽物品, 活动手配方);

            if (取出方式 == 通用工具.取出物品方式.已取出) { return; }

            通用工具.取出物品到目标槽位(活动手, 手槽物品, 取出方式, (null, (源槽位, 源槽位当前物品))).Forget();
            快捷轮盘菜单.关闭快捷轮盘菜单();
        }

        public void 复用初始化(Slot Arg_源槽位)
        {
            源槽位 = Arg_源槽位;
            源槽位当前物品 = Arg_源槽位.Get<Item>();
            左侧缩略图.sprite = 源槽位当前物品?.Thumbnail;
        }
        public override string 交互提示面板内容() { return 源槽位当前物品?.GetExtendedText().ToString(); }
    }


    public class 快捷工具按钮<T> where T : 快捷工具按钮
    {
        private static T m_拷贝母体 = null;
        public static T 获取拷贝母体(Canvas root)
        {
            if (m_拷贝母体 == null)
            {
                m_拷贝母体 = 构造拷贝母体(root);
            }
            return m_拷贝母体;
        }
        private static T 构造拷贝母体(Canvas root)
        {
            var New = new GameObject().AddComponent<T>();
            通用工具.变更激活状态(New.gameObject, false); // 母体保持隐藏
            New.name = $"{New.GetType().Name}";
            New.transform.SetParent(root.transform, false);

            var 按钮背景 = New.gameObject.AddComponent<RawImage>();
            按钮背景.rectTransform.sizeDelta = new Vector2(80f, 80f);
            按钮背景.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            按钮背景.rectTransform.anchoredPosition = Vector2.zero;
            按钮背景.raycastTarget = true;
            按钮背景.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);

            var 按钮 = New.gameObject.AddComponent<Button>();
            按钮.targetGraphic = 按钮背景;

            var 色板 = 按钮.colors;
            色板.normalColor = 按钮背景.color;
            色板.highlightedColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);
            色板.pressedColor = 按钮背景.color;
            按钮.colors = 色板;

            按钮.transition = Selectable.Transition.ColorTint;

            var 聚焦 = 按钮.navigation;
            聚焦.mode = Navigation.Mode.None;
            按钮.navigation = 聚焦;

            var 缩略图 = new GameObject().AddComponent<Image>();
            缩略图.transform.SetParent(New.transform, false);
            缩略图.rectTransform.sizeDelta = 按钮背景.rectTransform.rect.size * 0.75f;
            缩略图.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            缩略图.rectTransform.anchoredPosition = Vector2.zero;
            缩略图.raycastTarget = false;

            var 文本 = new GameObject().AddComponent<TextMeshProUGUI>();
            文本.transform.SetParent(缩略图.transform, false);
            文本.rectTransform.sizeDelta = 按钮背景.rectTransform.rect.size * 0.75f;
            文本.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            文本.rectTransform.anchoredPosition = Vector2.zero;
            文本.raycastTarget = false;

            文本.text = string.Empty;
            文本.color = Color.white.SetAlpha(0.8f);
            文本.alignment = TextAlignmentOptions.Center;
            文本.font = 前置_资源加载器.单例.当前TMP字体;
            文本.fontSize = 15f;
            文本.lineSpacing = 10f;
            文本.characterSpacing = 10f;

            New.GameObject = New.gameObject;       // 本级,这三个变量指向同一个层级
            New.Transform = New.transform;         // 本级,这三个变量指向同一个层级
            New.RectTransform = New.transform as RectTransform; // 本级,这三个变量指向同一个层级
            New.左侧缩略图 = 缩略图;
            New.右侧文本 = 文本;
            New.按钮 = 按钮;
            New.源槽位当前物品 = null;
            New.源槽位 = null;

            return New;
        }
    }
}