using Assets.Scripts.Objects;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.ui.things;
using meanran_xuexi_mods_xiaoyouhua.utils;
using Objects.Pipes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua.ui
{
    public class ThingsUi
    {
        private readonly DefaultUI defaultUi;
        private 标记语言解析器类 标记语言解析器 = new 标记语言解析器类();
        private readonly List<I文本> 表 = new List<I文本>();
        private readonly Dictionary<Type, I文本> 文本表 = new Dictionary<Type, I文本>();
        private readonly Dictionary<Type, I简易UI> UI表 = new Dictionary<Type, I简易UI>();
        private readonly List<I派生> 派生表 = new List<I派生>();
        public ThingsUi()
        {
            // 注意事项:TMP字库和标记语言解析器没有使用
            defaultUi = new DefaultUI();
            初始化();
        }
        private void 初始化()
        {
            // 使用反射加载加载所有绘制单元实例
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == typeof(BatteryUI).Namespace && !t.IsNested);
            foreach (var type in types)
            { 表.Add(Activator.CreateInstance(type) as I文本); }
            // Log.Info(() => $"已加载以下UI实例:\n{string.Join(",", 表.Select(t => t.GetType().Name))}");

            foreach (var UI in 表)
            {
                var thingType = UI.ThingType();
                文本表[thingType] = UI;
                if (UI is I简易UI 简易UI) { UI表[thingType] = 简易UI; }
                if (UI is I派生 派生) { 派生表.Add(派生); }
            }
            // Log.Info(() => $"已加载以下派生实例:\n{string.Join(",", 派生表.Select(t => t.GetType().Name))}");
        }
        private I文本 GetUi(Thing thing, UI代号 UI代号)
        {
            var thingType = thing.GetType();
            if (UI表.TryGetValue(thingType, out var UI))
            {
                switch (UI代号)
                {
                    case UI代号.简易UI:
                        if (UI is I简易UI) { return (I文本)UI; }
                        break;
                    case UI代号.完整UI:
                        if (UI is I完整UI) { return (I文本)UI; }
                        break;
                }
            }
            foreach (var 派生 in 派生表)
            {
                if (派生.派生么(thing))
                {
                    switch (UI代号)
                    {
                        case UI代号.简易UI:

                            if (派生 is I简易UI)
                            {
                                // Log.Debug(() => $"检测到 {thingType} 派生自: {(派生 as I文本).ThingType()}");
                                return (I文本)派生;
                            }
                            break;
                        case UI代号.完整UI:

                            if (派生 is I完整UI)
                            {
                                // Log.Debug(() => $"检测到 {thingType} 派生自: {(派生 as I文本).ThingType()}");
                                return (I文本)派生;
                            }
                            break;
                    }
                }
            }
            if (文本表.TryGetValue(thingType, out var 文本)) { return 文本; }
            return null;
        }
        public GameObject 更新绘制信息(Thing thing, RectTransform parentRect, UI代号 UI代号, bool 世界坐标系么, ConcurrentDictionary<string, GameObject> 可复用绘制单元表 = null, 标记语言解析器类.标记数据结构 thingKey = null)
        {
            // 本函数是一个消息分发函数,根据传入的thing的类型和UI代号将参数分发到具体的UI
            if (thing == null || parentRect == null) { return null; }

            var ui = GetUi(thing, UI代号);

            bool 复用标志 = 可复用绘制单元表 != null;

            var name = ui != null ? ui.ToString() : defaultUi.ToString(); // 一个物体类型对应一个UI实例,这个UI实例的名字=包含命名空间的类名
            GameObject 可复用绘制单元 = null;

            if (复用标志) { 可复用绘制单元表.TryGetValue(name, out 可复用绘制单元); }

            switch (UI代号)
            {
                // 同一个UI实例的不同虚表方法,在函数内部为面板节点构造绘制单元或者使用已有绘制单元,并且更新绘制文本
                // 若物体没有任何UI,则使用默认UI构造绘制单元;若物体有专用UI,则使用专用UI构造绘制单元;若物体只有I文本,则使用默认UI构造绘制单元,但传递文本
                case UI代号.简易UI:
                    {
                        if (ui == null) { 可复用绘制单元 = defaultUi.渲染简易UI(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息: defaultUi.ThingString(thing)); break; }
                        else if (ui is I简易UI 简易UI) { 可复用绘制单元 = 简易UI.渲染简易UI(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息: null); break; }
                        else { 可复用绘制单元 = defaultUi.渲染简易UI(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息: ui.ThingString(thing)); break; }
                    }
                case UI代号.完整UI:
                    {
                        if (ui == null) { 可复用绘制单元 = defaultUi.渲染完整UI(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息: defaultUi.ThingString(thing)); break; }
                        else if (ui is I完整UI 完整UI) { 可复用绘制单元 = 完整UI.渲染完整UI(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息: null); break; }
                        else { 可复用绘制单元 = defaultUi.渲染完整UI(thing, parentRect, 世界坐标系么, 可复用绘制单元, thingKey, 自定义消息: ui.ThingString(thing)); break; }
                    }
            }

            // Log.Debug(() => $"测试布局起始点坐标,测试信息->{Utils.获取区域左下角在屏幕上的精确坐标(可复用绘制单元.GetComponent<RectTransform>())}");

            if (可复用绘制单元 == null) { return null; }

            if (复用标志)
            {
                可复用绘制单元.transform.SetParent(parentRect.transform, false);
                可复用绘制单元.name = name;   // 以这个name作为键将绘制单元保存到可复用绘制单元表              
            }

            return 可复用绘制单元;        // 将所有创建的绘制单元统一返回,至于上级调用者是否保存再说
        }
    }
    public enum UI代号 { 简易UI, 完整UI }
    interface ThingType
    {
        Type ThingType();
    }
    interface I文本 : ThingType
    {
        // 某物体类型不需要专用UI,仅提供文本让默认UI来绘制
        string ThingString(Thing thing);
    }
    interface I派生
    {
        // 某些类型是在一个基类的基础上微调的,但是其增加的功能不需要在UI上渲染,所有可以共用一个基类UI
        bool 派生么(Thing thing);
    }
    interface I简易UI : ThingType
    {
        // TODO: thingKey->从物体的显示名称中解析标记数据来作出不同的操作,目前还只是空壳,没有实际用途
        GameObject 渲染简易UI(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元, 标记语言解析器类.标记数据结构 thingKey, string 自定义消息);
    }
    interface I完整UI : ThingType
    {
        // TODO: thingKey->从物体的显示名称中解析标记数据来作出不同的操作,目前还只是空壳,没有实际用途
        GameObject 渲染完整UI(Thing thing, RectTransform parentRect, bool 世界坐标系么, GameObject 可复用绘制单元, 标记语言解析器类.标记数据结构 thingKey, string 自定义消息);
    }
}
