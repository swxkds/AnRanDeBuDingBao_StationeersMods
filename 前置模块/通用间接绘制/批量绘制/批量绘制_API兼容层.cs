using UnityEngine;
using Assets.Scripts;
using UnityEngine.Rendering;
using System;
using System.Threading;
using System.Collections.Generic;
using Assets.Scripts.Util;

namespace meanran_xuexi_mods_xiaoyouhua
{

    public interface 批量绘制_API兼容层
    {
        public void Dispose();
        public void Clear();
        public void Render(ShadowCastingMode Arg_阴影模式 = ShadowCastingMode.Off, bool Arg_显示阴影 = false);
        public void 添加矩阵(Matrix4x4 Arg_ObjectToWorldMatrix_世界矩阵);
        public Bounds 视口裁剪 { get; set; }
        public int 所有矩阵计数 { get; }
    }

    public class 批量绘制_API兼容层_DrawMeshInstancedIndirect : 批量绘制_API兼容层
    {
        public struct 变换矩阵
        {
            public Matrix4x4 ObjectToWorldMatrix;
            public Matrix4x4 WorldToObjectMatrix;
        }
        // ------------------------------------------------------- //
        private readonly object 锁 = new();
        private static readonly int 着色器参数_InstanceData = Shader.PropertyToID("_InstanceData");    // 每个着色器从材质中读写数据的方式不同
        public const int 批量绘制最大支持 = 1023;
        public const int Field_子网格索引配置尺寸 = 5;
        // ------------------------------------------------------- //
        public Bounds Field_视口裁剪;
        public Bounds 视口裁剪 { get { lock (锁) { return Field_视口裁剪; } } set { lock (锁) { Field_视口裁剪 = value; } } }
        private 变换矩阵[] Field_所有矩阵 = null;   // 使用泛洪算法（Flood fill Algorithm）找到人物附近网格, 并从网格单元中获取结构体的变换矩阵
        public int Field_所有矩阵计数;
        public int 所有矩阵计数 { get { lock (锁) { return Field_所有矩阵计数; } } set { lock (锁) { Field_所有矩阵计数 = value; } } }
        private Mesh Field_源建模网格 = null;      // 该物体的建模网格
        private Material Field_源材质备份 = null;   // 该物体的材质 
        private Material Field_API材质 = null;
        private int Field_子网格索引 = 0;    // 建模网格以一个个子网格的形式存在 例:打开任意一个建模,然后鼠标单击选择,除非焊接了,否则都是一块块的
        private uint[] Field_子网格索引配置 = null;    // 子网格在顶点表和三角形表中的基址
        // ------------------------------------------------------- //
        private bool 脏标记 = true;
        // ------------------------------------------------------- //
        private 单图层_多物体_批量绘制 Field_父级 = null;
        private ComputeBuffer Field_API所有矩阵 = null;
        private ComputeBuffer Field_API网格采样参数 = null;
        // ------------------------------------------------------- //
        public Material API材质
        {
            get
            {
                lock (锁) { if (脏标记) { 更新API参数(); } }    // 后台线程调用<添加矩阵和移除矩阵>会对脏标记进行变更,此时阻塞主线程,以免资源竞争
                return Field_API材质;
            }
        }

        public ComputeBuffer API所有矩阵
        {
            get
            {
                lock (锁) { if (脏标记) { 更新API参数(); } }    // 后台线程调用<添加矩阵和移除矩阵>会对脏标记进行变更,此时阻塞主线程,以免资源竞争
                return Field_API所有矩阵;
            }
        }

        public ComputeBuffer API网格采样参数
        {
            get
            {
                lock (锁) { if (脏标记) { 更新API参数(); } }    // 后台线程调用<添加矩阵和移除矩阵>会对脏标记进行变更,此时阻塞主线程,以免资源竞争
                return Field_API网格采样参数;
            }
        }

