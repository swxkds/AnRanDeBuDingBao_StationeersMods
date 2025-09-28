using System.Collections.Generic;

namespace meanran_xuexi_mods_xiaoyouhua.utils
{
    public class 标记语言解析器类
    {
        public string 从名称中移除标记字符(string str)
        {
            string[] 标记表 = Split(str, '#');
            if (标记表 == null)
                return str;

            foreach (var 标记 in 标记表)
            {
                str = str.Replace(标记, "");
            }
            return str.Trim();
        }

        public List<标记数据结构> 从名称中提取标记数据(string str)
        {
            if (str == null)
                return null;
            if (!str.Contains("#"))     // 标记语言语法例:#AR_3-4#W_1-2-5  '#':条目项分割符,"AR":条目项名称,"_3-4":该项的值部分,3为面板UI代号,4为其他值
                return null;

            string[] 标记表 = Split(str, '#');               // 标记语言解析过程第一步:以'#'作为分割符,提取出条目项
            if (标记表 == null || 标记表.Length <= 0)
                return null;

            var 标记数据表 = new List<标记数据结构>();

            foreach (var 标记 in 标记表)
            {
                var (标记名称, 变长参数) = 从标记中提取条目信息(标记, '-', '_');    // 标记语言解析过程第二步:以'-''_'作为分割符,提取出标记名称和变长参数

                string[] 变长参数表_文本 = null;

                if (变长参数 != null)
                { 变长参数表_文本 = Split("-" + 变长参数, '-', '_'); }  // 标记语言解析过程第三步:将变长参数拆分成单个的序列并按序分布组成序列表

                int[] 变长参数表_数值 = null;
                string 变长参数文本 = null;

                if (变长参数表_文本 != null)
                {
                    变长参数表_文本 = 移除前导分割符残留(变长参数表_文本);     // 标记语言解析过程第四步:移除序列表中各序列的前导分割符残留  注:提取标记是通用函数,从名称中移除标记字符也要用,不能在函数中移除分割符
                    变长参数表_数值 = StringToInt(变长参数表_文本);              // 标记语言解析过程第五步:将序列的值由字符转换成可使用的数值
                    变长参数文本 = string.Join(", ", 变长参数表_文本);    // 防重,例:序列1,序列2,序列3,如果两个解析结果一致,就认为是同个物体
                }

                标记数据表.Add(new 标记数据结构 { 标记名称 = 标记名称, 变长参数文本 = 变长参数文本, 变长参数表_文本 = 变长参数表_文本, 变长参数表_数值 = 变长参数表_数值 });
            }

            if (标记数据表.Count == 0)
                return null;

            return 标记数据表;
        }

