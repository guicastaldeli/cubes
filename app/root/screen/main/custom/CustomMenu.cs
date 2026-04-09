namespace App.Root.Screen.Main.Custom;

class CustomMenu : Screen {
    public static readonly String PATH = DIR + "main/custom/custom_menu.xml";

    private CustomMenuActions customMenuActions;

    public CustomMenu(MainScreen mainScreen) :
    base(PATH, "custom_menu") {
        this.customMenuActions = new CustomMenuActions(this);
    }

    // Handle Action
    public override void handleAction(string action) {
        switch(action) {
            
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