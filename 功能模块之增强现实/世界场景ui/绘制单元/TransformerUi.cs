using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;

using meanran_xuexi_mods_xiaoyouhua.utils;

namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class TransformerUI : I文本
    {
        public Type ThingType() => typeof(Transformer);
        public string ThingString(Thing thing)
        {
            // 变压器
            var obj = thing as Transformer;
            var color = obj.Powered ? "green" : "red";
            return $"{obj.DisplayName}\n变压功率: <color={color}><b>{换算工具.PowerToString(obj.Setting)}</b></color>\n{词条库类.待机}: {换算工具.PowerToString(obj.UsedPower)}\n{词条库类.供电}:{换算工具.PowerToString(obj.AvailablePower)}";
        }
    }
}
