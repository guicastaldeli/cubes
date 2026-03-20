namespace App.Root.Env.World;
using App.Root.Collider;
using App.Root.Player;
using App.Root.World;

class WorldManager {
    private World world;
    private WorldBroadcaster worldBroadcaster;
    private NetworkWorld networkWorld;

    private Server server = null!;
    private Mesh.Mesh? mesh;
    private CollisionManager? collisionManager;
    private Network? network;
    private PlayerController playerController;

    public WorldManager(
        Mesh.Mesh mesh, 
        CollisionManager collisionManager,
        PlayerController playerController
    ) {
        this.playerController = playerController;

        this.world = new World(mesh, collisionManager);
        this.worldBroadcaster = new WorldBroadcaster(this);
        this.networkWorld = new NetworkWorld(this);
    }

    // Network
    public void setNetwork(Network network) {
        this.network = network;
    }

    public Network? getNetwork() {
        return network;
    }

    // Server
    public void setServer(Server server) {
        this.server = server;
    }

    public Server getServer() {
        return server;
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

    ///
    /// Render
    /// 
    public void render() {
        world.render();
    }

    ///
    /// Update
    /// 
    public void update() {
        world.update();
    }
}