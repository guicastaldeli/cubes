using App.Root.Mesh;

namespace App.Root.Player;

class PlayerMesh {
    private Window window;
    private PlayerController playerController;
    private Mesh.Mesh mesh;
    
    private string meshId;

    private bool isLocal = false;

    public PlayerMesh(
        Window window,
        PlayerController playerController, 
        Mesh.Mesh mesh
    ) {
        this.window = window;
        this.playerController = playerController;
        this.mesh = mesh;

        this.meshId = "player_" + playerController.getId();
    }

    // Set Id
    public void setId(string id) {
        this.meshId = "player_" + id;
    }

    ///
    /// Render (REFACTOR THIS LATER...)
    /// 
    public void render(bool local) {
        if(!mesh.hasMesh(meshId)) {
            string capId = meshId;
            window.queueOnRenderThread(() => {
                MeshData data = MeshLoader.load("cube");
                mesh.add(capId, data);
                if(local) mesh.setVisible(capId, false);
            });
        }
    }

    ///
    /// Update
    /// 
    public void update() {
        if(!mesh.hasMesh(meshId)) return;
        var pos = playerController.getPosition();
        mesh.setPosition(meshId, pos.X, pos.Y, pos.Z);
    }
}