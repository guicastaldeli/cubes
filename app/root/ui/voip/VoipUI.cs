namespace App.Root.UI.Voip;

class VoipUI : UI {
    public static readonly string PATH = DIR + "voip/voip_ui.xml";

    private VoipUIActions voipUIActions;

    public VoipUI() : 
    base(PATH, "voip") {
        this.voipUIActions = new VoipUIActions(this);
    }

    // Get Voip UI Actions
    public VoipUIActions getVoipUIActions() {
        return voipUIActions;
    }

    // Elements
    public UIElement? getVoiceDivElement {
        get => getElementById("voiceDiv");
    }

    public UIElement? getVoiceElement {
        get => getElementById("voice");
    }

    ///
    /// Render
    /// 
    public override void render() {
        base.render();
    }

    ///
    /// Update
    /// 
    public override void update() {
        base.update();
    }
}