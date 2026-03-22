namespace App.Root.ServerData;
using App.Root.Packets;
using App.Root.Player;
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

        // Player
        string id = Guid.NewGuid().ToString();
        var player = new ServerPlayer(id, remote);
        server.players[id] = player;

        server.send(new PacketJoin {
            playerId = id
        }, remote);

        // Data
        var snapshot = ServerSnapshot.getInstance().snapshot();
        server.send(PacketData.fromSnapshot(snapshot), remote);

        Console.WriteLine($"Player {id} joined from {remote}");
    }
}