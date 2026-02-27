namespace App.Root;
using App.Root.Player;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Resource;
using App.Root.Collider;
using App.Root.Collider.Types;

class Scene {
    private ShaderProgram shaderProgram;
    private Input input;
    private PlayerController playerController;
    private CollisionManager collisionManager;

    private GetMesh mesh;

    public Scene(ShaderProgram shaderProgram, Input input) {
        this.shaderProgram = shaderProgram;
        this.input = input;
        playerController = new PlayerController();
        collisionManager = new CollisionManager();
        mesh = new GetMesh(shaderProgram);
        init();
    }

    ///
    /// Set
    /// 
    private void set() {
        playerController.setCollisionManager(collisionManager);

        mesh.setCamera(playerController.getCamera());
        mesh.add("cube");
        mesh.setPosition("cube", 0.0f, 0.0f, -3.0f);

        int texId = TextureLoader.load("env/test.jpg");
        mesh.setTexture("cube", texId);

        collisionManager.addStaticCollider(new BoundaryObject(5.0f));
        collisionManager.addStaticCollider(new StaticObject(mesh.getBBox("cube"), "cube"));
    }

    ///
    /// Update
    /// 
    public void update() {
        input.update();
        playerController.update();
        playerController.getCamera().update();
        mesh.update();
    }

    ///
    /// Init
    /// 
    private void init() {
        input.setPlayerInputMap(playerController.getPlayerInputMap());
        input.lockMouse();
        
        set();
    }

    /// 
    /// Render
    /// 
    public void render() {
        mesh.renderAll();
    }
}