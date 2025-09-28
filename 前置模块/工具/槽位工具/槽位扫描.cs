using Assets.Scripts.Objects;
using Assets.Scripts.Inventory;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool 槽位可交互么(Slot 槽位) => 槽位 != null && !槽位.IsLocked && 槽位.IsInteractable && 槽位.IsSwappable;


        // IsBeingDragged: 玩家被拖动时, 便携储罐被拖动时......被拖动的物体会占用双手槽位, 并且该状态为true
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool 槽位物品可交互么(DynamicThing 槽位物品) => 槽位物品 != null && !槽位物品.IsBeingDestroyed && !槽位物品.IsBeingDragged;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool 槽位物品是背包么(DynamicThing 槽位物品, out IEnumerable<Slot> 槽位表)
        {
            if (槽位物品可交互么(槽位物品))
            {
                槽位表 = 槽位物品.Slots;
                if (槽位表 != null && 槽位表.Count() > 0)
                { return true; }
            }

            槽位表 = null;
            return false;
        }

        private static IEnumerable<Slot> 槽位扫描_广度优先(IEnumerable<Slot> 根槽位表)
        {
            if (根槽位表 == null) { return Array.Empty<Slot>(); }

            var 结果 = new List<Slot>();
            var 扫描去重 = new HashSet<Slot>();
            var 结果去重 = new HashSet<Slot>();
            var queue = new Queue<Slot>();

            foreach (var __ in 根槽位表) { if (槽位可交互么(__) && 扫描去重.Add(__)) { queue.Enqueue(__); } }

            while (queue.Count > 0)
            {
                var ___ = queue.Dequeue();
                if (结果去重.Add(___)) { 结果.Add(___); }
                var 槽位物品 = ___.Get();
                if (槽位物品是背包么(槽位物品, out var 槽位表))
                { foreach (var ____ in 槽位表) { if (槽位可交互么(____) && 扫描去重.Add(____)) { queue.Enqueue(____); } } }
            }

            扫描去重.Clear();
            结果去重.Clear();
            queue.Clear();
            return 结果;
        }

        public static IEnumerable<Slot> 槽位扫描_专用槽位优先()
        {
            var 结果 = 槽位扫描_广度优先(InventoryManager.Parent.Slots);

            // 优先使用专用槽位, 然后使用通用槽位
            var 专用槽位表 = new List<Slot>();
            var 通用槽位表 = new List<Slot>();
            foreach (var __ in 结果)
            {
                if (__.Type == Slot.Class.None) { 通用槽位表.Add(__); }
                else { 专用槽位表.Add(__); }
            }

            专用槽位表.AddRange(通用槽位表);
            return 专用槽位表;
        }
    }
}