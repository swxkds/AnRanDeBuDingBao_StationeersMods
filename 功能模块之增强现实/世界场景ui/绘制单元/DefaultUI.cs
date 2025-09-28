using Assets.Scripts.Objects;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;
using meanran_xuexi_mods_xiaoyouhua.utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui
{
    class DefaultUI : 绘制单元基类<DefaultUI.Default事件容器, 条目单元基类>, I文本, I简易UI, I完整UI
    {
        public class Default事件容器 : 自定义泛型事件容器类<条目单元基类> { }
        public DefaultUI() : base(new Default数据库()) { }
        public Type ThingType() => null;
        public string ThingString(Thing thing) => $"{thing.DisplayName} 未实现<color=red><b>{thing.GetType().Name}</b></color>类型";
        protected override Default事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么) => 构造UI绘制单元(parentRect, 世界坐标系么);
        protected override Default事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么) => 构造UI绘制单元(parentRect, 世界坐标系么);
        private Default事件容器 构造UI绘制单元(RectTransform parentRect, bool 世界坐标系么)
        {
            // Log.Debug(()=>$"成功进入DefaultUI.构造UI绘制单元");
            var layout = UI面板表格构造工具.构造VL(parentRect);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            var 事件容器 = layout.gameObject.AddComponent<Default事件容器>();

            Utils.相对偏移调整区域宽高_注意锚点位置(layoutRect, 世界坐标系么 ? Vector2.zero : new Vector2(-8, 0));
            layout.padding = 世界坐标系么 ? UI面板表格构造工具.root内缩_世界 : UI面板表格构造工具.root内缩_屏幕;
            layout.spacing = 世界坐标系么 ? UI面板表格构造工具.UI间距_世界 : UI面板表格构造工具.UI间距_屏幕;

            (TextMeshProUGUI tmp, RawImage bkgd) name = UI面板表格构造工具.构造单元格(layoutRect, 词条库类.消息, 1, 世界坐标系么);
            事件容器.添加事件((d) => name.tmp.text = d.自定义消息.Current ?? $"{d.name.Current} {d.typeName}");

            // var UI区域调整事件触发器 = parentRect.gameObject.AddComponent<UI事件钩子类>();
            // UI区域调整事件触发器.UI区域调整回调 += () =>
            // {
            //     Log.Debug(() => $"父级Rect {parentRect.rect}");
            //     Log.Debug(() => $"本级Rect {layoutRect.rect}");
            //     Log.Debug(() => $"子级Rect {name.tmp.rectTransform.rect}");
            // };

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            Utils.唤醒节点(layoutRect);
            return 事件容器;
        }
        class Default数据库 : 数据库基类
        {
            protected override 条目单元基类 GetOrAdd(Thing thing)
            {
                var thingId = Utils.GetReferenceId(thing);
                return 条目单元表.GetOrAdd(thingId, (_) =>
                {
                    var 节点 = 父节点.GetOrAdd(thingId, () => new 节点类());
                    return new 条目单元基类(节点, thing.GetType().Name);
                });
            }
        }
    }
}
