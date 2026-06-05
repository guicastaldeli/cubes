namespace App.Root.Screen.Main.Server;
using App.Root.Input;
using App.Root.Utils;
using AppWindow = App.Root.Window;
using OpenTK.Windowing.GraphicsLibraryFramework;

class ServerDialog : MainScreenHandler {
    private const string ID = "server_dialog";
    public static string PATH = DIR + "main/server/server_dialog.xml";
    private ServerDialogAction serverDialogAction;

    public ServerDialog( [Inject] MainScreen mainScreen) : base(PATH, ID) {
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

    // Get Main Screen
    public MainScreen getMainScreen() {
        return mainScreen;
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

    // Open
    public override void open() {
        mainScreen.hide();
        show();
    }

    // Close
    public override void close() {
        hide();
        mainScreen.show();
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
        if(mainScreen.getMainScene().isInit()) {
            mainScreen.getMainScene().update();
            return;
        }
        base.update();    
    }

    /**
    
        Render

        */ 
    public override void render() {
        if(mainScreen.getMainScene().isInit()) {
            mainScreen.getMainScene().render();
            return;
        }
        base.render();
    }
}