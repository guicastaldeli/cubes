namespace App.Root.ServerData;
using App.Root.Packets;

class ServerState {
    private Server server;

    public ServerState(Server server) {
        this.server = server;
    }

    public void handle(string json) {
        var packet = Packet.deserialize<PacketState>(json);
        if(packet?.playerId == null) return;

        if(server.players.TryGetValue(packet.playerId, out var player)) {
            player.updateState(
                packet.x, packet.y, packet.z,
                packet.yaw,
                packet.pitch
            );
            player.updatePing();
        }
    }
}