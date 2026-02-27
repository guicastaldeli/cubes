namespace App.Root.Text;
using OpenTK.Graphics.OpenGL;

class Atlas {
    private int textureId;
    private int width;
    private int height;
    private byte[] atlasBuffer;
    private List<Glyph> glyphs = new();

    private int cursorX = 0;
    private int cursorY = 0;
    private int rowHeight = 0;

    public Atlas(int width, int height) {
        this.width = width;
        this.height = height;
        atlasBuffer = new byte[width * height * 4];
        createTex();
    }

    // Copy To Atlas
    private void copyToAtlas(
        byte[] bitmap,
        int x, 
        int y,
        int w, 
        int h
    ) {
        for(int row = 0; row < h; row++) {
            for(int col = 0; col < w; col++) {
                int src = row * w + col;
                int dst = ((y + row) * width + (x + col)) * 4;
                byte alpha = src < bitmap.Length ? bitmap[src] : (byte)0;
                atlasBuffer[dst] = 0xFF;
                atlasBuffer[dst+1] = 0xFF;
                atlasBuffer[dst+2] = 0xFF;
                atlasBuffer[dst+3] = alpha;
            }
        }
    }

    ///
    /// Texture
    /// 
    public int getTextureId() {
        return textureId;
    }

    private void createTex() {
        textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, atlasBuffer);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    private void updateTex() {
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, atlasBuffer);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    ///
    /// Glyph
    /// 
    public Glyph? getGlyph(char c) {
        return glyphs.Find(g => g.codepoint == c);
    }

    public bool addGlyph(FontLoader fontLoader, char c) {
        Glyph? glyph = fontLoader.loadGlyph(c);
        if(glyph == null) return false;
        if(glyph.bitmapWidth <= 0 || glyph.bitmapHeight <= 0) return false;

        if(cursorX + glyph.bitmapWidth > width) {
            cursorX = 0;
            cursorY += rowHeight;
            rowHeight = 0;
        }
        if(cursorY + glyph.bitmapHeight > height) return false;
        rowHeight = Math.Max(rowHeight, glyph.bitmapHeight);

        byte[]? bitmap = fontLoader.rasterizeGlyph(glyph);
        if(bitmap == null) return false;

        copyToAtlas(
            bitmap,
            cursorX, cursorY,
            glyph.bitmapWidth, glyph.bitmapHeight
        );

        glyph.texCoordX = (float)cursorX / width;
        glyph.texCoordY = (float)cursorY / height;
        glyph.texWidth = (float)glyph.bitmapWidth / width;
        glyph.texHeight = (float)glyph.bitmapHeight / height;
        glyph.textureId = textureId;

        cursorX += glyph.bitmapWidth;
        glyphs.Add(glyph);
        updateTex();
        return true;
    }

    // Cleanup
    public void cleanup() {
        if(textureId != 0) GL.DeleteTexture(textureId);
    }
}