namespace App.Root;
using OpenTK.Windowing.GraphicsLibraryFramework;

class KeyboardInput {
    private string currentText = "";
    private bool capsLock = false;
    private bool shiftPressed = false;
    private int maxLen;

    public KeyboardInput() {
        this.maxLen = 50;
    }

    // Handle Key
    public bool handleKey(Keys key, int action) {
        if(key == Keys.LeftShift || key == Keys.RightShift) {
            shiftPressed = (action == 1 || action == 2);
            return false;
        }
        if(key == Keys.CapsLock && action == 1) {
            capsLock = !capsLock;
            return false;
        }
        if(action != 1 && action != 2) return false;
        if(key == Keys.Backspace) return handleBackspace();
        if(key == Keys.Enter || key == Keys.Escape) return false;

        return handleTextInput(key);
    }

    // Handle Backspace
    private bool handleBackspace() {
        if(currentText.Length > 0) {
            currentText = currentText[..^1];
            return true;
        }
        return false;
    }

    // Handle Text Input
    private bool handleTextInput(Keys key) {
        if(currentText.Length >= maxLen) return false;

        char c = getCharForKey(key);
        if(c != '\0') {
            currentText += c;
            return true;
        }
        return false;
    }

    // Get Char for Key
    private char getCharForKey(Keys key) {
        if(key >= Keys.A && key <= Keys.Z) {
            char baseChar = (char)('a' + (key - Keys.A));
            bool makeUppercase = shiftPressed ^ capsLock;
            return makeUppercase ? char.ToUpper(baseChar) : baseChar;
        }
        if(key >= Keys.D0 && key <= Keys.D9) {
            if(shiftPressed) {
                char[] symbols = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };
                return symbols[key - Keys.D0];
            }
            return (char)('0' + (key - Keys.D0));
        }
        if(key == Keys.Space) return ' ';

        return key switch {
            Keys.Minus         => shiftPressed ? '_' : '-',
            Keys.Equal         => shiftPressed ? '+' : '=',
            Keys.LeftBracket   => shiftPressed ? '{' : '[',
            Keys.RightBracket  => shiftPressed ? '}' : ']',
            Keys.Semicolon     => shiftPressed ? ':' : ';',
            Keys.Apostrophe    => shiftPressed ? '"' : '\'',
            Keys.Comma         => shiftPressed ? '<' : ',',
            Keys.Period        => shiftPressed ? '>' : '.',
            Keys.Slash         => shiftPressed ? '?' : '/',
            Keys.Backslash     => shiftPressed ? '|' : '\\',
            Keys.GraveAccent   => shiftPressed ? '~' : '`',
            _                  => '\0'
        };
    }

    // Text
    public void setText(string text) {
        currentText = text.Length <= maxLen ? text : text[..maxLen];
    }

    public string getText() => currentText;

    // Max Length
    public void setMaxLen(int maxLen) {
        this.maxLen = maxLen;
        if(currentText.Length > maxLen) currentText = currentText[..maxLen];
    }

    public int getMaxLen() => maxLen;
    public bool isShiftPressed() => shiftPressed;
    public bool isCapsLock() => capsLock;
    public int getLength() => currentText.Length;
    public bool isEmpty() => currentText.Length == 0;
    public void clear() => currentText = "";
}