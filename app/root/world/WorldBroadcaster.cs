namespace App.Root.Env.World;
using App.Root.Packets;
using App.Root.Player;

class WorldBroadcaster {
    private WorldManager worldManager;

    public WorldBroadcaster(WorldManager worldManager) {
        this.worldManager = worldManager;
    }

    public void broadcast() {
        var world = new PacketWorld();
        foreach(var player in worldManager.getServer().players.Values) {
            world.players.Add(new PlayerState {
                id = player.id,
                x = player.x,
                y = player.y,
                z = player.z,
                yaw = player.yaw,
                pitch = player.pitch
            });
        }
        foreach(var player in worldManager.getServer().players.Values) {
            worldManager.getServer().send(world, player.endPoint);
        }
    }

    public void start() {
        if(worldManager.getServer() != null) {
            worldManager.getServer().onTick = () => broadcast();
        }
    }
}