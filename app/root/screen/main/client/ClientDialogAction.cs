namespace App.Root.Screen.Main.Server;
using App.Root.Screen.Main.Client;

class ClientDialogAction {
    private Window window;
    private ScreenController screenController;
    private ClientDialog clientDialog;
    private Network network;

    public ClientDialogAction(
        Window window,
        ScreenController screenController, 
        ClientDialog clientDialog,
        Network network
    ) {
        this.window = window;
        this.screenController = screenController;
        this.clientDialog = clientDialog;
        this.network = network;
    }

    // Start
    public void start() {
        clientDialog.mainScreen.getScene().init();
    }

    // Back
    public void back() {
        clientDialog.hide();
        clientDialog.mainScreen.show();
        
        network.stop();
    }
}