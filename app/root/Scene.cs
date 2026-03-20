namespace App.Root;
using App.Root.Player;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Resource;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Text;
using App.Root.Screen;
using App.Root.Env;
using App.Root.Env.World;

class Scene {
    private Window window;
    private ShaderProgram shaderProgram;
    private Input input;
    private PlayerController playerController;
    private CollisionManager collisionManager;
    private ScreenController screenController = null!;

    private Mesh.Mesh mesh;
    private TextRenderer textRenderer = null!;

    private WorldManager worldManager;

    private Network? network;

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
        this.mesh = new Mesh.Mesh(shaderProgram);

        this.worldManager = new WorldManager(mesh, collisionManager, playerController);
    }

    public bool isInit() {
        return initialized;
    }

    public void setScreenController(ScreenController screenController) {
        this.screenController = screenController;
    }

    public void setNetwork(Network network) {
        this.network = network;
    }

    ///
    /// Set
    /// 
    private void set() {
        playerController.setCollisionManager(collisionManager);

        mesh.setCamera(playerController.getCamera());

        worldManager.render();
        worldManager.getNetworkWorld().set();
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
        worldManager.update();

        playerController.getNetworkPlayer()?.update();
    }

    ///
    /// Init
    /// 
    public void init() {
        reset();
        setInput();
        window.queueOnRenderThread(() => set());
        
        initialized = true;
    }

    /// 
    /// Render
    /// 
    public void render() {
        screenController.running = true;

        mesh.renderAll();

    }

    ///
    /// Reset
    /// 
    public void reset() {
        initialized = false;
        playerController = new PlayerController();
        collisionManager = new CollisionManager();
        mesh = new Mesh.Mesh(shaderProgram);
        worldManager = new WorldManager(mesh, collisionManager, playerController);
    }
}