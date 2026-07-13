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
using App.Root.Utils;

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
    private static Dictionary<string, string> typeToMethodMap = new();
    private static Dictionary<string, Action<object?>> typeToActionMap = new();
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

    // Handle By Type
    public static void HandleByType(string typeName, object? param = null) {
        if(!initialized) Register();

        if(typeToActionMap.TryGetValue(typeName, out var action)) {
            try {
                action(param);
            } catch(Exception err) {
                Console.WriteLine($"[GlobalInput] Error in {typeName}: {err.Message}");
            }
        } else {
            Console.WriteLine($"[GlobalInput] No action found for: {typeName}");
        } 
    }

    /**
     *
     * Find
     *
     */
    // Find Handler Type
    public static Type? FindHandlerType(object data) {
        var dataType = data.GetType();
        if(!dataType.IsGenericType || dataType.GetGenericTypeDefinition() != typeof(List<>)) {
            return null;
        }

        var elementType = dataType.GetGenericArguments()[0];
        var elementName = elementType.Name;

        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => {
            try { return a.GetTypes(); }
            catch { return new Type[0]; }
        }).Where(t => t.IsClass && !t.IsAbstract);

        foreach(var type in types) {
            var hasGlobalInput = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Any(m => m.GetCustomAttribute<GlobalInput>() != null);
            if(!hasGlobalInput) continue;

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach(var method in methods) {
                if(method.ReturnType == dataType) {
                    Console.WriteLine($"[GlobalInputHandler] Found handler: {type.Name} for {elementType.Name}");
                    return type;
                }
            }

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach(var prop in props) {
                if(prop.PropertyType == dataType) {
                    Console.WriteLine($"[GlobalInputHandler] Found handler: {type.Name} for {elementType.Name}");
                    return type;
                }
            }

            if(type.Name.Contains(elementName, StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine($"[GlobalInputHandler] Found handler by name: {type.Name} for {elementType.Name}");
                return type;
            }
        }

        Console.WriteLine($"[GlobalInputHandler] No handler found for {elementType.Name}");
        return null;
    }

    // Find Type From Action
    public static string? FindTypeFromAction(string action) {
        var types = typeToActionMap.Keys.ToList();

        foreach(var type in types) {
            if(action.Contains(type, StringComparison.OrdinalIgnoreCase)) return type;

            var singular = WordInflector.ToSingular(type);
            if(action.Contains(singular, StringComparison.OrdinalIgnoreCase)) return type;

            var plural = WordInflector.ToPlural(type);
            if(action.Contains(plural, StringComparison.OrdinalIgnoreCase)) return type;

            var actionType = WordInflector.ToSingular(action);
            if(type.Contains(actionType, StringComparison.OrdinalIgnoreCase)) return type;
        }

        return null;
    }

    // Get Element Type
    public static Type? GetElementType(object data) {
        var dataType = data.GetType();
        if(dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(List<>)) {
            return dataType.GetGenericArguments()[0];
        }

        return null;
    }

    /**
     *
     * Register
     *
     */
    // Register
    public static void Register() {
        if(initialized) return;

        var handlerType = typeof(GlobalInputHandler);
        var injectorMethods = handlerType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
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

    // Register Type
    public static void RegisterType(string typeName, Type type) {
        if(!initialized) Register();
        
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<GlobalInput>() != null);

        foreach(var method in methods) {
            if(injectorMethodNames.Contains(method.Name)) {
                Action<object?>? action = null;
                
                if(method.IsStatic) {
                    var param = method.GetParameters();
                    if(param.Length == 0) {
                        var delegateAction = (Action)Delegate.CreateDelegate(typeof(Action), method);
                        action = (p) => delegateAction();
                    } else if(param.Length == 1) {
                        action = (p) => method.Invoke(null, new object?[] { p });
                    } else {
                        action = (p) => method.Invoke(null, new[] { p });
                    }
                } else {
                    var instance = GetInstance(type);
                    if(instance != null) {
                        var param = method.GetParameters();
                        if(param.Length == 0) {
                            var delegateAction = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
                            action = (p) => delegateAction();
                        } else if(param.Length == 1) {
                            action = (p) => method.Invoke(instance, new[] { p });
                        }
                    }
                }

                if(action != null) {
                    typeToActionMap[typeName] = action;
                    Console.WriteLine($"[GlobalInput] Mapped {typeName} -> {type.Name}.{method.Name}");
                    return;
                }
            }
        }

        Console.WriteLine($"[GlobalInput] No action found for {typeName}");
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