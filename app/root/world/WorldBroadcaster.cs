namespace App.Root.Env.World;
using App.Root.Packets;

class WorldBroadcaster {
    private WorldManager worldManager;

    public WorldBroadcaster(WorldManager worldManager) {
        this.worldManager = worldManager;
    }

    ///
    /// Broadcast
    /// 
    public void broadcast() {
        var snapshot = ServerSnapshot.getInstance().snapshot();
        var packet = PacketData.fromSnapshot(snapshot);
        foreach(var player in worldManager.getServer().players.Values) {
            worldManager.getServer().send(packet, player.endPoint);
        }
    }

    ///
    /// Start
    /// 
    public void start() {
        if(worldManager.getServer() != null) {
            worldManager.getServer().onTick = () => broadcast();
        }
    }

    // Set
    public void set() {
        var server = worldManager.getNetwork()?.getServer();
        if(server != null) {
            worldManager.setServer(server);
            start();
        }
    }
}