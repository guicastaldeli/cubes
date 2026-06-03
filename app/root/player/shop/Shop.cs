using App.Root.Chat;
using App.Root.UI;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace App.Root.Player.Shop;

class Shop {
    public const string ID = "shop";

    private Input input;
    private UIController uiController;

    public Shop(Input input, UIController uiController) {
        this.input = input;
        this.uiController = uiController;

        Mapper.set<Shop>();
    }

    /**
    
        Open
    
        */
    public void open() {
        // Open Key
        Mapper.key(Keys.O, pressed => {
            if(!pressed) return;
            if(ChatController.getInstance().isOpen()) return;
            if(input.onPauseOverlayOpen()) return;

            uiController.toggle(ID);
            bool isActive = uiController.getActive() == ID;

            Action action = isActive ? () =>
                input.unlockMouse() : () =>
                input.lockMouse();
            action();
        });

        // Close Key
        Mapper.key(Keys.Escape, pressed => {
            if(!pressed) return;
            if(uiController.getActive() != ID) return;

            uiController.hide();
            input.lockMouse();
        });
    }

    /**
    
        Close
    
        */
    public void close() {
        
    }
}