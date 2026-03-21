namespace App.Root.World;
using App.Root.Env.World;

class NetworkWorld : NetworkUpdateHandler {
    private WorldManager worldManager;

    public NetworkWorld(WorldManager worldManager) {
        this.worldManager = worldManager;
        NetworkUpdate.register(this);
    }
    
    public override void update() {
        Mesh.Mesh mesh = worldManager.getWorld().getMesh();
        Network? network = worldManager.getNetwork();
        if(network != null) {
            var snapshot = network.pollData();
            if(snapshot != null) {
                Data.getInstance().apply(snapshot, DataType.MESH, entry => {
                    string? id = entry["id"] as string;
                    if(id == null) return;
                    if(!mesh.hasMesh(id)) mesh.add(id);
                    mesh.setPosition(id,
                        Convert.ToSingle(entry["x"]),
                        Convert.ToSingle(entry["y"]),
                        Convert.ToSingle(entry["z"])
                    );
                });
            }
        }
    }
}