namespace App.Root.Screen.Main.Server;
using App.Root.Player;
using App.Root.Utils;
using App.Root.Input;

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
     * 
     * Get
     *
     */
    public dynamic get() {
        object val = ElementEntry.C(id => serverDialog.getElementById(id), Elements);
        return val;
    }

    /**
     * 
     * Host
     *
     */
    [GlobalInput]
    public void hostServer() {
        int port = Port.Get();

        string maxPlayersEl = get().maxPlayersInput.text;
        int maxPlayers = string.IsNullOrEmpty(maxPlayersEl) ? ServerPlayer.SERVER_MAX_PLAYERS : int.Parse(maxPlayersEl);

        network.Server.Start(port, maxPlayers);
        
        string localIP = IP.Get();
        network.Client.Connect(localIP, port);
        
        serverDialog.getMainScene().init();
    }

    /**
     * 
     * Join
     *
     */
    [GlobalInput]
    public void joinServer() {
        string ip = get().ipInput.text;
        string port = get().joinPortInput.text;
        if(string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port)) return;

        network.Client.Connect(ip, int.Parse(port));

        serverDialog.getMainScreen().getMainScene().init();
    }

    /**
     * 
     * Back
     *
     */
    [GlobalInput]
    public void back() {
        serverDialog.hide();
        serverDialog.getMainScreen().show();
    }
}