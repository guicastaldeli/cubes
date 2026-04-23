namespace App.Root.ServerData;
using App.Root.Chat;
using App.Root.Packets;
using App.Root.Player;
using System.Net;

class ServerLeave : PacketHandler {
    private Server server;

    public ServerLeave(Server server) {
        this.server = server;
        PacketController.register(this, Context.SERVER);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.LEAVE;
    }

    // Send Alert
    private void sendAlert(ServerPlayer player) {
        var msg = ServerMessage.get(ServerMessage.USER_LEFT, player.username);
        foreach(var (_, p) in server.players) {
            server.send(msg, p.endPoint);
        }
    }

    /**
    
        Handle

        */
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketLeave>(json);
        if(packet?.userId == null) return;

        if(server.players.TryGetValue(packet.userId, out var player)) {
            sendAlert(player);
            server.players.TryRemove(packet.userId, out _);
            ServerSnapshot.getInstance().clearAll();
            foreach(var (_, p) in server.players) {
                ServerSnapshot.getInstance().register(DataType.PLAYER, p);
            }
        }

        string color = "\e[0;31m";
        Console.WriteLine($"{color}Player {packet.userId} left");
        Console.ResetColor();
    }
}