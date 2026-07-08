namespace App.Root.Packets;

class PacketChunk : Packet {
    public string packetId {
        get;
        set;
    } = "";

    public int chunkIndex {
        get;
        set;
    }

    public int totalChunks {
        get;
        set;
    }

    public string payload {
        get;
        set;
    } = "";

    public PacketType originalType {
        get;
        set;
    }

    public PacketChunk() {
        type = PacketType.CHUNK;
    }
}