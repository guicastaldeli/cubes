namespace App.Root.Packets;

class State : Packet {
    public float x { 
        get; 
        set; 
    }

    public float y { 
        get; 
        set; 
    }

    public float z { 
        get; 
        set; 
    }

    public float yaw { 
        get; 
        set; 
    }

    public float pitch { 
        get; 
        set; 
    }

    public State() {
        type = PacketType.STATE;
    }
}