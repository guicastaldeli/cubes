namespace App.Root.Packets;
using System.Collections.Generic;
using App.Root.Player;

class PacketWorld : Packet {
    public List<PlayerState> players {
        get;
        set;
    } = new();

    public PacketWorld() {
        type = PacketType.WORLD;
    }
}