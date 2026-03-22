namespace App.Root;
using OpenTK.Windowing.GraphicsLibraryFramework;

class InputField {
    private Dictionary<string, KeyboardInput> fields = new();
    private string? focusedId = null;
    private Screen.Screen screen;

    public InputField(Screen.Screen screen) {
        this.screen = screen;
    }

    // Register
    public void register(string id) {
        fields[id] = new KeyboardInput();
    }

    // Handle Click
    public void handleClick(int mouseX, int mouseY) {
        focusedId = null;
        foreach(var (id, _) in fields) {
            var el = screen.getElementById(id);
            if(el != null && el.containsPoint(mouseX, mouseY)) {
                focusedId = id;
                break;
            }
        }
    }

    // Handle Key Press
    public void handleKeyPress(Keys key, int action) {
        if(focusedId == null) return;
        if(fields.TryGetValue(focusedId, out var handler)) {
            handler.handleKey(key, action);
            sync(focusedId);
        }
    }

    // Sync
    private void sync(string id) {
        if(!fields.TryGetValue(id, out var handler)) return;
        var el = screen.getElementById(id);
        if(el != null) el.text = handler.getText();
    }

    // Get Text
    public string getText(string id) {
        return fields.TryGetValue(id, out var h) ? 
            h.getText() : 
            "";
    }

    // Clear
    public void clear(string id) {
        if(fields.TryGetValue(id, out var h)) {
            h.clear();
            sync(id);
        }
    }

    // Focus
    public bool isFocused(string id) {
        return focusedId == id;
    }

    public bool isFocus() {
        return focusedId != null;
    }
}