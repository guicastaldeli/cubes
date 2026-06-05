namespace App.Root.Screen.Main.Custom;
using App.Root.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

class CustomMenu : Screen {
    public static string PATH = DIR + "main/custom/custom_menu.xml";

    public MainScreen mainScreen;
    private CustomMenuActions customMenuActions;

    public CustomMenu(MainScreen mainScreen) : base(PATH, "custom_menu") {
        this.mainScreen = mainScreen;
        this.customMenuActions = new CustomMenuActions(this);

        this.registerInputs();
    }

    private void registerInputs() {
        InputField.register("usernameInput");
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