using App.Root.Screen.Main.Client;

namespace App.Root.Screen.Main.Server;

class ClientDialogAction {
    public ClientDialog clientDialog;

    public ClientDialogAction(ClientDialog clientDialog) {
        this.clientDialog = clientDialog;
    }

    // Back
    public void back() {
        clientDialog.hide();
        clientDialog.mainScreen.show();
    }
}