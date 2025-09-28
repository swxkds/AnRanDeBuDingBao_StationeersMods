using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public static void 变更激活状态(GameObject obj, bool 新状态)
        {
            if (obj && obj.activeSelf != 新状态)
            {
                obj.SetActive(新状态);
            }
        }
    }
}