namespace App.Root.Player.Inventory;

class InventoryUI : UI.UI {
    public static string PATH = DIR + "inventory.xml"; 

    private Inventory inventory;

    public InventoryUI() : base(PATH, "inventory") {
        var el = getElementById("iventory_bg");
        int width = el!.imgWidth;
        int height = el!.imgHeight;
        inventory = new Inventory(
            shaderProgram,
            screenWidth, screenWidth,
            width, height
        );
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
}