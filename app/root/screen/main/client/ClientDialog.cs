namespace App.Root.Screen.Main.Client;

using App.Root.Input;
using App.Root.Screen.Main.Server;
using App.Root.Utils;

class ClientDialog : MainScreenHandler {
    private const string ID = "client_dialog";
    public static readonly string PATH = Screen.DIR + "main/client/client_dialog.xml";
    
    private ClientDialogAction clientDialogAction;

    public ClientDialog([Inject] MainScreen mainScreen) : base(PATH, ID) {
        this.mainScreen = mainScreen;
        this.clientDialogAction = new ClientDialogAction(
            window, 
            screenController, 
            this,
            network
        );
    }

    // Get Main Screen
    public MainScreen getMainScreen() {
        return mainScreen;
    }

    // Handle Action
    public override void handleAction(string action) {
        switch(action) {
            case "create_save":
                clientDialogAction.createSave();
                break;
            case "delete_save":
                clientDialogAction.deleteSave();
                break;
            case "back":
                clientDialogAction.back();
                break;
        }
        
        var typeName = GlobalInputHandler.FindTypeFromAction(action);
        if(typeName != null) {
            var (_, id) = ActionConverter.Convert(action);
            if(id.HasValue) {
                GlobalInputHandler.HandleByType(typeName, id.Value);
                updateSaveList();
                return;
            }
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
     * 
     * On Window Resize
     *
     */
    public override void onWindowResize(int width, int height) {
        base.onWindowResize(width, height);
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        if(mainScreen.getMainScene().isInit()) {
            if(!tick.isPaused()) mainScreen.getMainScene().update();
            return;
        }  
        base.update();  
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        if(mainScreen.getMainScene().isInit()) {
            mainScreen.getMainScene().render();
            return;
        }
        base.render();
    }
}