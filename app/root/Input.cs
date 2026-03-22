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
    private Network? network = null!;

    public bool pauseOverlayOpen = false;

    public Input(Window window, Tick tick) {
        this.window = window;
        this.tick = tick;
    }

    // Set Screen Controller
    public void setScreenController(ScreenController screenController) {
        this.screenController = screenController;
    }
    
    // Set Player Input Map
    public void setPlayerInputMap(PlayerInputMap playerInputMap) {
        this.playerInputMap = playerInputMap;
    }

    // Set Network
    public void setNetwork(Network network) {
        this.network = network;
    }

    ///
    /// Keys
    /// 
    private void setKeys() {
        window.onKeyDown -= onKeyDown;
        window.onKeyUp -= onKeyUp;
        window.onKeyDown += onKeyDown;
        window.onKeyUp += onKeyUp;
    } 

    private void onKeyDown(Keys key) {
        if(key == Keys.Escape) {
            onPause();
        } else {
            screenController.handleKeyPress((int)key, 1);
            playerInputMap?.setKeyState(key, true);
        }
    }

    private void onKeyUp(Keys key) {
        screenController.handleKeyPress((int)key, 0);
        playerInputMap?.setKeyState(key, false);
    }

    // On Pause
    private void onPause() {
        if(!screenController.isRunning()) return;
        
        bool isMultiplayer = network!.isConnected;
        if(!isMultiplayer) tick.togglePause();

        pauseOverlayOpen = !pauseOverlayOpen;
        if(pauseOverlayOpen) {
            unlockMouse();
            screenController.switchToOverlay(ScreenController.SCREENS.PAUSE);
        }
        else {
            lockMouse();
            screenController.closeOverlay();
        }
    }

    /// 
    /// Mouse
    /// 
    private void onMouseMove(int x, int y) {
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.handleMouseMove(x, y);
    }

    private void onMouseClick(int x, int y) {
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.checkClick(x, y);
    }

    public void setMouse() {
        window.onMouseMove -= onMouseMove;
        window.onMouseClick -= onMouseClick;
        
        window.onMouseMove += onMouseMove;
        window.onMouseClick += onMouseClick;
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
        if(playerInputMap == null || tick.isPaused() || pauseOverlayOpen) return;

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
        setKeys();
    }
}