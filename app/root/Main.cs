namespace App.Root;
using App.Root.Shaders;
using OpenTK.Graphics.OpenGL;

class Main {
    private Window window;
    private ShaderProgram shaderProgram;
    private Scene scene;

    public Main() {
        window = new Window();
        shaderProgram = new ShaderProgram(loadShaders());
        scene = new Scene(shaderProgram);

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
        scene.getCamera().update();
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