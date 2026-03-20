namespace App.Root.ServerData;
using App.Root.Player;
using App.Root.Packets;
using System.Net;

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

        // Player Id
        string id = Guid.NewGuid().ToString();
        var player = new PlayerData(id, remote);
        server.players[id] = player;

        var res = new PacketJoin {
            playerId = id
        };
        server.send(res, remote);

        // World Data
        var worldData = server.getServerDataManager()
            .getServerWorldData()
            .getWorldData()
            .get();
        server.send(worldData, remote);

        Console.WriteLine($"Player {id} joined from {remote}");
    }
}