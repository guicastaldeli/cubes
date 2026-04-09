namespace App.Root.Packets;

class PacketJoin : Packet {
    public string? playerId {
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