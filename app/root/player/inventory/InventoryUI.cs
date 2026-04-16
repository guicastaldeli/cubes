namespace App.Root.Player.Inventory;

class InventoryUI : UI.UI {
    public static string INVENTORY_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "player/inventory/");
    public static string PATH = INVENTORY_DIR + "inventory.xml"; 

    private Inventory inventory = null!;

    public InventoryUI() : base(PATH, "inventory") {
        init();
    }

    // Get Inventory
    public Inventory getInventory() {
        return inventory;
    }

    // On Show
    public override void onShow() {
        base.onShow();
    }

    // On Hide
    public override void onHide() {
        base.onHide();
    }

    // On Window Resize
    public override void onWindowResize(int width, int height) {
        base.onWindowResize(width, height);
        inventory.onResize(width, height);
    }

    ///
    /// Render 
    /// 
    public override void render() {
        if(!visible) return;
        base.render();
        inventory.render();
    }

    ///
    /// Init
    /// 
    private void init() {
        inventory = build()!;
        inventory.setShaderProgram(shaderProgram);
        inventory.setTextRenderer(textRenderer!);
    }

    /**

        Build
    
        */
    private Inventory? build() {
        int cols = 9;
        int rows = 3;

        float edgePct = 0.012f; 
        float topPct = 0.055f; 
        float gapPct = 0.006f;
        
        // Bg El
        var bgEl = getElementById("inventory");
        if(bgEl == null) return null;
        
        // Slot El
        var slotEl = getElementById("slotconfig");
        if(slotEl != null) {
            if(slotEl.attr.TryGetValue("cols", out var c)) cols = int.Parse(c);
            if(slotEl.attr.TryGetValue("rows", out var r)) rows = int.Parse(r);
            if(slotEl.attr.TryGetValue("edgePaddingPct", out var ep)) edgePct = float.Parse(ep);
            if(slotEl.attr.TryGetValue("topPaddingPct", out var tp)) topPct = float.Parse(tp);
            if(slotEl.attr.TryGetValue("gapPct", out var gp)) gapPct = float.Parse(gp);
        }

        return new Inventory(
            screenWidth, screenHeight,
            bgEl.x, bgEl.y,
            bgEl.imgWidth, bgEl.imgHeight,
            cols, rows,
            edgePct, topPct, gapPct
        );
    }
}