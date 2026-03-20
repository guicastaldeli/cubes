namespace App.Root.World;
using App.Root.Env.World;
using App.Root.Player;

class NetworkWorld {
    private WorldManager worldManager;
    private PlayerController playerController;
    private Network network = null!;

    public NetworkWorld(WorldManager worldManager, PlayerController playerController) {
        this.worldManager = worldManager;
        this.playerController = playerController;
    }

    public void setNetwork(Network network) {
        this.network = network;
    }

    ///
    /// Set
    /// 
    public void set() {
        if(network != null) {
            worldManager.setNetwork(network);
            network.getServer()
                .getServerDataManager()
                .getServerWorldData()
                .setWorldManager(worldManager);
        } else {
            Console.Error.WriteLine("Network error!");
        }
    }

    ///
    /// Start
    /// 
    public void start() {
        if(network != null) {
            playerController.setNetworkPlayer(network);
            var server = network.getServer();
            if(server != null) {
                worldManager.setServer(server);
                worldManager.getWorldBroadcaster().start();
            }
        }
    }
}