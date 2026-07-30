namespace App.Root.Screen.Main.Client;
using App.Root.Screen.Main.Server;
using App.Root.Input;
using App.Root.Utils;
using System.Reflection;

class ClientDialog : MainScreenHandler {
    private const string ID = "client_dialog";
    public static readonly string PATH = DIR + "main/client/client_dialog.xml";
    
    private ClientDialogAction clientDialogAction;

    private bool needsUpdate = true;

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
        var converted = ActionConverter.Convert(action);
        if(converted.MethodName == null) return;

        var method = clientDialogAction.GetType().GetMethod(converted.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if(method != null) {
            var args = converted.Param != null ? 
                new object[] { converted.Param } :
                null;
            method.Invoke(clientDialogAction, args);
            
            if(active) updateSaves();
            return;
        }
    }

    // Open
    public override void open() {
        mainScreen.hide();
        show();

        needsUpdate = true;
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

    // Update Saves
    public void updateSaves() {
        clientDialogAction.registerObjects();
        
        refresh();
        
        clientDialogAction.elements = ElementEntry.C<ScreenElement>(id => getElementById(id), ClientDialogAction.Elements);
        needsUpdate = false;
        Console.WriteLine("[ClientDialog] Screen refreshed");
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

        if(needsUpdate) updateSaves();
        base.render();
    }
}