namespace App.Root.Player;

class NetworkPlayer : NetworkUpdateHandler {
    private PlayerController playerController;

    public NetworkPlayer(PlayerController playerController) {
        this.playerController = playerController;
    }

    public override void update() {
        Mesh.Mesh mesh = playerController.getMesh();
        Network? network = playerController.getNetwork();
        if(network != null) {
            var snapshot = network.pollData();
            if(snapshot != null) {
                Data.getInstance().apply(snapshot, DataType.PLAYER, entry => {
                    string? id = entry["id"] as string;
                    if(id == null || id == network.playerId) return;

                    string meshId = "player_" + id;
                    if(!mesh.hasMesh(meshId)) mesh.add(meshId);
                    mesh.setPosition(meshId,
                        Convert.ToSingle(entry["x"]),
                        Convert.ToSingle(entry["y"]),
                        Convert.ToSingle(entry["z"])
                    );
                });
            } 
        }
    }
}