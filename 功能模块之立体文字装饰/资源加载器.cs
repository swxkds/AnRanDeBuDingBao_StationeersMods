using System.IO;
using System.Reflection;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 功能模块之立体文字装饰_资源加载器
    {
        private static 功能模块之立体文字装饰_资源加载器 m_单例 = null;
        public static 功能模块之立体文字装饰_资源加载器 单例 { get { if (m_单例 == null) { m_单例 = new(); } return m_单例; } }

        public 通用工具.热加载资源管理器 资源视图 { get; private set; }
        public 功能模块之立体文字装饰_资源加载器()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location; // 获取DLL完整路径
            string dllDirectory = Path.GetDirectoryName(dllPath); // 获取DLL所在目录

            const string 物体名称 = "立体文字装饰";
            资源视图 = 通用工具.加载AssetBundle(dllDirectory, [Path.Combine("模型与纹理", 物体名称)]);
        }
    }
}

