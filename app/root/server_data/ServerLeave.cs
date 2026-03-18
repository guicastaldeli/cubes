using System.Collections.Concurrent;
using System.Net;
using App.Root.Packets;
using App.Root.Player;

namespace App.Root.ServerData;

class ServerLeave {
    private Server server;
    private ConcurrentDictionary<string, PlayerData> players;

    public ServerLeave(Server server, ConcurrentDictionary<string, PlayerData> players) {
        this.server = server;
        this.players = players;
    }

    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketLeave>(json);
        if(packet?.playerId == null) return;

        players.TryRemove(packet.playerId, out _);
        Console.WriteLine($"Player {packet.playerId} left");
    }
}