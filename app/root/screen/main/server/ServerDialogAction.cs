namespace App.Root.Screen.Main.Server;

class ServerDialogAction {
    public ServerDialog serverDialog;

    public ServerDialogAction(ServerDialog serverDialog) {
        this.serverDialog = serverDialog;
    }

    // Back
    public void back() {
        serverDialog.hide();
        serverDialog.mainScreen.show();
    }
}