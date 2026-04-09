namespace App.Root.Screen.Main.Custom;
using OpenTK.Windowing.GraphicsLibraryFramework;

class CustomMenu : Screen {
    public static readonly String PATH = DIR + "main/custom/custom_menu.xml";

    public MainScreen mainScreen;
    private CustomMenuActions customMenuActions;

    public InputField inputField;

    public CustomMenu(MainScreen mainScreen) :
    base(PATH, "custom_menu") {
        this.mainScreen = mainScreen;
        this.customMenuActions = new CustomMenuActions(this);
        this.inputField = new InputField(this);

        this.registerInputs();
    }

    private void registerInputs() {
        inputField.register("usernameInput");
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        inputField.handleClick(mouseX, mouseY);
        return base.checkClick(mouseX, mouseY);
    }

    // Handle Key Press
    public override void handleKeyPress(int key, int action) {
        inputField.handleKeyPress((Keys)key, action);
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

    ///
    /// Update
    /// 
    public override void update() {
        base.update();    
    }

    ///
    /// Render
    /// 
    public override void render() {
        base.render();
    }
}