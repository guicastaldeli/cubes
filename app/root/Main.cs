namespace App.Root;
using App.Root.Shaders;
using OpenTK.Graphics.OpenGL;

class Main {
    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Input input;

    private Scene scene = null!;

    public Main() {
        window = new Window();
        tick = new Tick();
        shaderProgram = new ShaderProgram(loadShaders());
        input = new Input(window, tick);

        init();
        set();
    }

    // Load Shaders
    private List<ShaderModule> loadShaders() {
        return new List<ShaderModule> {
            new ShaderModule(ShaderType.VertexShader, "vert.glsl"),
            new ShaderModule(ShaderType.FragmentShader, "frag.glsl")
        };
    }

    ///
    /// Init
    /// 
    public void init() {
        scene = new Scene(shaderProgram, input);
    }

    ///
    /// Set
    /// 
    private void set() {
        GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
        GL.Viewport(0, 0, Window.WIDTH, Window.HEIGHT);

        GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
    }

    ///
    /// Update
    /// 
    private void update() {
        tick.update();
        window.updateTitle(tick.getTickCount(), tick.getFps());
        scene.update();
    }

    /// 
    /// Render
    /// 
    private void render() {
        scene.render();
    }
    
    ///
    /// Run
    /// 
    public void run() {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        window.run(() => {
            render();
            update();
        });
    }
}