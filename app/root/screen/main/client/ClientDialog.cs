using App.Root.Screen.Main.Server;

namespace App.Root.Screen.Main.Client;

class ClientDialog : Screen {
    public static readonly String PATH = DIR + "main/client/client_dialog.xml";

    public MainScreen mainScreen;
    private ClientDialogAction clientDialogAction;

    public ClientDialog(MainScreen mainScreen) : 
    base(PATH, "client_dialog") {
        this.mainScreen = mainScreen;
        this.clientDialogAction = new ClientDialogAction(this);
    }

    // Handle Action
    public override void handleAction(string action) {
        switch(action) {
            case "back":
                clientDialogAction.back();
                break;
        }
    }

    ///
    /// Update
    /// 
    public override void update() {
        base.update();    
    }

    ///
    /// Render
    /// 
    public override void render() {
        base.render();
    }
}