        public 批量绘制_API兼容层_DrawMeshInstancedIndirect((单图层_多物体_批量绘制 间接绘制参数池管理者, (ComputeBuffer 所有矩阵, ComputeBuffer 网格采样参数) 参数) Arg_间接绘制参数, Mesh Arg_源建模网格, Material Arg_源材质, int Arg_子网格索引, int Arg_最大矩阵表容量 = 批量绘制最大支持)
        {
            Field_父级 = Arg_间接绘制参数.间接绘制参数池管理者;
            Field_API所有矩阵 = Arg_间接绘制参数.参数.所有矩阵;
            Field_API网格采样参数 = Arg_间接绘制参数.参数.网格采样参数;

            var 容量 = Mathf.Max(1, Arg_最大矩阵表容量);
            Field_所有矩阵 = new 变换矩阵[容量];

            Field_源材质备份 = Arg_源材质;
            Field_API材质 = new Material(Arg_源材质) { enableInstancing = true };

            Field_子网格索引配置 = new uint[Field_子网格索引配置尺寸];

            Initialize(Arg_源建模网格, Arg_子网格索引);

            前置模块.Log.LogMessage($"创建了新的单物体批量绘制实例 {Arg_源建模网格.name} {Arg_源材质.name} {Arg_子网格索引} {Arg_最大矩阵表容量}");
        }
        public void Initialize(Mesh Arg_源建模网格, int Arg_子网格索引)
        {
            Field_视口裁剪 = default;
            Field_所有矩阵计数 = 0;
            Field_源建模网格 = Arg_源建模网格;
            Field_子网格索引 = Arg_子网格索引;
            Field_子网格索引配置[0] = Arg_源建模网格.GetIndexCount(Arg_子网格索引);   // 三角形计数,网格的顶点统一存在一份顶点表中,每个三角形按照绘制顺序保存(顶点A下标,顶点B下标,顶点C下标)
            Field_子网格索引配置[1] = (uint)Field_所有矩阵计数;                           // 需要绘制的数量
            Field_子网格索引配置[2] = Arg_源建模网格.GetIndexStart(Arg_子网格索引);   // 三角形基址(网格用一维数组保存数据,然后分配每个子网格不同的基址)
            Field_子网格索引配置[3] = Arg_源建模网格.GetBaseVertex(Arg_子网格索引);   // 顶点表基址(三角形使用相对下标,加上基址才是真实的顶点下标)
            脏标记 = true;
        }
        public void Dispose()
        {
            Field_视口裁剪 = default;

            Array.Clear(Field_所有矩阵, 0, Field_所有矩阵.Length);
            Field_所有矩阵 = null;
            Field_所有矩阵计数 = 0;

            Field_源建模网格 = null;
            Field_源材质备份 = null;
            UnityEngine.Object.Destroy(Field_API材质);
            Field_API材质 = null;

            Field_子网格索引 = 0;

            Array.Clear(Field_子网格索引配置, 0, Field_子网格索引配置.Length);
            Field_子网格索引配置 = null;

            脏标记 = true;

            if (Field_API所有矩阵 != null)
            {
                Field_API所有矩阵.Release();
                Field_API所有矩阵 = null;
            }

            if (Field_API网格采样参数 != null)
            {
                Field_API网格采样参数.Release();
                Field_API网格采样参数 = null;
            }

            Field_父级 = null;
        }
        public void Clear()
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占

            // 矩阵计数重置为0,矩阵表不会扩容和减容,只是从0开始写入元素;<更新API参数>读取矩阵表<0至矩阵计数>的元素,后面的旧元素直接忽视;
            视口裁剪 = default;
            所有矩阵计数 = 0;
            脏标记 = true;
        }
        public bool 相同物体检查(Mesh Arg_源建模网格, Material Arg_源材质)
        {
            return Arg_源建模网格 == Field_源建模网格 && Arg_源材质 == Field_源材质备份;
        }

        public void 更新API参数()
        {
            lock (锁)
            {
                if (Field_API所有矩阵.count < Field_所有矩阵计数)
                {
                    Field_API所有矩阵.Release();
                    unsafe { Field_API所有矩阵 = new(Field_所有矩阵计数 + 64, sizeof(变换矩阵)); }
                }

                // 矩阵表尾部元素不用移除,因为只使用==矩阵计数的前面的元素
                Field_API所有矩阵.SetData(Field_所有矩阵, 0, 0, Field_所有矩阵计数);
                Field_子网格索引配置[1] = (uint)Field_所有矩阵计数;
            }

            Field_API材质.SetBuffer(着色器参数_InstanceData, Field_API所有矩阵);
            Field_API网格采样参数.SetData(Field_子网格索引配置);
            脏标记 = false;
        }

