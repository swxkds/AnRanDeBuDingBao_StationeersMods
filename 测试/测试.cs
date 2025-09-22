using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    [BepInPlugin("meanran_xuexi_mods_xiaoyouhua_ceshi", "测试", "1.0.0")]
    [BepInDependency("meanran_xuexi_mods_xiaoyouhua_qianzhi_mokuai", BepInDependency.DependencyFlags.HardDependency)]
    public class 测试 : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;
        public static Harmony 补丁;
        private void Awake()
        {
            Log = Logger;
            Log.LogMessage("测试加载完成!");
            补丁 = new Harmony("测试");
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
                                      //             SPUOre
                                      //             Assets.Scripts.Objects.Items.SPUMesonScanner
                                      // TerrainSystem.VoxelTerrain.MineableTypeInfo
                                      // InstancedIndirectDrawCall.MeshPerInstanceDatum
                                      //  TerrainSystem.MinableVisualiserData
                                      //  TerrainSystem.VeinCluster
                                      // CircuitHousing

            // if (前置_资源加载器.单例.逻辑组件字典.TryGetValue(typeof(InputPrefabs), out var 选择配方面板))
            // {

            // }


        }
    }

    public class Event函数对象容器<T> : MonoBehaviour   // 继承不继承MonoBehaviour无所谓,继承只是为了让容器能以组件的形式添加到unity中
    {
        public interface Interface函数签名匹配 { public void 调用函数(T data); }    // 声明一个虚表方法
        public delegate void 全局静态方法指针容器(T data);  // 声明一个全局静态方法指针容器
        public class 容器 : Interface函数签名匹配       // 声明一个子类,用于重写虚方法
        {
            public 全局静态方法指针容器 捕获 { get; private set; }
            public 容器(全局静态方法指针容器 __) { 捕获 = __; }
            public void 调用函数(T data) => 捕获(data); // 重写虚方法
        }


        public Event函数对象容器(bool __) { name = $"Event函数对象容器<{typeof(T).Name}>"; }    // 容器的名称
        private readonly HashSet<object> 查重 = new();          // 指针地址不可改,但是内容可改
        private readonly List<Interface函数签名匹配> 函数对象表 = new();    // 指针地址不可改,但是内容可改
        public void 调用函数(T data)
        {
            foreach (var __ in 函数对象表)
            {
                try { __.调用函数(data); }
                catch (Exception e)
                { Debug.LogError($"调用函数失败,错误信息->{__} , {e}"); }
            }
        }
        public void 添加函数对象(Interface函数签名匹配 __) { if (!查重.Contains(__)) { 查重.Add(__); 函数对象表.Add(__); } }
        public void 添加函数对象(容器 __) { if (!查重.Contains(__)) { 查重.Add(__); 函数对象表.Add(__); } }
        public void 添加函数对象(全局静态方法指针容器 __)
        {
            // delegate是一个语法糖,在底层是一个由编译器自动生成的匿名全局静态方法指针容器类,重写的GetMethodInfo方法指向的是全局静态方法指针
            // 匿名全局静态方法指针容器类无法显式继承Interface函数签名匹配,需要用容器包装一下
            var 全局静态指针 = __.GetMethodInfo();
            if (!查重.Contains(全局静态指针))
            {
                查重.Add(全局静态指针);
                函数对象表.Add(new 容器(__));
            }
        }
        public void 移除函数对象(Interface函数签名匹配 __) { if (查重.Contains(__)) { 查重.Remove(__); 函数对象表.Remove(__); } }
        public void 移除函数对象(容器 __) { if (查重.Contains(__)) { 查重.Remove(__); 函数对象表.Remove(__); } }
        public void 移除函数对象(全局静态方法指针容器 __)
        {
            // delegate是一个语法糖,在底层是一个由编译器自动生成的匿名全局静态方法指针容器类,重写的GetMethodInfo方法指向的是全局静态方法指针
            var 全局静态指针 = __.GetMethodInfo();
            if (查重.Contains(全局静态指针))
            {
                查重.Remove(全局静态指针);
                while (true)
                {
                    var index = 函数对象表.FindIndex(d => d is 容器 c && c.捕获.GetMethodInfo() == 全局静态指针);
                    if (index < 0) { return; }
                    else { 函数对象表.RemoveAt(index); }
                }
            }
        }

    }
}

