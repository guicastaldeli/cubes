namespace App.Root.ServerData;
using App.Root.Packets;
using System.Net;

class ServerPlayerData : PacketHandler {
    private Server server;

    public ServerPlayerData(Server server) {
        this.server = server;
        PacketController.register(this, Context.SERVER);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.DATA;
    }

    // Handle
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketData>(json);
        if(packet == null) return;

        var snapshot = packet.toSnapshot();

        foreach(var entry in snapshot.get(DataType.PLAYER)) {
            string? id = entry["id"] as string;
            if(id == null) continue;
            if(server.players.TryGetValue(id, out var player)) {
                player.x = Convert.ToSingle(entry["x"]);
                player.y = Convert.ToSingle(entry["y"]);
                player.z = Convert.ToSingle(entry["z"]);
                player.yaw = Convert.ToSingle(entry["yaw"]);
                player.pitch = Convert.ToSingle(entry["pitch"]);
                if(entry.TryGetValue("username", out var u) && u is string username) player.username = username;
                player.updatePing();
            }
        }
    }
}