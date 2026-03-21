namespace App.Root.Packets;

class PacketData : Packet {
    public Dictionary<DataType, List<Dictionary<string, object>>> data {
        get;
        set;
    } = new();

    public PacketData() {
        type = PacketType.DATA;
    }

    // Snapshot
    public static PacketData fromSnapshot(DataSnapshot snapshot) {
        return new PacketData {
            data = snapshot.data
        };
    }

    public DataSnapshot toSnapshot() {
        return new DataSnapshot {
            data = data
        };
    }
}