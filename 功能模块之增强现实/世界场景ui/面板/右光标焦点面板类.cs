using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;
using meanran_xuexi_mods_xiaoyouhua.utils;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua.ui
{
    public class 右光标焦点面板类 : MonoBehaviour
    {
        private RectTransform 右面板Rect;
        private RectTransform 面板节点;
        private ThingsUi thingsUi;
        private float 计时器;
        private Thing 当前光标焦点物体 = null;
        private readonly List<Thing> 嵌入物体表 = new List<Thing>();
        private readonly ConcurrentDictionary<string, GameObject> 可复用绘制单元表 = new ConcurrentDictionary<string, GameObject>();
        public static 右光标焦点面板类 构造函数(RectTransform 右面板Rect, ThingsUi thingsUi)
        {
            var 节点 = Utils.构造节点<右光标焦点面板类>();
            节点.初始化(右面板Rect, thingsUi);
            return 节点;
        }
        private void 初始化(RectTransform 右面板Rect, ThingsUi thingsUi)
        {
            this.右面板Rect = 右面板Rect;
            this.thingsUi = thingsUi;

            var vl = UI面板表格构造工具.构造VL(右面板Rect);
            面板节点 = vl.GetOrAddComponent<RectTransform>();
            vl.gameObject.SetActive(true);
        }
        void Update()
        {
            if (WorldManager.IsGamePaused) { return; }
            计时器 += Time.deltaTime;
            if (计时器 < 0.5f) { return; }
            计时器 = 0;
            if (当前光标焦点物体 == null) { 睡眠(); return; }
            更新绘制信息(当前光标焦点物体);
            foreach (var th in 嵌入物体表) { 更新绘制信息(th); }
            // Log.Debug(() => $"测试布局起始点坐标,测试信息->{Utils.获取区域左下角在屏幕上的精确坐标(面板节点)}");
        }
        public void 工作(Thing thing, List<Thing> otherThings = null)
        {
            // 请在unity引擎处注册回调函数,当光标射线检测到物体时,调用本函数,将物体传入
            // 获取包含物体:物体可能将包含的所有物体引用放置在一个哈希表中,减少间接访问层级
            // 一个面板节点随着传入物体类型的变化,会构造多个绘制单元挂在节点树上,首先休眠所有绘制单元 
            // 然后从可复用绘制单元表找物体类型对应的绘制单元引用,并唤醒它,如果没有则构造一个新绘制单元
            if (thing == 当前光标焦点物体) { return; }
            面板节点.gameObject.SetActive(false);
            Utils.休眠子级节点(面板节点);
            当前光标焦点物体 = thing;
            嵌入物体表.Clear();
            if (otherThings != null) { 嵌入物体表.AddRange(otherThings); }
            var 链接 = 获取包含物体(当前光标焦点物体);
            if (链接 is IEnumerable<Thing>) { 嵌入物体表.AddRange(链接 as IEnumerable<Thing>); }
            else { 嵌入物体表.Add(链接 as Thing); }
            面板节点.gameObject.SetActive(true);
            // Log.Debug(() => $"\n唤醒回调->已唤醒\n{Utils.GetDisplayName(当前光标焦点物体)}\n{string.Join("\n", 嵌入物体表.Select(th => Utils.GetDisplayName(th)))}\n");
        }
        public void 睡眠()
        {
            // 请在unity引擎处注册回调函数,当光标射线没有检测到物体时,调用本函数,停止渲染
            当前光标焦点物体 = null;
            面板节点.gameObject.SetActive(false);
            // Log.Debug(() => $"休眠回调->已休眠");
        }

        private void 更新绘制信息(Thing thing)
        {
            if (thing == null) { return; }

            var 绘制单元 = thingsUi.更新绘制信息(thing, 面板节点, UI代号.完整UI, 世界坐标系么: false, 可复用绘制单元表);
            if (绘制单元 != null && 绘制单元.name != null)
            {
                可复用绘制单元表.TryGetValue(绘制单元.name, out GameObject oldValue);
                if (oldValue != null && oldValue != 绘制单元)
                {
                    // Log.Warn(() => $"警告=>存在同名但内容不同的绘制单元,{oldValue.name} 已被替换为 {绘制单元.name} {绘制单元}");
                    Utils.销毁节点(oldValue);
                }

                // 保存所有创建的绘制单元
                可复用绘制单元表[绘制单元.name] = 绘制单元;
            }
        }
        private object 获取包含物体(Thing th)
        {
            switch (th)
            {
                // 芯片持有的外壳引用,IC代码中s db Setting 111,这里的Setting访问的是外壳的成员变量
                case DeviceInputOutputCircuit o: return o.ProgrammableChip;
                // 外壳持有的芯片引用,r0-r15/sp/ra是芯片的的成员变量
                case ICircuitHolder o: return th.Slots[0]?.Get();
            }
            return null;
        }
    }
}
