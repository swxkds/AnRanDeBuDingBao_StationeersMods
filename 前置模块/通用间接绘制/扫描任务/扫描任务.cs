using System;
using TerrainSystem;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public readonly struct 扫描任务 : IThreadable, IEquatable<扫描任务>
    {
        private readonly 单图层_多物体_批量绘制 Field_渲染上下文;
        public 扫描任务(单图层_多物体_批量绘制 Arg_渲染上下文) => Field_渲染上下文 = Arg_渲染上下文;
        public void 执行扫描任务() => Field_渲染上下文.扫描并添加矩阵();

        public int ThreadCost => 1;
        public bool CanThread() => true;            // 必须是true, 否则无法添加到任务容器中
        public string DebugName() => string.Empty;

        public override bool Equals(object obj) => obj is 扫描任务 other ? Equals(other) : false;
        public bool Equals(扫描任务 other) => Equals(Field_渲染上下文, other.Field_渲染上下文);
        public override int GetHashCode() => Field_渲染上下文.GetHashCode();
    }
}