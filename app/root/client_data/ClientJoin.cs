namespace App.Root.ClientData;
using App.Root.Packets;

class ClientJoin : PacketHandler {
    private Client client;

    public ClientJoin(Client client) {
        this.client = client;
        PacketController.register(this, Context.CLIENT);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.JOIN;
    }

    // Handle
    public void handle(string json) {
        var packet = Packet.deserialize<PacketJoin>(json);
        if(packet?.userId == null) return;

        client.userId = packet.userId;
        client.connected = true;

        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"Connected with ID: {client.userId}");
        Console.ResetColor();
    }
}