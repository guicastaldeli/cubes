namespace App.Root;
using App.Root.Player;
using App.Root.Shaders;
using App.Root.Collider;
using App.Root.Text;
using App.Root.Screen;
using App.Root.World;
using App.Root.Mesh;
using App.Root.UI;

class Scene {
    private Window window;
    private ShaderProgram shaderProgram;
    private Input input;
    private PlayerController playerController;
    private CollisionManager collisionManager;
    private Network? network;
    
    private Mesh.Mesh mesh;
    private WorldManager worldManager;

    private TextRenderer textRenderer = null!;
    private ScreenController screenController = null!;
    private UIController uiController = null!;

    public bool initialized = false;

    public Scene(
        Window window, 
        ShaderProgram shaderProgram, 
        Input input
    ) {
        this.window = window;
        this.shaderProgram = shaderProgram;
        this.input = input;

        this.mesh = new Mesh.Mesh(
            window, 
            shaderProgram, 
            input
        );
        this.playerController = new PlayerController(
            window, 
            input,
            shaderProgram, 
            mesh
        );
        this.collisionManager = new CollisionManager();

        this.worldManager = new WorldManager(
            window, 
            mesh, 
            collisionManager, 
            playerController
        );
    }

    // Is Init
    public bool isInit() {
        return initialized;
    }

    // Set Screen Controller
    public void setScreenController(ScreenController screenController) {
        this.screenController = screenController;
    }

    // Set UI Controller
    public void setUIController(UIController uiController) {
        this.uiController = uiController;
    }

    // Set Network
    public void setNetwork(Network network) {
        this.network = network;
    }

    // Get Player Controller
    public PlayerController getPlayerController() {
        return playerController;
    }

    // Get Camera
    public Camera getCamera() {
        return playerController.getCamera();
    }

    /**
    
        On Window Resize
    
        */
    public void onWindowResize(int width, int height) {
        getCamera().updateAspectRatio(width, height);
        playerController.onWindowResize(width, height);
        mesh.onWindowResize(width, height);
    }

    /**
    
        Set
    
        */
    private void set() {
        bool isClient = 
            network == null ||
            !network.isConnected || 
            network.isHost();
        
        playerController.setCollisionManager(collisionManager);
        playerController.setWorldManager(worldManager);

        mesh.setCamera(playerController.getCamera());
        mesh.setPlayerController(playerController);
        mesh.setCollisionManager(collisionManager);

        if(isClient) worldManager.render();
        mesh.initMeshInteractionController();
        
        if(network != null) {
            playerController.setNetwork(network);
            worldManager.getWorldBroadcaster().set();

            playerController.set();
        }
    }

    private void setInput() {
        input.setPlayerInputMap(playerController.getPlayerInputMap());
        input.lockMouse();
    }

    /**
    
        Update
    
        */
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

    /**
    
        Init
    
        */
    public void init() {
        reset();
        setInput();
        window.queueOnRenderThread(() => {
            mesh.init();
            set();
        });
        
        initialized = true;
    }

    /**
    
        Render
    
        */
    public void render() {
        if(!initialized) return;

        screenController.running = true;

        mesh.render();
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
        MeshRegistry.clear();

        mesh = new Mesh.Mesh(
            window, 
            shaderProgram, 
            input
        );
        collisionManager = new CollisionManager();
        playerController = new PlayerController(
            window, 
            input,
            shaderProgram, 
            mesh
        );
        
        worldManager = new WorldManager(
            window,
            mesh, 
            collisionManager, 
            playerController
        );
        if(network != null) worldManager.setNetwork(network);
    }
}