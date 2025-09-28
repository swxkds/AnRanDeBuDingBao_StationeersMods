using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Objects.Entities;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Assets.Scripts.Inventory;
using Assets.Scripts;
using static meanran_xuexi_mods_xiaoyouhua.通用工具;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public partial class 槽位API
    {
        public static void 整理背包()
        {
            合并双手槽位物品至满堆垛();
            var __ = Human.LocalHuman.BackpackSlot.Get();
            if (是普通背包么(__)) { 整理背包((Backpack)__); }
        }

        public static void 整理背包(Backpack 背包)
        {
            if (背包 == null) { return; }
            var 槽位表 = 背包.Slots;
            if (槽位表 == null) { return; }
            OnServer.SortContents(背包.ReferenceId);
        }

        public static async UniTaskVoid 收纳双手槽位物品到背包()
        {
            // 首先从背包中取出物品补充双手堆垛, 腾出背包格子, 然后将工具按照工具放回工具腰带, 最后将物品放回背包
            合并双手槽位物品至满堆垛();

            // 如果当前装备的是工具腰带, 则从所有槽位中将工具取出并放入工具腰带
            // 如果当前装备的是普通背包, 则从所有槽位中将物品取出并放入背包中找到的第一个工具腰带
            var 腰带 = Human.LocalHuman.ToolbeltSlot.Get();
            if (是工具腰带么(腰带))
            {
                await 整理工具腰带((ToolBelt)腰带);

                var 背包 = Human.LocalHuman.BackpackSlot.Get();
                if (是普通背包么(背包))
                {
                    var 槽位表 = 背包.Slots;
                    if (槽位表 != null)
                    {
                        var 匹配 = 槽位表.FirstOrDefault(槽位 =>
                        {
                            var 槽位物品 = 槽位.Get();
                            if (是采矿腰带么(槽位物品)) { return true; }
                            return false;
                        });

                        if (匹配 != null) { await 整理采矿腰带((MiningBelt)匹配.Get()); }
                    }
                }
            }
            else
            {
                if (是采矿腰带么(腰带)) { await 整理采矿腰带((MiningBelt)腰带); }

                var 背包 = Human.LocalHuman.BackpackSlot.Get();
                if (是普通背包么(背包))
                {
                    var 槽位表 = 背包.Slots;
                    if (槽位表 != null)
                    {
                        var 匹配 = 槽位表.FirstOrDefault(槽位 =>
                        {
                            var 槽位物品 = 槽位.Get();
                            if (是工具腰带么(槽位物品)) { return true; }
                            return false;
                        });

                        if (匹配 != null) { await 整理工具腰带((ToolBelt)匹配.Get()); }
                    }
                }
            }

            await 收纳双手槽到背包();
        }

        public static void 合并双手槽位物品至满堆垛()
        {
            var 玩家 = Human.LocalHuman;

            var 左手 = 玩家.LeftHandSlot;
            var 左手物品 = 左手.Get();
            switch (左手物品)
            {
                case Stackable 可堆垛:
                    合并目标槽位物品至满堆垛(左手, 可堆垛, 槽位扫描_专用槽位优先().GetEnumerator());
                    break;
            }

            var 右手 = 玩家.RightHandSlot;
            var 右手物品 = 右手.Get();
            switch (右手物品)
            {
                case Stackable 可堆垛:
                    合并目标槽位物品至满堆垛(右手, 可堆垛, 槽位扫描_专用槽位优先().GetEnumerator());
                    break;
            }
        }

        public static async UniTask 收纳双手槽到背包()
        {
            var 玩家 = Human.LocalHuman;
            var 左手物品 = 玩家.LeftHandSlot.Get();
            var 右手物品 = 玩家.RightHandSlot.Get();

            if (左手物品 == null && 右手物品 == null) { return; }

            var 槽位表 = 槽位扫描_专用槽位优先().GetEnumerator();
            if (左手物品) { 移动物品到任意非双手空槽位(左手物品, ref 槽位表); await UniTask.DelayFrame(5); }
            if (右手物品) { 移动物品到任意非双手空槽位(右手物品, ref 槽位表); await UniTask.DelayFrame(5); }
        }

        public static async UniTask 整理工具腰带(ToolBelt 工具腰带)
        {
            // 异步的作用: 为了避免瞬间完成整理, 每次交换之间停顿一下
            if (工具腰带 == null) { return; }
            var 槽位表 = 工具腰带.Slots;
            if (槽位表 == null) { return; }
            await 整理(槽位表, 快捷轮盘菜单.所有常用工具);
        }

        public static async UniTask 整理采矿腰带(MiningBelt 采矿腰带)
        {
            // 异步的作用: 为了避免瞬间完成整理, 每次交换之间停顿一下
            if (采矿腰带 == null) { return; }
            var 槽位表 = 采矿腰带.Slots;
            if (槽位表 == null) { return; }
            await 整理(槽位表, [(道具类型.采矿钻机, 无效哈希)]);
        }

        public static async UniTask 整理(List<Slot> 槽位表, List<(道具类型, int)> 所有待整理道具)
        {
            for (var i = 0; i < 槽位表.Count && i < 所有待整理道具.Count; ++i)
            {
                if (所有已创建道具匹配条件.TryGetValue(所有待整理道具[i], out var 当前条件)) { }
                else { 前置模块.Log.LogDebug($"调用 {MethodBase.GetCurrentMethod().Name} 方法时, 未找到({所有待整理道具[i].Item1})"); }

                Slot 源槽位 = 查找可取出的槽位(当前条件);

                if (源槽位 == null) { continue; }

                var 目标槽位 = 槽位表[i];
                var 目标槽位当前物品 = 目标槽位.Get();
                var 源槽位当前物品 = 源槽位.Get();

                if (可以移动到目标槽位么(源槽位当前物品, 目标槽位, 目标槽位当前物品)) { 移动槽位物品(源槽位当前物品, 目标槽位, 目标槽位当前物品); }
                else if (可以交换槽位物品么(源槽位, 源槽位当前物品, 目标槽位, 目标槽位当前物品)) { 交换槽位物品(源槽位, 源槽位当前物品, 目标槽位, 目标槽位当前物品); }

                await UniTask.DelayFrame(5);
            }
        }


        private static readonly Collider[] 附近所有碰撞体 = new Collider[1000];
        private static readonly HashSet<Stackable> 附近所有物体 = new HashSet<Stackable>(1000);
        public static async UniTaskVoid 自动拾取补充活动手至满堆垛()
        {
            var (活动手, 空闲手) = 获取活动手槽位和空闲手槽位();
            var 当前物品 = 活动手.Get() as Stackable;
            if (当前物品 == null) { return; }

            const float 自动拾取距离 = 3f;
            var 碰撞体计数 = Physics.OverlapSphereNonAlloc(InventoryManager.ParentHuman.transform.position, 自动拾取距离, 附近所有碰撞体, CursorManager.Instance.CursorHitMask, QueryTriggerInteraction.Ignore);
            if (碰撞体计数 <= 0) { return; }

            附近所有物体.Clear();
            for (var i = 0; i < 碰撞体计数; ++i)
            {
                var 当前 = 附近所有碰撞体[i];
                if (当前.TryGetComponent<Stackable>(out var 可堆垛物))
                {
                    if (附近所有物体.Contains(可堆垛物)) { continue; }
                    附近所有物体.Add(可堆垛物);
                }
            }

            foreach (var __ in 附近所有物体)
            {
                if (当前物品.Quantity >= 当前物品.MaxQuantity) { return; }
                if (当前物品.PrefabHash == __.PrefabHash)
                {
                    合并槽位物品(__, 当前物品);
                    await UniTask.DelayFrame(5);
                }
            }
        }
    }
}