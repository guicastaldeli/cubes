namespace App.Root.Packets;

class PacketJoin : Packet {
    public string? playerName {
        get;
        set;
    }

    public PacketJoin() {
        type = PacketType.JOIN;
    }
}