        public void Render(ShadowCastingMode Arg_阴影模式 = ShadowCastingMode.Off, bool Arg_显示阴影 = false)
        {
            // 渲染不可以多线程,否则提交给显卡的渲染信息无法排序
            if (Thread.CurrentThread.ManagedThreadId != GameManager.MainThreadId || 所有矩阵计数 <= 0) { return; }
            Graphics.DrawMeshInstancedIndirect(Field_源建模网格, Field_子网格索引, API材质, Field_视口裁剪, API网格采样参数, 0, null, Arg_阴影模式, Arg_显示阴影, layer: Assets.Scripts.Objects.Layers.Terrain, camera: CameraController.CurrentCamera);
            // 功能模块之房间闭合检测.Log.LogMessage($"调用绘制: 边界={Field_视口裁剪}");
        }

        public void 添加矩阵(Matrix4x4 Arg_ObjectToWorldMatrix_世界矩阵)
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            lock (锁)
            {
                脏标记 = true;
                // 例: Length = 10,则下标0-9, 扩容一格后, 长度Length=11, 末尾下标=10=矩阵计数
                if (Field_所有矩阵.Length <= Field_所有矩阵计数) { Array.Resize(ref Field_所有矩阵, Field_所有矩阵计数 + 64); }
                Field_所有矩阵[Field_所有矩阵计数] = new 变换矩阵 { ObjectToWorldMatrix = Arg_ObjectToWorldMatrix_世界矩阵, WorldToObjectMatrix = Arg_ObjectToWorldMatrix_世界矩阵.inverse };
                Field_所有矩阵计数++;
            }
        }
        public void 删除矩阵(Matrix4x4 Arg_ObjectToWorldMatrix_世界矩阵)
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            lock (锁)
            {
                bool flag = false;
                for (var i = 0; i < Field_所有矩阵计数; i++)
                {
                    // 匹配到第一个元素时,依次将后一个元素向前移动
                    if (!flag && Field_所有矩阵[i].ObjectToWorldMatrix == Arg_ObjectToWorldMatrix_世界矩阵) { flag = true; }
                    if (flag && i + 1 < Field_所有矩阵计数) { Field_所有矩阵[i] = Field_所有矩阵[i + 1]; }
                }

                // 若移除了元素,则需要更新API参数
                if (flag)
                {
                    Field_所有矩阵计数--;
                    脏标记 = true;
                }
            }
        }
    }



    public class 批量绘制_API兼容层_DrawMeshInstanced : 批量绘制_API兼容层
    {
        // ------------------------------------------------------- //
        private readonly object 锁 = new();
        // ------------------------------------------------------- //
        public Bounds Field_视口裁剪;
        public Bounds 视口裁剪 { get { lock (锁) { return Field_视口裁剪; } } set { lock (锁) { Field_视口裁剪 = value; } } }
        private Matrix4x4[] Field_所有矩阵 = null;   // 使用泛洪算法（Flood fill Algorithm）找到人物附近网格, 并从网格单元中获取结构体的变换矩阵
        public int Field_所有矩阵计数;
        public int 所有矩阵计数 { get { lock (锁) { return Field_所有矩阵计数; } } set { lock (锁) { Field_所有矩阵计数 = value; } } }
        private Mesh Field_源建模网格 = null;      // 该物体的建模网格
        private Material Field_源材质备份 = null;   // 该物体的材质 
        private Material Field_API材质 = null;
        private int Field_子网格索引 = 0;    // 建模网格以一个个子网格的形式存在 例:打开任意一个建模,然后鼠标单击选择,除非焊接了,否则都是一块块的
                                        // private MaterialPropertyBlock 着色器参数数据包 = null;
                                        // ------------------------------------------------------- //
        private bool 脏标记 = true;
        // ------------------------------------------------------- //

        // ------------------------------------------------------- //

        public 批量绘制_API兼容层_DrawMeshInstanced(Mesh Arg_源建模网格, Material Arg_源材质, int Arg_子网格索引, int Arg_最大矩阵表容量 = 批量绘制_API兼容层_DrawMeshInstancedIndirect.批量绘制最大支持)
        {
            var 容量 = Mathf.Max(1, Arg_最大矩阵表容量);
            Field_所有矩阵 = new Matrix4x4[容量];

            Field_源材质备份 = Arg_源材质;
            Field_API材质 = new Material(Arg_源材质) { enableInstancing = true };

            //  着色器参数数据包 = new();

            Initialize(Arg_源建模网格, Arg_子网格索引);

            前置模块.Log.LogMessage($"创建了新的单物体批量绘制实例 {Arg_源建模网格.name} {Arg_源材质.name} {Arg_子网格索引} {Arg_最大矩阵表容量}");
        }
        public void Initialize(Mesh Arg_源建模网格, int Arg_子网格索引)
        {
            Field_视口裁剪 = default;
            Field_所有矩阵计数 = 0;
            Field_源建模网格 = Arg_源建模网格;
            Field_子网格索引 = Arg_子网格索引;

            // 着色器参数数据包.Clear();

            脏标记 = true;
        }
        public void Dispose()
        {
            Field_视口裁剪 = default;

            Array.Clear(Field_所有矩阵, 0, Field_所有矩阵.Length);
            Field_所有矩阵 = null;
            Field_所有矩阵计数 = 0;

            Field_源建模网格 = null;
            Field_源材质备份 = null;
            UnityEngine.Object.Destroy(Field_API材质);
            Field_API材质 = null;

            Field_子网格索引 = 0;

            // 着色器参数数据包.Clear();
            // 着色器参数数据包 = null;

            脏标记 = true;
        }
        public void Clear()
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占

            // 矩阵计数重置为0,矩阵表不会扩容和减容,只是从0开始写入元素;<更新API参数>读取矩阵表<0至矩阵计数>的元素,后面的旧元素直接忽视;
            视口裁剪 = default;
            所有矩阵计数 = 0;
            脏标记 = true;
        }
        public bool 相同物体检查(Mesh Arg_源建模网格, Material Arg_源材质)
        {
            return Arg_源建模网格 == Field_源建模网格 && Arg_源材质 == Field_源材质备份;
        }

        public void Render(ShadowCastingMode Arg_阴影模式 = ShadowCastingMode.Off, bool Arg_显示阴影 = false)
        {
            // 渲染不可以多线程,否则提交给显卡的渲染信息无法排序
            if (Thread.CurrentThread.ManagedThreadId != GameManager.MainThreadId || 所有矩阵计数 <= 0) { return; }
            if (脏标记)
            {
                脏标记 = false;
                // 着色器参数数据包.Clear();
                // var 所有颜色 = new List<Vector4>(所有矩阵计数);
                // var 颜色 = Singleton<GameManager>.Instance.CustomColors[2].Color;
                // for (var i = 0; i < 所有矩阵计数; ++i) { 所有颜色.Add(颜色); }
                // 着色器参数数据包.SetVectorArray(通用工具.着色器参数_Color, 所有颜色);
            }
            Graphics.DrawMeshInstanced(Field_源建模网格, Field_子网格索引, Field_API材质, Field_所有矩阵, 所有矩阵计数, null, Arg_阴影模式, Arg_显示阴影, layer: Assets.Scripts.Objects.Layers.Default, camera: CameraController.CurrentCamera);
            // 前置模块.Log.LogMessage($"调用绘制:{Field_源建模网格.name} 所有矩阵计数={所有矩阵计数}");
        }

        public void 添加矩阵(Matrix4x4 Arg_ObjectToWorldMatrix_世界矩阵)
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            lock (锁)
            {
                // 前置模块.Log.LogMessage($"添加矩阵:{Field_源建模网格.name} 所有矩阵计数={所有矩阵计数}");
                脏标记 = true;
                // 例: Length = 10,则下标0-9, 扩容一格后, 长度Length=11, 末尾下标=10=矩阵计数
                if (Field_所有矩阵.Length <= Field_所有矩阵计数) { Array.Resize(ref Field_所有矩阵, Field_所有矩阵计数 + 64); }
                Field_所有矩阵[Field_所有矩阵计数] = Arg_ObjectToWorldMatrix_世界矩阵;
                Field_所有矩阵计数++;
            }
        }
        public void 删除矩阵(Matrix4x4 Arg_ObjectToWorldMatrix_世界矩阵)
        {
            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            lock (锁)
            {
                bool flag = false;
                for (var i = 0; i < Field_所有矩阵计数; i++)
                {
                    // 匹配到第一个元素时,依次将后一个元素向前移动
                    if (!flag && Field_所有矩阵[i] == Arg_ObjectToWorldMatrix_世界矩阵) { flag = true; }
                    if (flag && i + 1 < Field_所有矩阵计数) { Field_所有矩阵[i] = Field_所有矩阵[i + 1]; }
                }

                // 若移除了元素,则需要更新API参数
                if (flag)
                {
                    Field_所有矩阵计数--;
                    脏标记 = true;
                }
            }
        }
    }
}