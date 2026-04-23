using App.Root.Packets;

namespace App.Root.ClientData;

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
        
    }
}