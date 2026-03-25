using App.Root.Mesh;

namespace App.Root.Player;

class NetworkPlayer : NetworkUpdateHandler {
    private PlayerController playerController;

    public NetworkPlayer(PlayerController playerController) {
        this.playerController = playerController;
        NetworkUpdate.register(this);
    }

    ///
    /// Render
    /// 
    private void render(
        Mesh.Mesh mesh, 
        string id, 
        Dictionary<string, object> entry
    ) {
        float x = Convert.ToSingle(entry["x"]);
        float y = Convert.ToSingle(entry["y"]);
        float z = Convert.ToSingle(entry["z"]);
        
        if(!mesh.hasMesh(id)) {
            string meshName = PlayerMesh.PLAYER_MESH;
            MeshRegistry.register(id);

            playerController.getWindow().queueOnRenderThread(() => {
                if(string.IsNullOrEmpty(meshName)) return;

                MeshData data = MeshLoader.load(meshName);
                mesh.add(id, data);
                mesh.setPosition(id, x, y, z);
            });
        } else {
            mesh.setPosition(id, x, y, z);
        }
    }

    ///
    /// Update
    /// 
    public override void update() {
        playerController.sendState();
        
        Mesh.Mesh mesh = playerController.getMesh();
        Network? network = playerController.getNetwork();
        if(network == null) return;

        var snapshot = network.getCachedSnapshot();
        if(snapshot == null) return;

        Data.getInstance().apply(snapshot, DataType.PLAYER, entry => {
            string? id = entry["id"] as string;
            if(string.IsNullOrEmpty(id) || id == network.playerId) return;

            render(mesh, id, entry);
        });
    }
}