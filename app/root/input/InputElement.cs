using App.Root.Screen;
using App.Root.UI;

namespace App.Root.Input;

class InputElement {
    private Func<int, int, bool> containsPoint;

    private Func<string> getText;
    private Action<string> setText;
    public string text {
        get => getText();
        set => setText(value);
    }

    public InputElement(
        Func<int, int, bool> containsPoint,
        Func<string> getText,
        Action<string> setText
    ) {
        this.containsPoint = containsPoint;
        this.getText = getText;
        this.setText = setText;
    }

    // Contains Point
    public bool ContainsPoint(int x, int y) {
        return containsPoint(x, y);
    }

    /**
     * 
     * From
     *
     */
    public static InputElement? From(ScreenElement? el) {
        if(el == null) return null;
        return new InputElement(el.containsPoint, () => el.text, v => el.text = v);
    }

    public static InputElement? From(UIElement? el) {
        if(el == null) return null;
        return new InputElement(el.containsPoint, () => el.text, v => el.text = v);
    }
}