namespace App.Root.Screen.Main.Custom;

using System.Reflection;
using App.Root.Input;
using App.Root.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

class CustomMenu : MainScreenHandler {
    public const string ID = "custom_menu";
    public static string PATH = DIR + "main/custom/custom_menu.xml";

    private CustomMenuActions customMenuActions;

    public CustomMenu([Inject] MainScreen mainScreen) : base(PATH, ID) {
        this.mainScreen = mainScreen;
        this.customMenuActions = new CustomMenuActions(this);
    }

    // Get Main Screen
    public MainScreen getMainScreen() {
        return mainScreen;
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        InputField.HandleClick(mouseX, mouseY);
        return base.checkClick(mouseX, mouseY);
    }

    // Handle Key Press
    public override void handleKeyPress(int key, int action) {
        InputField.HandleKeyPress((Keys)key, action);
    }

    // Handle Action
    public override void handleAction(string action) {
        var converted = ActionConverter.Convert(action);
        if(converted.MethodName == null) return;

        var method = customMenuActions.GetType().GetMethod(converted.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if(method != null) {
            var args = converted.Param != null ? new object[] { converted.Param } : null;
            method.Invoke(customMenuActions, args);
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
        base.update();    
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        base.render();
    }
}