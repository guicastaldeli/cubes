namespace App.Root.Screen.Pause;
using App.Root.Screen.Main;
using App.Root.Input;

class PauseScreenAction {
    private Tick tick;
    private Input input;
    private ScreenController screenController;
    private PauseScreen pauseScreen;
    private Network network;

    public PauseScreenAction(
        Tick tick,
        Input input,
        ScreenController screenController, 
        PauseScreen pauseScreen,
        Network network
    ) {
        this.tick = tick;
        this.input = input;
        this.screenController = screenController;
        this.pauseScreen = pauseScreen;
        this.network = network;
    }

    /**
     * 
     * Resume
     *
     */
    public void resume() {
        if(!network.isConnected) tick.setPaused(false);
        input.lockMouse();
        screenController.closeOverlay();
        input.pauseOverlayOpen = false;
    }

    /**
     * 
     * Back to Menu
     *
     */
    public void backToMenu() {
        tick.setPaused(false);

        input.unlockMouse();
        input.pauseOverlayOpen = false;
        
        screenController.running = false;
        network.stop();
        
        pauseScreen.getMainScene().reset();

        var mainScreen = screenController.get<MainScreen>(MainScreen.ID);
        if(mainScreen != null) mainScreen.reset();
        screenController.switchTo(MainScreen.ID);
    }
}