namespace App.Root.Text;
using OpenTK.Mathematics;
using App.Root.Shaders;
using OpenTK.Graphics.OpenGL;

class TextRenderer {
    private ShaderProgram shaderProgram;
    private int screenWidth;
    private int screenHeight;

    private Dictionary<string, FontLoader> fontLoaders = new();
    private Dictionary<string, Atlas> atlases = new();
    private Dictionary<string, Dictionary<char, Glyph>> glyphCaches = new();

    private string currentFont = "default";
    private int vao;
    private int vbo;
    private int ebo;

    private static readonly int ATLAS_SIZE = 512;
    private static readonly string FONT_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource/font/");

    public TextRenderer(
        ShaderProgram shaderProgram,
        int screenWidth,
        int screenHeight
    ) {
        this.shaderProgram = shaderProgram;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        setupBuffers();
    }

    // Preload Chars
    private void preloadChars(string key) {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,!?:;-_/\\()[]{}@#$%^&*+=<>|~ ";
        foreach(char c in chars) loadGlyphToAtlas(c, key);
    }

    // Get Font Metrics
    public FontMetrics getFontMetrics(string fontKey = "default") {
        return fontLoaders[fontKey].getFontMetrics();
    }

    // Get Text Width
    public float getTextWidth(string text, float scale, string fontKey = "default") {
        if(!fontLoaders.ContainsKey(fontKey)) return 0f;

        var cache = glyphCaches[fontKey];
        var fontLoader = fontLoaders[fontKey];
        float width = 0f;

        for(int i = 0; i < text.Length; i++) {
            if(!cache.TryGetValue(text[i], out var glyph)) continue;
            width += glyph.advance * scale;
            if(i < text.Length - 1 && cache.TryGetValue(text[i + 1], out var next)) {
                width += fontLoader.getKerning(glyph.glypthIndex, next.glypthIndex) * scale;
            }
        }

        return width;
    }

    ///
    /// Setup
    ///
    private void setupBuffers() {
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        ebo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 4 * 8 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(3);

        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 4 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        int[] indices = { 0, 1, 2, 2, 3, 0 };
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
    }

    ///
    /// Update
    ///
    private void updateQuad(float x, float y, float w, float h, Glyph glyph, float[] color) {
        float r = color[0], g = color[1], b = color[2], a = color.Length > 3 ? color[3] : 1.0f;
        float[] verts = {
            x,     y + h, glyph.texCoordX,                  glyph.texCoordY + glyph.texHeight, r, g, b, a,
            x,     y,     glyph.texCoordX,                  glyph.texCoordY,                   r, g, b, a,
            x + w, y,     glyph.texCoordX + glyph.texWidth, glyph.texCoordY,                   r, g, b, a,
            x + w, y + h, glyph.texCoordX + glyph.texWidth, glyph.texCoordY + glyph.texHeight, r, g, b, a,
        };
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    private void updateQuadBillboard(float localX, float localY, float w, float h, Glyph glyph, float[] color) {
        float r = color[0], g = color[1], b = color[2], a = color.Length > 3 ? color[3] : 1.0f;
        float[] verts = {
            localX,     localY + h, glyph.texCoordX,                  glyph.texCoordY + glyph.texHeight, r, g, b, a,
            localX,     localY,     glyph.texCoordX,                  glyph.texCoordY,                   r, g, b, a,
            localX + w, localY,     glyph.texCoordX + glyph.texWidth, glyph.texCoordY,                   r, g, b, a,
            localX + w, localY + h, glyph.texCoordX + glyph.texWidth, glyph.texCoordY + glyph.texHeight, r, g, b, a,
        };
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void updateScreenSize(int w, int h) {
        screenWidth = w;
        screenHeight = h;
    }

    ///
    /// Render
    ///
    public void renderText(
        string text,
        float x,
        float y,
        float scale,
        float[] color,
        string fontKey = "default"
    ) {
        renderTextWithShadow(text, x, y, scale, color, 0, 0, 0, new float[]{ 0, 0, 0, 0 }, fontKey);
    }

    public void renderTextWithShadow(
        string text,
        float x, float y,
        float scale,
        float[] color,
        float shadowOffsetX, float shadowOffsetY,
        float shadowBlur,
        float[] shadowColor,
        string fontKey = "default"
    ) {
        if(string.IsNullOrEmpty(text)) return;
        if(!fontLoaders.ContainsKey(fontKey)) return;
        currentFont = fontKey;

        bool depthTest = GL.IsEnabled(EnableCap.DepthTest);
        if(depthTest) GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        shaderProgram.bind();
        shaderProgram.setUniform("shaderType", 1);
        shaderProgram.setUniform("screenSize", (float)screenWidth, (float)screenHeight);
        shaderProgram.setUniform("uSampler", 0);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, atlases[currentFont].getTextureId());
        GL.BindVertexArray(vao);

        FontMetrics metrics = fontLoaders[currentFont].getFontMetrics();
        float baseline = y + metrics.ascent * scale;

        if(shadowBlur > 0) {
            int blurPasses = Math.Min(5, (int)shadowBlur);
            for(int pass = 0; pass < blurPasses; pass++) {
                float blurOffset = pass * 0.5f;
                renderTextPass(text, x + shadowOffsetX - blurOffset, baseline + shadowOffsetY - blurOffset, scale, shadowColor);
                renderTextPass(text, x + shadowOffsetX + blurOffset, baseline + shadowOffsetY - blurOffset, scale, shadowColor);
                renderTextPass(text, x + shadowOffsetX - blurOffset, baseline + shadowOffsetY + blurOffset, scale, shadowColor);
                renderTextPass(text, x + shadowOffsetX + blurOffset, baseline + shadowOffsetY + blurOffset, scale, shadowColor);
            }
        } else {
            renderTextPass(text, x + shadowOffsetX, baseline + shadowOffsetY, scale, shadowColor);
        }

        renderTextPass(text, x, baseline, scale, color);

        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        if(depthTest) GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Blend);
        shaderProgram.unbind();
    }

