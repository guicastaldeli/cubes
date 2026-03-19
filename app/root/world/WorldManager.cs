namespace App.Root.Env.World;
using App.Root.Collider;

class WorldManager {
    private World world;
    private WorldBroadcaster worldBroadcaster;

    private Server? server;
    private Mesh.Mesh? mesh;
    private CollisionManager? collisionManager;

    public WorldManager(Mesh.Mesh mesh, CollisionManager collisionManager) {
        this.world = new World(mesh, collisionManager);
        this.worldBroadcaster = new WorldBroadcaster();
    }

    public void setServer(Server server) {
        this.server = server;
    }

    // Get World
    public World getWorld() {
        return world;
    }

    // Get World Broadcaster
    public WorldBroadcaster getWorldBroadcaster() {
        return worldBroadcaster;
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