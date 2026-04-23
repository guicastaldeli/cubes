namespace App.Root.ServerData;
using App.Root.Packets;
using System.Net;

class ServerChat : PacketHandler {
    private Server server;

    public ServerChat(Server server) {
        this.server = server;
        PacketController.register(this, Context.SERVER);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.CHAT;
    }

    /**
    
        Handle

        */
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketChat>(json);
        if(packet == null) return;

        foreach(var (_, player) in server.players) {
            server.send(packet, player.endPoint);
        }
    }
}