    private void renderTextPass(string text, float startX, float baseline, float scale, float[] color) {
        shaderProgram.setUniform("uColor", color[0], color[1], color[2], color.Length > 3 ? color[3] : 1.0f);

        float cursorX = startX;
        var cache = glyphCaches[currentFont];
        var fontLoader = fontLoaders[currentFont];

        for(int i = 0; i < text.Length; i++) {
            char c = text[i];
            Glyph? glyph = loadGlyphToAtlas(c, currentFont);
            if(glyph == null) continue;

            float xPos = cursorX + glyph.leftSideBearing * scale;
            float yPos = baseline + glyph.yOffset;
            float w = glyph.bitmapWidth * scale;
            float h = glyph.bitmapHeight * scale;

            if(w > 0 && h > 0) {
                updateQuad(xPos, yPos, w, h, glyph, color);
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            }

            cursorX += glyph.advance * scale;
            if(i < text.Length - 1 && cache.TryGetValue(text[i + 1], out var next)) {
                cursorX += fontLoader.getKerning(glyph.glypthIndex, next.glypthIndex) * scale;
            }
        }
    }

    public void renderTextBillboard(
        string text,
        Vector3 worldPos,
        Matrix4 view,
        Matrix4 projection
    ) {
        if(string.IsNullOrEmpty(text)) return;

        float scale = 0.05f;
        float[] color = { 1.0f, 1.0f, 1.0f, 1.0f };

        bool depthTest = GL.IsEnabled(EnableCap.DepthTest);
        if(depthTest) GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);        

        shaderProgram.bind();
        shaderProgram.setUniform("shaderType", 4);
        shaderProgram.setUniform("hasTex", 1); 
        shaderProgram.setUniform("uSampler", 0);
        shaderProgram.setUniform("uView", view);
        shaderProgram.setUniform("uProjection", projection);
        shaderProgram.setUniform("uColor", 
            color[0], 
            color[1], 
            color[2], 
            color.Length > 3 ? color[3] : 1.0f
        );

        var model = Matrix4.CreateTranslation(worldPos);
        shaderProgram.setUniform("uModel", model);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, atlases[currentFont].getTextureId());
        GL.BindVertexArray(vao);

        float totalWidth = getTextWidth(text, scale);
        float cursorX = -totalWidth / 2.0f;

        var cache = glyphCaches[currentFont];
        var fontLoader = fontLoaders[currentFont];

        for(int i = 0; i < text.Length; i++) {
            char c = text[i];
            Glyph? glyph = loadGlyphToAtlas(c, currentFont);
            if(glyph == null) continue;

            float localX = cursorX + glyph.leftSideBearing * scale;
            float localY = glyph.yOffset * scale;
            float w = glyph.bitmapWidth * scale;
            float h = glyph.bitmapHeight * scale;

            if(w > 0 && h > 0) {
                updateQuadBillboard(localX, localY, w, h, glyph, color);
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            }

            cursorX += glyph.advance * scale;
            if(i < text.Length - 1 && cache.TryGetValue(text[i+1], out var next)) {
                cursorX += fontLoader.getKerning(glyph.glypthIndex, next.glypthIndex) * scale;
            } 

        }

        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        if(depthTest) GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Blend);
        shaderProgram.setUniform("shaderType", 0);
        shaderProgram.unbind();
    }

    ///
    /// Load
    ///
    public void loadFont(string key, string fileName, float size = 16.0f) {
        if(fontLoaders.ContainsKey(key)) return;
        string path = Path.Combine(FONT_DIR, fileName);

        FontLoader fontLoader = new FontLoader(path, size);
        Atlas atlas = new Atlas(ATLAS_SIZE, ATLAS_SIZE);

        fontLoaders[key] = fontLoader;
        atlases[key] = atlas;
        glyphCaches[key] = new Dictionary<char, Glyph>();

        preloadChars(key);
    }

    private Glyph? loadGlyphToAtlas(char c, string fontKey) {
        var cache = glyphCaches[fontKey];
        if(cache.TryGetValue(c, out var cached)) return cached;
        atlases[fontKey].addGlyph(fontLoaders[fontKey], c);

        Glyph? glyph = atlases[fontKey].getGlyph(c);
        if(glyph != null) cache[c] = glyph;
        return glyph;
    }

    // Cleanup
    public void cleanup() {
        if(vao != 0) GL.DeleteVertexArray(vao);
        if(vbo != 0) GL.DeleteBuffer(vbo);
        if(ebo != 0) GL.DeleteBuffer(ebo);
        foreach(var a in atlases.Values) a.cleanup();
    }
}