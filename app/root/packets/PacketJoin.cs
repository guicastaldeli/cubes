namespace App.Root.Packets;

class PacketJoin : Packet {
    public string? userId {
        get;
        set;
    }

    public string? username {
        get;
        set;
    }

    public PacketJoin() {
        type = PacketType.JOIN;
    }
}