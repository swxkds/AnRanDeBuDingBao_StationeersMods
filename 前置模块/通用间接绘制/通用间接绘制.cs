using HarmonyLib;
using Assets.Scripts;
using System.Collections.Generic;
using Assets.Scripts.GridSystem;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Inventory;
using System.Linq;
using System;
using Assets.Scripts.Objects;
using DG.Tweening.Core.Easing;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 通用间接绘制
    {
        private readonly object 锁 = new();
        private bool Field_A在后台更新么;
        public bool A在后台更新么 { get { lock (锁) { return Field_A在后台更新么; } } set { lock (锁) { Field_A在后台更新么 = value; } } }
        private 线程任务_API兼容层 Field_后台线程控制块 = null;
        private 多图层_多物体_批量绘制 Field_双缓冲A = null;
        private 多图层_多物体_批量绘制 Field_双缓冲B = null;
        private List<(多图层_多物体_批量绘制.图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> Field_所有图层;
        private Func<bool> Field_访问快捷键 = null;
        private bool 正在提交么;
        public 多图层_多物体_批量绘制 只读 { get { lock (锁) { return A在后台更新么 ? Field_双缓冲B : Field_双缓冲A; } } }  // 一.只读和只写不可以同时访问 二.在<A在后台更新么>进行变更时,阻塞线程,以免资源竞争
        public 多图层_多物体_批量绘制 只写 { get { lock (锁) { return A在后台更新么 ? Field_双缓冲A : Field_双缓冲B; } } }  // 一.只读和只写不可以同时访问 二.在<A在后台更新么>进行变更时,阻塞线程,以免资源竞争
        public 通用间接绘制(List<(多图层_多物体_批量绘制.图层类型 图层, int 图层优先级, Func<List<Thing>> 获取渲染物体)> Arg_所有图层, Func<bool> Arg_访问快捷键)
        {
            Field_后台线程控制块 = new();
            Field_后台线程控制块.Field_控制块.Initialize(5);      // 创建5个子容器

            Field_双缓冲A = new(Field_后台线程控制块, Arg_所有图层);
            Field_双缓冲B = new(Field_后台线程控制块, Arg_所有图层);

            Field_所有图层 = Arg_所有图层;
            Field_访问快捷键 = Arg_访问快捷键;

            正在提交么 = false;

            Initialize();

            前置模块.Log.LogMessage($"成功创建双缓冲区_多图层_多物体_批量绘制实例 {Field_所有图层.Join(d => d.图层.ToString(), ",")}");
        }
        public void Initialize()
        { }

        public void Dispose()
        {
            Field_后台线程控制块.终结所有正在执行的线程任务();    // 请在任务容器的的DoTask中判断若是中断状态直接结束执行
            Field_后台线程控制块 = null;

            Field_双缓冲A.Dispose();
            Field_双缓冲A = null;

            Field_双缓冲B.Dispose();
            Field_双缓冲B = null;

            Field_所有图层.Clear();
            Field_所有图层 = null;

            前置模块.Log.LogMessage($"成功注销双缓冲区_多图层_多物体_批量绘制实例");
        }

        public void Clear()
        {
            Field_双缓冲A.Clear();
            Field_双缓冲B.Clear();
        }

        public void Update()
        {
            if (!Field_访问快捷键()) { return; }

            if (GameManager.GameState == GameState.Running && InventoryManager.Parent != null && !WorldManager.IsGamePaused)
            {
                while (更新双缓冲区())
                { }

                只读.Render();

                if (!只写.Field_IsWriting)
                {
                    // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
                    // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
                    if (!正在提交么)
                    {
                        正在提交么 = true;
                        添加任务到本地待处理队列().Forget();
                    }
                }
            }
        }

        private async UniTaskVoid 添加任务到本地待处理队列()
        {
            // 只有在<更新双缓冲区>函数中, 全部任务都处理完了切换了缓冲区, 然后才会对 只写.CanThread() 写入 false, 因此不会导致缓冲区的切换陷入死循环卡住

            // 警告: Unity引擎是单线程的, 因此协程实例也是按顺序依次调用的, 有可能多个协程连续添加任务, 然后才运行到<更新双缓冲区>函数将多个
            // 任务函数对象一次性全部上传到线程中执行, 此时会出现多个线程的同一个函数对象同时执行<扫描并添加矩阵>, 导致资源抢占
            while (只写.CanThread())
            {
                await UniTask.Yield();
            }

            只写.添加任务到本地待处理队列();
            正在提交么 = false;
        }

        private bool 更新双缓冲区()
        {
            // 若主机是服务器或者主机玩家不存在,无需更新双缓冲区
            if (GameManager.IsBatchMode || InventoryManager.Parent == null) { return false; }

            if (Field_后台线程控制块.是否有正在执行的线程任务()) { return false; }

            // 趁着任务全部处理完成的瞬间, 切换缓冲区
            if (Field_后台线程控制块.待处理任务计数 <= 0)
            {
                // 只需要切换一次读写缓冲区
                if (只写.Field_IsWriting)
                {
                    // 让只读使用更新完成的此缓冲区, 只写使用过期的缓冲区
                    A在后台更新么 = !A在后台更新么;
                    Field_双缓冲A.Field_IsWriting = Field_双缓冲B.Field_IsWriting = false;
                }

                return false;
            }

            // 如果一直添加任务到本地待处理队列, 只写永远不会切换缓冲区, 因此需要限制任务的添加速度
            if (Field_后台线程控制块.待处理任务计数 > 0)
            {
                只写.Field_IsWriting = true;
                Field_后台线程控制块.指派所有本地待处理任务到线程任务容器中();
                Field_后台线程控制块.提交线程任务容器到线程中执行();
            }

            return false;
        }
    }
}