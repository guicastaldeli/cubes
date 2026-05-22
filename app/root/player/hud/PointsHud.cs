namespace App.Root.Player.Hud;

class PointsHud : HudElement {
    private static string ID = "points_hud";
    private static string TEX_PATH = "player/hud/src/points.xml";

    private bool initialized = false;

    public PointsHud() : base(ID) {
        
    }

    // Set
    private void set() {
        
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
        if(!initialized) {
            set();
            initialized = true;
        }
        base.update();
    }
}