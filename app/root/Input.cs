using App.Root;
using App.Root.Player;
using App.Root.Screen;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = App.Root.Window;

class Input {
    private Window window;
    private Tick tick;
    private ScreenController screenController = null!;
    private PlayerInputMap? playerInputMap = null!;

    public Input(Window window, Tick tick) {
        this.window = window;
        this.tick = tick;
    }

    public void setScreenController(ScreenController screenController) {
        this.screenController = screenController;
    }
    
    public void setPlayerInputMap(PlayerInputMap playerInputMap) {
        this.playerInputMap = playerInputMap;
        setKeys();
    }

    // Set Keys
    private void setKeys() {
        window.onKeyDown += key => {
            if(key == Keys.Escape) {
                onEscape();
            } else {
                playerInputMap?.setKeyState(key, true);
            }
        };
        window.onKeyUp += key => playerInputMap?.setKeyState(key, false);
    } 

    // On Escape
    private void onEscape() {
        tick.togglePause();
        if(tick.isPaused()) unlockMouse();
        else lockMouse();
    }

    /// 
    /// Mouse
    /// 
    public void setMouse() {
        window.onMouseMove += (x, y) => screenController.handleMouseMove(x, y);
    }
    
    public void lockMouse() {
        window.CursorState = OpenTK.Windowing.Common.CursorState.Grabbed;
    }

    public void unlockMouse() {
        window.CursorState = OpenTK.Windowing.Common.CursorState.Normal;
    }

    ///
    /// Update
    /// 
    public void update() {
        if(playerInputMap == null || tick.isPaused()) return;

        var mouse = window.MouseState;
        float xOffset = mouse.Delta.X;
        float yOffset = -mouse.Delta.Y;

        playerInputMap.handleMouse(xOffset, yOffset);
        playerInputMap.keyboardCallback();
    }

    ///
    /// Init
    /// 
    public void init() {
        setMouse();
    }
}