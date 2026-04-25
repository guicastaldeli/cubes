namespace App.Root.Screen.Main.Server;
using App.Root.Player;

class ServerDialogAction {
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
    
        Host Server

        */
    public void hostServer() {
        int port = network.getPort().get();

        string maxPlayersEl = serverDialog.inputField.getText("maxPlayersInput");
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
        string ip = serverDialog.inputField.getText("ipInput");
        string port = serverDialog.inputField.getText("joinPortInput");
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