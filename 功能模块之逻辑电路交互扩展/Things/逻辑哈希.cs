using Assets.Scripts.Inventory;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using HarmonyLib;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [HarmonyPatch(typeof(LogicHashGen), nameof(LogicHashGen.InteractWith))]
    public class 逻辑哈希交互
    {
        [HarmonyPrefix]
        public static bool 交互(ref Thing.DelayedActionInstance __result, LogicHashGen __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            if (interaction.SourceSlot.Get() is Labeller 贴标机 && interactable.Action == InteractableType.Button1)
            {
                var 消息 = new Thing.DelayedActionInstance
                { Duration = 0, ActionMessage = interactable.ContextualName };

                if (!贴标机.IsOperable) { __result = 消息.Fail(GameStrings.DeviceNoPower); return false; }
                else if (!贴标机.OnOff) { __result = 消息.Fail(GameStrings.DeviceNotOn); return false; }

                消息.AppendStateMessage("单击打开选择哈希面板");
                __result = 消息.Succeed();

                if (doAction)
                {
                    __instance.ScrewSound();
                    if (InventoryManager.ParentHuman.OrganBrain.ClientId == (interaction.SourceThing as Entity)?.OrganBrain?.ClientId && InputPrefabs.ShowInputPanelAllDynamicThings("Select Thing"))
                    {
                        InputPrefabs.OnSubmit += __instance.InputFinished;
                    }
                }

                return false;
            }

            return true;
        }
    }

}


