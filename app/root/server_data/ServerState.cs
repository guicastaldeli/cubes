using System.Collections.Concurrent;
using System.Net;
using App.Root.Packets;
using App.Root.Player;

namespace App.Root.ServerData;

class ServerState {
    private Server server;
    private ConcurrentDictionary<string, PlayerData> players;

    public ServerState(Server server, ConcurrentDictionary<string, PlayerData> players) {
        this.server = server;
        this.players = players;
    }

    public void handle(string json) {
        var packet = Packet.deserialize<PacketState>(json);
        if(packet?.playerId == null) return;

        if(players.TryGetValue(packet.playerId, out var player)) {
            player.updateState(
                packet.x, packet.y, packet.z,
                packet.yaw,
                packet.pitch
            );
            player.updatePing();
        }
    }
}