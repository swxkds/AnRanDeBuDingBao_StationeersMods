using Assets.Scripts.Objects;
using HarmonyLib;
using BepInEx;
using Assets.Scripts.UI;
using UnityEngine;
using Assets.Scripts.Objects.Items;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.GridSystem;
using TerrainSystem;
using Assets.Scripts.Util;
using Assets.Scripts.Objects.Entities;
using UnityEngine.Rendering;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_gongneng_mokuai_zhi_kuangwusaomiaoyanjing", "功能模块之矿物扫描眼镜", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 功能模块之矿物扫描眼镜 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("功能模块之矿物扫描眼镜加载完成!");
            补丁 = new Harmony("功能模块之矿物扫描眼镜");
            补丁.PatchAll();

        }
    }

    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
    public class 将矿物扫描眼镜添加到游戏中
    {
        [HarmonyPrefix]
        public static void 执行()
        {
            var __ = 功能模块之矿物扫描眼镜_资源加载器.单例;

            {
                var 网格 = __.矿物扫描眼镜.GetComponent<MeshFilter>();  // MeshFilter组件从建模中提取了,无需添加
                var 蓝图网格 = __.矿物扫描眼镜_蓝图.GetComponent<MeshFilter>(); // MeshFilter组件从建模中提取了,无需添加

                // 由于网格是由预制体系统加载,不修改包围盒,因此必须先重建一下包围盒,这样其它组件才能从网格组件获取到正确的包围盒
                网格.sharedMesh.RecalculateBounds();
                蓝图网格.sharedMesh.RecalculateBounds();

                var 渲染 = __.矿物扫描眼镜.AddComponent<MeshRenderer>();
                var 碰撞 = __.矿物扫描眼镜.AddComponent<BoxCollider>();
                var 物理 = __.矿物扫描眼镜.AddComponent<Rigidbody>();

                {
                    // 着色器算法 => 从材质.mainTexture中采样,填充背景
                    var 着色器 = Shader.Find("Custom/StandardTextureArray");
                    var 材质 = new Material(着色器);
                    材质.mainTexture = __.贴图集合;         // 从贴图中采样
                    材质.color = Color.clear;               // 不需要额外滤镜颜色混叠
                    渲染.sharedMaterial = 材质;
                }

                {
                    var 镜片 = __.矿物扫描眼镜.transform.GetChild(0).gameObject;
                    var 镜片渲染 = 镜片.AddComponent<MeshRenderer>();

                    // 着色器算法 => 用<材质.color>的颜色填充整个背景,并混叠居于下一层的像素
                    var 着色器 = Shader.Find("Custom/Stationeers Transparent");
                    var 材质 = new Material(着色器);
                    材质.mainTexture = null;            // 镜片是一个半透明纯色,不需要从贴图中采样
                    材质.color = Color.blue.SetAlpha(0.05f);    // 半透明滤镜颜色
                    镜片渲染.sharedMaterial = 材质;
                }

                var thing = __.矿物扫描眼镜.AddComponent<矿物扫描眼镜>();
                thing.PrefabName = "ItemMineralScanningGlasses";
                thing.PrefabHash = Animator.StringToHash(thing.PrefabName);
                thing.ThingTransform = thing.transform;
                thing.Thumbnail = __.缩略图;
                thing.RigidBody = 物理;
                thing.SlotType = Slot.Class.Glasses;
                thing.SlotWearAction = SlotWearAction.HidePlayer;
                thing.Blueprint = __.矿物扫描眼镜_蓝图;
                thing.Wireframe = thing.Blueprint.AddComponent<Wireframe>();    // Wireframe.OnRenderObject方法中绘制线框
                thing.Wireframe.BlueprintTransform = thing.Blueprint.transform;
                thing.Wireframe.BlueprintMeshFilter = 蓝图网格;
                thing.Wireframe.BlueprintRenderer = thing.Blueprint.AddComponent<MeshRenderer>();

                {
                    // 着色器算法 => 用<材质.color>的颜色填充整个背景, 在Wireframe.OnRenderObject方法中绘制线框
                    var 着色器 = Shader.Find("Custom/Hologram");
                    var 材质 = new Material(着色器);
                    材质.mainTexture = null;
                    材质.color = Color.green.SetAlpha(0.2f);
                    thing.Wireframe.BlueprintRenderer.sharedMaterial = 材质;
                }

                // 线框图生成器需要保存了网格顶点表和三角形连线表的没有优化过的网格
                var 线框图生成器 = new WireframeGenerator(thing.Wireframe.BlueprintTransform);
                thing.Wireframe.BlueprintMeshFilter.sharedMesh = 线框图生成器.CombinedMesh;

                thing.Wireframe.WireframeEdges = 线框图生成器.Edges;
                thing.Wireframe.ShowTransformArrow = true;

                {
                    // 槽位_电池.NewType1 => 电池槽不用配置
                    // 槽位_电池.NewType2 => 电池槽不用配置
                    // 槽位_电池.Display => 不用配置,面板会读取thing.Slots然后分配按钮
                    // 槽位_电池._dynamicThing => 不用配置,这是由<物品进入槽位><物品离开槽位>事件自动赋值的
                    // 槽位_电池._index => 保持默认的-1,在初始化时自动查找槽位在thing.Slots的下标
                    // 槽位_电池.OnOccupantChange => 不用配置,槽位不需要额外事件,背包事件总线有通用的OnOccupantChange
                    // 槽位_电池.OnPlayerInventoryWindow => 不用配置,槽位不需要额外事件,背包事件总线有通用的OnPlayerInventoryWindow
                    // 槽位_电池.OnEnter => 不用配置,槽位不需要额外事件,背包事件总线有通用的OnEnter
                    // 槽位_电池.OnExit => 不用配置,槽位不需要额外事件,背包事件总线有通用的OnExit

                    var 槽位_电池 = new Slot();
                    槽位_电池.StringKey = "Battery";
                    槽位_电池.StringHash = Animator.StringToHash(槽位_电池.StringKey);
                    槽位_电池.Type = Slot.Class.Battery;
                    槽位_电池.SpecificTypePrefabHash = -1;
                    槽位_电池.Location = thing.ThingTransform;
                    槽位_电池.Parent = thing;
                    槽位_电池.EntityControlMode = MovementController.Mode.Seated;
                    槽位_电池.UseInternalAtmosphere = false;
                    槽位_电池.RealWorldScale = true;
                    槽位_电池.ScaleMultiplier = 1;
                    槽位_电池.OccupantCastsShadows = true;
                    槽位_电池.HidesOccupant = true;
                    槽位_电池.IsHiddenInSeat = false;
                    槽位_电池.IsInteractable = true;
                    槽位_电池.IsSwappable = true;
                    槽位_电池.AllowDragging = false;
                    槽位_电池.IsLocked = false;
                    槽位_电池.Action = InteractableType.Slot1;
                    槽位_电池.Interactable = null;
                    槽位_电池.SlotTypeIcon = Slot.GetSlotTypeSprite(槽位_电池.Type);   // 槽位背景文字,电池槽会显示电池两字
                    槽位_电池.Collider = 碰撞;          // Unity碰撞发生时,在Thing的<碰撞与槽位字典中>以碰撞体作为Key找到槽位
                    槽位_电池.Size = default;
                    槽位_电池.OccupantAlwaysVisible = false;

                    thing.Slots = [槽位_电池];
                }
                {
                    // 已供电.Display => 不用配置,面板会读取thing.Interactables然后分配按钮
                    // 已供电.AssociatedAudioEvents = ;  // 待定
                    // 已供电._hasAnimator => 不用配置,在初始化时自动赋值<_hasAnimator = Animator != null;>
                    // 已供电._propertyId  => 不用配置,<_propertyId = Animator.StringToHash(Action.ToString());>自动赋值
                    // 已供电._index => 保持默认的-1,在初始化时自动查找槽位在thing.Interactables的下标
                    // 已供电._updateSoundEffects => 不用配置,这是一个音效脏标记
                    // 已供电._state => 不用配置,这是一个动画脏标记
                    // 已供电._isDirty => 不用配置,这是一个脏标记

                    var 已供电 = new Interactable();
                    已供电.StringKey = "Powered";
                    已供电.StringHash = Animator.StringToHash(已供电.StringKey);
                    已供电.Parent = thing;
                    已供电.Collider = null;
                    已供电.FakeCollider = null;
                    已供电.Bounds = new(Vector3.zero, Vector3.zero);
                    已供电.Action = InteractableType.Powered;
                    已供电.Animator = null;
                    已供电.JoinInProgressSync = true;
                    已供电.Layer = 0;
                    已供电.CanKeyInteract = false;      // 不需要分配UI按钮,这个控件的状态值由<电池进入和离开电池槽位>事件来变更的
                    已供电.KeyMap = string.Empty;
                    已供电.Slot = null;
                    已供电.ActionName = string.Empty;
                    已供电.OriginalBounds = new(Vector3.zero, Vector3.zero);

                    var 开_关 = new Interactable();
                    开_关.StringKey = "OnOff";
                    开_关.StringHash = Animator.StringToHash(开_关.StringKey);
                    开_关.Parent = thing;
                    开_关.Collider = null;
                    开_关.FakeCollider = null;
                    开_关.Bounds = new(Vector3.zero, Vector3.zero);
                    开_关.Action = InteractableType.OnOff;
                    开_关.Animator = null;
                    开_关.JoinInProgressSync = true;
                    开_关.Layer = 0;
                    开_关.CanKeyInteract = true;        // 分配UI按钮用来切换状态值(开和关)
                    开_关.KeyMap = string.Empty;
                    开_关.Slot = null;
                    开_关.ActionName = string.Empty;
                    开_关.OriginalBounds = new(Vector3.zero, Vector3.zero);

                    thing.Interactables = [已供电, 开_关];
                }

                WorldManager.Instance.SourcePrefabs.Add(thing);
            }
        }

    }
    public class 矿物扫描眼镜 : PowerTool
    {
        // private static MinableDrawCallCollection 双缓冲A;
        // private static MinableDrawCallCollection 双缓冲B;
        // private static bool A是当前写入么;
        // private static object 锁 = new object();
        // public static MinableDrawCallCollection Write
        // {
        //     get
        //     {
        //         lock (锁)
        //         {
        //             return A是当前写入么 ? 双缓冲A : 双缓冲B;
        //         }
        //     }
        // }

        // public static MinableDrawCallCollection Read
        // {
        //     get
        //     {
        //         lock (锁)
        //         {
        //             return A是当前写入么 ? 双缓冲B : 双缓冲A;
        //         }
        //     }
        // }
        public override bool IsBurnable
        {
            get
            {
                if (ParentSlot?.Parent is Human human)
                {
                    var gasMask = human.HelmetSlot.Get<GasMask>();
                    if (gasMask != null && !gasMask.IsOpen)
                    {
                        return false;
                    }
                }

                return base.IsBurnable;
            }
        }

        public float 开机每Tick用电量;
        public override void OnPowerTick()
        {
            base.OnPowerTick();
            if (OnOff && Powered && (object)Battery != null)
            {
                Battery.PowerStored -= 开机每Tick用电量;
            }
        }

        public override void OnInteractableUpdated(Interactable interactable)
        {
            // 一个函数对象绑定在Button组件的点击事件上,当点击后,将捕获的Interactable传入并调用本函数
            base.OnInteractableUpdated(interactable);
            if (interactable.Action == InteractableType.OnOff)
            {
                PlayPooledAudioSound(OnOff ? Defines.Sounds.SwitchOn : Defines.Sounds.SwitchOff, Vector3.zero);
            }
        }

        public void Render()
        {
            if (!GameManager.IsBatchMode)
            {
                VoxelTerrain.Read.RenderVisualisers();
            }
        }

        public override void UpdateEachFrame()
        {
            // GameManager.Update方法中的OcclusionManager.UpdatingThings.ForEach(UpdateEachFrameAction)调用了所有物体的UpdateEachFrame方法
            if (GameManager.IsBatchMode)
            {
                return;
            }

            base.UpdateEachFrame();
            if (GameManager.GameState == GameState.Running && !(InventoryManager.ParentHuman == null) && !(InventoryManager.ParentHuman != RootParentHuman) && ParentSlot == InventoryManager.ParentHuman.GlassesSlot)
            {
                if (IsOperable && InteractOnOff.State == 1)
                {
                    CameraController.Instance.IsSensorLensesFxActive = true;
                    Render();
                }
                else
                {
                    CameraController.Instance.IsSensorLensesFxActive = false;
                }
            }
        }

        public override void OnChildEnterInventory(DynamicThing child)
        {
            base.OnChildEnterInventory(child);
        }

        public override void OnChildExitInventory(DynamicThing previousChild)
        {
            base.OnChildExitInventory(previousChild);
        }

        public override string GetStationpediaCategory()
        {
            return Localization.GetInterface(StationpediaCategoryStrings.PersonalEyeWear);
        }

        public override void OnExitInventory(Thing oldParent)
        {
            if (!GameManager.IsBatchMode && oldParent == InventoryManager.ParentHuman && ParentSlot != InventoryManager.ParentHuman.GlassesSlot)
            {
                CameraController.Instance.IsSensorLensesFxActive = false;
            }

            base.OnExitInventory(oldParent);
        }
    }
}