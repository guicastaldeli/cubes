namespace App.Root.World;
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
        var worldSnapshot = ServerSnapshot.getInstance().snapshot();

        foreach(var (id, player) in worldManager.getServer().players) {
            if(!worldSnapshot.data.ContainsKey(DataType.PLAYER)) {
                worldSnapshot.data[DataType.PLAYER] = new();
            }

            worldSnapshot.data[DataType.PLAYER].Add(new Dictionary<string, object> {
                ["id"] = player.id,
                ["x"] = player.x,
                ["y"] = player.y,
                ["z"] = player.z,
                ["yaw"] = player.yaw,
                ["pitch"] = player.pitch
            });
        }

        var packet = PacketData.fromSnapshot(worldSnapshot);
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