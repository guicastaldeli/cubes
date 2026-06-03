namespace App.Root;
using App.Root.Chat;
using App.Root.Player;
using App.Root.Screen;
using App.Root.Screen.Pause;
using App.Root.UI;
using App.Root.Voip;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

/**

    Key Action helper

    */
public static class KeyAction {
    public const int Press = 1;
    public const int Release = 0;
}

/**

    Input main class.

    */
class Input {
    private Window window;
    private Tick tick;

    private ScreenController screenController = null!;
    private UIController uiController = null!;
    private PlayerInput? playerInput = null!;
    private Network network = null!;

    private InputChat? inputChat;
    private InputVoip? inputVoip;

    public bool pauseOverlayOpen = false;

    private static Dictionary<string, (string eventId, Keys key)> keyListeners = new();

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
    
    // Player Input
    public void setPlayerInput(PlayerInput playerInput) {
        this.playerInput = playerInput;
    }

    public PlayerInput getPlayerInput() {
        return playerInput!;
    }

    // Set Network
    public void setNetwork(Network network) {
        this.network = network;
    }

    // On Pause Overlay Open
    public bool onPauseOverlayOpen() {
        return pauseOverlayOpen;
    }

    /**
    
        Listen
     
        */
    public static void listen(string eventId, Keys key) {
        keyListeners[eventId] = (eventId, key);
    }

    /**
    
        Stream
    
        */
    private static void stream(Keys key, int action) {
        foreach(var (eventId, bind) in keyListeners) {
            if(bind.key == key) {
                EventStream.set(eventId, (key: (int)key, action: action));
            }
        }
    }

    /**
    
        Keys
    
        */
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
        screenController.handleKeyPress((int)key, KeyAction.Press);

        // Player Input Map
        if(playerInput != null) {
            playerInput.openInventory(key);
            playerInput.setKeyState(key, true);
        }

        stream(key, KeyAction.Press);
    }

    private void onKeyUp(Keys key) {
        if(inputVoip != null) inputVoip.onKeyUp(key);
        screenController.handleKeyPress((int)key, KeyAction.Release);
        playerInput?.setKeyState(key, false);

        stream(key, KeyAction.Release);
    }

    /**
    
        On Pause
    
        */
    private void onPause() {
        // Screen Controller
        if(!screenController.isRunning()) return;

        // Inventory
        if(playerInput != null) playerInput.hideInventory();
        
        // Multiplayer
        bool isMultiplayer = network!.isConnected;
        if(!isMultiplayer) tick.togglePause();

        // Pause Screen
        activePauseScreen();
    }

    private void activePauseScreen() {
        pauseOverlayOpen = !pauseOverlayOpen;
        
        if(pauseOverlayOpen) {
            unlockMouse();
            screenController.switchToOverlay(PauseScreen.ID);
        } else {
            lockMouse();
            screenController.closeOverlay();
        }
    }

    /**
    
        Mouse
    
        */
    private void onMouseMove(int x, int y) {
        uiController.handleMouseMove(x, y);
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.handleMouseMove(x, y);
    }

    private void onMouseClick(int x, int y) {
        uiController.handleMouseClick(x, y, 0, 1);
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.checkClick(x, y);
    }

    private void onMouseButton(int button, bool pressed) {
        if(!pressed) return;

        if(!screenController.isRunning() || pauseOverlayOpen) return;
        
        if(ChatController.getInstance().isOpen()) return;

        if(playerInput != null && playerInput.isInventoryOpen()) return;
        if(playerInput != null) playerInput.onMouseButton(button);
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


    /**
    
        Update
    
        */
    public void update() {
        if(pauseUpdate()) return;

        var mouse = window.MouseState;
        float xOffset = mouse.Delta.X;
        float yOffset = -mouse.Delta.Y;

        playerInput?.handleMouse(xOffset, yOffset);
        playerInput?.keyboardCallback();
    }

    private bool pauseUpdate() {
        bool val = playerInput == null || 
            tick.isPaused() || 
            pauseOverlayOpen ||
            ChatController.getInstance().isOpen() ||
            playerInput.isInventoryOpen();

        return val;
    }

    /**
    
        Init
    
        */
    public void init() {
        setMouse();
        setKeys();

        inputChat = new InputChat(screenController, network);
        inputVoip = new InputVoip(screenController, uiController);
    }
}