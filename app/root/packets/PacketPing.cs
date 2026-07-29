namespace App.Root.Packets;

class PacketPing : Packet {
    public long timestamp {
        get;
        set;
    }

    public PacketPing() {
        type = PacketType.PING;
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}