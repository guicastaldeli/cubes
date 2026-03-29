namespace App.Root.ui;
using App.Root.Chat;

class ChatUI : UI {
    public static readonly string PATH = DIR + "chat/chat.xml";

    public ChatUI() : 
    base(PATH, "chat") {
        ChatController.getInstance().setUIController(uiController);
        ChatController.getInstance().setUI(this);

        uiController.show(UIController.UIType.CHAT);
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
        base.update();
    }
}