namespace App.Root.Voip;

class VoipUI : UI.UI {
    public const string ID = "voip";

    public static string VOIP_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "voip/");
    public static readonly string PATH = VOIP_DIR + "voip_ui.xml";

    private VoipUIActions voipUIActions;

    public VoipUI() : base(PATH, ID) {
        this.voipUIActions = new VoipUIActions(this);
    }

    // Get Voip UI Actions
    public VoipUIActions getVoipUIActions() {
        return voipUIActions;
    }

    /**
    
        On Window Resize

        */
    public override void onWindowResize(int width, int height) {
        base.onWindowResize(width, height);
    }

    /**
    
        Render

        */
    public override void render() {
        base.render();
    }

    /**
    
        Update

        */
    public override void update() {
        base.update();
    }
}