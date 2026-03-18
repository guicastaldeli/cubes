using App.Root.Packets;

namespace App.Root.ClientData;

class ClientWorld {
    private Client client;

    public ClientWorld(Client client) {
        this.client = client;
    }

    public void handle(string json) {
        var packet = Packet.deserialize<PacketWorld>(json);
        if(packet == null) return;
        client.incomingWorld.Enqueue(packet);
    }
}