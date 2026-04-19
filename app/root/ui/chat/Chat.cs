namespace App.Root.UI.Chat;
using App.Root.Chat;

class Chat : UI {
    public static readonly string PATH = DIR + "chat/chat.xml";

    public Chat() : 
    base(PATH, "chat") {
        ChatController.getInstance().setChat(this);
        ChatController.getInstance().setUIController(uiController);
    }

    /**
    
        On Window Resize

        */
    public override void onWindowResize(int width, int height) {
        base.onWindowResize(width, height);
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
        ChatController.getInstance().update();
        ChatController.getInstance().show();
        base.update();
    }
}