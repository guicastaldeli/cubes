namespace App.Root.Packets;

class PacketChat : Packet {
    public string? message {
        get;
        set;
    }

    public string? playerName {
        get;
        set;
    }

    public PacketChat() {
        type = PacketType.CHAT;
    }
}