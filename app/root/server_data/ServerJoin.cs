namespace App.Root.ServerData;
using System.Collections.Concurrent;
using System.Net;
using App.Root.Packets;
using App.Root.Player;

class ServerJoin {
    private Server server;

    private ConcurrentDictionary<string, PlayerData> players;
    private int maxPlayers;

    public ServerJoin(
        Server server,
        ConcurrentDictionary<string, PlayerData> players,
        int maxPlayers
    ) {
        this.server = server;
        this.players = players;
        this.maxPlayers = maxPlayers;
    }

    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketJoin>(json);
        if(packet == null) return;

        if(players.Count >= maxPlayers) {
            Console.WriteLine("Server full, rejecting " + remote);
            return;
        }

        string id = Guid.NewGuid().ToString();
        var player = new PlayerData(id, remote);
        players[id] = player;

        var res = new PacketJoin {
            playerId = id
        };
        server.send(res, remote);

        Console.WriteLine($"Player {id} joined from {remote}");
    }
}