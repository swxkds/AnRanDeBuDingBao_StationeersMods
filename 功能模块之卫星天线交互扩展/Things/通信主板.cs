using System;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Objects.Motherboards.Comms;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [HarmonyPatch(typeof(ContactsTab), nameof(ContactsTab.Initialise))]
    public class 在通信主板中添加追踪按钮
    {
        [HarmonyPostfix]
        public static void 执行(ContactsTab __instance)
        {
            var 贸易商列表 = __instance.ContactItems;

            foreach (var 当前 in 贸易商列表)
            {
                var 按钮列表 = 当前.transform.Find("Interaction Panel");
                if (按钮列表)
                {
                    var 联系 = 按钮列表.Find("ButtonInterrogate");
                    if (联系)
                    {
                        var 追踪 = UnityEngine.Object.Instantiate(联系, 联系.parent);
                        追踪.name = "ButtonTracking";

                        var UI层级管理器 = 当前.GetComponent<UiComponentRenderer>();
                        foreach (var 背景纹理 in 追踪.GetComponentsInChildren<UnityEngine.UI.Image>(includeInactive: true))
                        {
                            if (!UI层级管理器.ImageComponents.Any(d => d == 背景纹理))
                            {
                                Array.Resize(ref UI层级管理器.ImageComponents, UI层级管理器.ImageComponents.Length + 1);
                                UI层级管理器.ImageComponents[UI层级管理器.ImageComponents.Length - 1] = 背景纹理;   // 添加新元素 
                            }
                        }

                        var 按钮 = 追踪.GetComponentInChildren<UnityEngine.UI.Button>(includeInactive: true);
                        按钮.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
                        按钮.onClick.RemoveAllListeners();
                        按钮.onClick.AddListener(() =>
                        {
                            var 卫星天线 = 当前.ParentMotherboard.SelectedDish;
                            var 卫星天线控件 = 卫星天线.InteractButton1;
                            var 贸易商 = 当前.AssignedContact;
                            var 数据包 = 卫星天线数据包.创建数据包(卫星天线.ReferenceId, 贸易商.ReferenceId, 卫星天线控件.InteractableId, 通用可选择项目.数据解包标志.贸易商船);
                            数据包.Process(0);
                            AlertPanel.Instance.ShowAlert("已接管卫星天线转动, 正在追踪, 此消息弹窗会自动关闭, 请稍等......\n因存在特殊情况下数学计算有多个解, 若天线停止转动后, 与目标夹角不为0°, 可再次点击追踪\n提示: 天线的俯仰角垂直朝上时, 追踪容易失败", AlertState.Loading, 2f);
                        });
                    }
                }
            }

            功能模块之卫星天线交互扩展.Log.LogMessage("在通信主板中添加追踪按钮");
        }
    }
}