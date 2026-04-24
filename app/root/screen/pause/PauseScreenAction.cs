using App.Root.Screen.Main;

namespace App.Root.Screen.Pause;

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
    
        Resume
    
        */
    public void resume() {
        if(!network.isConnected) tick.setPaused(false);
        input.lockMouse();
        screenController.closeOverlay();
        input.pauseOverlayOpen = false;
    }

    /**
    
        Back to Menu
    
        */
    public void backToMenu() {
        tick.setPaused(false);

        input.unlockMouse();
        input.pauseOverlayOpen = false;
        
        screenController.running = false;
        network.stop();
        
        Screen.scene.reset();
        var mainSceen = (MainScreen)screenController.screens[ScreenController.SCREENS.MAIN];
        mainSceen.reset();

        screenController.switchTo(ScreenController.SCREENS.MAIN);
    }
}