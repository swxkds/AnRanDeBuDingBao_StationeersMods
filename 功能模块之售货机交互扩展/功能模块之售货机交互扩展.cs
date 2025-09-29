using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.UI;
using BepInEx;
using HarmonyLib;
using Assets.Scripts.Inventory;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using TMPro;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_shouhuojijiaohukuozhan", "功能模块之售货机交互扩展", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之售货机交互扩展 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之售货机交互扩展加载完成!");
            补丁 = new Harmony("功能模块之售货机交互扩展");
            补丁.PatchAll();
            StartCoroutine(并发构造());  // Unity引擎对第三方协程库的支持是后期开发的,因此API实例的加载顺序挺靠后,在Awake时请使用Unity原生协程
        }
        private IEnumerator 并发构造()
        {
            // InventoryWindowManager.Instance.WindowGrid:垂直排版多个背包窗口的布局组
            // InventoryWindowManager.Instance.WindowPrefab:背包窗口克隆母体
            while (InventoryWindowManager.Instance == null
            || InventoryWindowManager.Instance.WindowGrid == null
            || InventoryWindowManager.Instance.WindowPrefab == null)
            { yield return null; }    // 等待下一帧

            if (前置_资源加载器.单例.逻辑组件字典.TryGetValue(typeof(InputPrefabs), out var 选择配方面板))
            {
                foreach (InputPrefabs inputPrefabs in 选择配方面板)
                {
                    var 面板标题 = inputPrefabs.TitleText;
                    面板标题.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之售货机交互扩展工具.修改字体尺寸(面板标题, 23);
                    面板标题.overflowMode = TextOverflowModes.Overflow;

                    var 取消按钮 = inputPrefabs.transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>();
                    取消按钮.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之售货机交互扩展工具.修改字体尺寸(取消按钮, 23);

                    var 搜索栏 = inputPrefabs.SearchBar;
                    搜索栏.fontAsset = 前置_资源加载器.单例.当前TMP字体;

                    var 输入框输入显示 = 搜索栏.textComponent;
                    输入框输入显示.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之售货机交互扩展工具.修改字体尺寸(输入框输入显示, 22);

                    var 输入框淡色提示 = (TMP_Text)搜索栏.placeholder;
                    输入框淡色提示.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之售货机交互扩展工具.修改字体尺寸(输入框淡色提示, 22);

                    var 配方大类标题 = inputPrefabs.ControlGroupPrefab.Title;
                    配方大类标题.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之售货机交互扩展工具.修改字体尺寸(配方大类标题, 29);

                    var 配方描述 = inputPrefabs.PrefabReference.Text;
                    配方描述.font = 前置_资源加载器.单例.当前TMP字体;
                    功能模块之售货机交互扩展工具.修改字体尺寸(配方描述, 27);

                    Log.LogMessage("售货机选择物品无需创建自定义面板");
                }
            }
        }

        public static class 功能模块之售货机交互扩展工具
        {
            public static void 修改字体尺寸(TMP_Text component, float fontSize)
            {
                if (component == null)
                {
                    Log.LogDebug($"TMP_Text是空引用,无法修改字体尺寸");
                    return;
                }

                if (component.fontSize < fontSize)
                {
                    component.fontSize = fontSize;
                    component.fontSizeMin = fontSize;
                }
                if (component.fontSizeMax < fontSize) { component.fontSizeMax = fontSize; }
            }

            public static void 修改字体尺寸(UnityEngine.UI.Text component, int fontSize)
            {
                if (component == null)
                {
                    Log.LogDebug($"Text是空引用,无法修改字体尺寸");
                    return;
                }

                if (component.fontSize < fontSize)
                {
                    component.fontSize = fontSize;
                    component.resizeTextMinSize = fontSize;
                }
                if (component.resizeTextMaxSize < fontSize) { component.resizeTextMaxSize = fontSize; }
            }
        }

    }

    [HarmonyPatch(typeof(VendingMachineRefrigerated), nameof(VendingMachineRefrigerated.InteractWith))]
    public class 冷藏售货机交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, VendingMachineRefrigerated __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机)
            {
                switch (interactable.Action)
                {
                    case InteractableType.Button1:
                    case InteractableType.Button2:
                        __result = 售货机交互.打开选择面板(__instance, interactable, interaction, 贴标机, doAction);
                        if (__result == null) { return true; }
                        return false;
                    default: break;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.InteractWith))]
    public class 售货机交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, VendingMachine __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机)
            {
                switch (interactable.Action)
                {
                    case InteractableType.Button1:
                    case InteractableType.Button2:
                        __result = 打开选择面板(__instance, interactable, interaction, 贴标机, doAction);
                        if (__result == null) { return true; }
                        return false;
                    default: break;
                }
            }
            return true;
        }

        private static Dictionary<MachineTier, List<DynamicThing>> 可选物品表 = new() { { MachineTier.TierOne, new() } };
        private static Dictionary<int, int> 可选物品与储物格编号_字典 = new();
        public static Thing.DelayedActionInstance 打开选择面板(VendingMachine 光线命中的主物体, Interactable 光线命中的控件, Interaction 交互双方, Labeller 贴标机, bool doAction)
        {
            var 消息 = new Thing.DelayedActionInstance
            { Duration = 0, ActionMessage = ActionStrings.Vend };

            if (!贴标机.IsOperable) { 消息.Fail(GameStrings.DeviceNoPower); }
            else if (!贴标机.OnOff) { 消息.Fail(GameStrings.DeviceNotOn); }
            else
            {
                消息.AppendStateMessage("单击打开售货机选择物品面板"); 消息.Succeed();

                if (doAction)
                {
                    光线命中的主物体.PlaySound(SimpleFabricatorBase.Search4BeepHash, 1f, 1f);

                    可选物品表[MachineTier.TierOne].Clear();
                    可选物品与储物格编号_字典.Clear();
                    // 售货机的0和1分别是进口和出口槽位
                    for (var i = 2; i < 光线命中的主物体.Slots.Count; i++)
                    {
                        var 可选物品 = 光线命中的主物体.Slots[i].Get();
                        if (可选物品)
                        {
                            if (!可选物品与储物格编号_字典.ContainsKey(可选物品.PrefabHash))
                            {
                                可选物品表[MachineTier.TierOne].Add(可选物品);
                                可选物品与储物格编号_字典.Add(可选物品.PrefabHash, i);
                            }
                        }
                    }

                    if (交互双方.SourceThing is Human 玩家 && 玩家.State == EntityState.Alive && 玩家.OrganBrain != null && 玩家.OrganBrain.LocalControl)
                    {
                        if (InputPrefabs.ShowInputPanel("选择售货机物品", null, 可选物品表, null))
                        {
                            // 每次打开面板,捕获售货机引用创建一个函数对象赋给InputPrefabs.OnSubmit,仿函数内容=>根据传入的物品,将<售货机.当前选择货物下标>指到该物品处
                            // 每次打开面板,复用或者创建可选择项目UI实体(Button(鼠标交互)-Image(物品缩略图)-TMP_Text(物品名称)-DynamicThing(物品)-OnClick(函数对象))
                            // 将可选择项目UI实体的OnClick(函数对象)赋给Button的OnClick(事件),仿函数内容=>将DynamicThing(物品)赋给InputPrefabs.当前选择,然后调用InputPrefabs.OnClick(方法)
                            // InputPrefabs.OnClick(方法),其方法体内容=>InputPrefabs.OnSubmit(InputPrefabs.当前选择)
                            InputPrefabs.OnSubmit += ((thing) =>
                             {
                                 var 货物所在储物格编号 = 可选物品与储物格编号_字典[thing.PrefabHash];
                                 光线命中的主物体.CurrentIndex = 货物所在储物格编号;
                             });
                        }
                    }
                }
            }

            return 消息;
        }
    }
}





