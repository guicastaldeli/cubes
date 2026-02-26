namespace App.Root;
using App.Root.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Main {
    private Window window;
    private ShaderProgram shaderProgram;
    private Camera camera;

    public Main() {
        window = new Window();
        shaderProgram = new ShaderProgram(loadShaders());
        camera = new Camera();
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

    private void set() {
        shaderProgram.bind();
        shaderProgram.setUniform("uView", camera.getView());
        shaderProgram.setUniform("uProjection", camera.getProjection());
        shaderProgram.setUniform("uModel", Matrix4.Identity);
        shaderProgram.setUniform("uColor", 1f, 0.5f, 0.2f, 1f);
        shaderProgram.unbind();
    }
    
    ///
    /// Run
    /// 
    public void run() {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        window.run();
        set();
    }
}