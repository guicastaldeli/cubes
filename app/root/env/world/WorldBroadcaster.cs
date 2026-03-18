using App.Root.Packets;
using App.Root.Player;

namespace App.Root.Env.World;

class WorldBroadcaster {
    private Server server = null!;

    public void setServer(Server server) {
        this.server = server;
    }

    public void broadcast() {
        var world = new PacketWorld();
        foreach(var player in server.players.Values) {
            world.players.Add(new PlayerState {
                id = player.id,
                x = player.x,
                y = player.y,
                z = player.z,
                yaw = player.yaw,
                pitch = player.pitch
            });
        }
        foreach(var player in server.players.Values) {
            server.send(world, player.endPoint);
        }
    }
}