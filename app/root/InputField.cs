namespace App.Root;
using App.Root.Screen;
using OpenTK.Windowing.GraphicsLibraryFramework;

class InputField {
    private Dictionary<string, KeyboardInput> fields = new();
    private string? focusedId = null;
    private Screen.Screen screen;

    public InputField(Screen.Screen screen) {
        this.screen = screen;
    }
}