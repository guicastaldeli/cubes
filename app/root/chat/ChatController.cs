using App.Root.Screen;
using App.Root.ui;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace App.Root.Chat;

class ChatController {
    private static ChatController? instance;

    private UIController? uiController;
    private ui.Chat? chat;

    private KeyboardInput keyboardInput = new();
    private bool opened = false;

    private List<string> messages = new();
    private bool messageAdded = false;
    private float messageTimer = 5.0f;
    private float messageDuration = 5.0f;
    private int maxMessages = 8;
    private bool boxVisible = false;

    public static ChatController getInstance() {
        instance ??= new ChatController();
        return instance;
    }

    // Chat
    public void setChat(ui.Chat chat) {
        this.chat = chat;
    }

    private UIElement? chatBox {
        get => chat?.getElementById("chatBox");
    }

    private UIElement? chatInput {
        get => chat?.getElementById("chatInput");
    }

    // Set UI
    public void setUIController(UIController uiController) {
        this.uiController = uiController;
    }

    // Get Text
    public string getText() {
        return keyboardInput.getText();
    }

    // Handle Key
    public void handleKey(Keys key, int action) {
        keyboardInput.handleKey(key, action);
        if(chatInput != null) chatInput.text = keyboardInput.getText();
    }

    // Add Message
    public void addMessage(string? playerName, string message) {
        messageAdded = true;

        messages.Add($"{playerName}> {message}");
        if(messages.Count > maxMessages) messages.RemoveAt(0);

        if(chatBox != null) {
            chatBox.text = string.Join("\n", messages);
            chatBox.setVisible(true);
        }

        boxVisible = true;
        messageTimer = messageDuration;
    }

    ///
    /// Open
    /// 
    public bool isOpen() {
        return opened;
    }

    public void open() {
        opened = true;

        if(chatBox != null) chatBox.setVisible(true);
        chatInput?.setVisible(true);
    }

    ///
    /// Close
    /// 
    public void close() {
        opened = false;
        keyboardInput.clear();

        boxVisible = true;
        messageTimer = messageDuration;

        chatInput?.setVisible(false);
        if(chatInput != null) chatInput.text = "";
    }

    ///
    /// Update
    /// 
    public void update() {
        if(!boxVisible || opened) return;

        messageTimer -= Tick.getDeltaTimeI();
        if(messageTimer <= 0.0f) {
            messageTimer = 0.0f;
            boxVisible = false;
            chatBox?.setVisible(false);
        }
    }

    ///
    /// Show
    /// 
    public void show() {
        if(!opened && !messageAdded) return;
        uiController?.show(UIController.UIType.CHAT);
        messageAdded = false;
    }
}