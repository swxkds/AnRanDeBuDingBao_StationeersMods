using System.Linq;
using Assets.Scripts.UI;
using TMPro;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public partial class 通用选择面板 : InputWindowBase
    {
        public static void 通用选择面板构造函数()
        {
            if (前置_资源加载器.单例.TryGetAllComponent<InputPrefabs>(out var 选择配方面板))
            {
                var inputPrefabs = (InputPrefabs)选择配方面板.FirstOrDefault();
                {
                    var 面板标题 = inputPrefabs.TitleText;
                    面板标题.font = 前置_资源加载器.单例.当前TMP字体;
                    通用工具.变更字体尺寸(面板标题, 23);
                    面板标题.overflowMode = TextOverflowModes.Overflow;

                    var 取消按钮 = inputPrefabs.transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>();
                    取消按钮.font = 前置_资源加载器.单例.当前TMP字体;
                    通用工具.变更字体尺寸(取消按钮, 23);

                    var 搜索栏 = inputPrefabs.SearchBar;
                    搜索栏.fontAsset = 前置_资源加载器.单例.当前TMP字体;

                    var 输入框输入显示 = 搜索栏.textComponent;
                    输入框输入显示.font = 前置_资源加载器.单例.当前TMP字体;
                    通用工具.变更字体尺寸(输入框输入显示, 22);

                    var 输入框淡色提示 = (TMP_Text)搜索栏.placeholder;
                    输入框淡色提示.font = 前置_资源加载器.单例.当前TMP字体;
                    通用工具.变更字体尺寸(输入框淡色提示, 22);

                    var 配方大类标题 = inputPrefabs.ControlGroupPrefab.Title;
                    配方大类标题.font = 前置_资源加载器.单例.当前TMP字体;
                    通用工具.变更字体尺寸(配方大类标题, 29);

                    var 配方描述 = inputPrefabs.PrefabReference.Text;
                    配方描述.font = 前置_资源加载器.单例.当前TMP字体;
                    通用工具.变更字体尺寸(配方描述, 27);

                    var 新 = UnityEngine.Object.Instantiate(inputPrefabs.gameObject, inputPrefabs.transform.parent, false);

                    新.gameObject.name = "通用选择面板";
                    单例 = 新.gameObject.AddComponent<通用选择面板>();
                    var 旧 = 新.GetComponent<InputPrefabs>();

                    单例.面板标题 = 旧.TitleText;
                    单例.面板标题.text = "通用选择面板";

                    单例.内容区垂直布局组拷贝母体 = 旧.GroupParents;
                    单例.内容区垂直布局组拷贝母体.name = "通用选择面板分支布局拷贝母体";

                    单例.内容区垂直布局组父级 = 单例.内容区垂直布局组拷贝母体.parent;
                    单例.内容区垂直布局组拷贝母体.SetParent(null, false);
                    单例.内容区垂直布局组拷贝母体.gameObject.SetActive(false);

                    for (int i = 单例.内容区垂直布局组拷贝母体.childCount - 1; i >= 0; i--)
                    {
                        UnityEngine.Object.DestroyImmediate(单例.内容区垂直布局组拷贝母体.GetChild(i).gameObject);
                    }     // 拷贝母体自带一些大类层级需要删除掉

                    var __ = UnityEngine.Object.Instantiate(inputPrefabs.PrefabReference);
                    __.transform.SetParent(null, false);
                    __.gameObject.SetActive(false);
                    var 新__ = __.gameObject.AddComponent<通用选择面板按钮>();
                    var 旧__ = __.GetComponent<PrefabReference>();
                    新__.左侧缩略图 = 旧__.Thumbnail;
                    新__.右侧文本 = 旧__.Text;
                    新__.按钮 = 旧__.GetComponent<Button>();

                    UnityEngine.Object.Destroy(旧__);
                    单例.通用选择面板按钮拷贝母体 = 新__.gameObject;
                    单例.通用选择面板按钮拷贝母体.name = "通用选择面板按钮拷贝母体";

                    单例.面板搜索栏 = 旧.SearchBar;
                    单例.UiComponentRenderer = 旧.UiComponentRenderer; // 等于null
                    单例.GameObject = 旧.GameObject;       // 本级,这三个变量指向同一个层级
                    单例.Transform = 旧.Transform;         // 本级,这三个变量指向同一个层级
                    单例.RectTransform = 旧.RectTransform; // 本级,这三个变量指向同一个层级

                    单例.关闭面板按钮 = 单例.transform.GetChild(0).GetChild(4).GetComponent<Button>();
                    单例.滚动组件 = 单例.transform.GetChild(0).GetChild(7).GetComponent<ScrollRectNoDrag>();

                    UnityEngine.Object.Destroy(旧);
                    单例.Initialize();

                    前置模块.Log.LogMessage($"前置模块_构造通用选择面板成功");
                }
            }
        }
    }
}