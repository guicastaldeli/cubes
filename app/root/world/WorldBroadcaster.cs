namespace App.Root.World;
using App.Root.Packets;

class WorldBroadcaster {
    private WorldManager worldManager;

    public WorldBroadcaster(WorldManager worldManager) {
        this.worldManager = worldManager;
    }

    /**
     * 
     * Set
     *
     */
    public void set() {
        var server = worldManager.getNetwork()?.getServer();
        var client = worldManager.getNetwork()?.getClient();
        
        if(server != null && client != null) {
            worldManager.setServer(server);
            worldManager.setClient(client);
            start();
        }
    }

    /**
     * 
     * Broadcast
     *
     */
    public void broadcast() {
        
    }

    /**
     * 
     * Start
     *
     */
    public void start() {
        if(worldManager.getServer() != null) {
            worldManager.getServer()!.onTick = () => broadcast();
        }
    }
}