// MinableRenderJob => 刷新矩阵的消息结构
// VoxelTerrain._minableRenderJobs => 根据消息中的实例和包围盒,分别从矿脉网格中读取变换矩阵并更新实例的17种矿石变换矩阵
// BoundsInt boundsInt:世界坐标系下的矿脉网格搜索起始点与结束点(以玩家当前坐标为基准点)

// Vein:矿脉,有一个世界坐标;  
// Vein.VeinsLookup:根据网格坐标保存所有矿脉
// Vein._minables:某矿脉所有矿物
// vein.AddToDrawCall(drawCalls[(int)vein.Type]:根据矿脉类型添加矩阵到17种实例中

// MinableDrawCallCollection => 封装DrawCallBlock的类
// public readonly MinableDrawCallBlock[] DrawCallBlocks;
// public MinableVisualizerDrawCallBlock VisualizerDrawCallBlock;

// DrawCallBlock => 封装InstancedIndirectDrawCall的类
// 作用是以17种矿石的网格+通用的透视投影材质创建17个InstancedIndirectDrawCall实例
// 获取某个区块中的17种矿石变换矩阵,分别应用到17个实例中,然后只需绘制17次,所有矿石都渲染出来了

// InstancedIndirectDrawCall => 封装批量绘制API的类
// MeshPerInstanceDatum => 保存了某个物体分别在相对坐标系和绝对坐标系的变换矩阵
// Unity变换矩阵数组(ComputeBuffer _instanceDataBuffer)、Unity网格采样配置(ComputeBuffer _argsBuffer)
// Args[0]: 三角形连线计数,网格的顶点统一存在一份顶点表中,每个三角形连线按照绘制顺序保存(顶点A下标,顶点B下标,顶点C下标)
// Args[1]: 变换矩阵数组计数
// Args[2]: 子网格三角形连线基址(复合网格用一维数组保存数据,然后分配每个子网格不同的基址)
// Args[3]: 子网格顶点表基址(三角形连线使用相对下标,加上顶点表基址才是真实的顶点下标)
// 网格(Mesh _baseMesh)、材质(Material MaterialInstance)
// 变换矩阵数组(MeshPerInstanceDatum[] _instanceData)、用于视口裁剪的包围盒(Bounds Bounds)
// 变换矩阵数组计数(int InstanceCount)
// 完整流程 =>
// 1.初始化方法中给<Unity网格采样配置>赋值,每次变换矩阵计数变化时给Args[1]重新赋值
// 2.初始化方法中给<变换矩阵数组 _instanceData>赋值,每次变换矩阵计数变化时添加或移除该变换矩阵,然后更新包围盒
// 3.初始化方法中给<Unity变换矩阵数组 _instanceDataBuffer>赋值(_instanceDataBuffer.SetData(_instanceData, 0, 0, InstanceCount);),每次变换矩阵计数变化时重新赋值
//   然后执行MaterialInstance.SetBuffer(着色器ID, _instanceDataBuffer)更新材质(为变换矩阵数组生成对应的材质)
// 4.Graphics.DrawMeshInstancedIndirect(BaseMesh, 0, MaterialInstance, bounds, ArgsBuffer, 0, null, shadowMode, receiveShadows);
//   批量绘制API的参数分别是网格,子网格索引(在底层被转换成子网格三角形连线基址和子网格顶点表基址),变换矩阵数组对应的材质数组(变换矩阵和材质都有坐标),用于视口裁剪的包围盒,网格采样配置,其它参数省略

// InstancedIndirectDrawCall[] DrawCalls 