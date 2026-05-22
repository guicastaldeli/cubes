namespace App.Root.Chat;

class Chat : UI.UI {
    public const string ID = "chat";

    public static string CHAT_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chat/");
    public static readonly string PATH = CHAT_DIR + "chat.xml";

    public Chat() : base(PATH, ID) {
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