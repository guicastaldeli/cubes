namespace App.Root.ServerData;
using App.Root.Env.World;
using App.Root.World;

class ServerWorldData {
    private Server server;
    public WorldManager worldManager = null!;

    public ServerWorldData(Server server) {
        this.server = server;
    }
    
    public void setWorldManager(WorldManager worldManager) {
        this.worldManager = worldManager;
    }

    public WorldData getWorldData() {
        return worldManager.getWorldData();
    }
}