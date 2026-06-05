namespace App.Root.Input;
using App.Root.Screen;
using App.Root.UI;
using OpenTK.Windowing.GraphicsLibraryFramework;

interface IInputElement {
    bool containsPoint(int x, int y);
    string text { get; set; }
}

class InputField {
    private static Dictionary<string, KeyboardInput> fields = new();
    private static string? focusedId = null;

    // Get Element
    private static InputElement? getElement(string id) {
        foreach(var screen in Screen.screenController.screens.Values) {
            if(!screen.isActive()) continue;

            var found = screen.getElementById(id);
            if(found != null) return InputElement.From(found);
        }

        foreach(var ui in UI.uiController.getUIs().Values) {
            if(!ui.visible) continue;

            var found = ui.getElementById(id);
            if(found != null) return InputElement.From(found);
        }

        return null;
    }

    // Focus
    public static bool isFocused(string id) {
        return focusedId == id;
    }

    public static bool isFocus() {
        return focusedId != null;
    }

    // Get Text
    public static string getText(string id) {
        string val = fields.TryGetValue(id, out var h) ? 
            h.getText() : 
            "";
        return val;
    }

    /**
     * 
     * Register
     *
     */
    public static void register(string id) {
        fields[id] = new KeyboardInput();
    }

    /**
     * 
     * Handle Click
     *
     */
    public static void handleClick(int mouseX, int mouseY) {
        focusedId = null;
        foreach(var (id, _) in fields) {
            var el = getElement(id);
            if(el != null && el.ContainsPoint(mouseX, mouseY)) {
                focusedId = id;
                break;
            }
        }
    }

    /**
     * 
     * Handle Key Press
     *
     */
    public static void handleKeyPress(Keys key, int action) {
        if(focusedId == null) return;
        if(fields.TryGetValue(focusedId, out var handler)) {
            handler.handleKey(key, action);
            sync(focusedId);
        }
    }

    /**
     * 
     * Sync
     *
     */
    private static void sync(string id) {
        if(!fields.TryGetValue(id, out var handler)) return;
        var el = getElement(id);
        if(el != null) el.text = handler.getText();
    }

    /**
     * 
     * Clear
     *
     */
    public static void clear(string id) {
        if(fields.TryGetValue(id, out var h)) {
            h.clear();
            sync(id);
        }
    }
}