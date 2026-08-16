using Assets.Scripts.Objects.Structures;
using Assets.Scripts.Util;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 立体文字装饰 : WallLight
    {
        public enum 喷漆系统材质发光配置
        {
            未知, UV纹理, 自发光,
        }
        protected string 无光层级的名称 => "SmallGridStructureLiTiWenZiZhuangShi";
        protected string 自发光层级的名称 => "灯泡子层级";
        protected bool 材质索引表初始化了么 = false;
        protected 喷漆系统材质发光配置[] 材质索引表 = null;

        public override void Awake()
        {
            base.Awake();
            SmartRotate.AutomaticSetup(this);
        }

        public override void SetCustomColor(int 游戏内置喷漆色板编号, bool emissive = false)
        {
            base.SetCustomColor(游戏内置喷漆色板编号, emissive);
            if (!emissive) { return; }      // 关灯时不需要处理
            喷漆系统没有将自发光与无光分开单独处理_因此需要手动拦截材质切换(游戏内置喷漆色板编号, emissive);
        }

        public override void SetCustomColor(bool emissive = false)
        {
            base.SetCustomColor(emissive);
            if (!emissive) { return; }      // 关灯时不需要处理
            喷漆系统没有将自发光与无光分开单独处理_因此需要手动拦截材质切换(CustomColor.Index, emissive);
        }

        private void 喷漆系统没有将自发光与无光分开单独处理_因此需要手动拦截材质切换(int 游戏内置喷漆色板编号, bool emissive = false)
        {
            if (!emissive) { return; }      // 关灯时不需要处理
            if (CustomColor == null) { return; }

            if (_customMaterials != null)
            {
                var 自发光材质 = SelectColorSwatchMaterial(emissive);
                var 无光材质 = SelectColorSwatchMaterial(!emissive);

                if (!材质索引表初始化了么)
                {
                    材质索引表初始化了么 = true;
                    材质索引表 = new 喷漆系统材质发光配置[_customMaterials.Count];

                    for (var i = 0; i < _customMaterials.Count; i++)
                    {
                        var customMaterial = _customMaterials[i];
                        var 层级 = customMaterial.ThingRenderer.GetRendererGameObject();

                        if (!层级.CompareTag("NotPaintable"))
                        {
                            if (层级.name.StartsWith(无光层级的名称))
                            {
                                材质索引表[i] = 喷漆系统材质发光配置.UV纹理;
                            }
                            else if (层级.name.StartsWith(自发光层级的名称))
                            {
                                材质索引表[i] = 喷漆系统材质发光配置.自发光;
                            }
                            else
                            {
                                材质索引表[i] = 喷漆系统材质发光配置.未知;
                            }
                        }
                        else
                        {
                            材质索引表[i] = 喷漆系统材质发光配置.未知;
                        }
                    }
                }

                for (var i = 0; i < _customMaterials.Count; i++)
                {
                    var customMaterial = _customMaterials[i];
                    var 层级 = customMaterial.ThingRenderer.GetRendererGameObject();

                    if (!层级.CompareTag("NotPaintable"))
                    {
                        var 当前配置 = 材质索引表[i];
                        switch (当前配置)
                        {
                            case 喷漆系统材质发光配置.UV纹理:
                                customMaterial.SetColor(无光材质, 游戏内置喷漆色板编号);
                                break;
                            case 喷漆系统材质发光配置.自发光:
                                customMaterial.SetEmissive(自发光材质);
                                break;
                        }
                    }
                }
            }
        }
    }
}