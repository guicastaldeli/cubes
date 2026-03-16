namespace App.Root.Packets;

class Ping : Packet {
    public long timestamp {
        get;
        set;
    }

    public Ping() {
        type = PacketType.PING;
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}