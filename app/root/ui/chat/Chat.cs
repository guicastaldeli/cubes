namespace App.Root.ui;
using App.Root.Chat;

class Chat : UI {
    public static readonly string PATH = DIR + "chat/chat.xml";

    public Chat() : 
    base(PATH, "chat") {
        ChatController.getInstance().setChat(this);
        ChatController.getInstance().setUIController(uiController);
    }

    ///
    /// Render
    /// 
    public override void render() {
        base.render();
    }

    ///
    /// Update
    /// 
    public override void update() {
        ChatController.getInstance().update();
        base.update();
    }
}