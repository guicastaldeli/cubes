using App.Root.Screen.Main.Server;

namespace App.Root.Screen.Main.Client;

class ClientDialog : Screen {
    public static readonly String PATH = DIR + "main/client/client_dialog.xml";

    public MainScreen mainScreen;
    private ClientDialogAction clientDialogAction;

    public ClientDialog(MainScreen mainScreen) : 
    base(PATH, "client_dialog") {
        this.mainScreen = mainScreen;
        this.clientDialogAction = new ClientDialogAction(
            window, 
            screenController, 
            this,
            network
        );
    }

    // Handle Action
    public override void handleAction(string action) {
        switch(action) {
            case "start":
                clientDialogAction.start();
                break;
            case "back":
                clientDialogAction.back();
                break;
        }
    }

    ///
    /// Update
    /// 
    public override void update() {
        if(scene.isInit()) {
            if(!tick.isPaused()) scene.update();
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