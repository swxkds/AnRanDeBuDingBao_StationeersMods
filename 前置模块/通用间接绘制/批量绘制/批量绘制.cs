using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts;
using Assets.Scripts.Util;
using TerrainSystem;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Voxel;
using Assets.Scripts.Objects.Entities;
using System.Reflection;
using Assets.Scripts.Inventory;
using Assets.Scripts.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    using 可开采资源网格 = TerrainSystem.Vein;
    public abstract class 单图层_多物体_批量绘制
    {
        public static readonly FieldInfo FieldInfo_constructionCursors = typeof(InventoryManager).GetField("_constructionCursors", BindingFlags.NonPublic | BindingFlags.Static);
        public static readonly FieldInfo FieldInfo_dynamicThingCursors = typeof(InventoryManager).GetField("_dynamicThingCursors", BindingFlags.NonPublic | BindingFlags.Static);

        public Dictionary<long, 批量绘制_API兼容层> 所有批量绘制_索引视图 { get; private set; } // 每个新物体都需要创建持有自己的建模网格和材质的<批量绘制_API兼容层>
        public List<批量绘制_API兼容层> 所有批量绘制_遍历视图 { get; private set; }
        public int Field_图层优先级 = 0;
        public Stack<(ComputeBuffer 所有矩阵, ComputeBuffer 网格采样参数)> 间接绘制参数池 { get; private set; }
        public bool 正在创建间接绘制参数 { get; private set; }
        public Func<long, 单图层_多物体_批量绘制, 批量绘制_API兼容层> 创建新渲染器事件 { get; protected set; }
        public virtual bool 是否需要间接绘制参数池 { get { return false; } }

        public 单图层_多物体_批量绘制(int Arg_图层优先级)
        {
            所有批量绘制_索引视图 = new();
            所有批量绘制_遍历视图 = new();
            Field_图层优先级 = Arg_图层优先级;
            间接绘制参数池 = new();
            正在创建间接绘制参数 = false;
        }

        public void 扩容间接绘制参数池()
        {
            unsafe
            {
                var 默认池尺寸 = 20;

                for (var i = 0; i < 默认池尺寸; i++)
                {
                    间接绘制参数池.Push((new(批量绘制_API兼容层_DrawMeshInstancedIndirect.批量绘制最大支持, sizeof(批量绘制_API兼容层_DrawMeshInstancedIndirect.变换矩阵)), new(1, sizeof(uint) * 批量绘制_API兼容层_DrawMeshInstancedIndirect.Field_子网格索引配置尺寸, ComputeBufferType.IndirectArguments)));
                }

                正在创建间接绘制参数 = false;

                前置模块.Log.LogMessage($"已扩容间接绘制参数池一次");
            }
        }

        public virtual void Initialize()
        {
            创建新渲染器事件 = static (Arg_ID, Arg_this) =>
            {
                var thing = Thing.Find(Arg_ID);
                if (thing == null) { return null; }

                Mesh 匹配网格 = null;
                Material 匹配材质 = null;

                switch (thing)
                {
                    case Structure:
                        {
                            var 所有建筑蓝图 = (Dictionary<string, Structure>)FieldInfo_constructionCursors.GetValue(null);
                            if (所有建筑蓝图.TryGetValue(thing.name, out var __))
                            {
                                var A = __.Wireframe;
                                匹配网格 = A.BlueprintMeshFilter.sharedMesh;
                            }
                            break;
                        }
                    case DynamicThing:
                        {
                            // 玩家继承Entity类, Entity类继承DynamicThing类
                            var 所有道具蓝图 = (Dictionary<string, GameObject>)FieldInfo_dynamicThingCursors.GetValue(null);
                            if (所有道具蓝图.TryGetValue(thing.name, out var __))
                            {
                                var A = __.GetComponent<Wireframe>();
                                匹配网格 = A.BlueprintMeshFilter.sharedMesh;
                            }
                            break;
                        }
                }

                if (匹配网格 == null) { 匹配网格 = thing.Renderers.Last().SharedMesh; }
                if (匹配材质 == null) { 匹配材质 = 通用工具.材质_高亮全息投影; }

                if (匹配网格 == null || 匹配材质 == null) { return null; }

                return new 批量绘制_API兼容层_DrawMeshInstanced(匹配网格, 匹配材质, 0); ;
            };
        }
        public virtual void Dispose()
        {
            foreach (var __ in 所有批量绘制_遍历视图)
            {
                __.Dispose();
            }

            所有批量绘制_索引视图.Clear();
            所有批量绘制_索引视图 = null;

            所有批量绘制_遍历视图.Clear();
            所有批量绘制_遍历视图 = null;

            while (间接绘制参数池.Count > 0)
            {
                var __ = 间接绘制参数池.Pop();
                __.所有矩阵.Release();
                __.网格采样参数.Release();
            }
        }

        public virtual void Clear()
        {
            foreach (var __ in 所有批量绘制_遍历视图)
            {
                __.Clear();
            }
        }

        public void Render(Plane[] Arg_几何面)
        {
            foreach (var __ in 所有批量绘制_遍历视图)
            {
                if (GeometryUtility.TestPlanesAABB(Arg_几何面, __.视口裁剪))   // 摄像机视口裁剪,完全不可见的图层直接跳过
                {
                    __.Render();
                }
            }

            // 前置模块.Log.LogMessage($"绘制包围盒 => 中心点: {Field_视口裁剪.center}  尺寸: {Field_视口裁剪.size}");
        }

        public 批量绘制_API兼容层 获取渲染器(long Arg_ID)
        {
            if (所有批量绘制_索引视图.TryGetValue(Arg_ID, out var Old))
            {
                return Old;
            }
            else
            {
                if (是否需要间接绘制参数池 && 间接绘制参数池.Count <= 0)
                {
                    // 某些Unity资源的创建只能在Unity引擎主线程进行
                    if (正在创建间接绘制参数) { 前置模块.Log.LogMessage($"请稍等! 正在创建间接绘制参数1"); return null; }
                    正在创建间接绘制参数 = true;
                    前置模块.Log.LogMessage($"请稍等! 正在创建间接绘制参数2");
                    return null;
                }

                var 新渲染器 = 创建新渲染器事件(Arg_ID, this);
                if (新渲染器 == null) { return null; }

                所有批量绘制_索引视图[Arg_ID] = 新渲染器;
                var New = 所有批量绘制_索引视图[Arg_ID];
                所有批量绘制_遍历视图.Add(New);

                return New;
            }
        }

        public abstract void 扫描并添加矩阵();
    }

    public class 单图层_多物体_批量绘制_房间闭合检测特化版本 : 单图层_多物体_批量绘制
    {
        private HashSet<Grid3> Field_已入队网格 = null;
        private List<(Structure, string)> Field_六面以及中心 = null;
        private HashSet<long> Field_所有物体 = null;
        public int Field_探测步数 = 批量绘制_API兼容层_DrawMeshInstancedIndirect.批量绘制最大支持;          // 最多处理N个相邻网格, 避免无限扫描
        public override bool 是否需要间接绘制参数池 { get { return true; } }
        public 单图层_多物体_批量绘制_房间闭合检测特化版本(int Arg_图层优先级) : base(Arg_图层优先级)
        {
            Field_已入队网格 = new();
            Field_六面以及中心 = new(7);
            Field_所有物体 = new();
        }

        public override void Initialize()
        {
            创建新渲染器事件 = static (Arg_ID, Arg_this) =>
            {
                var thing = Thing.Find(Arg_ID);
                if (thing == null) { return null; }

                Mesh 匹配网格 = null;
                Material 匹配材质 = null;

                switch (thing)
                {
                    case Structure:
                        {
                            var 所有建筑蓝图 = (Dictionary<string, Structure>)FieldInfo_constructionCursors.GetValue(null);
                            if (所有建筑蓝图.TryGetValue(thing.name, out var __))
                            {
                                var A = __.Wireframe;
                                匹配网格 = A.BlueprintMeshFilter.sharedMesh;
                            }
                            break;
                        }
                    case DynamicThing:
                        {
                            // 玩家继承Entity类, Entity类继承DynamicThing类
                            var 所有道具蓝图 = (Dictionary<string, GameObject>)FieldInfo_dynamicThingCursors.GetValue(null);
                            if (所有道具蓝图.TryGetValue(thing.name, out var __))
                            {
                                var A = __.GetComponent<Wireframe>();
                                匹配网格 = A.BlueprintMeshFilter.sharedMesh;
                            }
                            break;
                        }
                }

                if (匹配网格 == null) { 匹配网格 = thing.Renderers.Last().SharedMesh; }
                if (匹配材质 == null) { 匹配材质 = 通用工具.材质_安然_高亮全息投影_扫描线; }

                if (匹配网格 == null || 匹配材质 == null) { return null; }

                return new 批量绘制_API兼容层_DrawMeshInstancedIndirect((Arg_this, Arg_this.间接绘制参数池.Pop()), 匹配网格, 匹配材质, 0);
            };
        }

        public override void Dispose()
        {
            base.Dispose();

            Field_已入队网格.Clear();
            Field_已入队网格 = null;

            Field_六面以及中心.Clear();
            Field_六面以及中心 = null;

            Field_所有物体.Clear();
            Field_所有物体 = null;
        }

        public override void Clear()
        {
            Clear_渲染信息();
            Clear_网格信息();
        }

        public void Clear_渲染信息()
        {
            base.Clear();
        }

        public void Clear_网格信息()
        {
            Field_已入队网格.Clear();
            Field_六面以及中心.Clear();
            Field_所有物体.Clear();
        }

        public override void 扫描并添加矩阵()
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            // 因此先清除上次的扫描结果, 这样多个函数对象都写入时, 最后一个写入的会覆盖掉前面的
            Clear_渲染信息();

            var 玩家所在房间 = RoomController.World.GetRoom(Human.LocalHuman.CenterPosition);   // WorldGrid内置了很多对齐算法, 分别对齐到 网格单元、细分小网格

            if (玩家所在房间 == null) { return; }       // 如果人物在室外, 不需要添加矩阵, 因此矩阵=0无需渲染

            Clear_网格信息();
            Span<Grid3> span = stackalloc Grid3[7];

            foreach (var 当前 in 玩家所在房间.Grids)
            {
                if (!Field_已入队网格.Contains(当前))
                {
                    Field_已入队网格.Add(当前);
                }

                // 获取当前网格的东/南/西/北/上/下/这六个相邻网格
                int count = 0;
                GridController.PopulateGridNeighbours(span, ref count, 当前);
                Span<Grid3> span3 = span.Slice(0, count);
                for (int i = 0; i < span3.Length; i++)
                {
                    Grid3 相邻 = span3[i];
                    if (!Field_已入队网格.Contains(相邻))
                    {
                        Field_已入队网格.Add(相邻);
                    }
                }

                // 前置模块.Log.LogMessage($"已处理网格计数: {已处理网格.Count}");

                foreach (Grid3 grid in Field_已入队网格)
                {
                    var 网格单元 = GridController.World.GetCell(grid);      // Cell: 网格单元, 和框架一样的大小, 网格单元持有内部所有的可放置设备(墙、框架、门.....)
                    if (网格单元 == null) { continue; }
                    {
                        Field_六面以及中心.Clear();

                        var 中 = 网格单元.Lookup[StructureElement.Center];
                        if (中) Field_六面以及中心.Add((中, "中"));

                        var 上 = 网格单元.Lookup[StructureElement.Up];
                        if (上) Field_六面以及中心.Add((上, "上"));

                        var 下 = 网格单元.Lookup[StructureElement.Down];
                        if (下) Field_六面以及中心.Add((下, "下"));

                        var 西 = 网格单元.Lookup[StructureElement.West];
                        if (西) Field_六面以及中心.Add((西, "西"));

                        var 东 = 网格单元.Lookup[StructureElement.East];
                        if (东) Field_六面以及中心.Add((东, "东"));

                        var 北 = 网格单元.Lookup[StructureElement.North];
                        if (北) Field_六面以及中心.Add((北, "北"));

                        var 南 = 网格单元.Lookup[StructureElement.South];
                        if (南) Field_六面以及中心.Add((南, "南"));
                    }

                    {
                        foreach (var (建筑, __) in Field_六面以及中心)
                        {
                            if (!建筑.CanAirPass) { continue; }    // BuildState.BlockAir: 气体密封性

                            if (!Field_所有物体.Contains(建筑.ReferenceId))
                            {
                                Field_所有物体.Add(建筑.ReferenceId);
                                var 渲染器 = 获取渲染器(建筑.ReferenceId);

                                if (渲染器 == null) { continue; }
                                if (渲染器.所有矩阵计数 > 批量绘制_API兼容层_DrawMeshInstancedIndirect.批量绘制最大支持) { continue; }

                                var 世界包围盒 = 建筑.CurrentBuildState.Visualizer.bounds;

                                if (渲染器.所有矩阵计数 == 0)
                                {
                                    渲染器.视口裁剪 = 世界包围盒;
                                }
                                else
                                {
                                    var 复制体 = 渲染器.视口裁剪;
                                    复制体.Encapsulate(世界包围盒);
                                    渲染器.视口裁剪 = 复制体;
                                }

                                渲染器.添加矩阵(建筑.GetBatchMatrix());
                                // var 变换矩阵 = 建筑.GetBatchMatrix();
                                // 渲染器.添加矩阵(Matrix4x4.TRS(变换矩阵.GetPosition(), Quaternion.Euler(0, 45, 0), new Vector3(0.1f, 0.3f, 0.1f)));
                                // 前置模块.Log.LogWarning($"当前位置:{变换矩阵.GetPosition()}  当前旋转:{Quaternion.Euler(0, 45, 0)}  当前缩放:{new Vector3(0.1f, 0.3f, 0.1f)}");
                            }
                        }
                    }
                }
            }
        }

    }
    public class 单图层_多物体_批量绘制_可开采资源检测特化版本 : 单图层_多物体_批量绘制
    {
        public static readonly float 探测角度 = Mathf.Cos(15f * Mathf.Deg2Rad);
        public static readonly FieldInfo FieldInfo_minables = typeof(可开采资源网格).GetField("_minables", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly Vector3 MinableRenderScale = new Vector3(0.85f, 0.85f, 0.85f);
        public BoundsInt Field_探测范围 = default;      // 以玩家位置为中心点的一个立体范围, 每次扫描前重新赋值
        private const bool Field_显示地下资源 = true;
        private HashSet<可开采资源网格> Field_所有网格 = null;
        public override bool 是否需要间接绘制参数池 { get { return true; } }

        public 单图层_多物体_批量绘制_可开采资源检测特化版本(int Arg_图层优先级) : base(Arg_图层优先级) { Field_所有网格 = new(); }

        public override void Initialize()
        {
            创建新渲染器事件 = static (Arg_ID, Arg_this) =>
          {
              if (VoxelTerrain.GetMineableInfo((MinableType)Arg_ID, out var info) && MinableVisualiserData.MinableVisualizers.TryGetValue(info.MinableType, out var value))
              {
                  var 匹配网格 = value.Mesh;
                  var 匹配材质 = 通用工具.创建材质_高亮矿物();
                  匹配材质.SetColor(通用工具.着色器参数_Color, value.ColorReference); // 发光颜色
                  return new 批量绘制_API兼容层_DrawMeshInstancedIndirect((Arg_this, Arg_this.间接绘制参数池.Pop()), 匹配网格, 匹配材质, 0);
              }
              else
              {
                  return null;
              }
          };
        }
        public override void Dispose()
        {
            base.Dispose();
            Field_所有网格.Clear();
            Field_所有网格 = null;
        }

        public override void Clear()
        {
            base.Clear();
            Field_所有网格.Clear();
        }

        public override void 扫描并添加矩阵()
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            // 因此先清除上次的扫描结果, 这样多个函数对象都写入时, 最后一个写入的会覆盖掉前面的
            Clear();

            var 探测范围 = new Bounds(Field_探测范围.center, Field_探测范围.size);
            var 摄像机前向 = CameraController.Instance.MainCameraForward;
            var 摄像机坐标 = CameraController.CameraPosition;

            var min = VoxelTerrain.WorldToOctreeSpaceClamped(Field_探测范围.min);
            var max = VoxelTerrain.WorldToOctreeSpaceClamped(Field_探测范围.max);
            min /= 32;
            min *= 32;
            max /= 32;
            max *= 32;

            for (var i = min.x; i <= max.x; i += 32)
            {
                for (var j = min.y; j <= max.y; j += 32)
                {
                    for (var k = min.z; k <= max.z; k += 32)
                    {
                        if (可开采资源网格.VeinsLookup.TryGetValue(new(i, j, k), out var 区域资源网格图层))
                        {
                            foreach (var 资源网格 in 区域资源网格图层)
                            {
                                var 所有资源 = (Minable[])FieldInfo_minables.GetValue(资源网格);
                                if (所有资源 == null || 所有资源.Length <= 0) { continue; }

                                var 世界包围盒 = new Bounds(资源网格.VeinBounds.center, 资源网格.VeinBounds.size);

                                if (!Field_所有网格.Contains(资源网格) && 探测范围.Intersects(世界包围盒))
                                {
                                    Field_所有网格.Add(资源网格);

                                    var 渲染器 = 获取渲染器((long)资源网格.Type);

                                    if (渲染器 == null) { continue; }
                                    if (渲染器.所有矩阵计数 > 批量绘制_API兼容层_DrawMeshInstancedIndirect.批量绘制最大支持) { continue; }

                                    if (渲染器.所有矩阵计数 == 0)
                                    {
                                        渲染器.视口裁剪 = 世界包围盒;
                                    }
                                    else
                                    {
                                        var 复制体 = 渲染器.视口裁剪;
                                        复制体.Encapsulate(世界包围盒);
                                        渲染器.视口裁剪 = 复制体;
                                    }

                                    foreach (var 资源 in 所有资源)
                                    {
                                        if (资源.IsActive && (Field_显示地下资源 || 资源网格.IsMinableVisible(资源)))
                                        {
                                            var 坐标 = 资源.WorldRenderPosition(资源网格.VeinWorldPosition) + VoxelConstants.TerrainMeshOffset;

                                            // 先计算出相对于摄像机的局部坐标,然后缩小为长度=1的单位向量,此时和摄像机的<方向单位向量>进行点乘,点乘结果越大,夹角越小
                                            // 负1(夹角=180) 至 钝角 至 0(夹角=90) 至 锐角 至 1(夹角=0)
                                            // 点乘的几何原理 => https://www.bilibili.com/video/BV1jG411L7kd/?spm_id_from=333.788.top_right_bar_window_history.content.click&vd_source=626c78ce6f1c0ca7a32ba497debdfc7c
                                            var 余弦 = Vector3.Dot(摄像机前向, (坐标 - 摄像机坐标).normalized);   // 使用单位向量时, 点乘的结果 = 两向量之间的夹角余弦
                                            if (余弦 >= 探测角度)
                                            {
                                                渲染器.添加矩阵(Matrix4x4.TRS(坐标, 资源.Rotation, MinableRenderScale));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }

    public class 单图层_多物体_批量绘制_通用渲染特化版本 : 单图层_多物体_批量绘制
    {
        private Func<List<Thing>> 获取渲染物体 = null;
        public override bool 是否需要间接绘制参数池 { get { return true; } }
        public 单图层_多物体_批量绘制_通用渲染特化版本(int Arg_图层优先级, Func<List<Thing>> Arg_获取渲染物体) : base(Arg_图层优先级) { 获取渲染物体 = Arg_获取渲染物体; }

        public override void Initialize()
        {
            创建新渲染器事件 = static (Arg_ID, Arg_this) =>
            {
                var thing = Thing.Find(Arg_ID);
                if (thing == null) { return null; }

                Mesh 匹配网格 = null;
                Material 匹配材质 = null;

                switch (thing)
                {
                    case Structure:
                        {
                            var 所有建筑蓝图 = (Dictionary<string, Structure>)FieldInfo_constructionCursors.GetValue(null);
                            if (所有建筑蓝图.TryGetValue(thing.name, out var __))
                            {
                                var A = __.Wireframe;
                                匹配网格 = A.BlueprintMeshFilter.sharedMesh;
                            }
                            break;
                        }
                    case DynamicThing:
                        {
                            // 玩家继承Entity类, Entity类继承DynamicThing类
                            var 所有道具蓝图 = (Dictionary<string, GameObject>)FieldInfo_dynamicThingCursors.GetValue(null);
                            if (所有道具蓝图.TryGetValue(thing.name, out var __))
                            {
                                var A = __.GetComponent<Wireframe>();
                                匹配网格 = A.BlueprintMeshFilter.sharedMesh;
                            }
                            break;
                        }
                }

                if (匹配网格 == null) { 匹配网格 = thing.Renderers.Last().SharedMesh; }
                if (匹配材质 == null) { 匹配材质 = 通用工具.材质_安然_高亮全息投影_扫描线; }

                if (匹配网格 == null || 匹配材质 == null) { return null; }

                return new 批量绘制_API兼容层_DrawMeshInstancedIndirect((Arg_this, Arg_this.间接绘制参数池.Pop()), 匹配网格, 匹配材质, 0);
            };
        }
        public override void 扫描并添加矩阵()
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            // 因此先清除上次的扫描结果, 这样多个函数对象都写入时, 最后一个写入的会覆盖掉前面的
            Clear();

            var 所有渲染物体 = 获取渲染物体?.Invoke();
            if (所有渲染物体 == null) { return; }

            foreach (var thing in 所有渲染物体)
            {
                var 渲染器 = 获取渲染器(thing.ReferenceId);

                if (渲染器 == null) { continue; }
                if (渲染器.所有矩阵计数 > 批量绘制_API兼容层_DrawMeshInstancedIndirect.批量绘制最大支持) { continue; }

                var 世界包围盒 = new Bounds(thing.CenterPosition, thing.GetLocalBounds.size);

                if (渲染器.所有矩阵计数 == 0)
                {
                    渲染器.视口裁剪 = 世界包围盒;
                }
                else
                {
                    var 复制体 = 渲染器.视口裁剪;
                    复制体.Encapsulate(世界包围盒);
                    渲染器.视口裁剪 = 复制体;
                }

                switch (thing)
                {
                    case Structure 建筑:
                        {
                            渲染器.添加矩阵(建筑.GetBatchMatrix());
                            break;
                        }
                    case DynamicThing 道具:
                        {
                            渲染器.添加矩阵(道具.Renderers.Last().GetRendererTransform().localToWorldMatrix);
                            break;
                        }
                }
            }
        }
    }






    public class 多图层_多物体_批量绘制
    {
        public enum 图层类型
        {
            未知, 房间, 可开采资源, 通用渲染,
        }
        private readonly Plane[] Field_几何面 = new Plane[6];
        private const int Field_网格尺寸 = 108;
        private Dictionary<图层类型, 单图层_多物体_批量绘制> Field_所有批量绘制_索引视图 = null;
        public List<单图层_多物体_批量绘制> Field_所有批量绘制_遍历视图 = null;
        private 线程任务_API兼容层 Field_后台线程控制块 = null;
        List<(图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> Field_所有图层;
        public bool Field_IsWriting;          // 不要修改这个值, 有可能造成数据抢占
        public bool CanThread() => Field_IsWriting;   // 是否正在独占式写入,若有待处理任务,则阻塞新的任务添加到待处理任务队列
        public 多图层_多物体_批量绘制(线程任务_API兼容层 Arg_后台线程控制块, List<(图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> Arg_所有图层)
        {
            Field_所有批量绘制_索引视图 = new();
            Field_所有批量绘制_遍历视图 = new();

            Field_后台线程控制块 = Arg_后台线程控制块;
            Field_所有图层 = Arg_所有图层;

            Initialize();
        }

        public void Initialize()
        {
            {
                {
                    Array.Clear(Field_几何面, 0, Field_几何面.Length);

                    foreach ((图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体) in Field_所有图层)
                    {
                        switch (图层)
                        {
                            case 图层类型.房间:
                                {
                                    if (Field_所有批量绘制_索引视图.ContainsKey(图层)) { continue; }
                                    var New = new 单图层_多物体_批量绘制_房间闭合检测特化版本(Arg_图层优先级: 图层优先级);
                                    New.Initialize();
                                    Field_所有批量绘制_索引视图[图层] = New;
                                    Field_所有批量绘制_遍历视图.Add(New);
                                    break;
                                }
                            case 图层类型.可开采资源:
                                {
                                    if (Field_所有批量绘制_索引视图.ContainsKey(图层)) { continue; }
                                    var New = new 单图层_多物体_批量绘制_可开采资源检测特化版本(Arg_图层优先级: 图层优先级);
                                    New.Initialize();
                                    Field_所有批量绘制_索引视图[图层] = New;
                                    Field_所有批量绘制_遍历视图.Add(New);
                                    break;
                                }
                            case 图层类型.通用渲染:
                                {
                                    if (Field_所有批量绘制_索引视图.ContainsKey(图层)) { continue; }
                                    var New = new 单图层_多物体_批量绘制_通用渲染特化版本(Arg_图层优先级: 图层优先级, 获取渲染物体);
                                    New.Initialize();
                                    Field_所有批量绘制_索引视图[图层] = New;
                                    Field_所有批量绘制_遍历视图.Add(New);
                                    break;
                                }
                        }
                    }

                    Field_所有批量绘制_遍历视图.Sort(static (A, B) => B.Field_图层优先级.CompareTo(A.Field_图层优先级));
                }
            }
        }

        public void Dispose()
        {
            Array.Clear(Field_几何面, 0, Field_几何面.Length);

            foreach (var __ in Field_所有批量绘制_遍历视图)
            {
                __.Dispose();
            }

            Field_所有批量绘制_索引视图.Clear();
            Field_所有批量绘制_索引视图 = null;

            Field_所有批量绘制_遍历视图.Clear();
            Field_所有批量绘制_遍历视图 = null;

            Field_后台线程控制块 = null;
            Field_所有图层 = null;
        }
        public void Clear()
        {
            Array.Clear(Field_几何面, 0, Field_几何面.Length);

            foreach (var __ in Field_所有批量绘制_遍历视图)
            {
                __.Clear();
            }
        }

        public void Render()
        {
            GeometryUtility.CalculateFrustumPlanes(CameraController.CurrentCamera, Field_几何面);

            foreach (var __ in Field_所有批量绘制_遍历视图)
            {
                if (__.正在创建间接绘制参数) { __.扩容间接绘制参数池(); }
                __.Render(Field_几何面);
            }
        }
        public void 添加任务到本地待处理队列()
        {
            // 添加一个任务到<待处理任务队列>中, 当后台线程执行完成变成空闲状态后, 将<待处理任务队列>中的所有任务一次性全部提交到后台线程执行
            foreach ((图层类型 图层, int __1, Func<List<Thing>> __2) in Field_所有图层)
            {
                switch (图层)
                {
                    case 图层类型.房间:
                        {
                            var 单图层渲染器 = Field_所有批量绘制_索引视图[图层];
                            if (单图层渲染器 is 单图层_多物体_批量绘制_房间闭合检测特化版本 房间闭合)
                            {
                                房间闭合.Field_探测步数 = 批量绘制_API兼容层_DrawMeshInstancedIndirect.批量绘制最大支持;
                                Field_后台线程控制块.添加任务到本地待处理队列(new 扫描任务(房间闭合));
                            }
                            break;
                        }
                    case 图层类型.可开采资源:
                        {
                            var 单图层渲染器 = Field_所有批量绘制_索引视图[图层];
                            if (单图层渲染器 is 单图层_多物体_批量绘制_可开采资源检测特化版本 可开采资源)
                            {
                                可开采资源.Field_探测范围 = 获取玩家九宫格包围盒(4);
                                Field_后台线程控制块.添加任务到本地待处理队列(new 扫描任务(可开采资源));
                            }
                            break;
                        }
                    case 图层类型.通用渲染:
                        {
                            var 单图层渲染器 = Field_所有批量绘制_索引视图[图层];
                            if (单图层渲染器 is 单图层_多物体_批量绘制_通用渲染特化版本 通用渲染)
                            {
                                Field_后台线程控制块.添加任务到本地待处理队列(new 扫描任务(通用渲染));
                            }
                            break;
                        }
                }
            }
        }

        private BoundsInt 获取玩家九宫格包围盒(int Arg_index)
        {
            // 网格尺寸并不是真实的网格尺寸, 而是计算得到某个方向的包围盒, index代表不同的方向
            Arg_index = Mathf.Clamp(Arg_index, 0, 8);
            int 半径x = Arg_index % 3 * Field_网格尺寸;
            int 半径z = Arg_index / 3 % 3 * Field_网格尺寸;
            int 半径y = 0;
            半径x -= 3 * Field_网格尺寸 / 2;
            半径z -= 3 * Field_网格尺寸 / 2;
            半径y -= Field_网格尺寸 / 2;
            return new BoundsInt(Human.LocalHuman.Position.FloorToInt() + new Vector3Int(半径x, 半径y, 半径z), new Vector3Int(Field_网格尺寸, Field_网格尺寸, Field_网格尺寸));
        }
    }
}
