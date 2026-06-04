namespace App.Root.World.Points;
using App.Root.Screen;
using App.Root.UI;

class PointsUI : UI {
    public const string ID = "points";
    
    public static string POINTS_HUD_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/points/");
    public static string PATH = POINTS_HUD_DIR + "points.xml"; 

    public PointsUI() : base(PATH, ID) {
        init();
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

    /**

        Init
    
        */
    private void init() {
        EventStream.on("points-added", (data) => {
            if(data is not int total) return;
            DocParser.Replace("points", total);
        });
    }
}