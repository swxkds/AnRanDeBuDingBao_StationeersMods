using Assets.Scripts.Objects;
using Assets.Scripts;
using Assets.Scripts.Objects.Items;
using System.Collections.Generic;
using Assets.Scripts.Inventory;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using System.Reflection;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public static Structure 获取视线处建筑类物体()
        {
            // 如果是建筑类物体, 则AsStructure返回自己, 否则返回null
            return CursorManager.CursorThing?.AsStructure;
        }

        private static bool 损伤检查(Structure 建筑类物体, out ToolBasic 维修配方)
        {
            var 损伤组件 = 建筑类物体.DamageState;
            if (损伤组件 != null && 损伤组件.Total > 0f)
            {
                维修配方 = 建筑类物体.RepairTools;
                if (维修配方 != null) { return true; }
            }

            维修配方 = null;
            return false;
        }

        private static 取出物品数据包[] 获取维修工具和材料(Structure 建筑类物体)
        {
            if (建筑类物体 == null) { return null; }

            if (损伤检查(建筑类物体, out var 维修配方))
            { return [取出物品数据包.创建取出数据包(维修配方.ToolEntry, 维修配方.EntryQuantity), 取出物品数据包.创建取出数据包(维修配方.ToolEntry2, 维修配方.EntryQuantity2)]; }

            return null;
        }

        private static bool 拆卸检查(Structure 建筑类物体, out ToolUse 拆卸配方)
        {
            var 当前建造阶段组件 = 建筑类物体.CurrentBuildState;
            if (当前建造阶段组件 != null)
            {
                拆卸配方 = 当前建造阶段组件.Tool;
                if (拆卸配方 != null) { return true; }
            }

            拆卸配方 = null;
            return false;
        }

        private static 取出物品数据包[] 获取拆卸工具和材料(Structure 建筑类物体)
        {
            if (建筑类物体 == null) { return null; }

            if (拆卸检查(建筑类物体, out ToolUse 拆卸配方))
            { return [取出物品数据包.创建取出数据包(拆卸配方.ToolExit, 拆卸配方.ExitQuantity), null]; }

            return null;
        }

        private static bool 建造或升级检查(Structure 建筑类物体, out ToolBasic 建造或升级配方)
        {
            var 下一建造阶段组件 = 建筑类物体.NextBuildState;
            if (下一建造阶段组件 != null)
            {
                建造或升级配方 = 下一建造阶段组件.Tool;
                if (建造或升级配方 != null) { return true; }
            }

            建造或升级配方 = null;
            return false;
        }

        private static 取出物品数据包[] 获取建造或升级工具和材料(Structure 建筑类物体)
        {
            if (建筑类物体 == null) { return null; }

            if (建造或升级检查(建筑类物体, out var 建造或升级配方))
            { return [取出物品数据包.创建取出数据包(建造或升级配方.ToolEntry, 建造或升级配方.EntryQuantity), 取出物品数据包.创建取出数据包(建造或升级配方.ToolEntry2, 建造或升级配方.EntryQuantity2)]; }

            return null;
        }

        public static void 获取建造或升级工具和材料()
        {
            var 建筑 = 获取视线处建筑类物体();
            if (建筑 == null) { return; }
            var 取出配方 = 获取建造或升级工具和材料(建筑);
            if (取出配方 == null) { return; }
            取出物品到双手槽位(取出配方).Forget();
        }

        public static void 获取维修工具和材料()
        {
            var 建筑 = 获取视线处建筑类物体();
            if (建筑 == null) { return; }
            var 取出配方 = 获取维修工具和材料(建筑);
            if (取出配方 == null) { return; }
            取出物品到双手槽位(取出配方).Forget();
        }

        public static void 获取拆卸工具和材料()
        {
            var 建筑 = 获取视线处建筑类物体();
            if (建筑 == null) { return; }
            var 取出配方 = 获取拆卸工具和材料(建筑);
            if (取出配方 == null) { return; }
            取出物品到双手槽位(取出配方).Forget();
        }

        public static readonly (Slot 源槽位, Item 源槽位当前物品) Empty = (null, null);
        public enum 取出物品方式
        {
            交换到目标槽位,
            合并到目标槽位,
            移动到目标槽位,
            已取出
        }

        public class 取出物品数据包
        {
            public Item 取出物品;
            public int 取出数量;
            private 取出物品数据包(Item Arg_取出物品, int Arg_取出数量)
            {
                复用初始化(Arg_取出物品, Arg_取出数量);
            }
            public static 取出物品数据包 创建取出数据包(Item 取出物品, int 取出数量)
            {
                if (取出物品) { return new 取出物品数据包(取出物品, 取出数量); }
                return null;
            }

            public void 复用初始化(Item Arg_取出物品, int Arg_取出数量)
            {
                取出物品 = Arg_取出物品;
                取出数量 = Mathf.Max(Arg_取出数量, 1);
            }
        }

        public static (Slot, Slot) 获取活动手槽位和空闲手槽位()
        {
            // 一般都是主手拿工具, 副手拿材料, 因此在取出物品前, 先搞清当前的主副手
            var 活动手 = InventoryManager.ActiveHandSlot;
            var 左手 = InventoryManager.LeftHandSlot;
            var 右手 = InventoryManager.RightHandSlot;
            var 空闲手 = (活动手 == 左手) ? 右手 : 左手;
            return (活动手, 空闲手);
        }

        public static async UniTaskVoid 取出物品到双手槽位(取出物品数据包[] 取出配方)
        {
            if (取出配方 == null) { return; }

            var (活动手, 空闲手) = 获取活动手槽位和空闲手槽位();
            var 活动手配方 = 取出配方[0];
            var 空闲手配方 = 取出配方[1];

            await 取出物品到目标槽位(活动手, 活动手配方);
            await 取出物品到目标槽位(空闲手, 空闲手配方);
        }

        public static async UniTask 取出物品到目标槽位(Slot 目标槽位, 取出物品数据包 取出配方)
        {
            if (目标槽位 == null || 取出配方 == null || 取出配方.取出物品 == null) { return; }

            var 目标槽位当前物品 = 目标槽位.Get<Item>();
            var 取出方式 = 取出方式检查(目标槽位当前物品, 取出配方);

            if (取出方式 == 取出物品方式.已取出) { return; }

            var 取出物品 = 取出配方.取出物品;

            槽位物品匹配条件 当前条件;

            switch (取出物品)
            {
                case Tool:
                    {
                        if (取出物品 is IWelder)
                        {
                            if (所有已创建道具匹配条件.TryGetValue((道具类型.焊枪, 无效哈希), out 当前条件)) { }
                            else { 前置模块.Log.LogDebug($"调用 {MethodBase.GetCurrentMethod().Name} 方法时, 未找到道具匹配条件"); }
                        }
                        else
                        {
                            if (所有已创建道具匹配条件.TryGetValue((道具类型.工具大类, 取出物品.PrefabHash), out 当前条件)) { }
                            else { 当前条件 = 创建工具大类道具匹配条件(取出物品); }
                        }
                        break;
                    }
                case Stackable:
                    {
                        if (所有已创建道具匹配条件.TryGetValue((道具类型.可堆垛大类, 取出物品.PrefabHash), out 当前条件)) { }
                        else { 当前条件 = 创建通用道具匹配条件(道具类型.可堆垛大类, [取出物品.PrefabHash]); }
                        break;
                    }
                default:
                    {
                        if (所有已创建道具匹配条件.TryGetValue((道具类型.其它道具大类, 取出物品.PrefabHash), out 当前条件)) { }
                        else { 当前条件 = 创建通用道具匹配条件(道具类型.其它道具大类, [取出物品.PrefabHash]); }
                        break;
                    }
            }

            // 功能模块之快捷轮盘菜单.Log.LogMessage($"{取出物品.PrefabName} {取出配方.取出数量} {取出方式}");
            await 取出物品到目标槽位(目标槽位, 目标槽位当前物品, 取出方式, (当前条件, Empty));
        }

        public static async UniTask 取出物品到目标槽位(Slot 目标槽位, Item 目标槽位当前物品, 取出物品方式 取出方式, (槽位物品匹配条件 当前条件, (Slot 源槽位, Item 源槽位当前物品) 源) 二选一)
        {
            Slot 源槽位;
            DynamicThing 源槽位当前物品;

            switch (取出方式)
            {
                case 取出物品方式.交换到目标槽位:
                    {
                        if (二选一.源 == Empty)
                        {
                            源槽位 = 查找可取出的槽位(二选一.当前条件);        // 可能没有可取出,结果=null
                            源槽位当前物品 = 源槽位 != null ? 源槽位.Get() : null;
                        }
                        else
                        {
                            源槽位 = 二选一.源.源槽位;
                            源槽位当前物品 = 二选一.源.源槽位当前物品;
                        }

                        if (源槽位当前物品)
                        {
                            if (可以交换槽位物品么(源槽位, 源槽位当前物品, 目标槽位, 目标槽位当前物品))
                            {
                                交换槽位物品(源槽位, 源槽位当前物品, 目标槽位, 目标槽位当前物品);
                            }
                            else
                            {
                                var 槽位表 = 槽位扫描_专用槽位优先().GetEnumerator();
                                移动物品到任意非双手空槽位(目标槽位当前物品, ref 槽位表);

                                await UniTask.DelayFrame(5);        // 联机同步机制: 副机发送操作数据包给主机, 主机操作后, 再发送同步数据包给副机,至少两帧

                                var 最新的目标槽位当前物品 = 目标槽位.Get();
                                if (可以移动到目标槽位么(源槽位当前物品, 目标槽位, 最新的目标槽位当前物品))
                                {
                                    移动槽位物品(源槽位当前物品, 目标槽位, 最新的目标槽位当前物品);   // 如果不存在空槽, 则当前物品原地不动, 交换失败
                                }
                            }
                        }
                        break;
                    }
                case 取出物品方式.合并到目标槽位:
                    {
                        合并目标槽位物品至满堆垛(目标槽位, 目标槽位当前物品 as Stackable, 槽位扫描_专用槽位优先().GetEnumerator());
                        break;
                    }
                case 取出物品方式.移动到目标槽位:
                    {
                        if (二选一.源 == Empty)
                        {
                            源槽位 = 查找可取出的槽位(二选一.当前条件);        // 可能没有可取出,结果=null
                            源槽位当前物品 = 源槽位 != null ? 源槽位.Get() : null;
                        }
                        else
                        {
                            源槽位 = 二选一.源.源槽位;
                            源槽位当前物品 = 二选一.源.源槽位当前物品;
                        }

                        if (源槽位当前物品)
                        {
                            if (可以移动到目标槽位么(源槽位当前物品, 目标槽位, 目标槽位当前物品))
                            {
                                移动槽位物品(源槽位当前物品, 目标槽位, 目标槽位当前物品);
                            }
                        }
                        break;
                    }
                case 取出物品方式.已取出:
                    break;
            }
        }

        public static Slot 查找可取出的槽位(槽位物品匹配条件 当前条件)
        {
            // 可能没有可取出,结果=null
            if (当前条件 == null) { return null; }

            var 匹配表 = 槽位扫描_专用槽位优先().Where(槽位 => { return 槽位物品过滤(槽位, 当前条件); });

            // 功能模块之快捷轮盘菜单.Log.LogMessage($"查找可取出的槽位=>1");
            if (匹配表 == null || 匹配表.Count() == 0) { return null; }

            Slot 结果 = null;
            switch (当前条件.类型)
            {
                case 道具类型.焊枪:
                case 道具类型.采矿钻机:
                    {
                        if (匹配表.Count() == 1) { 结果 = 匹配表.First(); }
                        else { 结果 = 获取高优先级槽位(匹配表, 当前条件.结构哈希表); }
                        break;
                    }
                case 道具类型.可堆垛大类:
                    {
                        if (匹配表.Count() == 1)
                        {
                            结果 = 匹配表.First();
                        }
                        else
                        {
                            结果 = 匹配表.First();
                            switch (结果.Get())
                            {
                                case Stackable 可堆垛:
                                    {
                                        合并目标槽位物品至满堆垛(结果, 可堆垛, 匹配表.GetEnumerator());
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case 道具类型.工具大类:
                    {
                        if (匹配表.Count() == 1)
                        {
                            结果 = 匹配表.First();
                        }
                        else
                        {
                            结果 = 匹配表.Aggregate((t1, t2) =>
                          {
                              var t1工具 = t1.Get() as Tool;
                              var t2工具 = t2.Get() as Tool;

                              if (t1工具 && t2工具)
                              {
                                  if (t1工具.getToolSpeed() >= t2工具.getToolSpeed()) { return t1; }
                                  return t2;
                              }

                              if (t1工具) { return t1; }
                              return t2;
                          });
                        }
                        break;
                    }
                default:
                    {
                        结果 = 匹配表.First();
                        break;
                    }
            }

            // 功能模块之快捷轮盘菜单.Log.LogMessage($"查找可取出的槽位=>2");
            return 结果;
        }

        public static 取出物品方式 取出方式检查(Item 目标槽位当前物品, 取出物品数据包 取出配方)
        {
            if (取出配方 == null) { return 取出物品方式.已取出; }

            var 取出物品 = 取出配方.取出物品;
            var 取出数量 = 取出配方.取出数量;

            if (取出物品 == null || 取出数量 <= 0) { return 取出物品方式.已取出; }
            if (目标槽位当前物品 == null) { return 取出物品方式.移动到目标槽位; }

            // 工具类物品有些是存在备用工具替代的, 比如电焊枪的替代为气焊枪, 其它物品的ReplacementOf为空直接跳过
            if (目标槽位当前物品.PrefabHash == 取出物品.PrefabHash || 目标槽位当前物品.ReplacementOf?.PrefabHash == 取出物品.PrefabHash || 取出物品.ReplacementOf?.PrefabHash == 目标槽位当前物品.PrefabHash)
            {
                // Stackable: 除了铸锭以外的所有可堆垛物品的基类
                if (取出物品 is not Stackable) { return 取出物品方式.已取出; }
                if (目标槽位当前物品.GetQuantity >= 取出数量) { return 取出物品方式.已取出; }
                else { return 取出物品方式.合并到目标槽位; }
            }

            return 取出物品方式.交换到目标槽位;
        }

        public static Slot 获取高优先级槽位(IEnumerable<Slot> 匹配表, int[] 所有优先级索引)
        {
            return 匹配表.Aggregate((t1, t2) =>
            {
                var t1优先级 = Array.IndexOf(所有优先级索引, t1.Get().PrefabHash);
                var t2优先级 = Array.IndexOf(所有优先级索引, t2.Get().PrefabHash);
                if (t1优先级 >= t2优先级) { return t1; }
                return t2;
            });
        }

    }
}