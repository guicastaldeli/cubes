/**

    Text Entity main class

    */
namespace App.Root.Text;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.Screen;
using App.Root.Shaders;
using App.Root.UI;
using App.Root.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class TextEntity {
    private const string MESH = "quad";

    private string id = null!;
    private string path = null!;

    private Vector3 worldPosition;
    private float scale = 1.0f;
    private float maxDistance = 10.0f;

    private Window window;
    private ShaderProgram shaderProgram;
    private Camera? camera;
    private Mesh mesh;

    private UIData? uiData;
    private TextRenderer textRenderer => UI.textRenderer!;

    private Dictionary<string, int> elTextures = new();
    private Dictionary<string, bool> elVisibility = new();

    private bool visible = false;
    private bool initialized = false;

    public TextEntity(Window window, ShaderProgram shaderProgram, Mesh mesh) {
        this.window = window;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
    }
    public TextEntity S(
        string id,
        string path,
        Vector3 worldPosition,
        float scale = 1.0f,
        float maxDistance = 10.0f
    ) {
        this.id = id;
        this.path = path;
        this.worldPosition = worldPosition;
        this.scale = scale;
        this.maxDistance = maxDistance;

        this.uiData = DocParser.parseUI(path, window.getWidth(), window.getHeight());

        return this;
    }

    // Set Camera
    public void setCamera(Camera camera) {
        this.camera = camera;
    }

    // Get Mesh Id
    private string getMeshId(string elId) {
        string val = $"text_entity_{id}_{elId}";
        return val;
    }

    // Get Element Y Offset
    private float getElementYOffset(UIElement el) {
        float normalizedY = el.y / (float)window.getHeight();
        float val = (0.5f - normalizedY) * scale * 2.0f;
        return val;
    }

    // Get Aspect
    private float getAspect(int tex) {
        if(tex == 0) return 1.0f;

        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int w);
        GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int h);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        float val = h > 0 ? (float)w / h : 1.0f;
        return val;
    }

    // Set Visible
    public void setVisible(bool visible) {
        this.visible = visible;
        if(uiData == null) return;

        foreach(var el in uiData.elements) {
            if(skip(el)) continue;
            mesh.setVisible(getMeshId(el.id), visible);
        }
    }

    // Set Element Visible
    public void setElementVisible(params string[] ids) {
        if(uiData == null) return;

        foreach(var el in uiData.elements) {
            if(skip(el)) continue;
            elVisibility[el.id] = false;
            mesh.setVisible(getMeshId(el.id), false);
        }
        foreach(var id in ids) {
            elVisibility[id] = true;
            mesh.setVisible(getMeshId(id), visible); 
        }
    }

    // Set Matrix
    private void setMatrix(string meshId, UIElement el) {
        float yOffset = getElementYOffset(el);
        Vector3 pos = worldPosition + new Vector3(0, yOffset, 0);

        float aspect = getAspect(elTextures.TryGetValue(el.id, out int t) ? t : 0);
        Matrix4 model = Matrix4.CreateScale(scale * aspect, scale, 1.0f) * Matrix4.CreateTranslation(pos);

        mesh.setModelMatrix(meshId, model);
    }

    // Skip
    private bool skip(UIElement el) {
        bool val = el.type != "label";
        return val;
    }

    // Get Font Key
    private string getFontKey(UIElement el) {
        string key = el.fontFamily;
        float size = 64.0f;

        textRenderer.ensureFont(key, size);

        return key;
    }

    // Get Texture Size
    private (int x, int h) getTextureSize(UIElement el) {
        string fontKey = getFontKey(el);

        string resolvedText = DocParser.Resolve(el.text);
        float textWidth = textRenderer.getTextWidth(resolvedText, el.scale, fontKey);
        FontMetrics fontMetrics = textRenderer.getFontMetrics(fontKey);

        float totalHeight = (fontMetrics.ascent + fontMetrics.descent) * el.scale;
        int width = Math.Max(1, (int)textWidth);
        int height = Math.Max(1, (int)totalHeight);

        return (width, height);
    }

    // Get Texture X
    private float getTextureX(UIElement el, int width) {
        float val = width - textRenderer.getTextWidth(DocParser.Resolve(el.text), el.scale, getFontKey(el));
        return val;
    }

    // Get Texture Y
    private float getTextureY(UIElement el, int height) {
        FontMetrics metrics = textRenderer.getFontMetrics(getFontKey(el));
        float val = height - (metrics.ascent + metrics.descent) * el.scale;
        return val;
    }

    // World Position
    public Vector3 getWorldPosition() {
        return worldPosition;
    }

    public void setWorldPosition(Vector3 pos) {
        worldPosition = pos;
    }

    // Get Element by Id
    public UIElement? getElementById(string elId) {
        return uiData != null ? DocParser.getElementById(uiData, elId) : null;
    }

    // Set Scale
    public void setScale(float scale) {
        if(uiData == null) return;

        this.scale = scale;
        foreach(var el in uiData.elements) {
            if(skip(el)) continue;
            setMatrix(getMeshId(el.id), el);
        }
    }

    /**
    
        Refresh
    
        */
    public void refresh(string elId) {
        if(uiData == null) return;

        var el = DocParser.getElementById(uiData, elId);
        if(el?.template == null) return;

        updateText(elId, DocParser.Resolve(el.template));
    }

    /**
    
        Update
    
        */
    public void updateText(string elId, string text) {
        if(uiData == null) return;

        var el = DocParser.getElementById(uiData, elId);
        if(el == null) return;
        el.text = text;

        int tex = renderElementToTexture(el);
        if(elTextures.TryGetValue(elId, out int old)) GL.DeleteTexture(old);
        elTextures[elId] = tex;
        mesh.setTexture(getMeshId(elId), tex);
    }

    /**
    
        Render
    
        */
    // Render
    public void render() {
        if(!initialized || uiData == null || camera == null) {
            Console.Error.WriteLine("Render null!");
            return;
        }

        float dist = Vector3.Distance(camera.getPosition(), worldPosition);
        bool inRange = dist <= maxDistance;

        foreach(var el in uiData.elements) {
            if(skip(el)) continue;

            string meshId = getMeshId(el.id);

            bool shouldShow = visible && inRange && elVisibility.GetValueOrDefault(el.id, true);
            mesh.setVisible(meshId, shouldShow);
            if(!shouldShow) continue;

            setMatrix(meshId, el);
        }
    }

    // Render Element to Texture
    private int renderElementToTexture(UIElement el) {
        string fontKey = getFontKey(el);
        
        var (width, height) = getTextureSize(el);
        float x = getTextureX(el, width);
        float y = getTextureY(el, height);

        string resolvedText = DocParser.Resolve(el.text);

        int fbo = GL.GenFramebuffer();
        int tex = GL.GenTexture();
        int rbo = GL.GenRenderbuffer();

        float r = el.color[0];
        float g = el.color[1];
        float b = el.color[2];

        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, tex, 0);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
        
        GL.Viewport(0, 0, width, height);
        GL.ClearColor(r, g, b, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        shaderProgram.setUniformb("screenSize", (float)width, (float)height);

        textRenderer.updateScreenSize(width, height);
        textRenderer.renderText(resolvedText, x, y, el.scale, el.color, fontKey);
        
        GL.Disable(EnableCap.Blend);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.DeleteFramebuffer(fbo);
        GL.DeleteRenderbuffer(rbo);
        GL.Viewport(0, 0, window.getWidth(), window.getHeight());
    
        textRenderer.updateScreenSize(window.getWidth(), window.getHeight());

        return tex;
    }

    /**
    
        Init
    
        */
    public void init() {
        if(initialized || uiData == null) return;

        foreach(var el in uiData.elements) {
            if(skip(el)) continue;

            string meshId = getMeshId(el.id);
            MeshData data = MeshDataLoader.load(MESH);
            data.shaderType = 11;
            data.isDynamic = true;

            mesh.add(meshId, data);

            int tex = renderElementToTexture(el);
            elTextures[el.id] = tex;
            mesh.setTexture(meshId, tex);
            mesh.setVisible(meshId, false);

            setMatrix(meshId, el);
        }

        initialized = true;
    }

    /**
    
        Cleanup
    
        */
    public void cleanup() {
        foreach(var tex in elTextures.Values) GL.DeleteTexture(tex);
        elTextures.Clear();

        if(uiData != null) {
            foreach(var el in uiData.elements) {
                mesh.remove(getMeshId(el.id));
            }
        }
    }
}