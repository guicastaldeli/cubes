namespace App.Root.ServerData;
using App.Root.Packets;
using App.Root.Player;
using System.Net;

class ServerJoin : PacketHandler {
    private Server server;

    public ServerJoin(Server server) {
        this.server = server;
        PacketController.register(this, Context.SERVER);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.JOIN;
    }

    // Handle
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketJoin>(json);
        if(packet == null) return;

        if(server.players.Count >= server.maxPlayers) {
            Console.WriteLine("Server full, rejecting " + remote);
            return;
        }

        // Player
        string id = packet.playerId!;
        if(server.players.ContainsKey(id)) {
            Console.WriteLine($"Duplicate ID {id}, error {remote}");
            return;
        }
        
        var player = new ServerPlayer(id, remote);
        if(!string.IsNullOrEmpty(packet.username)) player.username = packet.username;
        server.players[id] = player;
        ServerSnapshot.getInstance().register(DataType.PLAYER, player);

        server.send(new PacketJoin {
            playerId = id
        }, remote);

        // Data
        var snapshot = ServerSnapshot.getInstance().snapshot();
        server.send(PacketData.fromSnapshot(snapshot), remote);

        Console.ForegroundColor = ConsoleColor.Green;
        string italic = "\x1b[3m";
        Console.WriteLine($"{italic}Player {id} joined from {remote}");
        Console.ResetColor();
    }
}