namespace App.Root.World;
using App.Root.Packets;

class WorldBroadcaster {
    private WorldManager worldManager;

    public WorldBroadcaster(WorldManager worldManager) {
        this.worldManager = worldManager;
    }

    /**
    
        Set

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
    
        Broadcast

        */
    public void broadcast() {
        var serverSnapshot = ServerSnapshot.getInstance().snapshot();
        var worldSnapshot = Data.getInstance().snapshot();

        foreach(var (type, list) in worldSnapshot.data) {
            if(!serverSnapshot.data.ContainsKey(type)) {
                serverSnapshot.data[type] = new();
            }
            serverSnapshot.data[type].AddRange(list);
        }

        var packet = PacketData.fromSnapshot(serverSnapshot);
        foreach(var player in worldManager.getServer()!.players.Values) {
            worldManager.getServer()!.send(packet, player.endPoint);
        }
    }

    /**
    
        Start

        */
    public void start() {
        if(worldManager.getServer() != null) {
            worldManager.getServer()!.onTick = () => broadcast();
        }
    }
}