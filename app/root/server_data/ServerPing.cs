namespace App.Root.ServerData;
using App.Root.Packets;
using System.Net;

class ServerPing : PacketHandler {
    private Server server;

    public ServerPing(Server server) {
        this.server = server;
        PacketController.register(this, Context.SERVER);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.PING;
    }

    // Handle
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketPing>(json);
        if(packet?.playerId == null) return;
        if(server.players.TryGetValue(packet.playerId, out var player)) player.updatePing();
        server.send(packet, remote);
    }
}