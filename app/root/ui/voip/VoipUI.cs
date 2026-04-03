namespace App.Root.UI.Voip;

class VoipUI : UI {
    public static readonly string PATH = DIR + "voip/voip_ui.xml";

    public VoipUI() : 
    base(PATH, "voip") {
        
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