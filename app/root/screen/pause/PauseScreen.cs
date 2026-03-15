namespace App.Root.Screen.Pause;

class PauseScreen : Screen {
    public static readonly String PATH = DIR + "pause/pause_screen.xml";
    
    public PauseScreenAction pauseScreenAction;

    public PauseScreen() : 
    base(PATH, "pause") {
        this.pauseScreenAction = new PauseScreenAction(tick, screenController, this);
    }

    // Handle Action
    public override void handleAction(string action) {
        switch(action) {
            case "resume":
                pauseScreenAction.resume();
                break;
            case "back":
                pauseScreenAction.backToMenu();
                break;
        }
    }

    // Handle Mouse Move
    public override void handleMouseMove(int mouseX, int mouseY) {
        base.handleMouseMove(mouseX, mouseY);
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        return base.checkClick(mouseX, mouseY);
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