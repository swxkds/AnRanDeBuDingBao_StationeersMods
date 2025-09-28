using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using meanran_xuexi_mods_xiaoyouhua.utils;
using System.Collections.Generic;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua.ui
{
    public class 实体面板类 : MonoBehaviour
    {
        private delegate void 固定实体面板类_事件容器(实体面板节点类 面板UI节点);
        public static readonly string AR_TAG = "#AR";
        private ThingsUi thingsUi;
        private readonly Dictionary<string, Thing> 发现物体表 = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, Thing> 渲染物体表 = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, Thing> 过期物体表 = new Dictionary<string, Thing>(1000);
        private readonly Queue<实体面板节点类> 可变面板节点表 = new Queue<实体面板节点类>();
        private readonly Dictionary<string, 实体面板节点类> 活跃面板节点表 = new Dictionary<string, 实体面板节点类>(1000);
        private readonly Queue<实体面板节点类> 休眠面板节点表 = new Queue<实体面板节点类>();
        private float 计时器;
        private Thing 光标焦点物体 = null;
        public static 实体面板类 构造函数(ThingsUi thingsUi)
        {
            var 节点 = Utils.构造节点<实体面板类>();
            节点.初始化(thingsUi);
            return 节点;
        }
        private void 初始化(ThingsUi thingsUi)
        {
            this.thingsUi = thingsUi;

            // 可变面板节点->随时调用,随时销毁,哪里需要哪里搬
            for (int i = 0; i < 3; i++)
            {
                var 面板节点 = 实体面板节点类.构造(null, thingsUi);
                可变面板节点表.Enqueue(面板节点);
            }
        }
        void OnDestroy()
        {
            面板节点批处理((面板节点) => Utils.销毁节点(面板节点));
            可变面板节点表.Clear();
            活跃面板节点表.Clear();
            休眠面板节点表.Clear();
        }
        void OnEnable() => 面板节点批处理((面板节点) => { if (面板节点.锚点物体) { Utils.唤醒节点(面板节点); } });
        void OnDisable() => 面板节点批处理((面板节点) => 面板节点.重置面板节点());
        private void 面板节点批处理(固定实体面板类_事件容器 节点处理回调)
        {
            foreach (var 面板节点 in 可变面板节点表) { 节点处理回调(面板节点); }
            foreach (var 面板节点 in 活跃面板节点表.Values) { 节点处理回调(面板节点); }
            foreach (var 面板节点 in 休眠面板节点表) { 节点处理回调(面板节点); }
        }
        void Update()
        {
            if (WorldManager.IsGamePaused) { return; }

            计时器 += Time.deltaTime;
            if (计时器 > 0.5f)
            {
                计时器 = 0;
                if (this.isActiveAndEnabled)
                {
                    更新绘制信息_可变面板();
                }
            }
            更新锚点物体和绘制信息_可变面板();
        }
        private void 更新锚点物体和绘制信息_可变面板()
        {
            if (InputWindow.InputState != InputPanelState.None) { return; }
            if (Input.GetKey(KeyCode.P) || Input.GetKey(KeyCode.RightControl))
            {
                foreach (var 面板节点 in 可变面板节点表)
                { 面板节点.重置面板节点(); }
                return;
            }
            if (CursorManager.Instance == null || CursorManager.CursorThing == null) { return; }
            if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                光标焦点物体 = null;
                return;
            }
            if (光标焦点物体 == null)
            { 光标焦点物体 = new 交互消息(CursorManager.CursorThing).交互物体; }
            if (光标焦点物体 == null) { return; }

            var thingId = Utils.GetReferenceId(光标焦点物体);
            实体面板节点类 环形缓冲区 = null;
            foreach (var 面板节点 in 可变面板节点表)
            {
                // 节点的Active设置为true才会被纳入unity引擎循环;锚点物体:提供面板的坐标和UI内容;referenceId:判断UI的绘制单元和物体是否一致;  
                if (面板节点 != null && 面板节点.referenceId == thingId && 面板节点.是活跃么())
                {
                    环形缓冲区 = 面板节点;
                    break;
                }
            }
            if (环形缓冲区 == null)
            {
                环形缓冲区 = 可变面板节点表.Dequeue();
                if (环形缓冲区 == null)
                {
                    环形缓冲区 = 实体面板节点类.构造(null, thingsUi);
                }
                可变面板节点表.Enqueue(环形缓冲区);
            }

            环形缓冲区.更新绘制信息_可变面板(光标焦点物体, thingId);
        }
        private void 更新绘制信息_可变面板()
        {
            foreach (var 面板节点 in 可变面板节点表)
            {
                if (面板节点 == null || !面板节点.是活跃么()) { continue; }
                面板节点.更新绘制信息();
            }
        }
       
        private void 构造或唤醒面板节点(string thingId, Thing thing)
        {
            实体面板节点类 面板节点;

            if (休眠面板节点表.Count > 0)
            {
                面板节点 = 休眠面板节点表.Dequeue();
                Utils.唤醒节点(面板节点.gameObject);
            }
            else
            {
                面板节点 = 实体面板节点类.构造(null, thingsUi);
            }

            活跃面板节点表.Add(thingId, 面板节点);
        }
        private void 休眠面板节点(string thingId)
        {
            if (!活跃面板节点表.TryGetValue(thingId, out var 面板节点)) { return; }
            活跃面板节点表.Remove(thingId);
            面板节点.重置面板节点();
            休眠面板节点表.Enqueue(面板节点);
        }
    }
}
