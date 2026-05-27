namespace App.Root.World.Platform.Entity;
using App.Root.Player;
using App.Root.UI;
using App.Root.Utils;

[ClassRegistryIgnore]
class ChamberDialog : UI {
    private static List<string> Elements = new() {
        "deposit",
        "plusPoints"
    };

    public const string CHAMBER_DIALOG_ID = "chamber_dialog";
    
    public static string CHAMBER_DIALOG_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/platform/entity/");
    public static string PATH = CHAMBER_DIALOG_DIR + "chamber_dialog.xml";

    private bool initialized = false;

    private PlayerController playerController = null!;

    public ChamberDialog() : base(PATH, CHAMBER_DIALOG_ID) {
        
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
    
        Get
    
        */
    public dynamic get() {
        return ElementEntry.C(id => getElementById(id), Elements);
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
            if(id == ChamberEntity.CHAMBER_ENTITY_ID) {
                show();
            } else {
                hide();
            }
        };
    }
}