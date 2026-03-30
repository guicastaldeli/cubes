namespace App.Root.Voip;
using App.Root.Screen;
using OpenTK.Windowing.GraphicsLibraryFramework;

class InputVoip {
    private ScreenController screenController;

    public InputVoip(ScreenController screenController) {
        this.screenController = screenController;
    }

    // On Key Down
    public void onKeyDown(Keys key) {
        if(key == Keys.X && screenController.isRunning()) {
            VoiceController.getInstance().start();
            return;
        }
    }

    // On Key Up
    public void onKeyUp(Keys key) {
        if(key == Keys.X) {
            VoiceController.getInstance().stop();
            return;
        }
    }
}