namespace App.Root.Player.Aim;
using App.Root.Mesh;
using App.Root.Resource;
using App.Root.UI;
using OpenTK.Graphics.OpenGL;

class Aim : UI {
    private static string ID = "aim";
    private static string TEX_PATH = "player/hud/aim.png";
    private static string MESH = "quad"; 

    private int width = 18;
    private int height = 18;

    private bool initialized = false;

    private int screenTexHandle = 0;

    public Aim() : base(ID) {
        EnableGeneration = true;
    }

    // Set
    private void set() {
        setShader();

        int texId = TextureLoader.load(TEX_PATH);

        MeshData data = MeshDataLoader.load(MESH);
        mesh.add(ID, data);
        data.shaderType = 10;
        data.shaderAddon = -1;
        data.screenTexOverride = screenTexHandle;
        data.enableInverted = true;
        data.invertedValue = 1;

        var renderer = mesh.getMeshRenderer(ID);
        if(renderer != null) renderer.isHud = true;

        mesh.setTexture(ID, texId);
        mesh.setScale(ID, width, height, 1.0f);
        mesh.setPosition(
            ID,
            screenWidth / 2.0f - width / 2.0f,
            screenHeight / 2.0f - height / 2.0f,
            0.0f
        );
    }

    private void setShader() {
        screenTexHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, screenTexHandle);
        GL.TexImage2D(
            TextureTarget.Texture2D, 0,
            PixelInternalFormat.Rgb,
            screenWidth, screenHeight,
            0, PixelFormat.Rgb, PixelType.UnsignedByte,
            IntPtr.Zero
        );
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    /**
     * 
     * On Window Resize
     *
     */
    public override void onWindowResize(int width, int height) {
        var data = mesh.getData(ID);
        if(data != null) data.screenTexOverride = screenTexHandle;

        screenWidth = width;
        screenHeight = height;

        if(screenTexHandle != 0) {
            GL.BindTexture(TextureTarget.Texture2D, screenTexHandle);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgb,
                screenWidth, screenHeight,
                0, PixelFormat.Rgb, PixelType.UnsignedByte,
                IntPtr.Zero
            );
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        if(initialized) updatePosition();
        base.onWindowResize(width, height);
    }

    /**
     * 
     * Generate
     *
     */
    public override void generate() {
        if(initialized && mesh.getMeshRenderer(ID) == null) {
            initialized = false;
        }
        if(!initialized) {
            set();
            initialized = true;
        }
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        base.render();
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        base.update();

        var renderer = mesh.getMeshRenderer(ID);
        if(renderer != null) {
            bool paused = input.onPauseOverlayOpen();
            renderer.setVisible(!paused);
        }
    }

    public void updatePosition() {
        mesh.setScale(ID, width, height, 1.0f);
        mesh.setPosition(
            ID,
            screenWidth / 2.0f - width / 2.0f,
            screenHeight / 2.0f - height / 2.0f,
            0.0f
        );
    }
}