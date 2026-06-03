namespace App.Root.Player.Shop;
using App.Root.UI;

class ShopUI : UI {
    public static string SHOP_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "player/shop/");
    public static string PATH = SHOP_DIR + "shop.xml";

    private Shop shop;
    
    public ShopUI() : base(PATH, Shop.ID) {
        this.shop = new Shop(input, uiController);
        this.shop.open();
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