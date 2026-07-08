namespace App.Root.Player.Shop;
using App.Root.Screen;
using App.Root.UI;

class ShopUI : UI {
    public static string SHOP_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "player/shop/");
    public static string PATH = SHOP_DIR + "shop.xml";

    private Shop shop;

    private bool initialized = false;
    
    public ShopUI() : base(PATH, Shop.ID) {
        this.shop = new Shop(input, uiController);
        this.shop.initData();
        
        resolveContent();

        this.shop.open();
    }

    // Resolve Content
    private void resolveContent() {
        uiData = DocParser.parseUI(PATH, Window.WIDTH, Window.HEIGHT);
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