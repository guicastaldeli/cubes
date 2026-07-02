namespace App.Root.Player.Shop;
using App.Root.Chat;
using App.Root.UI;
using App.Root.Input;
using App.Root.World.Platform;
using App.Root.Screen;
using OpenTK.Windowing.GraphicsLibraryFramework;

class Shop {
    public const string ID = "shop";

    private Input input;
    private UIController uiController;

    public Shop(Input input, UIController uiController) {
        this.input = input;
        this.uiController = uiController;

        Mapper.Set<Shop>();
    }

    // Set Theme Data
    public void setThemeData() {
        var (id, data) = ThisData.EGetData<List<PlatformThemes.Theme>>()!;
        if(id == null || data == null) throw new ArgumentNullException("err setThemeData");
        DocParser.ReplaceObject(id, data);
    }

    /**
     * 
     * Open
     *
     */
    public void open() {
        // Open Key
        Mapper.Key(Keys.O, pressed => {
            if(!pressed) return;
            if(ChatController.getInstance().isOpen()) return;
            if(input.onPauseOverlayOpen()) return;

            uiController.toggle(ID);
            bool isActive = uiController.getActive() == ID;
            //Close here too!
            Action action = isActive ? () =>
                input.unlockMouse() : () =>
                input.lockMouse();
            action();
        });

        // Close Key
        Mapper.Key(Keys.Escape, pressed => {
            if(!pressed) return;
            if(uiController.getActive() != ID) return;

            uiController.hide();
            input.lockMouse();
        });
    }

    /**
     * 
     * Init Data
     *
     */
    public void initData() {
        setThemeData();
    }

    /**
     * 
     * Close
     *
     */
    public void close() {
        
    }
}