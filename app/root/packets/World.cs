namespace App.Root.Packets;
using System.Collections.Generic;
using App.Root.Player;

class World : Packet {
    public List<PlayerState> players {
        get;
        set;
    } = new();

    public World() {
        type = PacketType.WORLD;
    }
}