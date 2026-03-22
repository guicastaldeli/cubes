namespace App.Root.Screen.Main.Server;
using App.Root.Screen.Main.Client;

class ClientDialogAction {
    public Window window;
    public ScreenController screenController;
    public ClientDialog clientDialog;

    public ClientDialogAction(
        Window window,
        ScreenController screenController, 
        ClientDialog clientDialog
    ) {
        this.window = window;
        this.screenController = screenController;
        this.clientDialog = clientDialog;
    }

    // Start
    public void start() {
        clientDialog.mainScreen.getScene().init();
    }

    // Back
    public void back() {
        clientDialog.hide();
        clientDialog.mainScreen.show();
        
        screenController.main.getNetwork().stop();
    }
}