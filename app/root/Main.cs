namespace App.Root;
using App.Root.Shaders;
using OpenTK.Graphics.OpenGL;

class Main {
    private Window window;
    private ShaderProgram shaderProgram;

    public Main() {
        window = new Window();
        shaderProgram = new ShaderProgram(loadShaders());
        render();
    }

    // Load Shaders
    private List<ShaderModule> loadShaders() {
        return new List<ShaderModule> {
            new ShaderModule(ShaderType.VertexShader, "vert.glsl"),
            new ShaderModule(ShaderType.FragmentShader, "frag.glsl")
        };
    }

    /// 
    /// Render
    /// 
    private void render() {
        GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
        GL.Viewport(0, 0, Window.WIDTH, Window.HEIGHT);
    }
    
    ///
    /// Run
    /// 
    public void run() {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        window.run();
    }
}