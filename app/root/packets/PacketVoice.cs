namespace App.Root.Packets;

class PacketVoice : Packet {
    public string? playerId {
        get;
        set;
    }

    public byte[]? audio {
        get;
        set;
    }

    public PacketVoice() {
        type = PacketType.VOICE;
    }
}