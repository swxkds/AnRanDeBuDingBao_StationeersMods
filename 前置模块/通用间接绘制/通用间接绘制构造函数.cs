using Assets.Scripts;
using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.Scripts.Objects;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 通用间接绘制管理器 : MonoBehaviour
    {
        public static 通用间接绘制管理器 单例 = null;
        private List<通用间接绘制> Field_所有通用间接绘制 = null;
        public static void 通用间接绘制管理器构造函数(List<(List<(多图层_多物体_批量绘制.图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> 图层参数, Func<bool> 访问快捷键)> Arg_所有构造参数)
        {
            // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用
            if (GameManager.IsBatchMode) { return; }

            单例 = new GameObject().AddComponent<通用间接绘制管理器>();
            单例.gameObject.name = $"{单例.GetType().Name}";
            UnityEngine.Object.DontDestroyOnLoad(单例.gameObject);

            {
                单例.Field_所有通用间接绘制 = new(Arg_所有构造参数.Count);

                foreach ((List<(多图层_多物体_批量绘制.图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> 图层参数, Func<bool> 访问快捷键) 构造参数 in Arg_所有构造参数)
                {
                    单例.Field_所有通用间接绘制.Add(new(构造参数.图层参数, 构造参数.访问快捷键));
                }
            }

            前置模块.Log.LogMessage($"成功创建通用间接绘制管理器单例");
        }

        private void Update()
        {
            foreach (var __ in Field_所有通用间接绘制)
            {
                __.Update();
            }

        }

        private void OnDestroy()
        {
            foreach (var __ in Field_所有通用间接绘制)
            {
                __.Dispose();
            }

            Field_所有通用间接绘制.Clear();
            Field_所有通用间接绘制 = null;

            前置模块.Log.LogMessage($"成功注销房间闭合检测单例");
        }
    }
}
