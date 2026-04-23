namespace App.Root.ClientData;
using App.Root.Packets;

class ClientData : PacketHandler {
    private Client client;

    public ClientData(Client client) {
        this.client = client;
        PacketController.register(this, Context.CLIENT);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.DATA;
    }

    /**
    
        Handle

        */
    public void handle(string json) {
        var packet = Packet.deserialize<PacketData>(json);
        if(packet == null) return;
        client.incomingData.Enqueue(packet.toSnapshot());
    }
}