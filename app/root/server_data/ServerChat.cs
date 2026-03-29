namespace App.Root.ServerData;
using App.Root.Packets;
using System.Net;

class ServerChat {
    private Server server;

    public ServerChat(Server server) {
        this.server = server;
    }

    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketChat>(json);
        if(packet == null) return;

        foreach(var (_, player) in server.players) {
            server.send(packet, player.endPoint);
        }
    }
}