namespace App.Root.Screen.Pause;

class PauseScreenAction {
    public Tick tick;
    public ScreenController screenController;
    public PauseScreen pauseScreen;

    public PauseScreenAction(
        Tick tick,
        ScreenController screenController, 
        PauseScreen pauseScreen
    ) {
        this.tick = tick;
        this.screenController = screenController;
        this.pauseScreen = pauseScreen;
    }

    ///
    /// Resume
    /// 
    public void resume() {
        screenController.switchTo(null);
    }

    ///
    /// Back to Menu
    /// 
    public void backToMenu() {
        screenController.switchTo(ScreenController.SCREENS.MAIN);
    }
}