namespace App.Root.ServerData;
using App.Root.Packets;
using System.Net;

class ServerPlayerData : PacketHandler {
    private Server server;

    public ServerPlayerData(Server server) {
        this.server = server;
        PacketController.register(this, Context.SERVER);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.DATA;
    }

    /**
     * 
     * Handle
     *
     */
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketData>(json);
        if(packet == null) return;

    }
}