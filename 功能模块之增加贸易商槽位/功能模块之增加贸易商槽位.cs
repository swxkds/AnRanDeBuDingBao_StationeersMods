using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;
using Trading;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_zengjiamaoyishangcaowei", "功能模块之增加贸易商槽位", "1.0.0")]
    // [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之增加贸易商槽位 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之增加贸易商槽位加载完成!");
            补丁 = new Harmony("功能模块之增加贸易商槽位");
            补丁.PatchAll();
        }
    }

    [HarmonyPatch(typeof(WorldManager), "LoadDataFiles")]
    public class 加载StreamingAssets目录中的xml文件
    {
        [HarmonyPostfix]
        public static void 执行()
        {
            var 所有贸易用通信频道 = ContactSlotData.AllContactSlotData;
            var 所有贸易商 = TraderData.AllTraderData;

            添加贸易用通信频道号码(ref 所有贸易用通信频道, 所有贸易商, 原版小型贸易频道通信号码, 添加号码数量: 5);
            添加贸易用通信频道号码(ref 所有贸易用通信频道, 所有贸易商, 原版中型贸易频道通信号码, 添加号码数量: 4);
            添加贸易用通信频道号码(ref 所有贸易用通信频道, 所有贸易商, 原版大型贸易频道通信号码, 添加号码数量: 3);
            添加贸易用通信频道号码(ref 所有贸易用通信频道, 所有贸易商, 原版食物_硬件_消耗品_家电_贸易频道通信号码, 添加号码数量: 2);
            添加贸易用通信频道号码(ref 所有贸易用通信频道, 所有贸易商, 原版奇珍贸易频道通信号码, 添加号码数量: 1);


            static void 添加贸易用通信频道号码(ref List<ContactSlotData> 所有贸易用通信频道, List<TraderData> 所有贸易商, int 已存在的某个贸易频道通信号码, int 添加号码数量)
            {
                var 索引 = 所有贸易用通信频道.FindIndex(d => d.IdHash == 已存在的某个贸易频道通信号码);
                if (索引 >= 0)
                {
                    var 结果 = 所有贸易用通信频道[索引];
                    var NewList = new List<ContactSlotData>();

                    for (var i = 0; i < 添加号码数量; i++)
                    {
                        var 新号码 = $"{结果.Id}_{i}";
                        var New = new ContactSlotData
                        {
                            Id = 新号码,                                      // 频道的NameID
                            IdHash = Animator.StringToHash(新号码),
                            Icon = 结果.Icon,
                            MinimumWattsVisible = 结果.MinimumWattsVisible,   // 达到功率要求时, 更新<某某进入了信号范围, 某某离开了信号范围>
                            WattsToResolve = 结果.WattsToResolve,             // 达到功率要求时, 可以联系
                            MinimumWattsToContact = 结果.MinimumWattsToContact, // 达到功率要求时, 可以在通讯电脑上看到贸易商
                            SecondsToContact = 结果.SecondsToContact,          // 联系所需要的最低时间
                            LifeTime = 结果.LifeTime,                          // 贸易商能占用频道的时间, 过期后, 贸易商会自动退出频道
                            DownTime = 结果.DownTime,                          // 频道的冷却时间, 频道在冷却时间内, 不能被贸易商占用
                            BulkData = 结果.BulkData,                          // 每个贸易商在生成商品数据时, 携带几倍商品
                            ConditionSelect = 结果.ConditionSelect,            // 定义了任意个着陆条件(是否是飞机、着陆平台尺寸要求、呼吸环境要求), 贸易商在刷新时, 会根据权重值+随机数, 选择一个着陆条件并赋给自己
                            TraderSelectData = 结果.TraderSelectData,          // 定义了任意个贸易商NameID(在贸易商条目中定义能使用的频道NameID, 解析时会自动添加到此处), 贸易商在刷新时, 会随机选择一个贸易商并赋给自己
                            WorldCondition = 结果.WorldCondition               // 定义了任意个世界ID, 如果当前游玩世界ID不在定义列表中, 则跳过贸易商刷新 作用: 真空星球, 但是频道的着陆条件中有飞机, 该频道的贸易商不能刷新, 贸易商不刷新则通讯电脑看不到, 等于不存在
                        };

                        所有贸易用通信频道.Add(New);
                        NewList.Add(New);
                    }

                    为贸易商添加新的贸易许可(所有贸易商, 已存在的某个贸易频道通信号码, NewList);
                }
            }

            static void 为贸易商添加新的贸易许可(List<TraderData> 所有贸易商, int 已存在的某个贸易频道通信号码, List<ContactSlotData> 基于已存在的某个贸易频道通信号码创建的所有新号码)
            {
                foreach (var 当前 in 所有贸易商)
                {
                    var 号码本 = 当前.SlotTypes;        // 定义了任意个频道NameID
                    var 索引 = 号码本.FindIndex(d => d.SlotIdHash == 已存在的某个贸易频道通信号码);
                    if (索引 >= 0)
                    {
                        foreach (var d in 基于已存在的某个贸易频道通信号码创建的所有新号码)
                        {
                            var New = new SlotIdReference()
                            {
                                SlotId = d.Id,
                                SlotIdHash = d.IdHash,
                            };

                            号码本.Add(New);
                        }
                    }
                }
            }
        }

        public static readonly int 原版小型贸易频道通信号码 = Animator.StringToHash("BasicTrader");
        public static readonly int 原版中型贸易频道通信号码 = Animator.StringToHash("MediumTrader");
        public static readonly int 原版大型贸易频道通信号码 = Animator.StringToHash("LargeTrader");
        public static readonly int 原版食物_硬件_消耗品_家电_贸易频道通信号码 = Animator.StringToHash("UtilityTrader");
        public static readonly int 原版奇珍贸易频道通信号码 = Animator.StringToHash("ExoticsTrader");
    }
}
