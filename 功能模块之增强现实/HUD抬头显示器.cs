using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using meanran_xuexi_mods_xiaoyouhua.ui;
using meanran_xuexi_mods_xiaoyouhua.utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Assets.Scripts;
using System.Runtime.CompilerServices;
using TMPro;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;
using Assets.Scripts.Inventory;
using Assets.Scripts.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class HUD抬头显示器 : MonoBehaviour
    {
        public static HUD抬头显示器 单例;
        public readonly 状态管理器 m_状态管理器 = new();
        private 面板管理器 m_面板管理器;
        public readonly 可复用对象池管理器 m_可复用对象池管理器 = 可复用对象池管理器.构造函数();
        private void OnDestroy()
        {
            // this是玩家实体的子级, 自动销毁
            m_面板管理器.OnDestroy();
            // m_可复用对象池管理器是this的子级, 自动销毁
            m_状态管理器.OnDestroy();
        }

        public static void 构造函数()
        {
            单例 = Utils.构造节点<HUD抬头显示器>();
            单例.初始化();
        }

        private void 初始化()
        {
            var 玩家 = 玩家_API兼容层.玩家;
            var 颈椎 = 玩家.SpineBones[玩家.SpineBones.Count - 1];
            transform.SetParent(颈椎.transform, false);
            m_可复用对象池管理器.transform.SetParent(transform, false);

            m_面板管理器 = new();
            单例.m_状态管理器.m_切换到睡眠状态事件 += () => m_面板管理器.睡眠();
            单例.m_状态管理器.m_切换到工作状态事件 += () => m_面板管理器.工作();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void 将当前播放源添加到日志面板播放任务队列(string 播放源)
        {
            // m_面板管理器.将当前播放源添加到日志面板播放任务队列(播放源);
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public void 将当前播放源添加到日志面板播放任务队列(左下日志面板类_事件容器 捕获了播放源的函数对象)
        // {
        //     // m_面板管理器.将当前播放源添加到日志面板播放任务队列(捕获了播放源的函数对象);
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void 鼠标位移时玩家视线处的可交互物体(Thing 可交互物体)
        {
            if (m_面板管理器 == null || m_状态管理器 == null || !m_状态管理器.工作么()) { return; }
            显示视线处的可交互物体(new 交互消息(可交互物体));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void 鼠标单击_拖动时玩家视线处的可交互物体(Thing 可交互物体, Interactable 当前交互的内部控件)
        {
            if (m_面板管理器 == null || m_状态管理器 == null || !m_状态管理器.工作么()) { return; }
            显示视线处的可交互物体(new 交互消息(可交互物体, 当前交互的内部控件));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void 显示视线处的可交互物体(交互消息 交互消息)
        {
            m_面板管理器.显示视线处的可交互物体(交互消息);
        }
    }
    public struct 交互消息
    {
        // Unity通过GameObject容器搭建层级,所有Component组件都有本级GameObject的引用, 而GameObject内置的transform又可以访问父级和子级的GameObject
        // 交互的原理 => 通过Unity内置的射线命中API查找到Unity内置的碰撞体, 然后通过<碰撞体.gameObject.GetComponentInParent<Thing>获取到可交互物体的引用
        // 此时可交互物体与内部控件并没有关联性, 因此在Thing.Awake函数或者其它初始化操作中, 将碰撞体作为Key、内部控件作为Value添加到可交互物体的控件索引表中

        // Interactable是通用的控件数据结构, 各种控件只使用其中一部分数据, 以下示范Interactable在<笔记本电脑屏幕UI>的原理
        // 例: 将捕获了<Interactable.点击>的函数对象写入<笔记本电脑屏幕UI>中的某个Unity内置的Button的onClick
        //     给根GameObject添加Camera组件 + GraphicRaycaster组件, 调用GraphicRaycaster.Raycast生成一个方向向量射线, 然后递归子级所有rectTransform, 若射线投影在rectTransform(矩形面), 则返回该级的GameObject引用
        //     通过调用gameObject.GetComponent<Button>().onClick.Invoke()链式调用<Interactable.点击>

        public Thing 可交互物体 = null;
        public Collider 视线处碰撞体 = null;
        public Interactable 内部控件 = null;
        public Thing 槽位中的可交互物体 = null;
        public Thing 交互物体 { get => 槽位中的可交互物体 ?? 可交互物体; }

        public 交互消息(Thing 可交互物体)
        {
            if (可交互物体 == null) { return; }

            this.可交互物体 = 可交互物体;
            this.视线处碰撞体 = CursorManager.CursorHit.collider;

            if (this.可交互物体 != null && this.视线处碰撞体 != null)
            {
                this.内部控件 = this.可交互物体.GetInteractable(this.视线处碰撞体);
                this.槽位中的可交互物体 = this.内部控件?.Slot?.Get();
            }
        }

        public 交互消息(Thing 可交互物体, Interactable 当前交互的内部控件)
        {
            if (可交互物体 == null) { return; }

            this.可交互物体 = 可交互物体;
            this.视线处碰撞体 = CursorManager.CursorHit.collider;
            this.内部控件 = 当前交互的内部控件;

            if (this.内部控件 != null) { this.槽位中的可交互物体 = this.内部控件?.Slot?.Get(); }
        }
    }

    [HarmonyPatch(typeof(Assets.Scripts.CursorManager), nameof(Assets.Scripts.CursorManager.SetCursorTarget))]
    public class 射线检测碰撞体_1
    {
        [HarmonyPostfix]
        public static void 鼠标位移时玩家视线处的可交互物体(ref Assets.Scripts.CursorManager __instance)
        {
            HUD抬头显示器.单例?.鼠标位移时玩家视线处的可交互物体(__instance.FoundThing);
        }
    }

    [HarmonyPatch(typeof(Assets.Scripts.UI.InputMouse), "Idle")]
    public class 射线检测碰撞体_2
    {
        [HarmonyPostfix]
        public static void 鼠标单击_拖动时玩家视线处的可交互物体(ref Assets.Scripts.UI.InputMouse __instance)
        {
            // 一个交互事件的完整流程分为两部分, 第一次调用跳转到Idle获取主物体和控件, 第二次调用跳转到Click、Drag、DragSlot处理交互
            var 当前交互的内部控件 = WorldInteractableGetter();
            HUD抬头显示器.单例?.鼠标单击_拖动时玩家视线处的可交互物体(__instance.CursorThing, 当前交互的内部控件);
        }

        private static readonly Func<Interactable> WorldInteractableGetter = CreateGetter<Interactable>(type: typeof(Assets.Scripts.UI.InputMouse), fieldName: "WorldInteractable");
        public static Func<T> CreateGetter<T>(Type type, string fieldName)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            var fi = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (fi == null) { throw new MissingFieldException(type.FullName, fieldName); }

            var dm = new DynamicMethod(
                name: "Get_" + type.FullName + "_" + fieldName,
                returnType: typeof(T),
                parameterTypes: Type.EmptyTypes,
                m: fi.Module,
                skipVisibility: true);

            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, fi);
            il.Emit(OpCodes.Ret);

            return (Func<T>)dm.CreateDelegate(typeof(Func<T>));
        }
    }

    public class 状态管理器
    {
        enum 状态 { 禁用, 睡眠, 工作, }
        public delegate void 切换状态();

        public event 切换状态 m_切换到睡眠状态事件;
        public event 切换状态 m_切换到工作状态事件;
        private 状态 m_当前状态 = 状态.禁用;

        public void OnDestroy()
        {
            m_切换到睡眠状态事件 = null;
            m_切换到工作状态事件 = null;
        }

        private void 更新状态()
        {
            var 之前状态 = m_当前状态;

            switch (m_当前状态)
            {
                case 状态.禁用:
                    if (!禁用么())
                    {
                        if (睡眠么()) { m_当前状态 = 状态.睡眠; }
                        else { m_当前状态 = 状态.工作; }
                    }
                    break;
                case 状态.睡眠:
                    if (禁用么()) { m_当前状态 = 状态.禁用; }
                    else if (!睡眠么()) { m_当前状态 = 状态.工作; }
                    break;
                case 状态.工作:
                    if (禁用么()) { m_当前状态 = 状态.禁用; }
                    else if (睡眠么()) { m_当前状态 = 状态.睡眠; }
                    break;
            }

            if (m_当前状态 == 之前状态) { return; }

            switch (m_当前状态)
            {
                case 状态.禁用:
                    {

                    }
                    break;
                case 状态.睡眠:
                    {
                        m_切换到睡眠状态事件?.Invoke();
                    }
                    break;
                case 状态.工作:
                    {
                        m_切换到工作状态事件?.Invoke();
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool 工作么()
        {
            更新状态();
            return m_当前状态 == 状态.工作;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool 禁用么()
        {
            return !玩家_API兼容层.玩家;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool 睡眠么()
        {
            var 玩家 = 玩家_API兼容层.玩家;
            if (!玩家 || 玩家.State != EntityState.Alive) { return true; }
            var 眼镜栏物体 = 玩家.GlassesSlot.Get();
            if (眼镜栏物体 == null) { return true; }
            return !(眼镜栏物体 is Glasses);                // 佩戴了眼镜
        }
    }

    public class 可复用对象池管理器 : MonoBehaviour
    {
        private void OnDestroy() { }
        public static 可复用对象池管理器 构造函数()
        {
            var 实例 = Utils.构造节点<可复用对象池管理器>();
            实例.初始化();
            return 实例;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void 初始化() { gameObject.SetActive(false); }
    }
    public class 可复用对象 : MonoBehaviour
    {
        public enum 状态 { 未知, 睡眠, 工作 }

        private Transform m_还原到之前父级 = null;
        private 状态 m_当前状态 = 状态.未知;

        private void OnDestroy() { }

        private void Awake() { 复用初始化(); }

        private void 复用初始化()
        {
            var 当前父级 = transform.parent;
            var 管理器 = HUD抬头显示器.单例.m_可复用对象池管理器;
            if (管理器 == null || 当前父级 != 管理器.transform) { this.m_还原到之前父级 = 当前父级; }   // 如果已经移动到池中, 不要用对象池覆盖了还原到之前父级
        }

        public void 睡眠()
        {
            if (m_当前状态 == 状态.睡眠) { return; }
            var 管理器 = HUD抬头显示器.单例.m_可复用对象池管理器;
            if (管理器 == null) { return; }
            复用初始化();
            transform.SetParent(管理器.transform, false);
            m_当前状态 = 状态.睡眠;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void 工作()
        {
            工作(m_还原到之前父级);
        }

        public void 工作(Transform 父级)
        {
            if (m_当前状态 == 状态.工作) { return; }
            if (父级 != null)
            {
                transform.SetParent(父级, false);
                m_还原到之前父级 = null;
                m_当前状态 = 状态.工作;
            }
        }
    }

    public class 弹窗面板特效管理器
    {
        const float 默认开始播放时间 = -1200;
        const int 默认当前播放任务计数 = 0;

        public float m_时间片 = 60;   // 秒制
        public int m_播放任务队列最大长度 = 1;
        private float m_开始播放时间 = 默认开始播放时间;  // 绝对值>时间片的值, 确保第一个播放任务能播放
        private int m_当前播放任务计数 = 默认当前播放任务计数;
        private 弹窗面板 m_弹窗面板 = null;

        public void OnDestroy()
        {
            m_弹窗面板 = null;
        }

        public 弹窗面板特效管理器(弹窗面板 弹窗面板)
        {
            m_弹窗面板 = 弹窗面板;

            HUD抬头显示器.单例.m_状态管理器.m_切换到工作状态事件 += () =>
            {
                this.复用初始化();
                this.播放("欢迎使用<color=red>欢迎使用安然的增强现实模组</color>!");
            };
        }

        public void 复用初始化()
        {
            m_开始播放时间 = 默认开始播放时间;
            m_当前播放任务计数 = 默认当前播放任务计数;
        }

        public void 播放(object 播放源)
        {
            if (m_弹窗面板 == null) { return; }
            if (是否有未完成的播放任务()) { return; }
            var 淡出特效 = m_弹窗面板.GetComponentInChildren<文本动画_逐字显示完成后淡出>();
            if (淡出特效 == null) { 淡出特效 = Utils.构造节点<文本动画_逐字显示完成后淡出>(m_弹窗面板); }
            淡出特效.播放(播放源);
        }
        private bool 是否有未完成的播放任务()
        {
            var 最新时间 = Time.time;
            var 已播放时长 = 最新时间 - m_开始播放时间;
            // 防止高频次切换播放源
            if (已播放时长 < m_时间片 && m_时间片 > 0) { return true; }
            // 如果没有手动调用复用初始化, 则跳过队列中后续所有任务, 以达到暂停播放效果
            if (m_当前播放任务计数 >= m_播放任务队列最大长度 && m_播放任务队列最大长度 > 0) { return true; }
            m_当前播放任务计数++;
            记录播放时间(最新时间);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void 记录播放时间(float 最新时间) { m_开始播放时间 = 最新时间; }
    }

    public class 文本动画_逐字显示 : IEnumerator<string>
    {
        public const int 空帧 = -1;
        private string 播放源 = string.Empty;
        private List<int> 帧索引表 = new(32);
        private int 当前帧数 = 空帧;
        private string 当前播放 = string.Empty;
        public string Current => 当前播放;
        object IEnumerator.Current => 当前播放;
        public int Length => 播放源.Length;
        public string 源 => 播放源;
        public 文本动画_逐字显示(string str) { 复用初始化(str); }
        public void 复用初始化(string str)
        {
            播放源 = str ?? string.Empty;
            帧索引表.Clear();
            Reset();
            if (string.IsNullOrEmpty(播放源)) { return; }
            帧索引表初始化();
        }
        public void Reset()
        {
            当前帧数 = 空帧;
            当前播放 = string.Empty;
        }
        public void Dispose() { }
        public bool MoveNext()
        {
            if (string.IsNullOrEmpty(播放源)) { return false; }
            当前帧数++;
            if (当前帧数 >= 0 && 当前帧数 < 帧索引表.Count)
            {
                当前播放 = 播放源.Substring(0, 帧索引表[当前帧数]);
                return true;
            }
            return false;
        }
        private static readonly Regex 修饰标记结束符正则 = new(@"^\s*<\s*/", RegexOptions.Compiled);
        private static readonly Regex 插入标记正则 = new(@"/\s*>\s*$", RegexOptions.Compiled);
        private void 帧索引表初始化()
        {
            // 生成逐字显示的帧缓冲区
            // 富文本修饰标记: <修饰标记名称=XXX>框选文本</修饰标记名称> 
            // 富文本插入标记(例:插入图片): <插入标记名称/> 

            var 嵌套标记表 = new List<(string 完整标记, string 标记名)>(32);
            var 已处理 = new StringBuilder(播放源.Length + 32);  // 已经显示的可见字符（不含富文本标记）

            int i = 0;
            int 总字数 = 播放源.Length;
            int 空格计数 = 0;

            while (i < 总字数)
            {
                char c = 播放源[i];
                if (c == '<')
                {
                    // 在处理标记之前, 先把之前收集到的连续空格写入已处理
                    if (空格计数 > 0)
                    {
                        已处理.Append(new string(' ', 空格计数));
                        空格计数 = 0;
                    }

                    // 找到对应的 '>'
                    int end = 播放源.IndexOf('>', i);
                    if (end == -1)
                    {
                        // 没有标记结束符, 视为普通文字
                        已处理.Append(c);
                        帧索引表.Add(已处理.Length);
                        i++;
                        continue;
                    }

                    var 完整标记 = 播放源.Substring(i, end - i + 1); // 包含 < 和 > 的完整标记

                    // 判断是修饰标记起始、修饰标记结束符还是插入标记
                    if (修饰标记结束符正则.IsMatch(完整标记))
                    {
                        // 遇到修饰标记结束符, 说明前面内容的嵌套富文本标记都已经添加到帧中, 将该标记删除
                        var 标记结束名 = 解析标记名称(完整标记);
                        for (int t = 嵌套标记表.Count - 1; t >= 0; t--)
                        {
                            var 标记起始名 = 嵌套标记表[t].标记名;
                            if (string.Equals(标记起始名, 标记结束名, StringComparison.OrdinalIgnoreCase))
                            {
                                嵌套标记表.RemoveAt(t);
                                已处理.Append(完整标记);
                                break;
                            }
                        }

                        i = end + 1;
                        continue;
                    }
                    else if (插入标记正则.IsMatch(完整标记))
                    {
                        // 遇到插入标记, 将该标记视为一个单字即可
                        已处理.Append(完整标记);
                        帧索引表.Add(已处理.Length);

                        i = end + 1;
                        continue;
                    }
                    else
                    {
                        // 遇到修饰标记起始, 后续所有帧(新内容)都在前面加上标记起始, 后面加上标记结束符
                        var 标记起始名 = 解析标记名称(完整标记);
                        嵌套标记表.Add((完整标记, 标记起始名));

                        已处理.Append(完整标记);

                        i = end + 1;
                        continue;
                    }
                }
                else
                {
                    if (char.IsWhiteSpace(c)) { 空格计数++; }  // 如果是空白字符(空格/制表/换行等), 一次性收集连续空白到同一帧
                    else
                    {
                        if (空格计数 > 0)
                        {
                            已处理.Append(new string(' ', 空格计数));
                            空格计数 = 0;
                        }

                        已处理.Append(c);
                        帧索引表.Add(已处理.Length);
                    }

                    i++;
                    continue;
                }
            }

            string 解析标记名称(string 完整标记)
            {
                if (string.IsNullOrEmpty(完整标记)) { return string.Empty; }
                int len = 完整标记.Length;

                // 找到第一个 '<'
                int i = 0;
                while (i < len && 完整标记[i] != '<') { i++; }
                if (i >= len) { return string.Empty; }
                i++; // 指向 '<' 之后的字符

                // 跳过 '<' 之后的空格
                i = 跳过空格字符(i, 完整标记);
                if (i >= len) { return string.Empty; }

                // 如果是关闭标记，跳过 '/'
                if (完整标记[i] == '/')
                {
                    i++;
                    i = 跳过空格字符(i, 完整标记);
                    if (i >= len) { return string.Empty; }
                }

                int start = i;
                // 读取标记名（遇到 '>' '/' 空白 或 '=' 时结束）
                while (i < len)
                {
                    char ch2 = 完整标记[i];
                    if (ch2 == '>' || ch2 == '/' || char.IsWhiteSpace(ch2) || ch2 == '=') { break; }
                    i++;
                }

                if (i <= start) { return string.Empty; }
                return 完整标记.Substring(start, i - start);
            }

            int 跳过空格字符(int i, string 源)
            {
                // 找到不是空格的字符或者返回溢出的i
                if (string.IsNullOrEmpty(源)) { return i; }
                int len = 源.Length;
                while (i < len && char.IsWhiteSpace(源[i])) { i++; }
                return i;
            }
        }
    }

    public class 文本动画_逐字显示完成后淡出 : MonoBehaviour
    {
        private enum 状态 { 睡眠, 工作, 暂停, 淡出 }

        private 弹窗面板 m_弹窗面板;
        private string m_播放源;
        private 文本动画_逐字显示 m_逐字显示 = new(string.Empty);
        private float m_等待开始时间;
        private float m_特效开始时间;
        private float m_计时器;
        private 状态 m_当前状态 = 状态.睡眠;

        private void OnDestroy()
        {
            m_弹窗面板 = null;
            m_逐字显示.Dispose();
            m_逐字显示 = null;
        }

        private void start()
        {
            m_弹窗面板 = transform.parent.GetComponent<弹窗面板>();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (WorldManager.IsGamePaused) { return; }
            m_计时器 += Time.deltaTime;
            if (m_计时器 <= 0.1f) { return; }
            m_计时器 = 0;
            更新状态();
        }

        private void 更新状态()
        {
            var 之前状态 = m_当前状态;

            switch (m_当前状态)
            {
                case 状态.工作:
                    if (m_逐字显示.MoveNext()) { m_弹窗面板.m_当前文本动画帧 = m_逐字显示.Current; }
                    else { m_当前状态 = 状态.暂停; }
                    break;
                case 状态.暂停:
                    if (Time.time - m_等待开始时间 > 2) { m_当前状态 = 状态.淡出; }
                    break;
                case 状态.淡出:
                    if (Time.time - m_特效开始时间 > 5)
                    {
                        将当前播放源添加到日志面板播放任务队列();
                        m_当前状态 = 状态.睡眠;
                    }
                    break;
            }

            if (m_当前状态 == 之前状态) { return; }

            switch (m_当前状态)
            {
                case 状态.睡眠:
                    gameObject.SetActive(false);
                    m_播放源 = null;
                    m_逐字显示.复用初始化(string.Empty);
                    break;
                case 状态.暂停:
                    m_等待开始时间 = Time.time;
                    break;
                case 状态.淡出:
                    m_特效开始时间 = Time.time;
                    m_弹窗面板.播放淡出特效();
                    break;
            }
        }

        public void 播放(object 播放源_)
        {
            if (播放源_ == null || m_弹窗面板 == null) { return; }
            将当前播放任务从弹窗面板移动到日志面板();
            m_播放源 = $"{播放源_}";
            m_逐字显示.复用初始化(m_播放源);
            m_当前状态 = 状态.工作;
            gameObject.SetActive(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void 将当前播放任务从弹窗面板移动到日志面板()
        {
            if (m_当前状态 != 状态.睡眠)
            {
                m_当前状态 = 状态.睡眠;
                将当前播放源添加到日志面板播放任务队列();
                m_播放源 = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void 将当前播放源添加到日志面板播放任务队列()
        {
            if (string.IsNullOrEmpty(m_播放源)) { return; }
            HUD抬头显示器.单例?.将当前播放源添加到日志面板播放任务队列(m_播放源);
        }
    }

    public class 弹窗面板 : MonoBehaviour
    {
        private enum 状态 { 未知, 睡眠, 工作, 淡出, }

        private 弹窗面板特效管理器 m_弹窗面板特效管理器;
        private CanvasGroup m_淡出特效;
        private TextMeshProUGUI m_字幕播放器;
        private 状态 m_当前状态 = 状态.未知;
        public string m_当前文本动画帧;
        private float m_淡出增量;
        private float m_计时器;

        private void OnDestroy()
        {
            m_弹窗面板特效管理器.OnDestroy();
            m_弹窗面板特效管理器 = null;
            m_淡出特效 = null;
            m_字幕播放器 = null;
        }

        private void Update()
        {
            if (WorldManager.IsGamePaused) { return; }
            if (!HUD抬头显示器.单例.m_状态管理器.工作么()) { return; }
            m_计时器 += Time.deltaTime;
            if (m_计时器 <= 0.1f) { return; }
            m_计时器 = 0;
            更新状态();
        }

        public static 弹窗面板 构造函数()
        {
            var 节点 = Utils.构造节点<弹窗面板>(HUD抬头显示器.单例.transform);
            节点.初始化();
            return 节点;
        }

        private void 初始化()
        {
            m_弹窗面板特效管理器 = new 弹窗面板特效管理器(this);
        }

        private void 更新状态()
        {
            var 之前状态 = m_当前状态;

            switch (m_当前状态)
            {
                case 状态.未知:
                    if (HUD抬头显示器.单例.m_状态管理器.工作么())
                    {
                        构造UI绘制单元();
                        m_当前状态 = 状态.睡眠;
                    }
                    break;
                case 状态.睡眠:
                    if (m_当前文本动画帧 != null)
                    {
                        m_字幕播放器.text = m_当前文本动画帧;
                        m_当前状态 = 状态.工作;
                    }
                    break;
                case 状态.工作:
                    m_字幕播放器.text = m_当前文本动画帧;
                    if (m_淡出增量 > 0) { m_当前状态 = 状态.淡出; }     // 在<播放淡出特效>方法中将m_淡出增量赋值, 以进入淡出状态, 否则永久处于工作状态
                    break;
                case 状态.淡出:
                    m_字幕播放器.text = m_当前文本动画帧;
                    m_淡出特效.alpha -= m_淡出增量;
                    if (m_淡出特效.alpha <= 0 || m_淡出增量 <= 0) { m_当前状态 = 状态.睡眠; }
                    break;
            }

            if (m_当前状态 == 之前状态) { return; }

            switch (m_当前状态)
            {
                case 状态.睡眠:
                    m_当前文本动画帧 = null;
                    m_淡出增量 = 0;
                    m_字幕播放器.text = string.Empty;
                    m_淡出特效.gameObject.SetActive(false);
                    break;
                case 状态.工作:
                    m_淡出特效.alpha = 1;
                    m_淡出特效.gameObject.SetActive(true);
                    break;
            }
        }

        public void 播放淡出特效()
        {
            m_淡出增量 = 0.02f;
        }

        private void 构造UI绘制单元()
        {
            var canvas = Utils.构造节点<Canvas>(gameObject);
            canvas.renderMode = RenderMode.WorldSpace;
            m_淡出特效 = canvas.GetOrAddComponent<CanvasGroup>();
            m_淡出特效.alpha = 1f;

            var 面板尺寸 = new Vector2(0.7f, 0.7f);

            var layout = Utils.构造VL(canvas);
            var layoutRect = layout.GetOrAddComponent<RectTransform>();
            layoutRect.sizeDelta = 面板尺寸;

            m_字幕播放器 = UI面板表格构造工具.构造TMP(layoutRect, 词条库类.消息, 世界坐标系么: true);
            m_字幕播放器.GetComponent<RectTransform>().sizeDelta = new Vector2(0.15f, 0);
            m_字幕播放器.fontSize = 0.02f;

            var 眼镜变换 = 玩家_API兼容层.玩家.GlassesSlot.Get().transform;
            transform.rotation = Quaternion.LookRotation(眼镜变换.transform.forward);
            transform.Translate(眼镜变换.transform.forward * 0.15f, Space.World);
            transform.Translate(眼镜变换.transform.up * 0.07f, Space.World);
            transform.Translate(眼镜变换.transform.right * 0.34f, Space.World);
            transform.Rotate(Vector3.right, -20, Space.Self);
            transform.Rotate(Vector3.up, 25, Space.Self);
        }
    }

    public class 玩家_API兼容层
    {
        public static Human 玩家 => Human.LocalHuman;
        public static bool 玩家状态窗口是否已加载完成()
        {
            // 物品栏   HUD窗口   本地玩家
            return InventoryManager.Instance && PlayerStateWindow.Instance && Human.LocalHuman;
        }
    }
}
