namespace App.Root.ServerData;
using App.Root.Chat;
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

    // Send Alert
    private void sendAlert(ServerPlayer player) {
        var msg = ServerMessage.get(ServerMessage.USER_JOINED, player.username);
        foreach(var (_, p) in server.players) {
            server.send(msg, p.endPoint);
        }
    }

    /**
    
        Handle

        */
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketJoin>(json);
        if(packet == null) return;

        if(server.players.Count >= server.maxPlayers) {
            Console.WriteLine("Server full, rejecting " + remote);
            return;
        }

        // Player
        string id = packet.userId!;
        if(server.players.ContainsKey(id)) {
            Console.WriteLine($"Duplicate ID {id}, error {remote}");
            return;
        }
        
        var player = new ServerPlayer(id, remote);
        if(!string.IsNullOrEmpty(packet.username)) player.username = packet.username;
        server.players[id] = player;
        ServerSnapshot.getInstance().register(DataType.PLAYER, player);

        server.send(new PacketJoin { userId = id }, remote);

        // Server
        var serverSnapshot = ServerSnapshot.getInstance().snapshot();
        var worldSnapshot = Data.getInstance().snapshot();
        
        foreach(var (type, list) in worldSnapshot.data) {
            if(!serverSnapshot.data.ContainsKey(type)) {
                serverSnapshot.data[type] = new();
            }
            serverSnapshot.data[type].AddRange(list);
        }
        
        server.send(PacketData.fromSnapshot(serverSnapshot), remote);

        // Alert
        Console.ForegroundColor = ConsoleColor.Green;
        string italic = "\x1b[3m";
        Console.WriteLine($"{italic}Player {id} joined from {remote}");
        Console.ResetColor();

        sendAlert(player);
    }
}