namespace App.Root.UI.Voip;

class VoipUIActions {
    private VoipUI voipUI;

    public VoipUIActions(VoipUI voipUI) {
        this.voipUI = voipUI;
    }

    // Activate
    public void activate() {
        voipUI.onShow();
    }

    // Deactivate
    public void deactivate() {
        voipUI.onHide();
    }
}