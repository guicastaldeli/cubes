namespace App.Root.Packets;

class PacketChat : Packet {
    public string? message {
        get;
        set;
    }

    public string? username {
        get;
        set;
    }

    public PacketChat() {
        type = PacketType.CHAT;
    }
}