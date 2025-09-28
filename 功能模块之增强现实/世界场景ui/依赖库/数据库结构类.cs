using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua.ui.presenter
{
    public class 数据库结构类
    {
        public delegate 节点类 数据库结构类_事件容器();
        private readonly ConcurrentDictionary<string, 节点类> 节点表 = new ConcurrentDictionary<string, 节点类>();
        public void Add(string id, 节点类 节点)
        {
            if (id == null)
                throw new Exception("将节点添加到数据库节点表失败,错误信息->id=null");
            节点表[id] = 节点 ?? throw new Exception("将节点添加到数据库节点表失败,错误信息->节点=null");
        }
        public 节点类 GetOrAdd(string id, 数据库结构类_事件容器 构造节点回调)
        {
            if (id == null)
                throw new Exception("查找数据库节点失败,错误信息->id=null");
            return 节点表.GetOrAdd(id, (_) => 构造节点回调());  // 构造节点回调=>new 节点类();
        }
    }
    public class 节点类
    {
        private readonly ConcurrentDictionary<string, object> 条目单元表 = new ConcurrentDictionary<string, object>();
        public 条目类<T> Add<T>(string id, 条目类<T> 条目)
        {
            if (id == null)
                throw new Exception("将条目添加到节点失败,错误信息->id=null");
            条目单元表[id] = 条目 ?? throw new Exception("将条目添加到节点失败,错误信息->条目=null");
            return 条目;
        }
        public 条目类<T> Get<T>(string id)
        {
            if (id == null || !条目单元表.ContainsKey(id))
                throw new Exception("查找条目失败,错误信息->id=null或者无对应条目");

            var 条目 = 条目单元表[id];

            if (!(条目 is 条目类<T> result))
                throw new Exception($"查找条目失败,错误信息->实例类型{条目.GetType().Name}与查询类型{typeof(T).Name}不符");

            return result;
        }
    }
    public class 条目类<T>
    {
        public delegate float 条目类_事件容器();
        private readonly 条目类_事件容器 获取最新时间;
        private readonly float 备份周期;
        private readonly T[] buffer;
        public readonly Type type;
        private readonly Meta?[] meta;
        private int 当前下标 = -1;
        public 条目类(T[] buffer, float 备份周期 = 0.5f, 条目类_事件容器 获取最新时间回调 = null)
        {
            this.备份周期 = 备份周期;
            this.buffer = buffer;
            type = typeof(T);
            meta = new Meta?[buffer.Length];       // 用可空类型包装Meta是为了判断环形缓冲区某数据位是否写入过
            获取最新时间 = 获取最新时间回调 ?? (() => Time.time);
        }
        public void 输入数据(T data) => 输入数据(data, 获取最新时间());
        public void 输入数据(T data, float 最新时间)
        {
            // 注释:可能用异步输入数据
            // 如果下标=-1,本条目是新的,直接在下标0写入值
            // 如果输入数据和缓冲区中数据相同,直接抛弃
            // 如果输入数据的时间戳比已有数据要早,直接抛弃.  注:时间戳是时钟振荡次数转换的值,越大代表时间越新
            // 如果当前数据位的构造时间-输入数据时间<备份周期,就在当前数据位更新数据,否则将数据输入到下一个数据位
            // 数据位构造时间:在一个数据位第一次输入数据的时间

            lock (this)
            {
                if (是新条目么())
                    在指定位置写入数据(data, 最新时间, 最新时间, 0);
                else
                {
                    T 值 = buffer[当前下标];
                    if (data.Equals(值))
                        return;
                    Meta meta = (Meta)this.meta[当前下标];
                    if (meta.更新数据时间 >= 最新时间)
                        return;
                    if (meta.数据位构造时间 + 备份周期 > 最新时间)
                        更新数据(data, 最新时间, meta.数据位构造时间);
                    else
                        在指定位置写入数据(data, 最新时间, 最新时间, 下一个数据位(当前下标));
                }
            }
        }
        private bool 是新条目么() => 当前下标 < 0 || 当前下标 >= buffer.Length;
        private int 下一个数据位(int i)
        {
            // 如果刚刚构造此条目,还没有输入过数据,返回-1
            if (i < 0 || i >= buffer.Length)
                return -1;

            if (++i >= buffer.Length)
                i = 0;
            return i;
        }
        private int 上一个数据位(int i)
        {
            // 如果刚刚构造此条目,还没有输入过数据,返回-1
            if (i < 0 || i >= buffer.Length)
                return -1;

            if (--i < 0)
                i = buffer.Length - 1;
            return i;
        }
        private void 在指定位置写入数据(T data, float 更新数据时间, float 数据位构造时间, int 当前下标)
        {
            buffer[当前下标] = data;
            meta[当前下标] = new Meta()
            {
                更新数据时间 = 更新数据时间,
                数据位构造时间 = 数据位构造时间
            };
            this.当前下标 = 当前下标;
        }
        private void 更新数据(T data, float 更新数据时间, float 数据位构造时间)
        {
            buffer[当前下标] = data;
            meta[当前下标] = new Meta()
            {
                更新数据时间 = 更新数据时间,
                数据位构造时间 = 数据位构造时间
            };
        }
        public T Current { get => 获取指定位置数据_含Meta(当前下标).Item2; }
        public (Meta?, T, int) Current_含Meta { get => 获取指定位置数据_含Meta(当前下标); }
        private (Meta?, T, int) 获取指定位置数据_含Meta(int i)
        {
            // 如果刚刚构造此条目,还没有输入过数据,返回-1
            // default:申请一块内存默认是用0覆写,防止数据污染,如果没有显式声明默认值,就是指该类的所有成员变量都是0的状态
            // 用可空类型包装Meta是为了判断环形缓冲区某数据位是否写入过

            if (i < 0 || i >= buffer.Length)
                return (null, default, -1);
            Meta? meta = this.meta[i];
            if (meta == null)
                return (null, default, -1);
            return (this.meta[i], buffer[i], i);
        }
        public float ChangeAge { get => 获取距上一次更新时长(获取最新时间()); }
        private float 获取距上一次更新时长(float 最新时间)
        {
            var (m, _, _) = 获取指定位置数据_含Meta(当前下标);
            if (m == null)
                return -1;
            return 最新时间 - ((Meta)m).更新数据时间;
        }
        public int GetChangeCount(float 指定时长) => 获取指定时长内数据位步进次数(指定时长, 获取最新时间());
        private int 获取指定时长内数据位步进次数(float 指定时长, float 最新时间)
        {
            // 用可空类型包装Meta是为了判断环形缓冲区某数据位是否写入过
            // var i = 1; i < buffer.Length : 遍历整个缓冲区,一开始执行了一次 获取指定位置数据_含Meta ,所以遍历次数 = buffer.Length-1

            var 最小时间戳 = 最新时间 - 指定时长;
            if (最小时间戳 >= 最新时间) return -1;

            int 计数 = 0;
            var (meta, _, pos) = 获取指定位置数据_含Meta(当前下标);

            for (var i = 1; i < buffer.Length && meta != null; i++, (meta, _, pos) = 获取指定位置数据_含Meta(上一个数据位(pos)))
            {
                if (((Meta)meta).更新数据时间 < 最小时间戳)
                    break;
                计数++;
            }
            return 计数;
        }
        public (Meta?, T, int) GetOldData_含Meta(float 指定时长) => 获取指定时长内备份数据(指定时长, 获取最新时间());
        private (Meta?, T, int) 获取指定时长内备份数据(float 指定时长, float 最新时间)
        {
            // 用可空类型包装Meta是为了判断环形缓冲区某数据位是否写入过
            // var i = 1; i < buffer.Length : 遍历整个缓冲区,一开始执行了一次 获取指定位置数据_含Meta ,所以遍历次数 = buffer.Length-1
            // 为需要计算增量的数据提供方法,如 n秒内电量的增量,温度的增量

            var 最小时间戳 = 最新时间 - 指定时长;
            if (最小时间戳 >= 最新时间) return (null, default, -1);

            var (meta, v, pos) = 获取指定位置数据_含Meta(当前下标);
            (Meta?, T, int) results = (null, default, -1);

            for (var i = 1; i < buffer.Length && meta != null; i++, (meta, v, pos) = 获取指定位置数据_含Meta(上一个数据位(pos)))
            {
                if (((Meta)meta).更新数据时间 < 最小时间戳)
                    break;
                results = (meta, v, pos);
            }
            return results;
        }
        public override string ToString()
        {
            return $"{备份周期}s {type}[{buffer.Length}]";
        }
    }
    public struct Meta
    {
        public float 更新数据时间;
        public float 数据位构造时间;
        public override string ToString()
        {
            return $"{更新数据时间}s";
        }
    }
}
