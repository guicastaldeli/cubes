namespace App.Root.Screen.Main.Server;

class ServerDialog : Screen {
    public static readonly String PATH = DIR + "main/server/server_dialog.xml";

    public MainScreen mainScreen;
    private ServerDialogAction serverDialogAction;

    public ServerDialog(MainScreen mainScreen) : 
    base(PATH, "server_dialog") {
        this.mainScreen = mainScreen;
        this.serverDialogAction = new ServerDialogAction(window, this);
    }

    // Handle Action
    public override void handleAction(string action) {
        switch(action) {
            case "host":
                serverDialogAction.hostServer();
                break;
            case "join":
                serverDialogAction.joinServer();
                break;
            case "back":
                serverDialogAction.back();
                break;
        }
    }

    ///
    /// Update
    /// 
    public override void update() {
        if(scene.isInit()) {
            scene.update();
            return;
        }
        base.update();    
    }

    ///
    /// Render
    /// 
    public override void render() {
        if(scene.isInit()) {
            scene.render();
            return;
        }
        base.render();
    }
}