namespace App.Root.World.Platform.Entity;
using App.Root.Animation;
using App.Root.Player;
using App.Root.Text;
using App.Root.UI;
using App.Root.Utils;
using OpenTK.Mathematics;

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

    // Set Color
    private void setColor(string id, Vector3 color, TextEntity textEntity) {
        var label = getElementById(id);
        if(label != null) label.color = new float[] { color.X, color.Y, color.Z, 1.0f };

        var el = textEntity.getElementById(id);
        if(el != null) el.color = new float[] { color.X, color.Y, color.Z, 1.0f };
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

    /**
    
        Animation
    
        */
    // Set Animation
    public void setAnimation(TextEntity textEntity, dynamic els, Vector3 color) {
        setAnimationPoint(textEntity, els, color);
    }

    // Set Animation Point
    public void setAnimationPoint(TextEntity textEntity, dynamic els, Vector3 color) {
        string elId = els.plusPoints.id;

        setColor(elId, color, textEntity);
        textEntity.refresh(elId);

        string id = $"chamber_point_{elId}_y";
        Vector3 pos = ChamberEntity.storedPos;

        setPosition(textEntity);

        float start = pos.Y;
        float end = pos.Y + 0.5f;

        float duration = 1.0f;

        AnimationController.Play(
            id,
            start, end,
            duration,
            value => setPosition(textEntity, 
                pos.X, 
                value, 
                pos.Z
            ),
            EaseOut.OutCubic
        );
    }

    /**
    
        Set Position
    
        */
    private void setPosition(TextEntity textEntity) {
        textEntity.setWorldPosition(new Vector3(ChamberEntity.storedPos));
    }

    private void setPosition(TextEntity textEntity, float x, float y, float z) {
        textEntity.setWorldPosition(new Vector3(x, y, z));
    }
}