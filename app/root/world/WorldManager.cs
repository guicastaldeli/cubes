namespace App.Root.World;
using App.Root.Collider;
using App.Root.Player;
using App.Root.Shaders;

class WorldManager {
    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Mesh.Mesh? mesh;
    private CollisionManager? collisionManager;
    private PlayerController playerController;
    private TimeCycle timeCycle;

    private Network? network;
    private Server? server;
    private Client? client;

    private World world;
    private WorldBroadcaster worldBroadcaster;
    private NetworkWorld networkWorld;

    public WorldManager(
        Window window,
        Tick tick,
        ShaderProgram shaderProgram,
        Mesh.Mesh mesh, 
        CollisionManager collisionManager,
        PlayerController playerController,
        TimeCycle timeCycle
    ) {
        this.window = window;
        this.tick = tick;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.playerController = playerController;
        this.timeCycle = timeCycle;

        this.world = new World(window, tick, shaderProgram, mesh, this, collisionManager, timeCycle);
        this.worldBroadcaster = new WorldBroadcaster(this);
        this.networkWorld = new NetworkWorld(this);
    }

    // Get Player Controller
    public PlayerController getPlayerController() {
        return playerController;
    }

    // Network
    public void setNetwork(Network network) {
        this.network = network;
        networkWorld.setNetwork(network);
    }

    public Network? getNetwork() {
        return network;
    }

    // Server
    public void setServer(Server server) {
        this.server = server;
    }

    public Server? getServer() {
        return server;
    }

    // Client
    public void setClient(Client client) {
        this.client = client;
    }

    public Client? getClient() {
        return client;        
    }

    // Get World
    public World getWorld() {
        return world;
    }

    // Get World Broadcaster
    public WorldBroadcaster getWorldBroadcaster() {
        return worldBroadcaster;
    }

    // Get Network World
    public NetworkWorld getNetworkWorld() {
        return networkWorld;
    }

    // Get Window
    public Window getWindow() {
        return window;
    }

    // Get Collision Manager
    public CollisionManager? getCollisionManager() {
        return collisionManager;
    }
    
    /**
    
        Render

        */ 
    public void render() {
        world.render();
    }

    /**
    
        Update
    
        */
    public void update() {
        world.update();
    }
}