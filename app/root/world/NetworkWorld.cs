namespace App.Root.World;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Resource;
using OpenTK.Mathematics;
using System.Text.Json;

class NetworkWorld : NetworkUpdateHandler {
    private WorldManager worldManager;
    private Network? network;

    private bool isMultiplayer = false;

    public NetworkWorld(WorldManager worldManager) {
        this.worldManager = worldManager;
        NetworkUpdate.register(this);
    }

    public void setNetwork(Network network) {
        this.network = network;
        isMultiplayer = true;
    }

    private void spawnClient() {
        worldManager.getWorld().get<Platform.Platform>()?.setClient();
        worldManager.getPlayerController().set();
    }
    
    public override void update() {
        
    }
}