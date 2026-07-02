namespace App.Root.Player.Shop;
using App.Root.Screen;
using App.Root.UI;

class ShopUI : UI {
    public static string SHOP_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "player/shop/");
    public static string PATH = SHOP_DIR + "shop.xml";

    private Shop shop;
    
    public ShopUI() : base(PATH, Shop.ID) {
        this.shop = new Shop(input, uiController);
        this.shop.initData();
        this.shop.open();
    }

    // Resolve Content
    private void resolveContent() {
        if(File.Exists(PATH)) {
            var content = File.ReadAllText(PATH);
            var resolved = DocParser.LResolve(content);
            if(resolved != content) uiData = DocParser.parseUI(resolved, Window.WIDTH, Window.HEIGHT);
        }
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        resolveContent();
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