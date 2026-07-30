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
    private static InputElement? GetElement(string id) {
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
    public static bool IsFocused(string id) {
        return focusedId == id;
    }

    public static bool IsFocus() {
        return focusedId != null;
    }

    public static void Focus(string id) {
        if(string.IsNullOrEmpty(id)) return;

        if(!fields.ContainsKey(id)) Register(id);
        focusedId = id;

        Console.WriteLine($"[InputField] Focused: {id}");
    }

    public static void Unfocus() {
        if(focusedId != null) {
            Console.WriteLine($"[InputField] Unfocused: {focusedId}");
            focusedId = null;
        }
    }

    // Get Text
    public static string GetText(string id) {
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
    // Register
    public static void Register(string id) {
        fields[id] = new KeyboardInput();
    }
    
    // Register Elements
    private static void RegisterElements(IEnumerable<string> ids) {
        foreach(var id in ids) {
            if(!string.IsNullOrEmpty(id) && !fields.ContainsKey(id)) {
                Register(id);
            }
        }
    }
    
    // Scan and Register
    public static void ScanAndRegister() {
        foreach(var screen in Screen.screenController.screens.Values) {
            //Console.WriteLine($"[InputField] Screen: {screen.screenName} | data: {screen.screenData?.elements.Count}");
            if(screen.screenData == null) continue;
            RegisterElements(screen.screenData.elements.Select(e => e.id));
        }

        foreach(var ui in UI.uiController.getUIs().Values) {
            //Console.WriteLine($"[InputField] UI: {ui.uiName} | data: {ui.uiData?.elements.Count}");
            if(ui.uiData == null) continue;
            RegisterElements(ui.uiData.elements.Select(e => e.id));
        }
        
        //Console.WriteLine($"[InputField] Registered fields: {string.Join(", ", fields.Keys)}");
    }

    /**
     * 
     * Handle Click
     *
     */
    public static void HandleClick(int mouseX, int mouseY) {
        focusedId = null;

        foreach(var (id, _) in fields) {
            var el = GetElement(id);
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
    public static void HandleKeyPress(Keys key, int action) {
        if(focusedId == null) return;
        if(fields.TryGetValue(focusedId, out var handler)) {
            handler.handleKey(key, action);
            Sync(focusedId);
        }
    }

    /**
     * 
     * Sync
     *
     */
    private static void Sync(string id) {
        if(!fields.TryGetValue(id, out var handler)) return;
        var el = GetElement(id);
        if(el != null) el.text = handler.getText();
    }

    /**
     * 
     * Clear
     *
     */
    public static void Clear(string id) {
        if(fields.TryGetValue(id, out var h)) {
            h.clear();
            Sync(id);
        }
    }
}