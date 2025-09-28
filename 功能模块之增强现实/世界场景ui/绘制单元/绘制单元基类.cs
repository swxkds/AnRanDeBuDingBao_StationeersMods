using Assets.Scripts.Objects;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;
using meanran_xuexi_mods_xiaoyouhua.utils;
using System.Collections.Concurrent;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua.ui
{
    public abstract class 绘制单元基类<T事件容器, T> where T事件容器 : 自定义泛型事件容器类<T> where T : 条目单元基类
    {
        protected readonly 数据库基类 数据库;
        public 绘制单元基类(数据库基类 数据库)
        { this.数据库 = 数据库; }
        private delegate T事件容器 绘制单元基类_事件容器(RectTransform parentRect, bool 世界坐标系么);
        protected abstract T事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么);
        protected abstract T事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么);
        public GameObject 渲染简易UI(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元 = null, 标记语言解析器类.标记数据结构 thingKey = null, string 自定义消息 = null)
        { return 渲染(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息, 构造简易UI绘制单元); }
        public GameObject 渲染完整UI(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元 = null, 标记语言解析器类.标记数据结构 thingKey = null, string 自定义消息 = null)
        { return 渲染(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息, 构造完整UI绘制单元); }
        private GameObject 渲染(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元, 标记语言解析器类.标记数据结构 thingKey, string 自定义消息, 绘制单元基类_事件容器 构造UI回调)
        {
            var 条目单元 = 数据库.更新数据(thing, 自定义消息);
            if (条目单元 == null) { return null; }
            T事件容器 事件容器 = null;
            if (可复用绘制单元 != null) { 可复用绘制单元.TryGetComponent(out 事件容器); }
            else { 事件容器 = parentRect.GetComponentInChildren<T事件容器>(); }
            if (事件容器 == null) { 事件容器 = 构造UI回调(parentRect, 世界坐标系么); }
            事件容器.处理事件(条目单元);
            事件容器.gameObject.SetActive(true);
            return 事件容器.gameObject;
        }
        public abstract class 数据库基类
        {
            protected readonly 数据库结构类 父节点 = new 数据库结构类();
            protected readonly ConcurrentDictionary<string, T> 条目单元表 = new ConcurrentDictionary<string, T>();
            protected virtual T GetOrAdd(Thing thing)
            {
                var thingId = Utils.GetReferenceId(thing);
                return 条目单元表.GetOrAdd(thingId, (_) =>
                {
                    var 节点 = 父节点.GetOrAdd(thingId, () => new 节点类());
                    // 如果T是自己,构造一个条目单元基类 错误信息->无法强转->将数据库基类声明为虚基类
                    // 留着让子类重写时拷贝吧
                    return (T)new 条目单元基类(节点, thing.GetType().Name);
                });
            }
            public virtual T 更新数据(Thing thing, string 自定义消息)
            {
                var 最新时间 = Time.time;
                var 条目单元 = GetOrAdd(thing);
                条目单元.name.输入数据(Utils.GetDisplayName(thing), 最新时间);
                if (自定义消息 != null) { 条目单元.自定义消息.输入数据(自定义消息, 最新时间); }
                return 条目单元;
            }
        }
    }
    public class 条目单元基类
    {
        public readonly 条目类<string> name;
        public readonly 条目类<string> 自定义消息;
        public readonly string typeName;
        public 条目单元基类(节点类 节点, string typeName)
        {
            name = 节点.Add("name", new 条目类<string>(new string[2], 1));
            自定义消息 = 节点.Add("自定义消息", new 条目类<string>(new string[2], 1));
            this.typeName = typeName;
        }
    }

    public abstract class 绘制单元基类
    {
        public class object事件容器 : 自定义泛型事件容器类<object> { }
        private delegate object事件容器 绘制单元基类_事件容器(RectTransform parentRect, bool 世界坐标系么, Thing thing);
        protected abstract object事件容器 构造简易UI绘制单元(RectTransform parentRect, bool 世界坐标系么, Thing thing);
        protected abstract object事件容器 构造完整UI绘制单元(RectTransform parentRect, bool 世界坐标系么, Thing thing);
        public GameObject 渲染简易UI(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元 = null, 标记语言解析器类.标记数据结构 thingKey = null, string 自定义消息 = null)
        { return 渲染(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息, 构造简易UI绘制单元); }
        public GameObject 渲染完整UI(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元 = null, 标记语言解析器类.标记数据结构 thingKey = null, string 自定义消息 = null)
        { return 渲染(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息, 构造完整UI绘制单元); }
        private GameObject 渲染(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元, 标记语言解析器类.标记数据结构 thingKey, string 自定义消息, 绘制单元基类_事件容器 构造UI回调)
        {
            object事件容器 事件容器 = null;
            if (可复用绘制单元 != null) { 可复用绘制单元.TryGetComponent(out 事件容器); }
            else { 事件容器 = parentRect.GetComponentInChildren<object事件容器>(); }
            if (事件容器 == null) { 事件容器 = 构造UI回调(parentRect, 世界坐标系么, thing); }
            事件容器.处理事件(thing);
            事件容器.gameObject.SetActive(true);
            return 事件容器.gameObject;
        }
    }
}
