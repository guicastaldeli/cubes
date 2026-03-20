namespace App.Root.Packets;
using App.Root.World;

class PacketWorldData : Packet {
    public List<WorldObject> objs {
        get;
        set;
    } = new();

    public PacketWorldData() {
        type = PacketType.WORLD_DATA;
    }
}