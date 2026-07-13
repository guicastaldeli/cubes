namespace App.Root.Player.Shop;

using App.Root.Input;
using App.Root.Screen;
using App.Root.UI;

class ShopUI : UI {
    public static string SHOP_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "player/shop/");
    public static string PATH = SHOP_DIR + "shop.xml";

    private Shop shop;
    
    public ShopUI() : base(PATH, Shop.ID) {
        this.shop = new Shop(input, uiController);
        
        resolveContent();

        this.shop.open();
    }

    // Resolve Content
    private void resolveContent() {
        uiData = DocParser.parseUI(PATH, Window.WIDTH, Window.HEIGHT);
    }

    // Handle Action
    public override void handleAction(string action) {
        if(string.IsNullOrEmpty(action)) return;

        Console.WriteLine($"[ShopUI] Action: {action}");

        var typeName = GlobalInputHandler.FindTypeFromAction(action);
        if(typeName != null) {
            GlobalInputHandler.HandleByType(typeName);
            return;
        }
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {        
        base.render();
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        base.update();
    }
}