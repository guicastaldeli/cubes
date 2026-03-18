namespace App.Root.ServerData;
using System.Collections.Concurrent;
using System.Net;
using App.Root.Packets;
using App.Root.Player;

class ServerJoin {
    private Server server;

    public ServerJoin(Server server) {
        this.server = server;
    }

    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketJoin>(json);
        if(packet == null) return;

        if(server.players.Count >= server.maxPlayers) {
            Console.WriteLine("Server full, rejecting " + remote);
            return;
        }

        string id = Guid.NewGuid().ToString();
        var player = new PlayerData(id, remote);
        server.players[id] = player;

        var res = new PacketJoin {
            playerId = id
        };
        server.send(res, remote);

        Console.WriteLine($"Player {id} joined from {remote}");
    }
}