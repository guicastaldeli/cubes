namespace App.Root.Packets;

class PacketVoice : Packet {
    public string? userId {
        get;
        set;
    }

    public byte[]? audio {
        get;
        set;
    }
    
    public int sequence {
        get;
        set;
    }

    public PacketVoice() {
        type = PacketType.VOICE;
    }
}