namespace App.Root.Packets;

class PacketLeave : Packet {
    public PacketLeave() {
        type = PacketType.LEAVE;
    }
}