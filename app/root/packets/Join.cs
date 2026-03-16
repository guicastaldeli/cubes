namespace App.Root.Packets;

class Join : Packet {
    public string? playerName {
        get;
        set;
    }

    public Join() {
        type = PacketType.JOIN;
    }
}