namespace App.Root.Packets;

enum MeshAction {
    ADD,
    REMOVE
}

class PacketMeshUpdate : Packet {
    public MeshAction action {
        get;
        set;
    }

    public string meshId {
        get;
        set;
    } = "";

    public string meshType {
        get;
        set;
    } = "";

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

    public float scaleX {
        get;
        set;
    } = 1.0f;

    public float scaleY {
        get;
        set;
    } = 1.0f;

    public float scaleZ {
        get;
        set;
    } = 1.0f;

    public int texId {
        get;
        set;
    } = -1;

    public string texPath {
        get;
        set;
    } = "";

    public PacketMeshUpdate() {
        type = PacketType.MESH_UPDATE;
    }
}

