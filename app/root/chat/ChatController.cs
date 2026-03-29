using App.Root.Screen;
using App.Root.ui;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace App.Root.Chat;

class ChatController {
    private static ChatController? instance;

    public static ChatController getInstance() {
        instance ??= new ChatController();
        return instance;
    }

    private UIController? uiController;
    private ChatUI? ui;

    private KeyboardInput keyboardInput = new();
    private bool opened = false;
    private List<string> messages = new();
    private int maxMessages = 8;

    // Set UI
    public void setUIController(UIController uiController) {
        this.uiController = uiController;
    }

    public void setUI(ChatUI ui) {
        this.ui = ui;
    }

    // Get Text
    public string getText() {
        return keyboardInput.getText();
    }

    // Handle Key
    public void handleKey(Keys key, int action) {
        keyboardInput.handleKey(key, action);
        
        var el = ui?.getElementById("chatInput");
        if(el != null) el.text = keyboardInput.getText();
    }

    // Add Message
    public void addMessage(string? playerName, string message) {
        messages.Add($"{playerName}> {message}");
        if(messages.Count > maxMessages) messages.RemoveAt(0);

        var box = ui?.getElementById("chatBox");
        if(box != null) box.text = string.Join("\n", messages);
    }

    ///
    /// Open
    /// 
    public bool isOpen() {
        return opened;
    }

    public void open() {
        opened = true;

        uiController?.show(UIController.UIType.CHAT);
        ui?.getElementById("chatInput")?.setVisible(true);
    }

    ///
    /// Close
    /// 
    public void close() {
        opened = false;
        keyboardInput.clear();
        
        ui?.getElementById("chatInput")?.setVisible(false);
        var el = ui?.getElementById("chatInput");
        if(el != null) el.text = "";
    }
}