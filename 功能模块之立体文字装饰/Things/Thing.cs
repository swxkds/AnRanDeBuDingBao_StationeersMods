using Assets.Scripts.Objects.Structures;
using Assets.Scripts.Util;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 立体文字装饰 : WallLight
    {
        public override void Awake()
        {
            base.Awake();
            SmartRotate.AutomaticSetup(this);
        }
    }
}