        private string[] 移除前导分割符残留(string[] 表)
        {
            if (表 == null)
                return null;

            for (int i = 0; i < 表.Length; i++)
            {
                string str = 表[i];
                for (int j = 0; j < str.Length; j++)
                {
                    char ch = str[j];
                    // 每个序列前面有一个用于分割的'-''_',这些分割符的编码区域规定属于符号类
                    if (char.IsLetterOrDigit(ch))
                    {
                        表[i] = str.Substring(j);   // 用截取后的替换掉截取前的
                        break;
                    }
                }
            }
            return 表;
        }
        private int[] StringToInt(string[] 表)
        {
            var 表_数值 = new List<int>();
            foreach (var str in 表)
            {
                if (int.TryParse(str, out int i))
                {
                    表_数值.Add(i);
                }
            }

            if (表_数值.Count == 0)
                return null;
            return 表_数值.ToArray();
        }
        private (string, string) 从标记中提取条目信息(string str, char delim1, char delim2)
        {
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                // 当前字符如果是分割符,就截取前后,得到条目名称和条目值部分,如果值部分的前导忘记打分割符,那么条目名称后的第一个十进制数字当分割符

                if (ch == delim1 || ch == delim2 || char.IsDigit(ch))
                {
                    // 例:i=6,说明是第7个字,条目名称截取6个字, 这样十进制数字当分割符时,数字被截取到值部分
                    return (str.Substring(0, i), str.Substring(i));
                }
            }
            return (str, null);
        }
        private string[] Split(string str, char delim1, char delim2 = '\0')
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            var 标记表 = new List<string>();
            int 截取_起 = -1;
            int 截取_末 = -1;

            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (截取_起 < 0)        // 遍历直到遇到分割符,将分割符所在下标视为截取起点
                {
                    if (ch == delim1 || ch == delim2)
                        截取_起 = i;
                }
                else if (截取_起 >= 0 && 截取_末 < 0)
                {
                    if (ch == delim1 || ch == delim2)   // 当截取起点已标记,截取末没有标记时,如果连续遇到多个重复的分割符,以最后一个为准
                        截取_起 = i;
                    else if (ch == ' ')                 // 当截取起点已标记,截取末没有标记时,如果遇到空格,视为空标记,通过重置截取起点忽略掉空标记. 
                        截取_起 = -1;
                    else
                    {
                        截取_末 = i;                    // 当截取起点已标记,截取末没有标记时,且不是空标记,更新截取范围
                        if (i == str.Length - 1)       // 如果此时遍历到最后一个字符,就直接截取
                        {
                            var 标记 = str.Substring(截取_起, i - 截取_起 + 1);
                            if (!string.IsNullOrWhiteSpace(标记))
                                标记表.Add(标记);

                            截取_起 = -1;
                            截取_末 = -1;
                        }
                    }
                }
                else if (截取_起 >= 0 && 截取_末 >= 0)  // 当截取起点已标记,截取末也已标记,也不是最后一个字符时
                {
                    if (ch == delim1 || ch == delim2)   // 再次遇到分割符,截取上一个分割符到这个分割符之间
                    {
                        var 标记 = str.Substring(截取_起, 截取_末 - 截取_起 + 1);   // 注:此时截取_末是这个分割符的前一个下标,截取不包括这个分割符,但包括上一个分割符
                        if (!string.IsNullOrWhiteSpace(标记))
                            标记表.Add(标记);

                        截取_起 = i;            // 将截取_起更新到每二个标记的分割符
                        截取_末 = -1;
                    }
                    else if (ch == ' ')     // 当截取起点已标记,截取末也已标记,遇到了空格,就将这个空格当成分割符,截取这一段
                    {
                        var 标记 = str.Substring(截取_起, 截取_末 - 截取_起 + 1);
                        if (!string.IsNullOrWhiteSpace(标记))
                            标记表.Add(标记);

                        截取_起 = -1;       // 因为这次截取没有分割符,所以截取_起也要重置,重新遍历获取分割符
                        截取_末 = -1;
                    }
                    else if (i == str.Length - 1)
                    {
                        var token = str.Substring(截取_起, 截取_末 - 截取_起 + 2);  // 截取_起 + 2是因为截取_末下标还停留在倒数第二 注:下标从0开始,截取字数=下标+1
                        if (!string.IsNullOrWhiteSpace(token))
                            标记表.Add(token);

                        截取_起 = -1;
                        截取_末 = -1;
                    }
                    else
                    {
                        截取_末 = i;
                    }
                }
            }

            if (标记表.Count == 0)
                return null;

            return 标记表.ToArray();
        }

        public class 标记数据结构
        {
            public string 标记名称;
            public string 变长参数文本;
            public string[] 变长参数表_文本;
            public int[] 变长参数表_数值;

            public override bool Equals(object obj)
            {
                return obj is 标记数据结构 tag &&
                       标记名称 == tag.标记名称 &&
                       变长参数文本 == tag.变长参数文本;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                if (标记名称 != null)
                    hash = hash * 23 + 标记名称.GetHashCode();
                if (变长参数文本 != null)
                    hash = hash * 23 + 变长参数文本.GetHashCode();
                return hash;
            }

            public override string ToString()
            {
                if (变长参数文本 == null)
                    return 标记名称;

                return 标记名称 + $"({变长参数文本})";
            }
        }
    }
}
