using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.UI;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using System;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        private static Traverse FieldInfo_controlsGroupLookup = Traverse.Create(typeof(KeyManager)).Field("_controlsGroupLookup");
        public static void 创建游戏主菜单按键键位配置选项卡布局组(string Arg_按键布局组名称)
        {
            var all = FieldInfo_controlsGroupLookup.GetValue<Dictionary<string, ControlsGroup>>();
            if (all.ContainsKey(Arg_按键布局组名称)) { return; }
            new ControlsGroup(Arg_按键布局组名称);
        }

        public static void 创建游戏主菜单按键键位配置选项卡(string Arg_按键名称兼索引key, KeyCode Arg_初始默认按键, string Arg_按键布局组名称, bool Arg_hidden = false)
        {
            创建游戏主菜单按键键位配置选项卡布局组(Arg_按键布局组名称);

            var all = FieldInfo_controlsGroupLookup.GetValue<Dictionary<string, ControlsGroup>>();
            if (all.TryGetValue(Arg_按键布局组名称, out var 布局组))
            {
                // 在Settings.SetupValues函数中, 可以看到为什么要用Arg_按键名称兼索引key保存布局组
                all[Arg_按键名称兼索引key] = 布局组;

                var keyItem = new KeyItem(Arg_按键名称兼索引key, Arg_初始默认按键, Arg_hidden);
                KeyManager.KeyItemLookup[Arg_按键名称兼索引key] = keyItem;
                KeyManager.AllKeys.Add(keyItem);

                ControlsAssignment.RefreshState();
            }
        }

        public class 按键键位状态轮询组件
        {
            public static readonly Dictionary<string, 按键键位状态轮询组件> 所有按键键位状态轮询组件 = new();
            private readonly string 按键名称兼索引key;
            private KeyCode 当前监听键位;
            private readonly Action 点击事件;
            private readonly InputSystem.KeyWrap pImpl;

            private void 从游戏主菜单按键键位配置选项卡中读取_然后变更按键键位状态轮询组件的监听键位()
            {
                当前监听键位 = KeyManager.GetKey(按键名称兼索引key);
                pImpl.AssignKey(当前监听键位);
                前置模块.Log.LogMessage($"按键键位轮询组件的监听键位已更新 {pImpl.Key}");
            }

            private void 点击了监听键位()
            {
                if (GameManager.GameState == GameState.Running && InventoryManager.Parent != null && !WorldManager.IsGamePaused)
                {
                    点击事件?.Invoke();
                }
            }

            public 按键键位状态轮询组件(string Arg_按键名称兼索引key, KeyCode Arg_初始默认按键, Action Arg_点击事件)
            {
                按键名称兼索引key = Arg_按键名称兼索引key;
                当前监听键位 = Arg_初始默认按键;
                点击事件 = Arg_点击事件;
                pImpl = new InputSystem.KeyWrap(当前监听键位);
                pImpl.KeyUp += 点击了监听键位;
                所有按键键位状态轮询组件.Add(按键名称兼索引key, this);
                KeyManager.OnControlsChanged += 从游戏主菜单按键键位配置选项卡中读取_然后变更按键键位状态轮询组件的监听键位;
            }

            private static Traverse Traverse_PollingSet = Traverse.Create(typeof(KeyMap)).Field("PollingSet");

            public void Dispose()
            {
                var all = Traverse_PollingSet.GetValue<HashSet<InputSystem.KeyWrap>>();
                if (all.Contains(pImpl))
                {
                    all.Remove(pImpl);
                    所有按键键位状态轮询组件.Remove(按键名称兼索引key);
                    KeyManager.OnControlsChanged -= 从游戏主菜单按键键位配置选项卡中读取_然后变更按键键位状态轮询组件的监听键位;
                }
            }
        }
    }
}