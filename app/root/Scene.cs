namespace App.Root;
using App.Root.Player;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Resource;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Text;

class Scene {
    private Window window;
    private ShaderProgram shaderProgram;
    private Input input;
    private PlayerController playerController;
    private CollisionManager collisionManager;

    private GetMesh mesh;
    private TextRenderer textRenderer = null!;

    public bool initialized = false;

    public Scene(
        Window window, 
        ShaderProgram shaderProgram, 
        Input input
    ) {
        this.window = window;
        this.shaderProgram = shaderProgram;
        this.input = input;
        this.playerController = new PlayerController();
        this.collisionManager = new CollisionManager();
        this.mesh = new GetMesh(shaderProgram);
    }

    public bool isInit() {
        return initialized;
    }

    ///
    /// Set
    /// 
    private void set() {
        playerController.setCollisionManager(collisionManager);

        mesh.setCamera(playerController.getCamera());

        // Test Cube
            mesh.add("cube");
            mesh.setPosition("cube", 0.0f, 0.0f, -3.0f);

            int texId = TextureLoader.load("env/test.jpg");
            mesh.setTexture("cube", texId);

            collisionManager.addStaticCollider(new BoundaryObject(5.0f));
            collisionManager.addStaticCollider(new StaticObject(mesh.getBBox("cube"), "cube"));
        //
    }

    private void setInput() {
        input.setPlayerInputMap(playerController.getPlayerInputMap());
        input.lockMouse();
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
    public void init() {
        setInput();
        window.queueOnRenderThread(() => set());
        
        initialized = true;
    }

    /// 
    /// Render
    /// 
    public void render() {
        mesh.renderAll();
    }
}