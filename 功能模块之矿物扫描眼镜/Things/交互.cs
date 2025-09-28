using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Util;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public partial class 矿物扫描眼镜 : PowerTool
    {
        public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
        {
            // Thing交互事件
            return base.AttackWith(attack, doAction);
        }
        public override Thing.DelayedActionInstance InteractWith(Interactable interactable, Interaction interaction, bool doAction = true)
        {
            // Thing的所有控件交互事件
            return base.InteractWith(interactable, interaction, doAction);
        }

        public override void OnInteractableUpdated(Interactable interactable)
        {
            // Interactable.State属性中调用, 在此函数中写上控件状态变更后具体要干什么, 需要自己读取最新状态
            base.OnInteractableUpdated(interactable);
            if (interactable.Action == InteractableType.OnOff)
            { PlayPooledAudioSound(OnOff ? Defines.Sounds.SwitchOn : Defines.Sounds.SwitchOff, Vector3.zero); }
        }

        public override void OnEnterInventory(Thing parent)
        {
            base.OnEnterInventory(parent);
        }

        public override bool MoveToWorld(float force = 0f)
        {
            // 从槽位进入世界空间前, 设置物品与槽位所属物体这两方的所有碰撞体互相忽略碰撞 例:飞弹与发射飞弹的对象发生碰撞
            bool result = base.MoveToWorld(force);
            return result;
        }

        public override void OnExitInventory(Thing oldParent)
        {
            base.OnExitInventory(oldParent);

            var newSlot = ParentSlot;
            if (!GameManager.IsBatchMode && oldParent == InventoryManager.ParentHuman && newSlot != InventoryManager.ParentHuman.GlassesSlot)
            {
                HUD抬头显示器.显示状态 = false;
                HUD抬头显示器.单例.逐字显示.Reset();
                通用工具.变更激活状态(HUD抬头显示器.单例.gameObject, false);
            }
        }

        public override void OnChildEnterInventory(DynamicThing newChild)
        {
            base.OnChildEnterInventory(newChild);
        }

        public override void OnChildExitInventory(DynamicThing previousChild)
        {
            base.OnChildExitInventory(previousChild);
        }
    }
}