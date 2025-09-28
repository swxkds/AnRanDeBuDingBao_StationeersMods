using Assets.Scripts.Objects.Items;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.GridSystem;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public partial class 矿物扫描眼镜 : PowerTool
    {
        public override void UpdateEachFrame()
        {
            // GameManager.Update方法中的OcclusionManager.UpdatingThings.ForEach(UpdateEachFrameAction)调用了所有物体的UpdateEachFrame方法
            base.UpdateEachFrame();
            if (GameManager.IsBatchMode) { return; }
            if (GameManager.GameState == GameState.Running && InventoryManager.Parent != null && !WorldManager.IsGamePaused)
            {
                if (InventoryManager.ParentHuman != null && InventoryManager.ParentHuman == RootParentHuman && ParentSlot == InventoryManager.ParentHuman.GlassesSlot)
                {
                    var 条件A = IsOperable;
                    var 能源矿物 = 能源矿物槽位.Get();
                    var 条件B = 能源矿物 && 能源矿物.PrefabHash == 能源矿物ID && ((Ore)能源矿物).Quantity > 0;

                    if ((条件A || 条件B) && InteractOnOff.State == 1)
                    {
                        HUD抬头显示器.显示状态 = true;
                        通用工具.变更激活状态(HUD抬头显示器.单例.gameObject, true);
                        HUD抬头显示器.单例.文本更新.Invoke(null);
                    }
                    else
                    {
                        HUD抬头显示器.显示状态 = false;
                        HUD抬头显示器.单例.逐字显示.Reset();
                        通用工具.变更激活状态(HUD抬头显示器.单例.gameObject, false);
                    }
                }
            }
        }

        public override void CheckPower()
        {
            if (!GameManager.RunSimulation)
            {
                return;
            }

            var 条件A = IsOperable;
            var 能源矿物 = 能源矿物槽位.Get();
            var 条件B = 能源矿物 && 能源矿物.PrefabHash == 能源矿物ID && ((Ore)能源矿物).Quantity > 0;

            if ((条件A || 条件B) && OnOff)
            {
                // 当前没有供电时, 变更为供电状态
                if (!Powered)
                {
                    OnServer.Interact(base.InteractPowered, 1);
                }
            }
            else if (Powered)       // 没有电池供电, 也没有能源矿物供电, 并且电源开关已经打开时, 变更为无供电状态
            {
                OnServer.Interact(base.InteractPowered, 0);
            }

            _checkPower = false;        // 锁, 在此处解锁表示已经检查完成
        }

        public override void OnPowerTick()
        {
            // 这个函数会在每帧调用一次, 需要自己在函数中添加逻辑
            if (!OnOff || !Powered) { return; }

            var 能源矿物 = 能源矿物槽位.Get();

            var 条件A = IsOperable;
            var 条件B = 能源矿物 && 能源矿物.PrefabHash == 能源矿物ID && ((Ore)能源矿物).Quantity > 0;
            var 条件C = OnOff && Powered;

            if (条件A && 条件B && 条件C)
            {
                能源矿物消耗计时 += GameManager.DeltaTime;
                if (能源矿物消耗计时 >= 2f)
                {
                    能源矿物消耗计时 = 0;
                    ((Ore)能源矿物).RemoveQuantity(1);
                }
                UsedPowerPassive = 使用能源矿物时开机电量消耗;
                base.OnPowerTick();
                return;
            }

            if (条件A && 条件C)
            {
                UsedPowerPassive = 使用电池时开机电量消耗;
                base.OnPowerTick();
                return;
            }

            if (条件B && 条件C)
            {
                能源矿物消耗计时 += GameManager.DeltaTime;
                if (能源矿物消耗计时 >= 0.3f)
                {
                    能源矿物消耗计时 = 0;
                    ((Ore)能源矿物).RemoveQuantity(1);
                }
                return;
            }
        }
    }
}
