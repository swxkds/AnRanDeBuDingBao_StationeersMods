using Assets.Scripts.UI;
using Assets.Scripts.Objects.Items;
using Assets.Scripts;
using System.Text;
using Assets.Scripts.Objects.Entities;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public partial class 矿物扫描眼镜 : PowerTool
    {
        public override string GetStationpediaCategory()
        {
            return Localization.GetInterface(StationpediaCategoryStrings.PersonalEyeWear);
        }

        public override StringBuilder GetExtendedText()
        {
            StringBuilder extendedText = base.GetExtendedText();
            return extendedText;
        }

        public override bool IsBurnable
        {
            get
            {
                if (ParentSlot?.Parent is Human human)
                {
                    var gasMask = human.HelmetSlot.Get<GasMask>();
                    if (gasMask != null && !gasMask.IsOpen)
                    {
                        return false;
                    }
                }

                return base.IsBurnable;
            }
        }
    }
}