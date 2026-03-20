namespace App.Root.ServerData;
using System.Net;
using App.Root.Packets;

class ServerData {
    private Server server;

    public ServerData(Server server) {
        this.server = server;
    }

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
                player.updatePing();
            }
        }
    }
}