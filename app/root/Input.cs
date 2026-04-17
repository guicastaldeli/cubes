using App.Root;
using App.Root.Chat;
using App.Root.Player;
using App.Root.Screen;
using App.Root.UI;
using App.Root.Voip;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = App.Root.Window;

class Input {
    private Window window;
    private Tick tick;

    private ScreenController screenController = null!;
    private UIController uiController = null!;
    private PlayerInputMap? playerInputMap = null!;
    private Network network = null!;

    private InputChat? inputChat;
    private InputVoip? inputVoip;

    public bool pauseOverlayOpen = false;

    public Input(Window window, Tick tick) {
        this.window = window;
        this.tick = tick;
    }

    // Screen Controller
    public void setScreenController(ScreenController screenController) {
        this.screenController = screenController;
    }

    public ScreenController getScreenController() {
        return screenController;
    }

    // UI Controller
    public void setUIController(UIController uiController) {
        this.uiController = uiController;
    }

    public UIController getUIController() {
        return uiController;
    }
    
    // Player Input Map
    public void setPlayerInputMap(PlayerInputMap playerInputMap) {
        this.playerInputMap = playerInputMap;
    }

    public PlayerInputMap getPlayerInputMap() {
        return playerInputMap!;
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
        // Chat
        if(inputChat != null) {
            inputChat.onKeyDown(key);
            if(ChatController.getInstance().isOpen()) return;
        }
        if(inputVoip != null) {
            inputVoip.onKeyDown(key);
        }    

        // Pause
        if(key == Keys.Escape) {
            onPause();
            return;
        } 
        
        // Screen Controller
        screenController.handleKeyPress((int)key, 1);

        // Player Input Map
        if(playerInputMap != null) {
            playerInputMap.openInventory(key);
            playerInputMap.setKeyState(key, true);
        }
    }

    private void onKeyUp(Keys key) {
        if(inputVoip != null) inputVoip.onKeyUp(key);
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
        uiController.handleMouseMove(x, y);
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.handleMouseMove(x, y);
    }

    private void onMouseClick(int x, int y) {
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.checkClick(x, y);
    }

    private void onMouseButton(int button, bool pressed) {
        if(!pressed) return;
        if(!screenController.isRunning() || pauseOverlayOpen) return;
        if(ChatController.getInstance().isOpen()) return;

        if(playerInputMap != null) playerInputMap.onMouseButton(button);
    }

    public void setMouse() {
        window.onMouseMove -= onMouseMove;
        window.onMouseClick -= onMouseClick;
        window.onMouseButton -= onMouseButton;
        
        window.onMouseMove += onMouseMove;
        window.onMouseClick += onMouseClick;
        window.onMouseButton += onMouseButton;
    }
    
    public void lockMouse() {
        window.CursorState = CursorState.Grabbed;
    }

    public void unlockMouse() {
        window.CursorState = CursorState.Normal;
    }

    ///
    /// Update
    ///
    public void update() {
        if(pauseUpdate()) return;

        var mouse = window.MouseState;
        float xOffset = mouse.Delta.X;
        float yOffset = -mouse.Delta.Y;

        playerInputMap?.handleMouse(xOffset, yOffset);
        playerInputMap?.keyboardCallback();
    }

    private bool pauseUpdate() {
        bool val = playerInputMap == null || 
            tick.isPaused() || 
            pauseOverlayOpen ||
            ChatController.getInstance().isOpen() ||
            playerInputMap.isInventoryOpen();

        return val;
    }

    ///
    /// Init
    /// 
    public void init() {
        setMouse();
        setKeys();

        inputChat = new InputChat(screenController, network);
        inputVoip = new InputVoip(screenController, uiController);
    }
}