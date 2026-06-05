namespace App.Root.Screen.Main.Server;
using App.Root.Input;
using App.Root.Player;
using App.Root.Utils;

class ServerDialogAction {
    private static List<string> Elements = new() {
        "maxPlayersInput",
        "ipInput",
        "joinPortInput"
    };

    private Window window;
    private ScreenController screenController;
    private ServerDialog serverDialog;
    private Network network;

    public ServerDialogAction(
        Window window,
        ScreenController screenController, 
        ServerDialog serverDialog,
        Network network
    ) {
        this.window = window;
        this.screenController = screenController;
        this.serverDialog = serverDialog;
        this.network = network;
    }

    /**
    
        Get
    
        */
    public dynamic get() {
        return ElementEntry.C(id => serverDialog.getElementById(id), Elements);
    }

    /**
    
        Host Server

        */
    public void hostServer() {
        int port = network.getPort().get();

        string maxPlayersEl = get().maxPlayersInput.text;
        int maxPlayers = string.IsNullOrEmpty(maxPlayersEl) ?
            ServerPlayer.SERVER_MAX_PLAYERS :
            int.Parse(maxPlayersEl);

        network.host(port, maxPlayers);
        serverDialog.mainScreen.getScene().init();
    }

    /**
    
        Join Server

        */
    public void joinServer() {
        string ip = get().ipInput.text;
        string port = get().joinPortInput.text;
        if(string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port)) return;

        network.join(ip, int.Parse(port));

        serverDialog.mainScreen.getScene().init();
    }

    // Back
    public void back() {
        serverDialog.hide();
        serverDialog.mainScreen.show();
        
        network.stop();
    }
}