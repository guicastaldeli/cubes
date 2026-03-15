namespace App.Root.Screen.Main.Server;

class ServerDialogAction {
    public Window window;
    public ServerDialog serverDialog;

    public ServerDialogAction(Window window, ServerDialog serverDialog) {
        this.window = window;
        this.serverDialog = serverDialog;
    }

    ///
    /// Host Server
    /// 
    public void hostServer() {
        serverDialog.mainScreen.getScene().init();
    }

    ///
    /// Join Server
    /// 
    public void joinServer() {
        serverDialog.mainScreen.getScene().init();
    }

    // Back
    public void back() {
        serverDialog.hide();
        serverDialog.mainScreen.show();
    }
}