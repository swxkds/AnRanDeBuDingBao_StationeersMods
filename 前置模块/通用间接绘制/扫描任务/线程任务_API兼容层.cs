using UnityEngine;
using System;
using TerrainSystem;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 线程任务_API兼容层 : ThreadWorker<线程任务_API兼容层, 扫描任务>
    {
        public readonly WorkerCollection<线程任务_API兼容层, 扫描任务> Field_控制块 = new();
        private readonly UniqueQueue<扫描任务> Field_本地待处理任务队列 = new();
        public 线程任务_API兼容层() => DoTask = DoTask_入口函数;          // DoTask => 当任务容器被提交到后台线程时的Main入口函数
        public int 待处理任务计数 => Field_本地待处理任务队列.Count;
        public bool 是否有正在执行的线程任务() => Field_控制块.IsAnyWorking();           // 任意一个子容器(Worker)的状态 == 已提交或者运行中时,返回true
        public void 终结所有正在执行的线程任务() => Field_控制块.AbortAll();
        public void 添加任务到本地待处理队列(扫描任务 Arg_任务) => Field_本地待处理任务队列.Enqueue(Arg_任务);          // 将任务添加到本地缓冲区
        public void 指派所有本地待处理任务到线程任务容器中() { while (Field_本地待处理任务队列.Count > 0) { Field_控制块.Assign(Field_本地待处理任务队列.Dequeue()); } }    // 查找_objects(函数对象容器)里元素最少的子容器, 然后将新任务函数对象添加进去
        public void 提交线程任务容器到线程中执行() { Field_控制块.ExecuteAll(); }           // 将所有子容器提交到ThreadPool并执行(ThreadPool.QueueUserWorkItem(DoTask);)
        private void DoTask_入口函数(object state)
        {
            if (!StartTask()) { return; }   // 启动
            try { while (_objects.Count > 0 && _state != WorkerState.Abort) { _objects.Dequeue().执行扫描任务(); } }    // 状态 == 中断时,结束执行
            catch (Exception exception) { Debug.LogException(exception); }
            finally { FinishTask(); }          // 重置
        }
    }
}