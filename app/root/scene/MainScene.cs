namespace App.Root.Scene;
using App.Root.Player;
using App.Root.Shaders;
using App.Root.Collider;
using App.Root.Text;
using App.Root.Screen;
using App.Root.World;
using App.Root.Mesh;
using App.Root.UI;
using App.Root.Input;

class MainScene {
    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Input input;
    private PlayerController playerController;
    private CollisionManager collisionManager;
    private Network? network;
    
    private TimeCycle timeCycle;
    private Mesh mesh;
    private WorldManager worldManager;

    private TextRenderer textRenderer = null!;
    private ScreenController screenController = null!;
    private UIController uiController = null!;

    public bool initialized = false;

    private float SHOW_DELAY = 1.0f;
    private float SHOW_TIMER = 0.0f;
    private bool ReadyToShow = false;

    public MainScene(
        Window window, 
        Tick tick,
        ShaderProgram shaderProgram, 
        Input input,
        Mesh mesh
    ) {
        this.window = window;
        this.tick = tick;
        this.shaderProgram = shaderProgram;
        this.input = input;
        this.mesh = mesh;

        this.timeCycle = new TimeCycle(tick);
        window.setTimeCycle(timeCycle);

        this.playerController = new PlayerController(
            window, 
            input,
            shaderProgram, 
            mesh
        );

        this.collisionManager = new CollisionManager();

        this.worldManager = new WorldManager(
            window, 
            tick,
            shaderProgram,
            mesh, 
            collisionManager, 
            playerController,
            timeCycle,
            playerController.getCamera()
        );
    }

    // Is Init
    public bool isInit() {
        return initialized;
    }

    // Get Tick
    public Tick getTick() {
        return tick;
    }

    // Screen Controller
    public void setScreenController(ScreenController screenController) {
        this.screenController = screenController;
    }

    public ScreenController getScreenController() {
        return screenController;
    }

    // UI Controller
    public void setUIController(UIController uiController) {
        this.uiController = uiController;
    }

    public UIController getUIController() {
        return uiController;
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
     * 
     * On Window Resize
     *
     */
    public void onWindowResize(int width, int height) {
        getCamera().updateAspectRatio(width, height);
        mesh.onWindowResize(width, height);
    }

    /**
     * 
     * Set
     *
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
        input.setPlayerInput(playerController.getPlayerInput());
        input.lockMouse();
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        timeCycle.update();
        
        input.update();
        playerController.getCamera().update();

        worldManager.update();
        mesh.update();

        playerController.update();

        playerController.getNetworkPlayer()?.update();

        network?.pollAndCache();
        NetworkUpdate.getInstance().update();

        updateShow();
    }

    private void updateShow() {
        if(!ReadyToShow) {
            SHOW_TIMER += tick.getDeltaTime();
            if(SHOW_TIMER >= SHOW_DELAY) {
                ReadyToShow = true;
            }   
        }
    }

    /**
     * 
     * Init
     *
     */
    public void init() {
        if(initialized) reset();
        setInput();
        window.queueOnRenderThread(() => {
            mesh.init();
            set();
        });
        
        initialized = true;
    }

    /**
     * 
     * Render
     *
     */
    public void render() {
        if(!initialized) return;

        screenController.running = true;
        if(!ReadyToShow) return;

        mesh.render();
        playerController.render();
    }

    /**
     * 
     * Reset
     *
     */
    public void reset() {
        initialized = false;
        ReadyToShow = false;
        SHOW_TIMER = 0.0f;
        screenController.running = false;

        Controller.Reset();
    }
}