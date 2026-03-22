namespace App.Root.ServerData;
using System.Net;
using App.Root.Packets;

class ServerLeave {
    private Server server;

    public ServerLeave(Server server) {
        this.server = server;
    }

    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketLeave>(json);
        if(packet?.playerId == null) return;

        server.players.TryRemove(packet.playerId, out _);

        string color = "\e[0;31m";
        Console.WriteLine($"{color}Player {packet.playerId} left");
    }
}