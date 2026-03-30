namespace App.Root.ServerData;
using App.Root.Packets;
using System.Net;

class ServerVoice {
    private Server server;

    public ServerVoice(Server server) {
        this.server = server;
    }

    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketVoice>(json);
        if(packet == null || packet.audio == null) return;

        foreach(var (id, player) in server.players) {
            if(id == packet.playerId) continue;
            server.send(packet, player.endPoint);
        }
    }
}