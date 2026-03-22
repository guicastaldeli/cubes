using App.Root.Mesh;

namespace App.Root.Player;

class NetworkPlayer : NetworkUpdateHandler {
    private PlayerController playerController;

    public NetworkPlayer(PlayerController playerController) {
        this.playerController = playerController;
        NetworkUpdate.register(this);
    }

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

            string meshId = "player_" + id;
            float x = Convert.ToSingle(entry["x"]);
            float y = Convert.ToSingle(entry["y"]);
            float z = Convert.ToSingle(entry["z"]);
            
            if(!mesh.hasMesh(meshId)) {
                string capId = meshId;
                playerController.getWindow().queueOnRenderThread(() => {
                    MeshData data = MeshLoader.load("cube");
                    mesh.add(capId, data);
                    mesh.setPosition(capId, x, y, z);
                });
            } else {
                mesh.setPosition(meshId, x, y, z);
            }
        });
    }
}