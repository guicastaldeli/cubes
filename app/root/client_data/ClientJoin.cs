using App.Root.Packets;

namespace App.Root.ClientData;

class ClientJoin {
    private Client client;

    public ClientJoin(Client client) {
        this.client = client;
    }

    public void handle(string json) {
        var packet = Packet.deserialize<PacketJoin>(json);
        if(packet?.playerId == null) return;

        client.playerId = packet.playerId;
        client.connected = true;

        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"Connected with ID: {client.playerId}");
        Console.ResetColor();
    }
}