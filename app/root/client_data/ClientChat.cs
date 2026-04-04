using App.Root.Chat;
using App.Root.Packets;

namespace App.Root.ClientData;

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

    // Handle
    public void handle(string json) {
        var packet = Packet.deserialize<PacketChat>(json);
        if(packet == null || packet.message == null) return;

        string playerName = packet.playerName ?? "?"; 

        ChatController.getInstance().addMessage(
            playerName,
            packet.message
        );
    }
}