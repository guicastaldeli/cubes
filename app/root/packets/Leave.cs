namespace App.Root.Packets;

class Leave : Packet {
    public Leave() {
        type = PacketType.LEAVE;
    }
}