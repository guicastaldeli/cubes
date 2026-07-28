namespace App.Root.Screen.Main.Client;
using App.Root.Screen.Main.Server;
using App.Root.Input;
using App.Root.Utils;

class ClientDialog : MainScreenHandler {
    private const string ID = "client_dialog";
    public static readonly string PATH = DIR + "main/client/client_dialog.xml";
    
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
            case "back":
                clientDialogAction.back();
                return;
            case "create_save":
                clientDialogAction.createSave();
                return;
            case "confirm_save":
                clientDialogAction.confirmCreateSave();
                return;
            case "cancel_save":
                clientDialogAction.cancelSave();
                return;
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
    // Update
    public override void update() {
        if(mainScreen.getMainScene().isInit()) {
            if(!tick.isPaused()) mainScreen.getMainScene().update();
            return;
        }  
        base.update();  
    }

    // Update Save List
    public void updateSaveList() {
        screenData = DocParser.parseScreen(PATH, Screen.screenWidth, Screen.screenHeight);
        clientDialogAction.elements = ElementEntry.C<ScreenElement>(
            id => getElementById(id), 
            ClientDialogAction.Elements
        );
        Console.WriteLine("[ClientDialog] Save list updated");
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