namespace App.Root.Text;
using StbTrueTypeSharp;

class FontLoader {
    private StbTrueType.stbtt_fontinfo? fontInfo = null!;
    private byte[] fontData;
    private float fontSize;
    private float scale;
    private int ascent;
    private int descent;
    private int lineGap;
    public float lineHeight;

    public FontLoader(string fontPath, float fontSize) {
        this.fontSize = fontSize;
        fontData = File.ReadAllBytes(fontPath);
        load();
    }

    // Get Font Metrics 
    public FontMetrics getFontMetrics() {
        return new FontMetrics(
            lineHeight,
            ascent * scale,
            MathF.Abs(descent * scale),
            0, 0, 0
        );
    }

    // Get Kerning
    public unsafe float getKerning(int g1, int g2) {
        fixed(byte* ptr = fontData) {
            return StbTrueType.stbtt_GetGlyphKernAdvance(fontInfo, g1, g2) * scale;
        }
    }

    // Rasterize Glyph
    public unsafe byte[]? rasterizeGlyph(Glyph glyph) {
        if(glyph == null || 
            glyph.bitmapWidth <= 0 || 
            glyph.bitmapHeight <= 0
        ) return null;

        fixed(byte* ptr = fontData) {
            byte[] bitmap = new byte[glyph.bitmapWidth * glyph.bitmapHeight];
            fixed(byte* bitmapPtr = bitmap) {
                StbTrueType.stbtt_MakeGlyphBitmap(
                    fontInfo,
                    bitmapPtr,
                    glyph.bitmapWidth,
                    glyph.bitmapHeight,
                    glyph.bitmapWidth,
                    scale, scale,
                    glyph.glypthIndex
                );
            }
            return bitmap;
        }
    }

    ///
    /// Load
    /// 
    public unsafe Glyph? loadGlyph(char c) {
        fixed(byte* ptr = fontData) {
            int glyphIndex = StbTrueType.stbtt_FindGlyphIndex(fontInfo, c);
            if(glyphIndex == 0) return null;

            int x0;
            int y0;
            int x1;
            int y1;
            StbTrueType.stbtt_GetGlyphBitmapBox(
                fontInfo,
                glyphIndex,
                scale,
                scale,
                &x0, &y0,
                &x1, &y1
            );
            int w = x1 - x0;
            int h = y1 - y0;

            int adv;
            int lsb;
            StbTrueType.stbtt_GetGlyphHMetrics(
                fontInfo,
                glyphIndex,
                &adv,
                &lsb
            );

            return new Glyph {
                codepoint = c,
                glypthIndex = glyphIndex,
                width = w,
                height = h,
                xOffset = x0,
                yOffset = y0,
                advance = adv * scale,
                leftSideBearing = lsb * scale,
                bitmapWidth = w,
                bitmapHeight = h
            };
        }
    }

    private unsafe void load() {
        fixed(byte* ptr = fontData) {
            fontInfo = new StbTrueType.stbtt_fontinfo();
            StbTrueType.stbtt_InitFont(fontInfo, ptr, 0);

            float rasterSize = fontSize * 2.0f;
            scale = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, rasterSize);

            int a;
            int d;
            int lg;
            StbTrueType.stbtt_GetFontVMetrics(
                fontInfo,
                &a,
                &d,
                &lg
            );
            ascent = a;
            descent = d;
            lineGap = lg;
            lineHeight = (ascent - descent + lineGap) * scale;
        }
    }
}