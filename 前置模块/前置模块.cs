using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using BepInEx;
using HarmonyLib;
using UnityEngine.Networking;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", "前置模块", "1.0.0")]
    public class 前置模块 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private static MethodInfo set_IsInitialized = AccessTools.PropertySetter(typeof(GameManager), nameof(GameManager.IsInitialized));
        private static MethodInfo set_IsInitialized_Patch = AccessTools.Method(typeof(游戏初始化完成), nameof(游戏初始化完成.执行));
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("前置模块加载完成!");

            补丁 = new Harmony("前置模块");
            补丁.Patch(set_IsInitialized, prefix: new(set_IsInitialized_Patch));

            添加初始化事件(通用选择面板.通用选择面板构造函数);
            添加初始化事件(static () => 通用间接绘制管理器.通用间接绘制管理器构造函数(所有通用间接绘制构造参数));
        }

        private static List<Type> 所有新增消息类型 = new();
        public static void 添加联机数据包消息类型(Type 消息类型)
        {
            // 只需在添加类型时锁定, 避免重复添加
            lock (所有新增消息类型)
            {
                if (!所有新增消息类型.Contains(消息类型))
                {
                    所有新增消息类型.Add(消息类型);
                }
            }
        }

        public delegate void 初始化事件();
        private static event 初始化事件 初始化事件包 = null;
        private static readonly object 锁 = new();
        public static void 添加初始化事件(初始化事件 静态方法)
        {
            // 只接收public static方法
            lock (锁)
            {
                if (初始化事件包 == null) { 初始化事件包 = 静态方法; return; }
                foreach (var d in 初始化事件包.GetInvocationList())
                { if (d == (Delegate)静态方法) { return; } }
                初始化事件包 += 静态方法;
            }
        }

        private static List<(List<(多图层_多物体_批量绘制.图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> 图层参数, Func<bool> 访问快捷键)> 所有通用间接绘制构造参数 = new();
        public static void 添加通用间接绘制构造参数(List<(多图层_多物体_批量绘制.图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> 图层参数, Func<bool> 访问快捷键)
        {
            lock (锁)
            {
                所有通用间接绘制构造参数.Add((图层参数, 访问快捷键));
            }
        }

        public static IReadOnlyDictionary<Type, (Func<object, Interactable, 通用可选择项目>, Action<object, Interactable, 通用可选择项目>)> 交互过程函数表 => m_交互过程函数表;
        private static Dictionary<Type, (Func<object, Interactable, 通用可选择项目>, Action<object, Interactable, 通用可选择项目>)> m_交互过程函数表 = new();
        public static void 添加交互过程函数(params (Type, (Func<object, Interactable, 通用可选择项目>, Action<object, Interactable, 通用可选择项目>))[] 函数对象表)
        {
            // 只需在添加类型时锁定, 避免重复添加
            // 字典的顺序是按照哈希算法固定排列的, 无需排序
            lock (m_交互过程函数表)
            {
                if (函数对象表 == null) { return; }
                foreach (var __ in 函数对象表)
                {
                    if (!m_交互过程函数表.ContainsKey(__.Item1))
                    { m_交互过程函数表[__.Item1] = __.Item2; }
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.IsInitialized), MethodType.Setter)]
        public class 游戏初始化完成
        {
            [HarmonyPrefix]
            public static void 执行(ref bool value)
            {
                添加所有自定义联机同步数据包序列化类型();
                // 打印所有联机同步数据包序列化类型();

                if (初始化事件包 != null)
                {
                    var sorted = 初始化事件包.GetInvocationList().OrderBy(d => d.Method.DeclaringType.Assembly.GetName().Name, StringComparer.Ordinal).ThenBy(d => d.Method.DeclaringType.FullName, StringComparer.Ordinal).ToList();
                    初始化事件包 = null;
                    foreach (var del in sorted)
                    { 初始化事件包 += (初始化事件)del; }
                    初始化事件包();
                }

                // 强制调用, 确保最新的键位配置被同步到轮询键位组件中
                KeyManager.LoadKeyboardSetting();

                补丁.Unpatch(set_IsInitialized, HarmonyPatchType.Prefix, 补丁.Id);
                Log.LogMessage($"成功卸载补丁=><游戏初始化完成>补丁方法");
            }

            private static void 添加所有自定义联机同步数据包序列化类型()
            {
                // 由于dll的加载顺序随机, 因此需要排序, 让所有电脑的结果一致
                所有新增消息类型 = 所有新增消息类型.OrderBy(t => t.Assembly.GetName().Name, StringComparer.Ordinal).ThenBy(t => t.FullName, StringComparer.Ordinal).ToList();

                // 请在进入游戏世界前注册
                var messageFactory = Traverse.Create(typeof(MessageFactory));

                var indexToMessageType = messageFactory.Field("IndexToMessageType");
                {
                    var __ = (Type[])indexToMessageType.GetValue();
                    Type[] New = [.. __, .. 所有新增消息类型,];     // 先包含原版消息类型, 防止原版消息类型的索引错位
                    indexToMessageType.SetValue(New);
                }

                var messageTypeToIndex = messageFactory.Field("MessageTypeToIndex");
                {
                    var __ = (Dictionary<Type, byte>)messageTypeToIndex.GetValue();
                    var 尾部计数 = (byte)__.Count;

                    var New = new Dictionary<Type, byte>();
                    foreach (var 新增 in 所有新增消息类型)
                    {
                        New.Add(新增, 尾部计数);
                        ++尾部计数;                 // 消息类型ID = 元素的真实下标
                    }

                    __.AddRange(New);
                    messageTypeToIndex.SetValue(__);
                }
            }

            public static void 打印所有联机同步数据包序列化类型()
            {
                var messageFactory = Traverse.Create(typeof(MessageFactory));
                var indexToMessageType = messageFactory.Field("IndexToMessageType");
                var messageTypeToIndex = messageFactory.Field("MessageTypeToIndex");

                var 消息类型表 = (Type[])indexToMessageType.GetValue();

                ConsoleWindow.Print($"打印所有联机同步数据包的序列化类型_下标转类型");
                Log.LogMessage($"打印所有联机同步数据包的序列化类型_下标转类型");
                for (var i = 0; i < 消息类型表.Length; i++)
                {
                    ConsoleWindow.Print($"[{消息类型表[i]},{i}]");
                    Log.LogMessage($"[{消息类型表[i]},{i}]");
                }

                var 消息下标表 = (Dictionary<Type, byte>)messageTypeToIndex.GetValue();

                ConsoleWindow.Print($"打印所有联机同步数据包的序列化类型_类型转下标");
                Log.LogMessage($"打印所有联机同步数据包的序列化类型_类型转下标");
                foreach (var i in 消息下标表)
                {
                    ConsoleWindow.Print($"[{i.Key},{i.Value}]");
                    Log.LogMessage($"[{i.Key},{i.Value}]");
                }
            }
        }
    }
}
