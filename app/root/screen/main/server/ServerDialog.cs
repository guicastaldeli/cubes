namespace App.Root.Screen.Main.Server;
using App.Root.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

class ServerDialog : Screen {
    public static string PATH = DIR + "main/server/server_dialog.xml";

    public MainScreen mainScreen;
    private ServerDialogAction serverDialogAction;

    public ServerDialog(MainScreen mainScreen) : base(PATH, "server_dialog") {
        this.mainScreen = mainScreen;
        this.serverDialogAction = new ServerDialogAction(
            window, 
            screenController, 
            this,
            network
        );
        
        registerInputs();
    }

    private void registerInputs() {
        InputField.register("portInput");
        InputField.register("maxPlayersInput");
        InputField.register("ipInput");
        InputField.register("joinPortInput");
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        InputField.handleClick(mouseX, mouseY);
        return base.checkClick(mouseX, mouseY);
    }

    // Handle Key Press
    public override void handleKeyPress(int key, int action) {
        InputField.handleKeyPress((Keys)key, action);
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

    /**
    
        On Window Resize

        */
    public override void onWindowResize(int width, int height) {
        base.onWindowResize(width, height);
        registerInputs();
    }

    /**
    
        Update

        */
    public override void update() {
        if(scene.isInit()) {
            scene.update();
            return;
        }
        base.update();    
    }

    /**
    
        Render

        */ 
    public override void render() {
        if(scene.isInit()) {
            scene.render();
            return;
        }
        base.render();
    }
}