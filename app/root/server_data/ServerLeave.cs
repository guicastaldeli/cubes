namespace App.Root.ServerData;
using System.Net;
using App.Root.Packets;

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

    // Handle
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketLeave>(json);
        if(packet?.playerId == null) return;

        server.players.TryRemove(packet.playerId, out _);

        ServerSnapshot.getInstance().clearAll();
        foreach(var (_, p) in server.players) {
            ServerSnapshot.getInstance().register(DataType.PLAYER, p);
        }

        string color = "\e[0;31m";
        Console.WriteLine($"{color}Player {packet.playerId} left");
    }
}