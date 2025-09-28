using TMPro;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public static void 变更字体尺寸(TMP_Text obj, float 新尺寸)
        {
            if (obj == null)
            {
                前置模块.Log.LogError($"传入的TMPro.TMP_Text(文本组件)为空, 无法修改字体尺寸");
                return;
            }

            if (obj.fontSize < 新尺寸)
            {
                obj.fontSizeMin = 新尺寸;
                obj.fontSize = 新尺寸;
            }

            if (新尺寸 > obj.fontSizeMax)
            {
                obj.fontSizeMax = 新尺寸;
            }
        }

        public static void 变更字体尺寸(UnityEngine.UI.Text obj, int 新尺寸)
        {
            if (obj == null)
            {
                前置模块.Log.LogError($"传入的UnityEngine.UI.Text(文本组件)为空, 无法修改字体尺寸");
                return;
            }

            if (obj.fontSize < 新尺寸)
            {
                obj.resizeTextMinSize = 新尺寸;
                obj.fontSize = 新尺寸;
            }

            if (新尺寸 > obj.resizeTextMaxSize)
            {
                obj.resizeTextMaxSize = 新尺寸;
            }
        }
    }
}