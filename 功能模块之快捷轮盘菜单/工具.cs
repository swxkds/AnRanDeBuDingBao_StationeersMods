using UnityEngine;

namespace meanran_xuexi_mods_xiaoyouhua
{
    public class 工具
    {
        public const float FULL_CIRCLE = 360f;

        public static Color32[] 生成纯圆像素表(int 直径, Color 颜色, bool 背景透明么)
        {
            int w = 直径;
            int h = 直径;
            var pixels = new Color32[w * h];

            // 颜色 & 背景
            Color32 bgColor32 = 背景透明么 ? new Color32(0, 0, 0, 0) : new Color32(255, 255, 255, 255);
            byte rByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.r) * 255f);
            byte gByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.g) * 255f);
            byte bByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.b) * 255f);
            byte aByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.a) * 188f);

            // 初始化为背景色
            for (int i = 0; i < pixels.Length; i++) { pixels[i] = bgColor32; }

            // 圆心（以像素坐标为基准）
            float cx = (直径 - 1) * 0.5f;
            float cy = cx;
            int icx = Mathf.RoundToInt(cx);
            int icy = Mathf.RoundToInt(cy);

            // 使用 floor/int 可以保证不会越界
            int rInt = Mathf.FloorToInt(直径 * 0.5f);

            // 每行的左右边界（minX/maxX）
            int[] minX = new int[h];
            int[] maxX = new int[h];
            for (int i = 0; i < h; i++) { minX[i] = int.MaxValue; maxX[i] = int.MinValue; }

            // 中点算法（Bresenham）画圆周并更新每行的边界
            int x = rInt;
            int y = 0;
            int d = 1 - rInt;

            System.Action<int, int> mark = (px, py) =>
            {
                if (py < 0 || py >= h) return;
                if (px < 0) px = 0;
                if (px >= w) px = w - 1;
                if (px < minX[py]) minX[py] = px;
                if (px > maxX[py]) maxX[py] = px;
            };

            while (x >= y)
            {
                // 8 对称点
                mark(icx + x, icy + y);
                mark(icx - x, icy + y);
                mark(icx + x, icy - y);
                mark(icx - x, icy - y);

                mark(icx + y, icy + x);
                mark(icx - y, icy + x);
                mark(icx + y, icy - x);
                mark(icx - y, icy - x);

                y++;
                if (d < 0)
                {
                    d += 2 * y + 1;
                }
                else
                {
                    x--;
                    d += 2 * (y - x) + 1;
                }
            }

            // 有时顶部或底部行可能未被标记，向上/向下填充最近已知边界，避免断裂
            int lastMin = int.MaxValue, lastMax = int.MinValue;
            for (int row = 0; row < h; row++)
            {
                if (minX[row] <= maxX[row])
                {
                    lastMin = minX[row];
                    lastMax = maxX[row];
                }
                else if (lastMin != int.MaxValue)
                {
                    minX[row] = lastMin;
                    maxX[row] = lastMax;
                }
            }
            lastMin = int.MaxValue; lastMax = int.MinValue;
            for (int row = h - 1; row >= 0; row--)
            {
                if (minX[row] <= maxX[row])
                {
                    lastMin = minX[row];
                    lastMax = maxX[row];
                }
                else if (lastMin != int.MaxValue)
                {
                    minX[row] = lastMin;
                    maxX[row] = lastMax;
                }
            }

            // 填充每行的水平段（无抗锯齿）
            for (int row = 0; row < h; row++)
            {
                if (minX[row] > maxX[row]) continue;
                int rowStart = row * w;
                for (int col = minX[row]; col <= maxX[row]; col++)
                {
                    pixels[rowStart + col] = new Color32(rByte, gByte, bByte, aByte);
                }
            }

            return pixels;
        }

        public static Texture2D 创建背景贴图(int 外圈直径, int 内圈直径, int 扇区数量, bool useMipmaps = false)
        {
            var pixels = 生成纯圆像素表(外圈直径, new(0.4f, 0.4f, 0.4f, 1), 背景透明么: true);
            var pixels_2 = 生成纯圆像素表(内圈直径, new(0.1f, 0.1f, 0.1f, 1), 背景透明么: true);
            pixels = 中心对齐混叠像素(pixels, 外圈直径, 外圈直径, pixels_2, 内圈直径, 内圈直径);

            var 扇区颜色表 = new Color[扇区数量];
            // HSV（色相、饱和度、明度）空间：
            var 色相步长 = 1f / 扇区数量; // HSV 色相从0到1循环
            var 饱和度 = 0.4f;
            var 明度 = 0.4f;

            for (int i = 0; i < 扇区数量; i++)
            {
                var 色相 = i * 色相步长; // 均匀分布色相
                扇区颜色表[i] = Color.HSVToRGB(色相, 饱和度, 明度);
            }

            for (var i = 0; i < 扇区数量; i++)
            {
                pixels_2 = 生成扇形像素表(外圈直径, 内圈直径, 扇区数量, 扇区颜色表[i], 要绘制的扇区索引: i, 收缩: 3, 外扩: 3);
                pixels = 中心对齐混叠像素(pixels, 外圈直径, 外圈直径, pixels_2, 外圈直径, 外圈直径);
            }

            var tex = new Texture2D(外圈直径, 外圈直径, TextureFormat.RGBA32, useMipmaps);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels32(pixels);
            tex.Apply(updateMipmaps: useMipmaps, makeNoLongerReadable: true);
            return tex;
        }

        public static Texture2D 创建高亮扇区贴图(int 外圈直径, int 内圈直径, int 扇区数量, bool useMipmaps = false, float 收缩 = 0f, float 外扩 = 0f)
        {
            var 直径 = 外圈直径;
            var pixels = 生成扇形像素表(外圈直径, 内圈直径, 扇区数量, new Color(0.6f, 0.4f, 0.6f, 1f), 要绘制的扇区索引: 0, 收缩: 收缩, 外扩: 外扩);

            var tex = new Texture2D(直径, 直径, TextureFormat.RGBA32, useMipmaps);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels32(pixels);
            tex.Apply(updateMipmaps: useMipmaps, makeNoLongerReadable: true);
            return tex;
        }

        public static Color32[] 生成扇形像素表(int 外圈直径, int 内圈直径, int 扇区数量, Color 颜色, int 要绘制的扇区索引, float 收缩 = 0f, float 外扩 = 0f)
        {
            要绘制的扇区索引 = Mathf.Clamp(要绘制的扇区索引, 0, 扇区数量 - 1);
            var pixels = new Color32[外圈直径 * 外圈直径];

            float cx = (外圈直径 - 1) * 0.5f;
            float cy = cx;

            // 原始半径（基于直径）
            float 内圈半径_原 = 内圈直径 * 0.5f;
            float 外圈半径_原 = 外圈直径 * 0.5f;

            // 应用用户指定的收缩/扩大（像素单位）
            float 内圈半径 = 内圈半径_原 + Mathf.Max(0f, 外扩);
            float 外圈半径 = Mathf.Max(0f, 外圈半径_原 - Mathf.Max(0f, 收缩));

            if (内圈半径 >= 外圈半径)
            {
                内圈半径 = Mathf.Max(0f, 外圈半径 - 0.01f);
            }

            float innerSq = 内圈半径 * 内圈半径;
            float outerSq = 外圈半径 * 外圈半径;

            float 扇区角 = FULL_CIRCLE / 扇区数量;

            byte rByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.r) * 255f);
            byte gByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.g) * 255f);
            byte bByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.b) * 255f);
            byte aByte = (byte)Mathf.RoundToInt(Mathf.Clamp01(颜色.a) * 188f);

            float targetSectorStart = 要绘制的扇区索引 * 扇区角;

            for (int y = 0; y < 外圈直径; y++)
            {
                float dy = y - cy;
                for (int x = 0; x < 外圈直径; x++)
                {
                    float dx = x - cx;
                    int idx = y * 外圈直径 + x;

                    float r2 = dx * dx + dy * dy;

                    // 超出内外半径范围，透明
                    if (r2 < innerSq || r2 > outerSq)
                    {
                        pixels[idx] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float theta = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (theta < 0f) theta += FULL_CIRCLE;

                    float angleInTarget = theta - targetSectorStart;
                    if (angleInTarget < 0f) angleInTarget += FULL_CIRCLE;

                    // 不在目标扇区内，透明
                    if (angleInTarget < 0f || angleInTarget > 扇区角)
                    {
                        pixels[idx] = new Color32(0, 0, 0, 0);
                    }
                    else
                    {
                        pixels[idx] = new Color32(rByte, gByte, bByte, aByte);
                    }
                }
            }

            return pixels;
        }

        // 将 b 层叠在 a 的中心。如果 inPlace=true，则返回原数组，否则返回新的 Color32[]。
        public static Color32[] 中心对齐混叠像素(Color32[] a, int aWidth, int aHeight, Color32[] b, int bWidth, int bHeight,
            bool inPlace = true, bool alphaBlend = false) // 如果为真，则使用 b.alpha 混合；如果为假，则直接用 b 的非零 alpha 替代
        {
            Color32[] dst = inPlace ? a : (Color32[])a.Clone();

            int offsetX = (aWidth - bWidth) / 2;
            int offsetY = (aHeight - bHeight) / 2;

            for (int by = 0; by < bHeight; by++)
            {
                int ty = by + offsetY;
                if (ty < 0 || ty >= aHeight) continue;

                int bRow = by * bWidth;
                int aRow = ty * aWidth;

                for (int bx = 0; bx < bWidth; bx++)
                {
                    int tx = bx + offsetX;
                    if (tx < 0 || tx >= aWidth) continue;

                    Color32 bc = b[bRow + bx];
                    if (bc.a == 0) continue; // 完全透明，跳过

                    int ti = aRow + tx;
                    if (!alphaBlend || bc.a == 255)
                    {
                        // 直接覆盖（或不进行计算的完全不透明）
                        dst[ti] = bc;
                    }
                    else
                    {
                        // 按 alpha 混合： out = src*alpha + dst*(1-alpha)
                        // 使用整数运算避免分配和 Mathf
                        int aAlpha = bc.a; // 0..255
                        int inv = 255 - aAlpha;

                        Color32 ac = dst[ti];

                        int r = (bc.r * aAlpha + ac.r * inv + 127) / 255;
                        int g = (bc.g * aAlpha + ac.g * inv + 127) / 255;
                        int bcol = (bc.b * aAlpha + ac.b * inv + 127) / 255;
                        int aOut = (bc.a * aAlpha + ac.a * inv + 127) / 255; // 可选：合成 alpha

                        dst[ti] = new Color32((byte)r, (byte)g, (byte)bcol, (byte)aOut);
                    }
                }
            }

            return dst;
        }
    }
}