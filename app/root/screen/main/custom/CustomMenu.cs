namespace App.Root.Screen.Main.Custom;
using App.Root.Input;
using App.Root.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

class CustomMenu : MainScreenHandler {
    public const string ID = "custom_menu";
    public static string PATH = Screen.DIR + "main/custom/custom_menu.xml";

    private CustomMenuActions customMenuActions;

    public CustomMenu([Inject] MainScreen mainScreen) : base(PATH, ID) {
        this.mainScreen = mainScreen;
        this.customMenuActions = new CustomMenuActions(this);

        this.registerInputs();
    }

    private void registerInputs() {
        InputField.register("usernameInput");
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
            case "confirm":
                customMenuActions.confirm();
                break;
            case "back":
                customMenuActions.back();
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
        base.update();    
    }

    /**
    
        Render

        */
    public override void render() {
        base.render();
    }
}