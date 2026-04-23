namespace App.Root.ClientData;
using App.Root.Chat;
using App.Root.Packets;

class ClientChat : PacketHandler {
    private Client client;

    public ClientChat(Client client) {
        this.client = client;
        PacketController.register(this, Context.CLIENT);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.CHAT;
    }

    /**
    
        Handle

        */
    public void handle(string json) {
        var packet = Packet.deserialize<PacketChat>(json);
        if(packet == null || packet.message == null) return;

        string username = packet.username ?? "?"; 

        ChatController.getInstance().addMessage(
            username,
            packet.message,
            packet.isServer
        );
    }
}