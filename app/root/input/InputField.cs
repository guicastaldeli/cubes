namespace App.Root.Input;
using App.Root.Screen;
using OpenTK.Windowing.GraphicsLibraryFramework;

class InputField {
    private static Dictionary<string, KeyboardInput> fields = new();
    private static string? focusedId = null;

    // Register
    public static void register(string id) {
        fields[id] = new KeyboardInput();
    }

    // Handle Click
    public static void handleClick(int mouseX, int mouseY) {
        focusedId = null;
        foreach(var (id, _) in fields) {
            var el = Screen.getElementByIdI(id);
            if(el != null && el.containsPoint(mouseX, mouseY)) {
                focusedId = id;
                break;
            }
        }
    }

    // Handle Key Press
    public static void handleKeyPress(Keys key, int action) {
        if(focusedId == null) return;
        if(fields.TryGetValue(focusedId, out var handler)) {
            handler.handleKey(key, action);
            sync(focusedId);
        }
    }

    // Sync
    private static void sync(string id) {
        if(!fields.TryGetValue(id, out var handler)) return;
        var el = Screen.getElementByIdI(id);
        if(el != null) el.text = handler.getText();
    }

    // Get Text
    public static string getText(string id) {
        return fields.TryGetValue(id, out var h) ? 
            h.getText() : 
            "";
    }

    // Clear
    public static void clear(string id) {
        if(fields.TryGetValue(id, out var h)) {
            h.clear();
            sync(id);
        }
    }

    // Focus
    public static bool isFocused(string id) {
        return focusedId == id;
    }

    public static bool isFocus() {
        return focusedId != null;
    }
}