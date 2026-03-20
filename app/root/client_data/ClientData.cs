using App.Root.Packets;

namespace App.Root.ClientData;

class ClientData {
    private Client client;

    public ClientData(Client client) {
        this.client = client;
    }

    public void handle(string json) {
        var packet = Packet.deserialize<PacketData>(json);
        if(packet == null) return;
        client.incomingData.Enqueue(packet.toSnapshot());
    }
}