namespace App.Root.Screen.Main;
using App.Root.Info;
using App.Root.Utils;
using App.Root.Input;

[ActionConverter]
class MainScreenAction {
    private static List<string> Elements = new() {
        "usernameInfoLabel",
        "idInfoLabel",
        "idBtn"
    };

    private ScreenController screenController;
    private MainScreen mainScreen;

    public MainScreenAction(ScreenController screenController, MainScreen mainScreen) {
        this.screenController = screenController;
        this.mainScreen = mainScreen;
    }

    // Open Client
    [GlobalInput]
    public void openClient() {
        mainScreen.show();
        mainScreen.setActive(false);
        mainScreen.clientDialog.setActive(true);
    }

    // Open Server 
    [GlobalInput]
    public void openServer() {
        mainScreen.hide();
        mainScreen.serverDialog.setActive(true);
    }

    // Open Custom Menu
    [GlobalInput]
    public void openCustomMenu() {
        mainScreen.hide();
        mainScreen.customMenu.setActive(true);
    }

    /**
     * 
     * Get
     * 
     */
    public dynamic get() {
        return ElementEntry.C(id => mainScreen.getElementById(id), Elements);
    }
    
    /**
     * 
     * Info
     *
     */
    // Username
    public void refreshUsername() {
        string username = InfoController.Username;
        DocParser.Replace("username", username);
    }

    // Id
    [GlobalInput]
    public void generateTempId() {
        InfoController.getInstance().getUserInfo().switchTempId();
        switchId();
    }

    private void showId() {
        ScreenElement? label = get().idInfoLabel;
        ScreenElement? button = get().idBtn;
        if(label != null && button != null) {
            label.visible = true;
            button.visible = true;
        }
    }

    public void switchId() {
        string id = InfoController.getInstance().getUserInfo().getId();
        DocParser.Replace("id", id);
    }
}