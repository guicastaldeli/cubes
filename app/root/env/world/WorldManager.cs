namespace App.Root.Env.World;

class WorldManager {
    private World world;
    private WorldBroadcaster worldBroadcaster;

    private Server server = null!;

    public WorldManager() {
        this.world = new World();
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
}