using App.Root;
using App.Root.Player;

class Input {
    private Window window;
    private PlayerInputMap? playerInputMap = null!;

    private float lastMouseX = 0;
    private float lastMouseY = 0;
    private bool firstMouse = true;

    public Input(Window window) {
        this.window = window;
    }

    public void setPlayerInputMap(PlayerInputMap playerInputMap) {
        this.playerInputMap = playerInputMap;
        setKeys();
    }

    // Set Keys
    private void setKeys() {
        window.onKeyDown += key => playerInputMap?.setKeyState(key, true);
        window.onKeyUp += key => playerInputMap?.setKeyState(key, false);
    } 

    /// 
    /// Lock/Unlock Mouse
    /// 
    public void lockMouse() {
        window.CursorState = OpenTK.Windowing.Common.CursorState.Grabbed;
    }

    public void unlockMouse() {
        window.CursorState = OpenTK.Windowing.Common.CursorState.Normal;
        firstMouse = true;
    }

    ///
    /// Update
    /// 
    public void update() {
        if(playerInputMap == null) return;

        var mouse = window.MouseState;
        float xOffset = mouse.Delta.X;
        float yOffset = mouse.Delta.Y;

        playerInputMap.handleMouse(xOffset, yOffset);
        playerInputMap.keyboardCallback();
    }
}