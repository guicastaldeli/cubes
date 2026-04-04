namespace App.Root.ClientData;
using App.Root.Packets;
using App.Root.Voip;

class ClientVoice : PacketHandler {
    private Client client;

    public ClientVoice(Client client) {
        this.client = client;
        PacketController.register(this, Context.CLIENT);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.VOICE;
    }

    // Handle
    public void handle(string json) {
        var packet = Packet.deserialize<PacketVoice>(json);
        if(packet == null || 
            packet.audio == null || 
            packet.playerId == null
        ) return;

        VoiceController.getInstance().receive(
            packet.playerId, 
            packet.audio,
            packet.sequence
        );
    }
}