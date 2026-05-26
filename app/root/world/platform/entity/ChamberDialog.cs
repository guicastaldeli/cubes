using App.Root.Player;

namespace App.Root.World.Platform.Entity;

class ChamberDialog : UI.UI {
    public static string ID = "chamber_dialog";
    
    public static string CHAMBER_DIALOG_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/platform/entity/");
    public static string PATH = CHAMBER_DIALOG_DIR + "chamber_dialog.xml";

    private bool initialized = false;

    private PlayerController playerController = null!;

    public ChamberDialog() : base(PATH, ID) {
        
    }

    // Set Player Controller
    public void setPlayerController(PlayerController playerController) {
        this.playerController = playerController;
    }

    // On Show
    public override void onShow() {
        base.onShow();
    }

    // On Hide
    public override void onHide() {
        base.onHide();
    }

    /**
    
        Show
    
        */
    public void show() {
        var label = getElementById("deposit");
        if(label != null) label.visible = true;
    
        visible = true;
    }

    /**
    
        Hide
    
        */
    public void hide() {
        visible = false;
    }

    /**
    
        Init
    
        */
    public void init() {
        if(initialized) return;
        initialized = true;
    }

    /**
    
        Render
    
        */
    public override void render() {
        if(!visible) return;
        base.render();
    }

    /**
    
        Activate
    
        */
    public void activate() {
        var raycaster = playerController.getRaycaster();
        raycaster.onHit += (string? id) => {
            if(id == "chamber") {
                show();
            } else {
                hide();
            }
        };
    }
}