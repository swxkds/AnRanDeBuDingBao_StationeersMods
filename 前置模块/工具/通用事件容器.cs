
using System;
using System.Collections.Generic;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 通用事件容器<T>
    {
        public interface I事件函数格式 { void Invoke(T data); int GetHashCode(); }
        public class 事件包装 : I事件函数格式, IEquatable<事件包装>
        {
            // 无论创建多少个(delegate委托)实例, 因为(delegate委托)的(Equals相等比较)是比较 目标对象引用 和 全局数据区方法指针(MethodInfo), 因此是可靠的   注: 静态方法的目标对象引用 = null  
            // 警告: 每个Lambda表达式都是独立的全新匿名类对象, 哪怕语句一模一样, 传给(delegate委托)实例的目标对象引用都不一样, 因此Lambda表达式请用变量保存或者使用static修饰让Lambda表达式变成静态方法
            public Action<T> 事件 { get; }
            public 事件包装(Action<T> __) => 事件 = __;
            public void Invoke(T data) => 事件(data);
            public override int GetHashCode() => 事件.GetHashCode();        // 不同包装类以哈希值进行等于比较, 判断持有的内容是否相同
            public override bool Equals(object obj) => obj is 事件包装 other ? Equals(other) : false;   // 相同包装类直接比较持有的内容是否相同
            public bool Equals(事件包装 other) => Equals(事件, other.事件);         // 相同包装类直接比较持有的内容是否相同
        }


        private readonly Dictionary<int, int> 所有事件_索引视图 = new();
        private readonly List<I事件函数格式> 所有事件_遍历视图 = new();
        public string name { get; }

        public 通用事件容器() => name = $"通用事件容器<{typeof(T).Name}>";
        
        public void Dispose()
        {
            所有事件_索引视图.Clear();
            所有事件_遍历视图.Clear();
        }


        public void Invoke(T data)
        {
            foreach (var 事件 in 所有事件_遍历视图)
            {
                try
                {
                    事件.Invoke(data);
                }
                catch (Exception e)
                {
                    前置模块.Log.LogError($"执行事件失败,错误信息->{事件} , {e}");
                }
            }
        }

        public void 添加事件(I事件函数格式 __)
        {
            if (__ == null) { return; }

            var ID = __.GetHashCode();
            if (所有事件_索引视图.ContainsKey(ID)) { return; }

            var 索引 = 所有事件_遍历视图.Count;
            所有事件_索引视图.Add(ID, 索引);
            所有事件_遍历视图.Add(__);
        }

        public void 添加事件(Action<T> __)
        {
            if (__ == null) { return; }

            var ID = __.GetHashCode();
            if (所有事件_索引视图.ContainsKey(ID)) { return; }

            var 索引 = 所有事件_遍历视图.Count;
            所有事件_索引视图.Add(ID, 索引);
            所有事件_遍历视图.Add(new 事件包装(__));
        }

        public void 删除事件(I事件函数格式 __)
        {
            if (__ == null) { return; }

            var ID = __.GetHashCode();
            if (!所有事件_索引视图.ContainsKey(ID)) { return; }

            var 索引 = 所有事件_索引视图[ID];
            所有事件_遍历视图.RemoveAt(索引);
            所有事件_索引视图.Remove(索引);
        }

        public void 删除事件(Action<T> __)
        {
            if (__ == null) { return; }

            var ID = __.GetHashCode();
            if (!所有事件_索引视图.ContainsKey(ID)) { return; }

            var 索引 = 所有事件_索引视图[ID];
            所有事件_遍历视图.RemoveAt(索引);
            所有事件_索引视图.Remove(索引);
        }
    }
}