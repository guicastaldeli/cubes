namespace App.Root.Packets;

class PacketChat : Packet {
    public string? userId {
        get;
        set;
    }

    public string? message {
        get;
        set;
    }

    public string? username {
        get;
        set;
    }

    public bool isServer {
        set;
        get;
    } = false;

    public PacketChat() {
        type = PacketType.CHAT;
    }
}