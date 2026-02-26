namespace App.Root;

using App.Root.Mesh;
using App.Root.Shaders;

class Scene {
    private ShaderProgram shaderProgram;
    private Camera camera;

    private GetMesh mesh;

    public Scene(ShaderProgram shaderProgram) {
        this.shaderProgram = shaderProgram;
        camera = new Camera();
        mesh = new GetMesh(shaderProgram);
        init();
    }

    ///
    /// Set
    /// 
    private void set() {
        mesh.setCamera(camera);
        mesh.add("cube");
        mesh.setPosition("cube", 0.0f, 0.0f, -3.0f);
    }

    ///
    /// Update
    /// 
    public void update() {
        mesh.update();
    }

    ///
    /// Init
    /// 
    private void init() {
        set();
    }

    /// 
    /// Render
    /// 
    public void render() {
        mesh.renderAll();
    }
}