using Assets.Scripts.Objects;
using Assets.Scripts.Util;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 立体文字装饰 : SmallGrid, ISmartRotatable
    {
        public override void Awake()
        {
            base.Awake();
            SmartRotate.AutomaticSetup(this);
        }

        public SmartRotate.ConnectionType GetConnectionType()
        {
            return ConnectionType;
        }

        public void SetOpenEndsPermutation(int[] permutation)
        {
            this.OpenEndsPermutation = (int[])permutation.Clone();
        }

        public void SetConnectionType(SmartRotate.ConnectionType connectionType)
        {
            ConnectionType = connectionType;
        }

        public int[] GetOpenEndsPermutation()
        {
            return (int[])OpenEndsPermutation.Clone();
        }

        public SmartRotate.ConnectionType ConnectionType = SmartRotate.ConnectionType.Exhaustive;

        public int[] OpenEndsPermutation =
        [
            0,
            1,
            2,
            3,
            4,
            5
        ];
    }
}