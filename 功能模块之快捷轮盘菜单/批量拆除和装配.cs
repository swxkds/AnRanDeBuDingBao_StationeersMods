using UnityEngine;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using System.Runtime.CompilerServices;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 批量拆除和装配 : 批量种植和收获
    {
        public enum 批量选择网格状态
        {
            没有框选, 开始框选, 结束框选,
        }
        public enum 批量选择建筑类型
        {
            未知, 墙体, 框架,
        }
        private 批量选择网格状态 框选状态 = 批量选择网格状态.没有框选;
        private (Grid3 网格坐标, long 建筑Id, 批量选择建筑类型 建筑类型) 框选起点;
        private (Grid3 网格坐标, long 建筑Id, 批量选择建筑类型 建筑类型) 框选终点;

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public override void Update()
        {
            var 之前状态 = 当前状态;
            switch (当前状态)
            {
                case 批量操作任务状态.睡眠:
                    {
                        当前状态 = 目标状态;
                        break;
                    }
                case 批量操作任务状态.开关高亮:
                    {
                        清理已失效();

                        if (KeyManager.GetMouseUp("Secondary")) { 当前状态 = 批量操作任务状态.睡眠; break; }
                        break;
                    }
                case 批量操作任务状态.选择:
                    {
                        if (KeyManager.GetMouseUp("Secondary")) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        // if (!所有已选择.ContainsKey(Human.LocalHuman.ReferenceId)) { 所有已选择.Add(Human.LocalHuman.ReferenceId, Human.LocalHuman); }

                        var 建筑 = 通用工具.获取视线处建筑类物体();

                        if (建筑 && KeyManager.GetMouseUp("Primary"))
                        {
                            var Id = 建筑.ReferenceId;
                            if (所有已选择.ContainsKey(Id))
                            {
                                减少缓存物体(Id, 建筑);
                            }
                            else
                            {
                                switch (建筑)
                                {
                                    case Assets.Scripts.Objects.Wall:
                                    case Objects.Structures.Frame:
                                        增加缓存物体(Id, 建筑);
                                        break;
                                }
                            }
                        }

                        break;
                    }
                case 批量操作任务状态.选择支路:
                    {
                        if (KeyManager.GetMouseUp("Secondary")) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        // if (!所有已选择.ContainsKey(Human.LocalHuman.ReferenceId)) { 所有已选择.Add(Human.LocalHuman.ReferenceId, Human.LocalHuman); }

                        var 建筑 = 通用工具.获取视线处建筑类物体();

                        if (建筑 && KeyManager.GetMouseUp("Primary"))
                        {
                            switch (建筑)
                            {
                                case Assets.Scripts.Objects.Electrical.Cable 电缆:
                                    {
                                        if (所有已选择.ContainsKey(电缆.ReferenceId))
                                        {
                                            var 支路 = 电缆.CableNetwork;
                                            var 所有电缆 = 支路.CableList;
                                            foreach (var 当前 in 所有电缆)
                                            {
                                                var Id = 当前.ReferenceId;
                                                if (所有已选择.ContainsKey(Id))
                                                {
                                                    所有已选择.Remove(Id);
                                                }
                                            }

                                            更新缓存();
                                        }
                                        else
                                        {
                                            if (所有已选择.Count == 0)
                                            {
                                                var 支路 = 电缆.CableNetwork;
                                                var 所有电缆 = 支路.CableList;
                                                foreach (var 当前 in 所有电缆)
                                                {
                                                    var Id = 当前.ReferenceId;
                                                    if (!所有已选择.ContainsKey(Id))    // 避免所有电缆中有重复项目
                                                    {
                                                        所有已选择.Add(Id, 当前);
                                                    }
                                                }

                                                更新缓存();
                                            }
                                        }
                                        break;
                                    }
                                case Assets.Scripts.Objects.Pipes.Piping 管道:
                                    {
                                        if (所有已选择.ContainsKey(管道.ReferenceId))
                                        {
                                            var 支路 = 管道.PipeNetwork;
                                            var 所有管道 = 支路.StructureList;
                                            foreach (var 当前 in 所有管道)
                                            {
                                                var Id = 当前.ReferenceId;
                                                if (所有已选择.ContainsKey(Id))
                                                {
                                                    所有已选择.Remove(Id);
                                                }
                                            }

                                            更新缓存();
                                        }
                                        else
                                        {
                                            if (所有已选择.Count == 0)
                                            {
                                                var 支路 = 管道.PipeNetwork;
                                                var 所有管道 = 支路.StructureList;
                                                foreach (var 当前 in 所有管道)
                                                {
                                                    var Id = 当前.ReferenceId;
                                                    if (!所有已选择.ContainsKey(Id) && 当前 is Thing)   // 避免所有管道中有重复项目
                                                    {
                                                        所有已选择.Add(Id, (Thing)当前);
                                                    }
                                                }

                                                更新缓存();
                                            }
                                        }
                                        break;
                                    }
                                case Assets.Scripts.Objects.Pipes.Chute 滑槽:
                                    {
                                        if (所有已选择.ContainsKey(滑槽.ReferenceId))
                                        {
                                            var 支路 = 滑槽.ChuteNetwork;
                                            var 所有滑槽 = 支路.StructureList;
                                            foreach (var 当前 in 所有滑槽)
                                            {
                                                var Id = 当前.ReferenceId;
                                                if (所有已选择.ContainsKey(Id))
                                                {
                                                    所有已选择.Remove(Id);
                                                }
                                            }

                                            更新缓存();
                                        }
                                        else
                                        {
                                            if (所有已选择.Count == 0)
                                            {
                                                var 支路 = 滑槽.ChuteNetwork;
                                                var 所有滑槽 = 支路.StructureList;
                                                foreach (var 当前 in 所有滑槽)
                                                {
                                                    var Id = 当前.ReferenceId;
                                                    if (!所有已选择.ContainsKey(Id) && 当前 is Thing)   // 避免所有滑槽中有重复项目
                                                    {
                                                        所有已选择.Add(Id, (Thing)当前);
                                                    }
                                                }

                                                更新缓存();
                                            }
                                        }
                                        break;
                                    }
                            }
                        }

                        break;
                    }
                case 批量操作任务状态.框选:
                    {
                        if (KeyManager.GetMouseUp("Secondary")) { 框选状态 = 批量选择网格状态.没有框选; 当前状态 = 批量操作任务状态.睡眠; break; }

                        switch (框选状态)
                        {
                            case 批量选择网格状态.没有框选:
                                {
                                    var 建筑 = 通用工具.获取视线处建筑类物体();
                                    if (建筑 && KeyManager.GetMouseUp("Primary"))
                                    {
                                        switch (建筑)
                                        {
                                            case Assets.Scripts.Objects.Wall:
                                                {
                                                    if (所有已选择.Count == 0)
                                                    {
                                                        框选状态 = 批量选择网格状态.开始框选;
                                                        var Id = 建筑.ReferenceId;
                                                        框选起点 = (建筑.GridPosition, Id, 批量选择建筑类型.墙体);
                                                        增加缓存物体(Id, 建筑);
                                                    }
                                                    break;
                                                }
                                            case Objects.Structures.Frame:
                                                {
                                                    if (所有已选择.Count == 0)
                                                    {
                                                        框选状态 = 批量选择网格状态.开始框选;
                                                        var Id = 建筑.ReferenceId;
                                                        框选起点 = (建筑.GridPosition, Id, 批量选择建筑类型.框架);
                                                        增加缓存物体(Id, 建筑);
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                }
                            case 批量选择网格状态.开始框选:
                                {
                                    var 建筑 = 通用工具.获取视线处建筑类物体();
                                    if (建筑 && KeyManager.GetMouseUp("Primary"))
                                    {
                                        switch (建筑)
                                        {
                                            case Assets.Scripts.Objects.Wall:
                                                {
                                                    if (所有已选择.Count == 1 && 所有已选择.Values.First().GridPosition == 框选起点.网格坐标)
                                                    {
                                                        // 取消当前框选起点
                                                        var Id = 建筑.ReferenceId;
                                                        if (所有已选择.ContainsKey(Id))
                                                        {
                                                            框选状态 = 批量选择网格状态.没有框选;
                                                            减少缓存物体(Id, 建筑);
                                                        }
                                                        else
                                                        {
                                                            框选状态 = 批量选择网格状态.结束框选;
                                                            框选终点 = (建筑.GridPosition, Id, 批量选择建筑类型.墙体);

                                                            if (框选起点.建筑类型 == 框选终点.建筑类型)
                                                            {
                                                                增加缓存物体(Id, 建筑);
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            case Objects.Structures.Frame:
                                                {
                                                    if (所有已选择.Count == 1 && 所有已选择.Values.First().GridPosition == 框选起点.网格坐标)
                                                    {
                                                        // 取消当前框选起点
                                                        var Id = 建筑.ReferenceId;
                                                        if (所有已选择.ContainsKey(Id))
                                                        {
                                                            框选状态 = 批量选择网格状态.没有框选;
                                                            减少缓存物体(Id, 建筑);
                                                        }
                                                        else
                                                        {
                                                            框选状态 = 批量选择网格状态.结束框选;
                                                            框选终点 = (建筑.GridPosition, Id, 批量选择建筑类型.框架);

                                                            if (框选起点.建筑类型 == 框选终点.建筑类型)
                                                            {
                                                                增加缓存物体(Id, 建筑);
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                            case 批量选择网格状态.结束框选:
                                {
                                    var 起点 = 框选起点.网格坐标;
                                    var 终点 = 框选终点.网格坐标;
                                    var 框选目标类型 = 框选起点.建筑类型;

                                    var min = new Grid3(Mathf.Min(起点.x, 终点.x), Mathf.Min(起点.y, 终点.y), Mathf.Min(起点.z, 终点.z));
                                    var max = new Grid3(Mathf.Max(起点.x, 终点.x), Mathf.Max(起点.y, 终点.y), Mathf.Max(起点.z, 终点.z));
                                    var 网格尺寸 = Grid3.Directions.First().z;

                                    int 已处理网格计数 = 0;

                                    for (var i = min.x; i <= max.x; i += 网格尺寸)
                                    {
                                        for (var j = min.y; j <= max.y; j += 网格尺寸)
                                        {
                                            for (var k = min.z; k <= max.z; k += 网格尺寸)
                                            {
                                                ++已处理网格计数;

                                                var 网格单元 = GridController.World.GetCell(new Grid3(i, j, k));      // Cell: 网格单元, 和框架一样的大小, 网格单元持有内部所有的可放置设备(墙、框架、门.....)
                                                if (网格单元 == null) { continue; }

                                                for (var 当前方位 = StructureElement.South; 当前方位 >= StructureElement.Center; 当前方位--)
                                                {
                                                    // 东、南、西、北、中、上、下
                                                    var 当前放置结构 = 网格单元.Lookup[当前方位];
                                                    if (当前放置结构 == null) { continue; }

                                                    var Id = 当前放置结构.ReferenceId;
                                                    if (!所有已选择.ContainsKey(Id))
                                                    {
                                                        switch (框选目标类型)
                                                        {
                                                            case 批量选择建筑类型.墙体:
                                                                if (当前放置结构 is Assets.Scripts.Objects.Wall) { 增加缓存物体(Id, 当前放置结构); }
                                                                break;
                                                            case 批量选择建筑类型.框架:
                                                                if (当前放置结构 is Objects.Structures.Frame) { 增加缓存物体(Id, 当前放置结构); }
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    框选状态 = 批量选择网格状态.没有框选;
                                    当前状态 = 批量操作任务状态.选择;
                                    功能模块之快捷轮盘菜单.Log.LogMessage($"已扫描了 {已处理网格计数} 个网格单元");
                                    break;
                                }
                        }
                        break;
                    }
                case 批量操作任务状态.清空高亮:
                    {
                        Clear();
                        当前状态 = 批量操作任务状态.睡眠;
                        break;
                    }
                case 批量操作任务状态.拆除:
                    {
                        清理已失效();

                        if (所有已选择.Count == 0) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        var (活动手, 空闲手) = 通用工具.获取活动手槽位和空闲手槽位();
                        var 主手物 = 活动手.Get<Item>();
                        var 副手物 = 空闲手.Get();

                        int 可拆次数 = 0;
                        int 仓库剩余空间 = 0;
                        int 拆除返还数量 = 0;

                        (Item 材料, int 数量) 安装A = (null, 0);
                        (Item 材料, int 数量) 安装B = (null, 0);
                        bool 是否可拆除 = false;

                        Tool 主手槽工具 = null;

                        // 建造阶段里的材料配方里的材料若是工具, 则消耗数量始终为0
                        foreach (Structure 建筑 in 所有已选择.Values)
                        {
                            var 建造阶段 = 建筑.CurrentBuildState;
                            if (建造阶段 == null) { continue; }
                            {
                                var 材料配方 = 建造阶段.Tool;
                                if (材料配方 == null) { continue; }

                                if (材料配方.ToolExit)
                                {
                                    if (材料配方.IsToolExit(主手物))
                                    {
                                        主手槽工具 = 主手物 as Tool;
                                        if (主手槽工具 && !主手槽工具.IsOperable)
                                        {
                                            continue;
                                        }

                                        // 材料配方中的安装配方固定是单材料或者一工具一材料或者单工具, 材料配方中的拆除配方固定是一工具, 拆除返还就是安装配方中的材料
                                        // 因此在此处主手物判断是否是拆除工具, 副手物则分别比较安装A材料与安装B材料, 判断副手是否被占用, 占用了是否可合并, 避免拆除后物资掉在地上
                                        // 例: 铁框架需要焊枪与铁板安装, 安装A材料为焊枪, 安装B材料为铁板, 此时拆除返还的为B材料铁板
                                        // 例: 铁墙只需要铁板安装, 因此安装A为铁板, 安装B材料为空, 此时返还的材料为A材料铁板
                                        // 例: 自动车床最后阶段只需要螺丝刀安装, 因此安装A为螺丝刀, 安装B材料为空, 此时返还的材料为无
                                        if ((材料配方.ToolEntry == null || 材料配方.ToolEntry is Tool) && (材料配方.ToolEntry2 == null || 材料配方.ToolEntry2 is Tool))
                                        {
                                            可拆次数 = int.MaxValue;
                                            拆除返还数量 = 0;
                                            仓库剩余空间 = int.MaxValue;

                                            安装A = (材料配方.ToolEntry, 材料配方.EntryQuantity);
                                            安装B = (材料配方.ToolEntry2, 材料配方.EntryQuantity2);
                                            是否可拆除 = true;
                                            break;
                                        }
                                        else if (材料配方.ToolEntry is not Tool)
                                        {
                                            switch (材料配方.ToolEntry)
                                            {
                                                case Stackable 可堆叠材料A:
                                                    可拆次数 = int.MaxValue;
                                                    拆除返还数量 = 材料配方.EntryQuantity;

                                                    if (副手物 == null)
                                                    {
                                                        仓库剩余空间 = 可堆叠材料A.MaxQuantity;
                                                    }
                                                    else if (副手物.PrefabHash == 可堆叠材料A.PrefabHash)
                                                    {
                                                        仓库剩余空间 = 可堆叠材料A.MaxQuantity - ((Stackable)副手物).Quantity;
                                                    }
                                                    else
                                                    {
                                                        仓库剩余空间 = 0;
                                                    }

                                                    break;
                                            }

                                            if (仓库剩余空间 == 0) { continue; }

                                            安装A = (材料配方.ToolEntry, 材料配方.EntryQuantity);
                                            安装B = (材料配方.ToolEntry2, 材料配方.EntryQuantity2);
                                            是否可拆除 = true;
                                            break;
                                        }
                                        else if (材料配方.ToolEntry2 is not Tool)
                                        {
                                            switch (材料配方.ToolEntry2)
                                            {
                                                case Stackable 可堆叠材料B:
                                                    可拆次数 = int.MaxValue;
                                                    拆除返还数量 = 材料配方.EntryQuantity2;

                                                    if (副手物 == null)
                                                    {
                                                        仓库剩余空间 = 可堆叠材料B.MaxQuantity;
                                                    }
                                                    else if (副手物.PrefabHash == 可堆叠材料B.PrefabHash)
                                                    {
                                                        仓库剩余空间 = 可堆叠材料B.MaxQuantity - ((Stackable)副手物).Quantity;
                                                    }
                                                    else
                                                    {
                                                        仓库剩余空间 = 0;
                                                    }

                                                    break;
                                            }

                                            if (仓库剩余空间 == 0) { continue; }

                                            安装A = (材料配方.ToolEntry, 材料配方.EntryQuantity);
                                            安装B = (材料配方.ToolEntry2, 材料配方.EntryQuantity2);
                                            是否可拆除 = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (!是否可拆除) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        if (主手槽工具)
                        {
                            // 例: 手电钻使用时, 按住按钮钻头转动, 松开按钮钻头停止
                            添加事件(() =>
                            {
                                // if (NetworkManager.IsClient) { 开始使用工具事件与结束使用工具事件数据包.发送数据包(主手槽工具.ReferenceId, 按压了么_: true); }
                                // else { 主手槽工具.OnPrimaryUseStart(); }
                                主手槽工具.OnPrimaryUseStart();
                            });
                        }

                        foreach (Structure 建筑 in 所有已选择.Values)
                        {
                            if (可拆次数 <= 0 || 仓库剩余空间 <= 0) { break; }

                            var 建造阶段 = 建筑.CurrentBuildState;
                            if (建造阶段 == null) { continue; }

                            var 材料配方 = 建造阶段.Tool;
                            if (材料配方 == null) { continue; }

                            if (材料配方.ToolExit)
                            {
                                if (材料配方.IsToolExit(主手物))
                                {
                                    if ((材料配方.ToolEntry == null || 材料配方.ToolEntry is Tool) && (材料配方.ToolEntry2 == null || 材料配方.ToolEntry2 is Tool))
                                    {
                                        添加事件(() => OnServer.AttackWith(InventoryManager.Parent, (byte)活动手.SlotIndex, (byte)空闲手.SlotIndex, 建筑.ReferenceId, 建筑.CenterPosition, 1f, false, false));
                                        continue;
                                    }
                                    else if (材料配方.ToolEntry is not Tool)
                                    {
                                        if (材料配方.ToolEntry.PrefabHash == 安装A.材料.PrefabHash && 材料配方.EntryQuantity == 安装A.数量)
                                        {
                                            --可拆次数;
                                            仓库剩余空间 -= 拆除返还数量;
                                            添加事件(() => OnServer.AttackWith(InventoryManager.Parent, (byte)活动手.SlotIndex, (byte)空闲手.SlotIndex, 建筑.ReferenceId, 建筑.CenterPosition, 1f, false, false));
                                            continue;
                                        }
                                    }
                                    else if (材料配方.ToolEntry2 is not Tool)
                                    {
                                        if (材料配方.ToolEntry2.PrefabHash == 安装B.材料.PrefabHash && 材料配方.EntryQuantity2 == 安装B.数量)
                                        {
                                            --可拆次数;
                                            仓库剩余空间 -= 拆除返还数量;
                                            添加事件(() => OnServer.AttackWith(InventoryManager.Parent, (byte)活动手.SlotIndex, (byte)空闲手.SlotIndex, 建筑.ReferenceId, 建筑.CenterPosition, 1f, false, false));
                                            continue;
                                        }
                                    }
                                }
                            }
                        }

                        if (主手槽工具)
                        {
                            添加事件(() =>
                         {
                             //  if (NetworkManager.IsClient) { 开始使用工具事件与结束使用工具事件数据包.发送数据包(主手槽工具.ReferenceId, 按压了么_: false); }
                             //  else { 主手槽工具.OnPrimaryUseEnd(); }
                             主手槽工具.OnPrimaryUseEnd();
                         });
                        }

                        当前状态 = 批量操作任务状态.睡眠;
                        break;
                    }
                case 批量操作任务状态.装配:
                    {
                        清理已失效();

                        if (所有已选择.Count == 0) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        (Item 材料, int 数量) 安装A = (null, 0);
                        (Item 材料, int 数量) 安装B = (null, 0);
                        bool 是否可装配 = false;

                        int 可装次数 = 0;

                        var (活动手, 空闲手) = 通用工具.获取活动手槽位和空闲手槽位();
                        var 主手物 = 活动手.Get<Item>();
                        var 副手物 = 空闲手.Get<Item>();

                        Tool 主手槽工具 = null;

                        // 建造阶段里的材料配方里的材料若是工具, 则消耗数量始终为0
                        foreach (Structure 建筑 in 所有已选择.Values)
                        {
                            var 建造阶段 = 建筑.NextBuildState;
                            if (建造阶段 == null) { continue; }
                            {
                                var 材料配方 = 建造阶段.Tool;
                                if (材料配方 == null) { continue; }

                                if (材料配方.ToolEntry)
                                {
                                    if (材料配方.IsToolEntry(主手物))
                                    {
                                        主手槽工具 = 主手物 as Tool;
                                        if (主手槽工具 && !主手槽工具.IsOperable)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                if (材料配方.ToolEntry2)
                                {
                                    if (材料配方.IsToolEntry2(副手物))
                                    {
                                        if (副手物 is Tool 副手槽工具 && !副手槽工具.IsOperable)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                // 材料配方中的安装配方固定是单材料或者一工具一材料或者单工具, 材料配方中的拆除配方固定是一工具, 安装消耗就是安装配方中的材料
                                // 例: 铁框架需要焊枪与铁板安装, 安装A材料为焊枪, 安装B材料为铁板, 此时消耗的为B材料铁板
                                // 例: 铁墙只需要铁板安装, 因此安装A为铁板, 安装B材料为空, 此时消耗的材料为A材料铁板
                                // 例: 自动车床最后阶段只需要螺丝刀安装, 因此安装A为螺丝刀, 安装B材料为空, 此时消耗的材料为无
                                if ((材料配方.ToolEntry == null || 材料配方.ToolEntry is Tool) && (材料配方.ToolEntry2 == null || 材料配方.ToolEntry2 is Tool))
                                {
                                    可装次数 = int.MaxValue;
                                }
                                else if (材料配方.ToolEntry is not Tool)
                                {
                                    switch (材料配方.ToolEntry)
                                    {
                                        // 如果材料不是可堆叠材料, 可装次数为0
                                        case Stackable 可堆叠材料A:
                                            if (主手物 && 主手物.PrefabHash == 可堆叠材料A.PrefabHash)
                                            {
                                                可装次数 = Mathf.FloorToInt(((Stackable)主手物).Quantity / 材料配方.EntryQuantity);
                                            }
                                            break;
                                    }
                                }
                                else if (材料配方.ToolEntry2 is not Tool)
                                {
                                    switch (材料配方.ToolEntry2)
                                    {
                                        // 如果材料不是可堆叠材料, 可装次数为0
                                        case Stackable 可堆叠材料B:
                                            // 主手在上面已经判断过了, 此处不需要重复判断主手是否匹配主手材料
                                            if (副手物 && 副手物.PrefabHash == 可堆叠材料B.PrefabHash)
                                            {
                                                可装次数 = Mathf.FloorToInt(((Stackable)副手物).Quantity / 材料配方.EntryQuantity2);
                                            }
                                            break;
                                    }
                                }

                                if (可装次数 == 0) { continue; }

                                是否可装配 = true;
                                安装A = (材料配方.ToolEntry, 材料配方.EntryQuantity);
                                安装B = (材料配方.ToolEntry2, 材料配方.EntryQuantity2);

                                break;
                            }
                        }

                        if (!是否可装配) { 当前状态 = 批量操作任务状态.睡眠; break; }

                        if (主手槽工具)
                        {
                            // 例: 手电钻使用时, 按住按钮钻头转动, 松开按钮钻头停止
                            添加事件(() =>
                            {
                                // if (NetworkManager.IsClient) { 开始使用工具事件与结束使用工具事件数据包.发送数据包(主手槽工具.ReferenceId, 按压了么_: true); }
                                // else { 主手槽工具.OnPrimaryUseStart(); }
                                主手槽工具.OnPrimaryUseStart();
                            });
                        }

                        foreach (Structure 建筑 in 所有已选择.Values)
                        {
                            if (可装次数 <= 0) { break; }

                            var 建造阶段 = 建筑.NextBuildState;
                            if (建造阶段 == null) { continue; }

                            var 材料配方 = 建造阶段.Tool;
                            if (材料配方 == null) { continue; }

                            if ((材料配方.ToolEntry == null || 材料配方.ToolEntry is Tool) && (材料配方.ToolEntry2 == null || 材料配方.ToolEntry2 is Tool))
                            {
                                if (材料配方.IsToolEntry(主手物))
                                {
                                    添加事件(() => OnServer.AttackWith(InventoryManager.Parent, (byte)活动手.SlotIndex, (byte)空闲手.SlotIndex, 建筑.ReferenceId, 建筑.CenterPosition, 1f, false, false));
                                    continue;
                                }
                            }
                            else if (材料配方.ToolEntry is not Tool)
                            {
                                if (材料配方.ToolEntry.PrefabHash == 安装A.材料.PrefabHash && 材料配方.EntryQuantity == 安装A.数量)
                                {
                                    --可装次数;
                                    添加事件(() => OnServer.AttackWith(InventoryManager.Parent, (byte)活动手.SlotIndex, (byte)空闲手.SlotIndex, 建筑.ReferenceId, 建筑.CenterPosition, 1f, false, false));
                                    continue;
                                }
                            }
                            else if (材料配方.ToolEntry2 is not Tool)
                            {
                                if (材料配方.IsToolEntry(主手物) && 材料配方.ToolEntry2.PrefabHash == 安装B.材料.PrefabHash && 材料配方.EntryQuantity2 == 安装B.数量)
                                {
                                    --可装次数;
                                    添加事件(() => OnServer.AttackWith(InventoryManager.Parent, (byte)活动手.SlotIndex, (byte)空闲手.SlotIndex, 建筑.ReferenceId, 建筑.CenterPosition, 1f, false, false));
                                    continue;
                                }
                            }
                        }

                        if (主手槽工具)
                        {
                            添加事件(() =>
                         {
                             //  if (NetworkManager.IsClient) { 开始使用工具事件与结束使用工具事件数据包.发送数据包(主手槽工具.ReferenceId, 按压了么_: false); }
                             //  else { 主手槽工具.OnPrimaryUseEnd(); }
                             主手槽工具.OnPrimaryUseEnd();
                         });
                        }

                        当前状态 = 批量操作任务状态.睡眠;
                        break;
                    }
            }

            if (当前状态 == 之前状态) { return; }

            switch (当前状态)
            {
                case 批量操作任务状态.睡眠:
                    {
                        快捷键配置.快捷轮盘菜单_批量拆除和装配_高亮开关 = false;
                        功能模块之快捷轮盘菜单.Log.LogMessage("批量拆除和装配：已选择建筑关闭渲染");
                        break;
                    }
                default:
                    {
                        清理已失效();
                        快捷键配置.快捷轮盘菜单_批量拆除和装配_高亮开关 = true;
                        功能模块之快捷轮盘菜单.Log.LogMessage("批量拆除和装配：已选择建筑开启渲染");
                        break;
                    }
            }
        }
    }
}