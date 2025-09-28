using System;
using System.Collections.Generic;
using meanran_xuexi_mods_xiaoyouhua.utils;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua.ui.presenter
{
    public class 自定义泛型事件容器类<T> : MonoBehaviour
    {   
        public interface I处理事件 { void 处理事件(T data); }
        public delegate void 函数对象容器(T data);  // 函数对象有且只有一个方法,那就是被重写的仿函数
        public class 函数指针包装类 : I处理事件
        {
            // 添加事件函数只接收继承了I处理事件的类型,但是lambda表达式是编译器自动生成的类,无法手动指定继承,需要用包装类封装一层绕过编译器的类型检查
            private readonly 函数对象容器 捕获;
            public 函数指针包装类(函数对象容器 函数对象) => this.捕获 = 函数对象;
            public void 处理事件(T data) => 捕获.Invoke(data);  // 调用函数对象指的是调用被重写的仿函数
        }
        private readonly List<I处理事件> 事件表 = new List<I处理事件>();
        public 自定义泛型事件容器类() => name = this.ToString();    // 编译器自动根据模板参数生成的元数据实例名
        public void 处理事件(T data)
        {
            foreach (var 事件 in 事件表)
            {
                try
                {
                    // 实例->类型指针->虚方法表[方法哈希] 获取到该方法的绝对地址,用哈希表机制查找方法地址
                    事件.处理事件(data);
                }
                catch (Exception e)
                {
                    //Log.Error(e, () => $"处理事件失败,错误信息->{事件}");
                }
            }
        }
        public void 添加事件(I处理事件 父类引用子类实例) => 事件表.Add(父类引用子类实例);
        public void 添加事件(函数对象容器 函数对象) => 事件表.Add(new 函数指针包装类(函数对象));
    }
}
