namespace App.Root.ClientData;
using App.Root.Packets;
using App.Root.World;
using OpenTK.Mathematics;

class ClientMeshUpdate : PacketHandler {
    private Client client;

    public ClientMeshUpdate(Client client) {
        this.client = client;
        PacketController.register(this, Context.CLIENT);
    }

    // Get Type
    public PacketType getType() {
        return PacketType.MESH_UPDATE;
    }

    /**
    
        Handle

        */
    public void handle(string json) {
        var packet = Packet.deserialize<PacketMeshUpdate>(json);
        if(packet == null) return;

        if(packet.userId == client.userId) return;

        var updater = WorldUpdater.getInstance();

        switch(packet.action) {
            case MeshAction.ADD:
                updater.applyAddMesh(
                    packet.meshId,
                    packet.meshType,
                    new Vector3(packet.x, packet.y, packet.z),
                    new Vector3(packet.scaleX, packet.scaleY, packet.scaleZ),
                    packet.texId,
                    packet.texPath
                );
                break;
            case MeshAction.REMOVE:
                updater.applyRemoveMesh(packet.meshId);
                break;
        }
    }
}