using System.Collections.Concurrent;
using System.Net;
using App.Root.Packets;
using App.Root.Player;

namespace App.Root.ServerData;

class ServerPing {
    private Server server;
    private ConcurrentDictionary<string, PlayerData> players;

    public ServerPing(Server server, ConcurrentDictionary<string, PlayerData> players) {
        this.server = server;
        this.players = players;
    }

    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketPing>(json);
        if(packet?.playerId == null) return;
        if(players.TryGetValue(packet.playerId, out var player)) player.updatePing();
        server.send(packet, remote);
    }
}