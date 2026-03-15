namespace App.Root.Screen.Main.Server;

class ServerDialog : Screen {
    public static readonly String PATH = DIR + "main/server/server_dialog.xml";

    public MainScreen mainScreen;
    private ServerDialogAction serverDialogAction;

    public ServerDialog(MainScreen mainScreen) : 
    base(PATH, "server_dialog") {
        this.mainScreen = mainScreen;
        this.serverDialogAction = new ServerDialogAction(this);
    }

    // Handle Action
    public override void handleAction(string action) {
        switch(action) {
            case "back":
                serverDialogAction.back();
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