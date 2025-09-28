using System;
using System.Runtime.InteropServices;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;


namespace meanran_xuexi_mods_xiaoyouhua.ui.things
{
    class LogicSorterUi : I文本
    {
        public Type ThingType() => typeof(LogicSorter);
        public string ThingString(Thing thing)
        {
            // 变压器
            var obj = thing as LogicSorter;
            var LogicStack = obj.GetLogicStack();
            var 文本 = "";
            for (var i = 0; i < LogicStack.Size; i++)
            {
                if (LogicStack[i] != 0)
                {
                    var v = new WritableStackAddress(i, LogicStack[i]);
                    var opcode = (SorterInstruction)v.Opcode;
                    var value = Convert.ToString(v.Value, 2);
                    文本 += $"<color=green><b>LogicStack[{i + 1}] = {value}\nOpcode:{opcode}</b></color>";
                }
            }
            return $"{obj.DisplayName}\n{文本}";
        }
    }
}
