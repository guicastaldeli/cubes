namespace App.Root.Screen.Main;

class MainScreenAction {
    private ScreenController screenController;
    private MainScreen mainScreen;

    public MainScreenAction(ScreenController screenController, MainScreen mainScreen) {
        this.screenController = screenController;
        this.mainScreen = mainScreen;
    }

    // Open Client
    public void openClient() {
        mainScreen.show();
        mainScreen.setActive(false);
        mainScreen.clientDialog.setActive(true);
    }

    // Open Server 
    public void openServer() {
        mainScreen.hide();
        mainScreen.serverDialog.setActive(true);
    }

    // Open Custom Menu
    public void openCustomMenu() {
        mainScreen.hide();
        mainScreen.customMenu.setActive(true);
    }
}