using App.Root.Voip;

namespace App.Root;

class NetworkSetter {
    private Network network;

    private Input input;
    private Scene scene;

    public NetworkSetter(
        Network network,
        Input input,
        Scene scene
    ) {
        this.network = network;
        this.input = input;
        this.scene = scene;
    }

    // Set
    public void set() {
        scene.setNetwork(network);
        input.setNetwork(network);
        scene.getPlayerController().setNetwork(network);
        VoiceController.getInstance().setNetwork(network);
    }

    // Init 
    public void init() {
        if(network == null) return;

        scene.getPlayerController().set();
        scene.getWorldManager().getWorldBroadcaster().set();
    }

    // Reset
    public void reset() {
        scene.getWorldManager().setNetwork(network);
    }
}