namespace App.Root.Player;
using App.Root.Mesh;

class PlayerMesh {
    private Window window;
    private PlayerController playerController;
    private Mesh mesh;
    
    public string PLAYER_ID => PlayerController.getId();
    public static string PLAYER_MESH = "sphere";

    public PlayerMesh(
        Window window,
        PlayerController playerController, 
        Mesh mesh
    ) {
        this.window = window;
        this.playerController = playerController;
        this.mesh = mesh;
    }

    // Set 
    public void set(bool local) {
        MeshRegistry.register(PLAYER_ID);

        if(!mesh.hasMesh(PLAYER_ID)) {
            window.queueOnRenderThread(() => {
                MeshData data = MeshLoader.load(PLAYER_MESH);
                mesh.add(PLAYER_ID, data);
                if(local) mesh.setVisible(PLAYER_ID, false);
            });
        }
    }

    ///
    /// Update
    /// 
    public void update() {
        if(!mesh.hasMesh(PLAYER_ID)) return;

        var pos = playerController.getPosition();
        mesh.setPosition(PLAYER_ID, pos.X, pos.Y, pos.Z);
    }
}