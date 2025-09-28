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
                {
                    m_单例 = new 资源加载器();
                }
                return m_单例;
            }
        }
        public 资源加载器()
        {
            var 游戏根目录 = System.AppDomain.CurrentDomain.BaseDirectory;             // .exe所在的目录
            string 游戏语言目录 = Path.Combine(游戏根目录, "rocketstation_Data/StreamingAssets/Language");

            if (!Directory.Exists(游戏语言目录)) { Directory.CreateDirectory(游戏语言目录); }

            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录
            string 模组语言目录 = Path.Combine(dllDirectory, "通过覆盖掉原版_避免被原版覆盖");

            const string 前缀 = "simplified_chinese";
            const string 后缀名 = ".xml";

            foreach (var 当前 in (string[])["", "_help", "_keys", "_tips", "_tooltips"])
            {
                var 当前文件名 = 前缀 + 当前 + 后缀名;

                var 模组路径 = Path.Combine(模组语言目录, 当前文件名);

                // 模组目录中的语言文件必须检查是否则存在, 避免读取不存在的文件错误
                if (!File.Exists(模组路径)) { 汉化模块.Log.LogDebug($"{模组路径} 找不到语言文件, 可能是文件被移动, 可能是目录名被修改, 可能是目录结构被修改"); continue; }

                var 游戏路径 = Path.Combine(游戏语言目录, 当前文件名);
                if (!File.Exists(游戏路径)) { 汉化模块.Log.LogWarning($"{游戏路径} 找不到语言文件, 可能是文件被移动, 可能是目录名被修改, 可能是目录结构被修改"); }

                File.Copy(模组路径, 游戏路径, overwrite: true);     // 指定操作系统创建一个新文件, 如果文件已存在，它将被覆盖
                汉化模块.Log.LogMessage($"已覆盖中文语言文件: {当前文件名}");
            }
        }
    }
}