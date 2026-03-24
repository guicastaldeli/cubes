namespace App.Root;
using App.Root.Player;
using App.Root.Shaders;
using App.Root.Collider;
using App.Root.Text;
using App.Root.Screen;
using App.Root.World;

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

        this.mesh = new Mesh.Mesh(shaderProgram);
        this.playerController = new PlayerController(window, mesh);
        this.collisionManager = new CollisionManager();

        this.worldManager = new WorldManager(
            window, 
            mesh, 
            collisionManager, 
            playerController
        );
    }

    public bool isInit() {
        return initialized;
    }

    // Set Screen Controller
    public void setScreenController(ScreenController screenController) {
        this.screenController = screenController;
    }

    // Set Network
    public void setNetwork(Network network) {
        this.network = network;
    }

    ///
    /// Set
    /// 
    private void set() {
        playerController.setCollisionManager(collisionManager);

        mesh.setCamera(playerController.getCamera());

        if(network == null ||
            !network.isConnected || 
            network.isHost()
        ) {
            worldManager.render();
        }
        
        if(network != null) {
            playerController.setNetwork(network);
            worldManager.getWorldBroadcaster().set();
        }
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

        network?.pollAndCache();
        NetworkUpdate.getInstance().update();
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
        if(!initialized) return;

        screenController.running = true;

        mesh.renderAll();
    }

    ///
    /// Reset
    /// 
    public void reset() {
        initialized = false;
        screenController.running = false;

        NetworkUpdate.clear();
        Data.getInstance().clearAll();
        ServerSnapshot.getInstance().clearAll();

        mesh = new Mesh.Mesh(shaderProgram);
        collisionManager = new CollisionManager();
        playerController = new PlayerController(window, mesh);
        
        worldManager = new WorldManager(
            window,
            mesh, 
            collisionManager, 
            playerController
        );
        if(network != null) worldManager.setNetwork(network);
    }
}