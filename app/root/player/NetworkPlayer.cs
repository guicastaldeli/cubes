namespace App.Root.Player;
using App.Root.Mesh;
using App.Root.Screen;
using OpenTK.Mathematics;

class NetworkPlayer : NetworkUpdateHandler {
    private PlayerController playerController;
    private Dictionary<string, Vector3> namePos = new();
    private Dictionary<string, string> nameLabels = new();

    public NetworkPlayer(PlayerController playerController) {
        this.playerController = playerController;
        NetworkUpdate.register(this);

        renderName();
    }

    ///
    /// Render
    /// 
    private void render(
        Mesh mesh, 
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

                MeshData data = MeshDataLoader.load(meshName);
                mesh.add(id, data);
                mesh.setPosition(id, x, y, z);
            });
        } else {
            mesh.setPosition(id, x, y, z);
        }

        namePos[id] = new Vector3(x, y + 1.5f, z);
    }

    private void renderName() {
        playerController.getWindow().addPersistentAction(() => {
            var view = playerController.getCamera().getView();
            var projection = playerController.getCamera().getProjection();
            
            foreach(var (id, pos) in namePos) {
                string label = nameLabels.TryGetValue(id, out var username)
                    ? username
                    : id;

                Screen.textRenderer?.renderTextBillboard(
                    label, 
                    pos, 
                    view, 
                    projection
                );
            }
        });
    }

    ///
    /// Update
    /// 
    public override void update() {
        playerController.sendState();
        
        Mesh mesh = playerController.getMesh();
        Network? network = playerController.getNetwork();
        if(network == null) return;

        var snapshot = network.getCachedSnapshot();
        if(snapshot == null) return;

        var view = playerController.getCamera().getView();
        var projection = playerController.getCamera().getProjection();

        Data.getInstance().apply(snapshot, DataType.PLAYER, entry => {
            string? id = entry["id"] as string;
            if(string.IsNullOrEmpty(id) || id == network.userId) return;

            string username = 
                entry.TryGetValue("username", out var u) &&
                u is string s &&
                !string.IsNullOrEmpty(s)
                    ? s
                    : id;
            nameLabels[id] = username;

            render(mesh, id, entry);
        });
    }
}