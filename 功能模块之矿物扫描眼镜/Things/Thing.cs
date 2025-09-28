using Assets.Scripts.Objects;
using UnityEngine;
using Assets.Scripts.Objects.Items;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public partial class 矿物扫描眼镜 : PowerTool
    {
        private Slot 能源矿物槽位;
        private float 能源矿物消耗计时;
        private float 使用能源矿物时开机电量消耗;
        private float 使用电池时开机电量消耗;
        public static readonly int 能源矿物ID = Animator.StringToHash("ItemUraniumOre");
        public override void Awake()
        {
            base.Awake();
            能源矿物槽位 = Slots.Find((Slot s) => s.Type == Slot.Class.Ore);
            使用能源矿物时开机电量消耗 = UsedPowerPassive * 0.1f;
            使用电池时开机电量消耗 = UsedPowerPassive;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}