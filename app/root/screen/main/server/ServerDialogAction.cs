using App.Root.Player;

namespace App.Root.Screen.Main.Server;

class ServerDialogAction {
    private Window window;
    private ScreenController screenController;
    private ServerDialog serverDialog;

    public ServerDialogAction(
        Window window,
        ScreenController screenController, 
        ServerDialog serverDialog
    ) {
        this.window = window;
        this.screenController = screenController;
        this.serverDialog = serverDialog;
    }

    ///
    /// Host Server
    /// 
    public void hostServer() {
        int port = screenController.main.getNetwork().getPort().get();

        string maxPlayersEl = serverDialog.inputField.getText("maxPlayersInput");
        int maxPlayers = string.IsNullOrEmpty(maxPlayersEl) ?
            ServerPlayer.SERVER_MAX_PLAYERS :
            int.Parse(maxPlayersEl);

        screenController.main.getNetwork().host(port, maxPlayers);
        serverDialog.mainScreen.getScene().init();
    }

    ///
    /// Join Server
    /// 
    public void joinServer() {
        string ip = serverDialog.inputField.getText("ipInput");
        string port = serverDialog.inputField.getText("joinPortInput");
        if(string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port)) return;

        screenController.main.getNetwork().join(ip, int.Parse(port));

        serverDialog.mainScreen.getScene().init();
    }

    // Back
    public void back() {
        serverDialog.hide();
        serverDialog.mainScreen.show();
        
        screenController.main.getNetwork().stop();
    }
}