namespace App.Root.Screen.Main;

using System.Reflection;
using App.Root.Screen.Main.Client;
using App.Root.Screen.Main.Custom;
using App.Root.Screen.Main.Server;
using App.Root.Utils;

class MainScreen : Screen {
    public const string ID = "main";
    public static readonly string PATH = DIR + "main/main_screen.xml";
    
    private MainScreenAction mainScreenAction;

    public ClientDialog clientDialog;
    public ServerDialog serverDialog;
    public CustomMenu customMenu;

    private MainScreenRegistry mainScreenRegistry;

    public MainScreen() : base(PATH, ID) {
        this.mainScreenAction = new MainScreenAction(screenController, this);

        mainScreenRegistry = new MainScreenRegistry(this);
        mainScreenRegistry.init();

        this.clientDialog = mainScreenRegistry.get<ClientDialog>()!;
        this.serverDialog = mainScreenRegistry.get<ServerDialog>()!;
        this.customMenu = mainScreenRegistry.get<CustomMenu>()!;

        this.updateScreen();
    }

    // Get Main Screen Actions
    public MainScreenAction getMainScreenAction() {
        return mainScreenAction;
    }

    // Handle Action
    public override void handleAction(string action) {
        if(mainScreenRegistry.anyActive()) {
            mainScreenRegistry.handleAction(action);
            return;
        }
        
        var converted = ActionConverter.Convert(action);
        if(converted.MethodName == null) return;

        var method = mainScreenAction.GetType().GetMethod(converted.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if(method != null) {
            var args = converted.Param != null ? new object[] { converted.Param } : null;
            method.Invoke(mainScreenAction, args);

            if(active && method.GetCustomAttribute<UpdateAfter>() != null) {
                updateScreen();
            }
        }
    }

    // Handle Mouse Move
    public override void handleMouseMove(int mouseX, int mouseY) {
        if(mainScreenRegistry.anyActive()) { 
            mainScreenRegistry.handleMouseMove(mouseX, mouseY); 
            return; 
        }
        
        base.handleMouseMove(mouseX, mouseY);
    }

    // Handle Key Press
    public override void handleKeyPress(int key, int action) {
        if(mainScreenRegistry.anyActive()) { 
            mainScreenRegistry.handleKeyPress(key, action); 
            return; 
        }

        base.handleKeyPress(key, action);
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        if(mainScreenRegistry.anyActive()) return mainScreenRegistry.checkClick(mouseX, mouseY);
        return base.checkClick(mouseX, mouseY);
    }

    /**
     * 
     * On Window Resize
     *
     */
    public override void onWindowResize(int width, int height) {
        base.onWindowResize(width, height);

        updateScreen();
        mainScreenRegistry.onWindowResize(width, height);
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        if(mainScreenRegistry.anyActive()) {
            mainScreenRegistry.update();
            return;
        }

        base.update();    
    }

    private void updateScreen() {
        mainScreenAction.refreshUsername();
        mainScreenAction.switchId();
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        if(mainScreenRegistry.anyActive()) {
            mainScreenRegistry.render();
            return;
        }

        base.render();
    }

    /**
     * 
     * Reset
     *
     */
    public void reset() {
        show();
    }
}