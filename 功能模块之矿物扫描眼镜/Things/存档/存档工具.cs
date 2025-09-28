using HarmonyLib;
using Assets.Scripts.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Objects;
using System.Xml.Serialization;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [XmlInclude(typeof(DynamicThingSaveData))]
    public class 矿物扫描眼镜SaveData : DynamicThingSaveData
    {
        [XmlElement]
        public float 能源矿物消耗时间;
    }

    [HarmonyPatch()]
    public class 注册XML序列化工具
    {
        [HarmonyTargetMethod]
        public static MethodBase 获取带有ref形参的函数信息()
        {
            var __ = AccessTools.Method(typeof(XmlSaveLoad), nameof(XmlSaveLoad.AddExtraTypes), [typeof(List<Type>).MakeByRefType()]);
            return __;
        }
        [HarmonyPrefix]
        public static void 执行(ref List<Type> extraTypes)
        {
            var type = typeof(矿物扫描眼镜SaveData);
            if (!extraTypes.Contains(type))
            {
                extraTypes.Add(type);
                功能模块之矿物扫描眼镜.Log.LogMessage("成功注册矿物扫描眼镜的XML序列化工具");
            }
        }
    }

}