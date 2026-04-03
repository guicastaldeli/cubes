namespace App.Root.Voip;
using App.Root.Screen;
using App.Root.UI;
using App.Root.UI.Voip;
using OpenTK.Windowing.GraphicsLibraryFramework;

class InputVoip {
    private ScreenController screenController;
    private UIController uiController;

    public InputVoip(ScreenController screenController, UIController uiController) {
        this.screenController = screenController;
        this.uiController = uiController;
    }

    // Get Voip UI
    public VoipUI? getVoipUI() {
        var voipUI = uiController.get<VoipUI>(UIController.UIType.VOIP);
        if(voipUI != null) return voipUI;
        return null;
    }

    public VoipUIActions? getVoipUIActions() {
        var voipUI = uiController.get<VoipUI>(UIController.UIType.VOIP);
        if(voipUI != null) return voipUI.getVoipUIActions();
        return null;
    }

    // On Key Down
    public void onKeyDown(Keys key) {
        if(key == Keys.X && screenController.isRunning()) {
            VoiceController.getInstance().start();
            getVoipUIActions()?.activate();
            return;
        }
    }

    // On Key Up
    public void onKeyUp(Keys key) {
        if(key == Keys.X) {
            VoiceController.getInstance().stop();
            getVoipUIActions()?.deactivate();
            return;
        }
    }
}