namespace App.Root.Player.Inventory;
using System.Globalization;

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

    // Is Open
    public bool isOpen() {
        return visible;
    }

    // On Window Resize
    public override void onWindowResize(int width, int height) {
        base.onWindowResize(width, height);
        inventory.onResize(width, height);
    }

    // Handle Mouse Move
    public override void handleMouseMove(int mouseX, int mouseY) {
        base.handleMouseMove(mouseX, mouseY);
        inventory.handleMouseMove(mouseX, mouseY);
    }

    // Handle Mouse Click
    public override void handleMouseClick(int mouseX, int mouseY) {
        base.handleMouseClick(mouseX, mouseY);
        if(!visible) return;
        inventory.handleMouseClick(mouseX, mouseY);
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
        var bgEl = getElementById("inventory");
        if(bgEl == null) return null;

        var slotEl = getElementById("slotconfig");
        if(slotEl == null) return null;

        if(!slotEl.attr.TryGetValue("cols", out var c)) return null;
        if(!slotEl.attr.TryGetValue("rows", out var r)) return null;
        if(!slotEl.attr.TryGetValue("edgePaddingPct", out var ep)) return null;
        if(!slotEl.attr.TryGetValue("topPaddingPct", out var tp)) return null;
        if(!slotEl.attr.TryGetValue("gapPct", out var gp)) return null;
        if(!slotEl.attr.TryGetValue("slotWidthPct", out var swp)) return null;
        if(!slotEl.attr.TryGetValue("slotHeightPct", out var shp)) return null;

        int cols = int.Parse(c, CultureInfo.InvariantCulture);
        int rows = int.Parse(r, CultureInfo.InvariantCulture);
        float edgePct = float.Parse(ep, CultureInfo.InvariantCulture);
        float topPct = float.Parse(tp, CultureInfo.InvariantCulture);
        float gapPct = float.Parse(gp, CultureInfo.InvariantCulture);
        float slotWidthPct = float.Parse(swp, System.Globalization.CultureInfo.InvariantCulture);
        float slotHeightPct = float.Parse(shp, System.Globalization.CultureInfo.InvariantCulture);

        return new Inventory(
            screenWidth, screenHeight,
            bgEl.x, bgEl.y,
            bgEl.width, bgEl.height,
            cols, rows,
            edgePct, topPct, gapPct,
            slotWidthPct, slotHeightPct
        );
    }
}