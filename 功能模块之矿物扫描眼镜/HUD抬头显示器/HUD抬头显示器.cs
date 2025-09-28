using UnityEngine;
using Assets.Scripts.Inventory;
using Assets.Scripts.Util;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts;
using Cysharp.Threading.Tasks;
using Assets.Scripts.GridSystem;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class HUD抬头显示器 : MonoBehaviour
    {
        public class 事件容器 : 通用事件容器<string> { }
        private void OnDestroy()
        {
            单例 = null;
            显示状态 = false;
            逐字显示.Dispose();
            逐字显示 = null;
            文本更新.Dispose();
            文本更新 = null;
            标题文本 = null;  // 在层级视图中, 随着玩家销毁而链式销毁, 无需手动释放资源
            功能模块之矿物扫描眼镜.Log.LogMessage($"已销毁HUD抬头显示器,HUD抬头显示器.单例:{单例}");
        }
        private void OnDisable()
        {
            标题文本.text = string.Empty;
        }
        public static HUD抬头显示器 单例 = null;
        public static bool 显示状态;
        public 文本动画_逐字显示 逐字显示;
        public 事件容器 文本更新;
        private TextMeshProUGUI 标题文本;
        private static readonly char[] 旋转字符 = ['|', '/', '-', '\\'];
        public static void 构造函数()
        {
            // 若是无图形化游戏模式(纯服务器), 则跳过图形API的调用
            if (GameManager.IsBatchMode) { return; }
            if (单例) { 功能模块之矿物扫描眼镜.Log.LogMessage("已跳过重复构造HUD抬头显示器"); return; }

            var HUD抬头显示器 = new GameObject().AddComponent<HUD抬头显示器>();
            HUD抬头显示器.gameObject.name = $"{HUD抬头显示器.GetType().Name}";
            单例 = HUD抬头显示器;
            单例.初始化().Forget();
        }
        private async UniTaskVoid 初始化()
        {
            while (GameManager.GameState != GameState.Running || InventoryManager.Parent == null)
            {
                await UniTask.Yield();
            }

            var canvas = transform.GetOrAddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var 玩家 = InventoryManager.ParentHuman;
            var 颈椎 = 玩家.SpineBones[玩家.SpineBones.Count - 1];
            transform.SetParent(颈椎.transform, false);

            var vLayoutGroup = new GameObject("内容区垂直布局组").AddComponent<VerticalLayoutGroup>();
            vLayoutGroup.transform.SetParent(transform, false);

            var 内容区背景 = vLayoutGroup.GetOrAddComponent<RawImage>();
            var 内容区布局变换 = vLayoutGroup.GetOrAddComponent<RectTransform>();

            标题文本 = new GameObject("标题文本").AddComponent<TextMeshProUGUI>();
            标题文本.transform.SetParent(内容区布局变换, false);

            var 面板尺寸 = new Vector2(0.12f, 0.08f); // 尺寸小是因为模型小,人物也才1.几高
            内容区布局变换.sizeDelta = 面板尺寸;
            内容区背景.color = Color.blue.SetAlpha(0.05f);
            标题文本.font = 前置_资源加载器.单例.当前TMP字体;
            标题文本.fontSize = 0.0065f;
            标题文本.characterSpacing = 标题文本.characterSpacing * 6f;

            vLayoutGroup.childControlWidth = true;    // 启用父级布局组件读取子级尺寸样式设置子级布局尺寸
            vLayoutGroup.childControlHeight = true;   // 启用父级布局组件读取子级尺寸样式设置子级布局尺寸
            var 父级布局组件读取子级尺寸样式设置子级布局尺寸 = 标题文本.GetOrAddComponent<LayoutElement>();
            父级布局组件读取子级尺寸样式设置子级布局尺寸.preferredWidth = 面板尺寸.x;
            父级布局组件读取子级尺寸样式设置子级布局尺寸.preferredHeight = 面板尺寸.y;
            标题文本.margin = new Vector4(0.01f, 0.01f, 0.01f, 0.01f);      // 实际的文本渲染比布局变换稍微缩小一点

            逐字显示 = new("欢迎使用安然的矿物扫描眼镜 ");
            var 旋转字符索引 = -1;
            var dtTime = 0f;

            文本更新 = new 事件容器();

            文本更新.添加事件((d) =>
            {
                dtTime += Time.deltaTime;
                if (dtTime < 0.15f) { return; }
                dtTime = 0f;
                if (逐字显示.MoveNext()) { 标题文本.text = 逐字显示.Current; }
                else
                {
                    // TODO: 创建一个表格母体, 赋值时写入创建时间, 每帧写入最新时间, 根据时间差对透明度进行修饰
                    // var 淡出特效 = CanvasGroup.alpha
                    标题文本.text = 逐字显示.源 + 旋转字符[(++旋转字符索引) % 旋转字符.Length];
                }
            });
            标题文本.enableWordWrapping = false;              // 禁用自动换行

            gameObject.SetActive(false);
            校正面板方向();

            功能模块之矿物扫描眼镜.Log.LogMessage("成功将HUD抬头显示器添加到游戏中");
        }

        public void 校正面板方向()
        {
            var 玩家变换 = InventoryManager.ParentHuman.transform;
            transform.rotation = Quaternion.LookRotation(玩家变换.transform.forward);  // 校正前向向量
            transform.Translate(玩家变换.transform.forward * 0.28f, Space.World);      // 微调面板位置
            transform.Translate(玩家变换.transform.up * 0.4f, Space.World);
            transform.Translate(玩家变换.transform.right * 0.2f, Space.World);
            transform.Rotate(Vector3.right, -20, Space.Self);                         // 微调面板倾角
            transform.Rotate(Vector3.up, 25, Space.Self);
        }
    }
}