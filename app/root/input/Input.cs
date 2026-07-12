namespace App.Root.Input;
using App.Root.Chat;
using App.Root.Player;
using App.Root.Screen;
using App.Root.Screen.Pause;
using App.Root.UI;
using App.Root.Voip;
using System.Reflection;
using AppWindow = App.Root.Window;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;

/**

    Key Action helper

    */
public static class KeyAction {
    public const int Press = 1;
    public const int Release = 0;
}

/**

    Global Input

    */
[AttributeUsage(AttributeTargets.Method)]
public class InputInjector : Attribute {}

[AttributeUsage(AttributeTargets.Method)]
public class GlobalInput : Attribute {}

public abstract class GlobalInputHandler {
    private static Dictionary<string, List<Action>> handlers = new();
    private static HashSet<string> injectorMethodNames = new();

    private static bool initialized = false;

    [InputInjector] public static void HandleMouseClick() { Init(); }
    [InputInjector] public static void HandleKeyPress() { Init(); }

    // Get Instance
    private static object? GetInstance(Type type) {
        try {
            object? val = Activator.CreateInstance(type);
            return val;
        } catch {
            return null;
        }
    }

    /**
     *
     * Register
     *
     */
    public static void Register() {
        if(initialized) return;

        var handlerType = typeof(GlobalInputHandler);
        var injectorMethods = handlerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<InputInjector>() != null);

        foreach(var method in injectorMethods) {
            injectorMethodNames.Add(method.Name);
            if(!handlers.ContainsKey(method.Name)) handlers[method.Name] = new List<Action>();
        }

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach(var type in types) {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<GlobalInput>() != null);

            foreach(var method in methods) {
                try {
                    if(injectorMethodNames.Contains(method.Name)) {
                        Action? action = null;

                        if(method.IsStatic) {
                            action = (Action)Delegate.CreateDelegate(typeof(Action), method);
                        } else {
                            var instance = GetInstance(type);
                            if(instance != null) action = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
                        }

                        if(action != null) {
                            handlers[method.Name].Add(action);
                            Console.WriteLine($"[GlobalInput] Registered: {type.Name}.{method.Name}");
                        }
                    }
                } catch(Exception err) {
                    Console.WriteLine($"[GlobalInput] Error registering {type.Name}.{method.Name}: {err.Message}");
                }
            }
        }

        initialized = true;
    }

    /**
     *
     * Init
     *
     */
    public static void Init([CallerMemberName] string? name = null) {
        if(!initialized) Register();

        if(name != null && handlers.TryGetValue(name, out var actions)) {
            foreach(var action in actions) {
                try {
                    action();
                } catch(Exception err) {
                    Console.WriteLine($"[GlobalInput] Error in {name}: {err.Message}");
                }
            }
        }
    }
}

/**

    Input main class.

    */
class Input {
    private AppWindow window;
    private Tick tick;

    private ScreenController screenController = null!;
    private UIController uiController = null!;
    private PlayerInput? playerInput = null!;
    private Network network = null!;

    private InputChat? inputChat;
    private InputVoip? inputVoip;

    public bool pauseOverlayOpen = false;

    private static Dictionary<string, (string eventId, Keys key)> keyListeners = new();
    private static Dictionary<Type, Func<bool>> pauseChecks = new();

    public Input(AppWindow window, Tick tick) {
        this.window = window;
        this.tick = tick;

        AddPause<Input>(() => pauseOverlayOpen);

        GlobalInputHandler.Register();
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
     * 
     * Listen
     *
     */
    public static void listen(string eventId, Keys key) {
        keyListeners[eventId] = (eventId, key);
    }

    /**
     * 
     * Stream
     *
     */
    private static void stream(Keys key, int action) {
        foreach(var (eventId, bind) in keyListeners) {
            if(bind.key == key) {
                EventStream.set(eventId, (key: (int)key, action: action));
            }
        }
    }

    /**
     * 
     * Keys
     *
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

        if(InputField.isFocus()) {
            InputField.handleKeyPress(key, KeyAction.Press);
            return;
        }

        // Pause
        if(key == Keys.Escape) {
            onPause();
            return;
        } 
        
        // Screen Controller
        screenController.handleKeyPress((int)key, KeyAction.Press);

        // Player Input
        if(playerInput != null) {
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
     * 
     * Pause
     *
     */
    // On Pause
    private void onPause() {
        // Screen Controller
        if(!screenController.isRunning()) return;
        
        // Multiplayer
        bool isMultiplayer = network!.isConnected;
        if(!isMultiplayer) tick.togglePause();

        // Pause Screen
        activePauseScreen();
    }

    // Active Pause Screen
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

    // Add Pause
    public static void AddPause<T>(Func<bool> cond) {
        pauseChecks[typeof(T)] = cond;
    }

    /**
     * 
     * Mouse
     *
     */
    private void onMouseMove(int x, int y) {
        uiController.handleMouseMove(x, y);
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.handleMouseMove(x, y);
    }

    private void onMouseClick(int x, int y) {
        uiController.handleMouseClick(x, y, 0, 1);
        InputField.handleClick(x, y);
        if(screenController.isRunning() && !pauseOverlayOpen) return;
        screenController.checkClick(x, y);
    }

    private void onMouseButton(int button, bool pressed) {
        if(!pressed) return;
        if(!screenController.isRunning() || pauseOverlayOpen) return;
        if(ChatController.getInstance().isOpen()) return;
        if(playerInput != null) playerInput.onMouseButton(button, pressed);
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
     * 
     * Update
     *
     */
    public void update() {
        if(updatePause()) return;

        var mouse = window.MouseState;
        float xOffset = mouse.Delta.X;
        float yOffset = -mouse.Delta.Y;

        playerInput?.handleMouse(xOffset, yOffset);
    }

    private bool updatePause() {
        if(playerInput  == null) return true;

        foreach(var check in pauseChecks.Values) {
            if(check()) return true;
        }

        return false;
    }

    /**
     * 
     * Init
     *
     */
    public void init() {
        setMouse();
        setKeys();

        inputChat = new InputChat(screenController, network);
        inputVoip = new InputVoip(screenController, uiController);
    }
}