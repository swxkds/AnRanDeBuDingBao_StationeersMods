using System.IO;
using System.Reflection;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 资源加载器
    {
        private static 资源加载器 m_单例 = null;
        public static 资源加载器 单例
        {
            get
            {
                if (m_单例 == null)
                    m_单例 = new 资源加载器();
                return m_单例;
            }
        }
        public 资源加载器()
        {
            // 用dnSpy.exe打开编译后的dll,然后在资源目录可以看到嵌入文件的完整路径
            // 嵌入文件的路径的目录分割符与操作系统的目录分割符不一样,因此将汉化文件替换前需要还原成系统能识别的目录结构

            var 游戏根目录 = System.AppDomain.CurrentDomain.BaseDirectory;             // .exe所在的目录
            var 程序集 = Assembly.GetExecutingAssembly();
            const string 游戏数据目录 = "rocketstation_Data";

            foreach (var 嵌入文件的路径 in 程序集.GetManifestResourceNames())                // 遍历嵌入文件的完整路径
            {
                var 下标 = 嵌入文件的路径.IndexOf(游戏数据目录);
                if (下标 != -1)
                {
                    var 相对 = 嵌入文件的路径.Substring(下标);      // 移除掉"rocketstation_Data"的上级目录路径
                    var 后缀名 = Path.GetExtension(相对);
                    var 纯目录 = 后缀名.Length > 0 ? 相对.Substring(0, 相对.Length - 后缀名.Length) : 相对;
                    var 路径 = 纯目录.Replace('.', Path.DirectorySeparatorChar);
                    var 绝对 = Path.Combine(游戏根目录, 路径 + 后缀名);

                    汉化模块.Log.LogMessage($"已覆盖汉化文件: {绝对}");

                    var 目录路径 = Path.GetDirectoryName(绝对);
                    if (!string.IsNullOrEmpty(目录路径)) { Directory.CreateDirectory(目录路径); }

                    using (Stream 读 = 程序集.GetManifestResourceStream(嵌入文件的路径))
                    {
                        using (FileStream 写 = new FileStream(绝对, FileMode.Create, FileAccess.Write))
                        { 读.CopyTo(写); }
                    }
                }
            }
        }
    }
}