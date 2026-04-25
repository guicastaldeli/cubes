namespace App.Root.World;
using App.Root.Collider;
using App.Root.Player;

class WorldManager {
    private Window window;
    private Server? server;
    private Client? client;
    private Mesh.Mesh? mesh;
    private CollisionManager? collisionManager;
    private Network? network;
    private PlayerController playerController;
    
    private World world;
    private WorldBroadcaster worldBroadcaster;
    private NetworkWorld networkWorld;

    public WorldManager(
        Window window,
        Mesh.Mesh mesh, 
        CollisionManager collisionManager,
        PlayerController playerController
    ) {
        this.window = window;
        this.collisionManager = collisionManager;
        this.playerController = playerController;

        this.world = new World(window, mesh, this, collisionManager);
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