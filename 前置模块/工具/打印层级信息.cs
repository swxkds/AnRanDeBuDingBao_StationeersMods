using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public static partial class 通用工具
    {
        public static string 打印层级信息(GameObject 根节点, string 抬头 = "")
        {
            var 结果 = new StringBuilder();
            var stack = new Stack<(GameObject 节点, int 制表符, int 层级)>();
            stack.Push((根节点, 0, 0));                 // 将根节点压入栈中，制表符和层级从0开始

            while (stack.Count > 0)
            {
                var (当前节点, 当前制表符, 当前层级) = stack.Pop();

                结果.AppendLine($"\n{抬头.PadLeft(当前制表符)}层级[{当前层级}] = {当前节点.name}\n");

                所有组件信息(ref 结果, 当前节点, 当前制表符);

                // 将子节点按从下到上的顺序压入栈中，这样栈顶的子节点就会先被处理
                // 若子节点依然有子节点,则孙节点同样按照逆序入栈,将一条线性节点全部入栈后,切换到第二条节点
                for (int i = 当前节点.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = 当前节点.transform.GetChild(i);
                    stack.Push((child.gameObject, 当前制表符 + 10, 当前层级 + 1));  // 增加制表符和层级
                }
            }

            return 结果.ToString();
        }

        private static void 所有组件信息(ref StringBuilder 结果, GameObject 节点, int 制表符)
        {
            var 组件前缀 = string.Empty.PadLeft(制表符 + 5);
            var 所有组件 = 节点.GetComponents<Component>();
            for (var i = 0; i < 所有组件.Length; ++i)
            {
                var 当前 = 所有组件[i];
                结果.AppendLine($"{组件前缀}---组件[{i}]: ({当前.GetType().Name}) {当前.name}");
            }
        }
    }
}