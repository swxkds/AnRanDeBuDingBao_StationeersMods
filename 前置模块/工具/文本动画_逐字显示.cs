
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 文本动画_逐字显示 : IEnumerator<string>
    {
        public const int 空帧 = -1;
        private string 播放源 = string.Empty;
        private List<int> 帧索引表 = new(32);
        private int 当前帧数 = 空帧;
        private string 当前播放 = string.Empty;
        public string Current => 当前播放;
        object IEnumerator.Current => 当前播放;
        public int Length => 播放源.Length;
        public string 源 => 播放源;
        public 文本动画_逐字显示(string str) { 复用初始化(str); }
        public void 复用初始化(string str)
        {
            播放源 = str ?? string.Empty;
            帧索引表.Clear();
            Reset();
            if (string.IsNullOrEmpty(播放源)) { return; }
            帧索引表初始化();
        }
        public void Reset()
        {
            当前帧数 = 空帧;
            当前播放 = string.Empty;
        }
        public void Dispose() { 帧索引表.Clear(); 帧索引表 = null; }
        public bool MoveNext()
        {
            if (string.IsNullOrEmpty(播放源)) { return false; }
            当前帧数++;
            if (当前帧数 >= 0 && 当前帧数 < 帧索引表.Count)
            {
                当前播放 = 播放源.Substring(0, 帧索引表[当前帧数]);
                return true;
            }
            return false;
        }
        private static readonly Regex 修饰标记结束符正则 = new(@"^\s*<\s*/", RegexOptions.Compiled);
        private static readonly Regex 插入标记正则 = new(@"/\s*>\s*$", RegexOptions.Compiled);
        private void 帧索引表初始化()
        {
            // 生成逐字显示的帧缓冲区
            // 富文本修饰标记: <修饰标记名称=XXX>框选文本</修饰标记名称> 
            // 富文本插入标记(例:插入图片): <插入标记名称/> 

            var 嵌套标记表 = new List<(string 完整标记, string 标记名)>(32);
            var 已处理 = new StringBuilder(播放源.Length + 32);  // 已经显示的可见字符（不含富文本标记）

            int i = 0;
            int 总字数 = 播放源.Length;
            int 空格计数 = 0;

            while (i < 总字数)
            {
                char c = 播放源[i];
                if (c == '<')
                {
                    // 在处理标记之前, 先把之前收集到的连续空格写入已处理
                    if (空格计数 > 0)
                    {
                        已处理.Append(new string(' ', 空格计数));
                        空格计数 = 0;
                    }

                    // 找到对应的 '>'
                    int end = 播放源.IndexOf('>', i);
                    if (end == -1)
                    {
                        // 没有标记结束符, 视为普通文字
                        已处理.Append(c);
                        帧索引表.Add(已处理.Length);
                        i++;
                        continue;
                    }

                    var 完整标记 = 播放源.Substring(i, end - i + 1); // 包含 < 和 > 的完整标记

                    // 判断是修饰标记起始、修饰标记结束符还是插入标记
                    if (修饰标记结束符正则.IsMatch(完整标记))
                    {
                        // 遇到修饰标记结束符, 说明前面内容的嵌套富文本标记都已经添加到帧中, 将该标记删除
                        var 标记结束名 = 解析标记名称(完整标记);
                        for (int t = 嵌套标记表.Count - 1; t >= 0; t--)
                        {
                            var 标记起始名 = 嵌套标记表[t].标记名;
                            if (string.Equals(标记起始名, 标记结束名, StringComparison.OrdinalIgnoreCase))
                            {
                                嵌套标记表.RemoveAt(t);
                                已处理.Append(完整标记);
                                break;
                            }
                        }

                        i = end + 1;
                        continue;
                    }
                    else if (插入标记正则.IsMatch(完整标记))
                    {
                        // 遇到插入标记, 将该标记视为一个单字即可
                        已处理.Append(完整标记);
                        帧索引表.Add(已处理.Length);

                        i = end + 1;
                        continue;
                    }
                    else
                    {
                        // 遇到修饰标记起始, 后续所有帧(新内容)都在前面加上标记起始, 后面加上标记结束符
                        var 标记起始名 = 解析标记名称(完整标记);
                        嵌套标记表.Add((完整标记, 标记起始名));

                        已处理.Append(完整标记);

                        i = end + 1;
                        continue;
                    }
                }
                else
                {
                    if (char.IsWhiteSpace(c)) { 空格计数++; }  // 如果是空白字符(空格/制表/换行等), 一次性收集连续空白到同一帧
                    else
                    {
                        if (空格计数 > 0)
                        {
                            已处理.Append(new string(' ', 空格计数));
                            空格计数 = 0;
                        }

                        已处理.Append(c);
                        帧索引表.Add(已处理.Length);
                    }

                    i++;
                    continue;
                }
            }

            string 解析标记名称(string 完整标记)
            {
                if (string.IsNullOrEmpty(完整标记)) { return string.Empty; }
                int len = 完整标记.Length;

                // 找到第一个 '<'
                int i = 0;
                while (i < len && 完整标记[i] != '<') { i++; }
                if (i >= len) { return string.Empty; }
                i++; // 指向 '<' 之后的字符

                // 跳过 '<' 之后的空格
                i = 跳过空格字符(i, 完整标记);
                if (i >= len) { return string.Empty; }

                // 如果是关闭标记，跳过 '/'
                if (完整标记[i] == '/')
                {
                    i++;
                    i = 跳过空格字符(i, 完整标记);
                    if (i >= len) { return string.Empty; }
                }

                int start = i;
                // 读取标记名（遇到 '>' '/' 空白 或 '=' 时结束）
                while (i < len)
                {
                    char ch2 = 完整标记[i];
                    if (ch2 == '>' || ch2 == '/' || char.IsWhiteSpace(ch2) || ch2 == '=') { break; }
                    i++;
                }

                if (i <= start) { return string.Empty; }
                return 完整标记.Substring(start, i - start);
            }

            int 跳过空格字符(int i, string 源)
            {
                // 找到不是空格的字符或者返回溢出的i
                if (string.IsNullOrEmpty(源)) { return i; }
                int len = 源.Length;
                while (i < len && char.IsWhiteSpace(源[i])) { i++; }
                return i;
            }
        }
    }
}
