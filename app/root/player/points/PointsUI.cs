namespace App.Root.Player.Points;

class PointsUI : UI.UI {
    public static string POINTS_HUD_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "player/points/");
    public static string PATH = POINTS_HUD_DIR + "points.xml"; 

    public PointsUI() : base(PATH, "points") {
        
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
}