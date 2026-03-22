using App.Root.Screen.Main;

namespace App.Root.Screen.Pause;

class PauseScreenAction {
    public Tick tick;
    public Input input;
    public ScreenController screenController;
    public PauseScreen pauseScreen;

    public PauseScreenAction(
        Tick tick,
        Input input,
        ScreenController screenController, 
        PauseScreen pauseScreen
    ) {
        this.tick = tick;
        this.input = input;
        this.screenController = screenController;
        this.pauseScreen = pauseScreen;
    }

    ///
    /// Resume
    /// 
    public void resume() {
        if(!screenController.main.getNetwork().isConnected) tick.setPaused(false);
        input.lockMouse();
        screenController.closeOverlay();
        input.pauseOverlayOpen = false;
    }

    ///
    /// Back to Menu
    /// 
    public void backToMenu() {
        tick.setPaused(false);
        input.unlockMouse();
        screenController.running = false;
        
        Screen.scene.reset();
        var mainSceen = (MainScreen)screenController.screens[ScreenController.SCREENS.MAIN];
        mainSceen.reset();

        screenController.switchTo(ScreenController.SCREENS.MAIN);
    }
}