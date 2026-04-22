namespace App.Root.Player.Hud;
using App.Root.Mesh;
using App.Root.Resource;

class Aim : HudElement {
    private static string ID = "aim";
    private static string TEX_PATH = "player/hud/aim.png";
    private static string MESH = "quad"; 

    private int width = 8;
    private int height = 8;

    private bool initialized = false;

    public Aim() : base(ID) {
        
    }

    // Set
    private void set() {
        int texId = TextureLoader.load(TEX_PATH);

        MeshData data = MeshLoader.load(MESH);
        mesh.add(ID, data);

        var renderer = mesh.getMeshRenderer(ID);
        if(renderer != null) renderer.isHud = true;

        mesh.setTexture(ID, texId);
        mesh.setScale(ID, width, height, 1.0f);
        mesh.setPosition(
            ID,
            screenWidth / 2.0f - width / 2.0f,
            screenHeight / 2.0f - height / 2.0f,
            0.0f
        );
    }

    // Update Position
    public void updatePosition() {
        mesh.setScale(ID, width, height, 1.0f);
        mesh.setPosition(
            ID,
            screenWidth / 2.0f - width / 2.0f,
            screenHeight / 2.0f - height / 2.0f,
            0.0f
        );
    }

    /**

        On Window Resize
    
        */
    public override void onWindowResize(int width, int height) {
        screenWidth = width;
        screenHeight = height;
        if(initialized) updatePosition();
        base.onWindowResize(width, height);
    }

    /**

        Render
    
        */
    public override void render() {
        //base.render();
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