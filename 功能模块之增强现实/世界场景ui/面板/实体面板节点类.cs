using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using meanran_xuexi_mods_xiaoyouhua.ui.presenter;
using meanran_xuexi_mods_xiaoyouhua.utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace meanran_xuexi_mods_xiaoyouhua.ui
{
    public class 实体面板节点类 : MonoBehaviour
    {
        private ThingsUi thingsUi;
        private Canvas canvas;
        private VerticalLayoutGroup layout;
        private RectTransform layoutRect;
        public string referenceId;
        public Thing 锚点物体;
        public static 实体面板节点类 构造(RectTransform parentRect, ThingsUi thingsUi)
        {
            // 实体面板节点有区域宽高,但是没有坐标,需要提供用来当锚定坐标的物体
            var 节点 = Utils.构造节点<实体面板节点类>(parentRect);
            节点.初始化(thingsUi);
            return 节点;
        }
        private void 初始化(ThingsUi thingsUi )
        {
            this.thingsUi = thingsUi;

            canvas = Utils.构造节点<Canvas>(this.gameObject);
            canvas.renderMode = RenderMode.WorldSpace;

            var size = new Vector2(1.0f, 0);
            // 3D面板需要两面都有背景,否则人物走到另一面时会显示大块白色
            var bkgdFront = Utils.构造节点<RawImage>(canvas);
            bkgdFront.color = UI面板表格构造工具.默认面板BkgdColor;
            bkgdFront.rectTransform.sizeDelta = size;

            var bkgdBack = Utils.构造节点<RawImage>(canvas);
            bkgdBack.color = UI面板表格构造工具.默认面板BkgdColor;
            bkgdBack.transform.Rotate(Vector3.up, 180);
            bkgdBack.rectTransform.sizeDelta = size;

            layout = (VerticalLayoutGroup)Utils.VlHl_Init(Utils.构造节点<VerticalLayoutGroup>(canvas));
            layoutRect = layout.GetOrAddComponent<RectTransform>();
            layoutRect.sizeDelta = size;

            var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            // Log.Debug(()=>"创建了一个固定节点");
            Utils.唤醒节点(this.gameObject);
        }
        private void OnDestroy() { }// Log.Debug(() => "本面板节点跟随锚点物体被销毁");
        public void 更新绘制信息_可变面板(Thing thing, string id)
        {
            // 更新绘制单元中的TextMeshProUGUI等等绘制组件的内容前,先将这些组件从unity引擎的循环中移出
            this.gameObject.SetActive(false);
            // 可变面板的物体经常变,面板节点会创建多个绘制单元,每个绘制单元都有自己的的TextMeshProUGUI等等绘制组件
            // 先将所有的绘制单元都从unity引擎的循环中移出,然后单独唤醒要使用的那个绘制单元
            Utils.休眠子级节点(layoutRect);

            // 锚点物体:提供面板的坐标和UI内容;referenceId:判断UI的绘制单元和物体是否一致;    
            锚点物体 = thing;
            referenceId = id;

            // 将面板的初始坐标设置为锚点物体的坐标并将面板的初始角度设置为朝向玩家摄像机
            transform.SetParent(thing.transform, false);
            transform.SetPositionAndRotation(thing.transform.position, Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0));

      
            // 获取或者构造绘制单元并更新绘制单元中的TextMeshProUGUI等等绘制组件的内容
            更新绘制信息();

            // 然后将整个面板的高度坐标位移->不低于玩家脚高度坐标,不高于玩家眼高度坐标
            var 限高系数 = 1.2f;
            var human = 玩家_API兼容层.玩家;

            var 眼坐标 = human.GlassesSlot.Get().transform.position;
            var 脚坐标 = human.transform.position;

            var 不低于 = 脚坐标.y;
            var 不高于 = 脚坐标.y + (眼坐标.y - 脚坐标.y) * 限高系数;
            // 面板初始和眼坐标重合
            var 面板坐标 = transform.position = 眼坐标;

            Vector3[] vector3 = new Vector3[4];
            layoutRect.GetWorldCorners(vector3);
            // 面板的左下角高度坐标
            var y1 = vector3[0].y;
            // 面板的右上角高度坐标
            var y2 = vector3[2].y;

            var 面板高 = y2 - y1;
            if (y2 > 不高于)
            { transform.position = new Vector3(面板坐标.x, 不高于, 面板坐标.z); }
            else if (y1 < 不低于)
            { transform.position = new Vector3(面板坐标.x, 脚坐标.y + 面板高, 面板坐标.z); }

            // 在X\Z平面坐标系进行位移,位移Z=sin(摄像机的弧度)*0.5,位移X=cos(摄像机的弧度)*0.5
            // 让面板向前位移,不要和眼坐标重合
            transform.Translate(Camera.main.transform.forward * 0.5f, Space.World);

            this.gameObject.SetActive(true);
        }
        public void 更新绘制信息_固定面板(Thing thing, string id)
        {
            this.gameObject.SetActive(false);

            锚点物体 = thing;
            referenceId = id;

            transform.SetParent(thing.transform, false);
            transform.position = thing.transform.position;
            // 位移Y=1.2
            transform.Translate(Vector3.up * 0.3f, Space.World);
            更新绘制信息();

            var human = 玩家_API兼容层.玩家;
            Vector3 相对向量 = thing.transform.position - human.transform.position;
            // 两个向量的夹角越大,点积的结果值越大，夹角为0时，点积=1，夹角为90时，点积=0，夹角为180时，点积=-1
            var 方向 = Vector3.forward;
            var 夹角 = Vector3.Dot(相对向量, Vector3.forward);

            var bd = Vector3.Dot(相对向量, Vector3.back);
            if (bd > 夹角)
            {
                方向 = Vector3.back;
                夹角 = bd;
            }
            var ld = Vector3.Dot(相对向量, Vector3.left);
            if (ld > 夹角)
            {
                方向 = Vector3.left;
                夹角 = ld;
            }
            var rd = Vector3.Dot(相对向量, Vector3.right);
            if (rd > 夹角)
            {
                方向 = Vector3.right;
                夹角 = rd;
            }
            transform.rotation = Quaternion.LookRotation(方向);
            // X或者Z轴向前位移
            transform.Translate(方向 * -0.35f, Space.World);

            this.gameObject.SetActive(true);
        }
        public void 更新绘制信息() => thingsUi.更新绘制信息(锚点物体, layoutRect, UI代号.完整UI, 世界坐标系么: true);
        public bool 是活跃么() => isActiveAndEnabled && 锚点物体 != null;
        public void 重置面板节点()
        {
            // 面板节点出于复用性,只会销毁绘制单元而不会销毁节点本身
            this.gameObject.SetActive(false);
            Utils.销毁子级节点(this.layoutRect);
            锚点物体 = null;
            referenceId = null;
        }
    }
}
