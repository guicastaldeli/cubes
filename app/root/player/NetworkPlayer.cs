namespace App.Root.Player;

class NetworkPlayer {
    private Network network;
    private PlayerController playerController;

    public NetworkPlayer(Network network, PlayerController playerController) {
        this.network = network;
        this.playerController = playerController;
    }

    ///
    /// Update
    /// 
    public void update() {
        if(network.isConnected) {
            var pos = playerController.getCamera().getPosition();
            var yaw = playerController.getCamera().getYaw();
            var pitch = playerController.getCamera().getPitch();
            network.sendState(
                pos.X, pos.Y, pos.Z,
                yaw,
                pitch
            );
        }
    }
}