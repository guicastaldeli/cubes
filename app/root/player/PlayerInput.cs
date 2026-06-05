namespace App.Root.Player;
using App.Root.Mesh;
using OpenTK.Windowing.GraphicsLibraryFramework;

/**

    Mapper

    */
static class Mapper {
    private static Type? currentType = null;

    private static readonly Dictionary<Type, List<Keys>> keyBindings = new();
    private static readonly Dictionary<Type, Action<Keys, bool>> keyHandlers = new();
    private static readonly Dictionary<Keys, Action<bool>> keyActions = new();
    private static readonly HashSet<Keys> heldKeys = new();

    private static readonly Dictionary<Type, List<int>> mouseBindings = new();
    private static readonly Dictionary<Type, Action<int, bool>> mouseHandlers = new();
    private static readonly Dictionary<int, Action> mouseActions = new();
    private static readonly HashSet<(Type, int)> registeredMouse = new();

    /**
    
        Set
    
        */
    public static void Set<T>() {
        currentType = typeof(T);
        if(!keyBindings.ContainsKey(typeof(T))) keyBindings[typeof(T)] = new();
        if(!mouseBindings.ContainsKey(typeof(T))) mouseBindings[typeof(T)] = new();
        
    }

    public static void Set<T>(Keys k) {
        Set<T>();
        Key(k);
    }

    public static void Set<T>(int button) {
        Set<T>();
        Mouse(button);
    }

    /**
    
        Key
    
        */
    public static void Key(Keys k) {
        if(currentType == null) return;
        if(!keyBindings[currentType].Contains(k)) {
            keyBindings[currentType].Add(k);
        }
    }

    public static void Key(Keys k, Action<bool> action) {
        Key(k);
        keyActions[k] = action;
    }

    /**
    
        Mouse
    
        */
    public static void Mouse(int button) {
        if(currentType == null) return;

        if(!mouseBindings.ContainsKey(currentType)) {
            mouseBindings[currentType] = new();
        }
        if(!mouseBindings[currentType].Contains(button)) {
            mouseBindings[currentType].Add(button);
        }
    }

    public static void Mouse(int button, Action action) {
        if(currentType == null) return;

        var key = (currentType, button);
        if(registeredMouse.Contains(key)) return;
        registeredMouse.Add(key);

        if(!mouseBindings.ContainsKey(currentType)) mouseBindings[currentType] = new();
        if(!mouseBindings[currentType].Contains(button)) mouseBindings[currentType].Add(button);

        mouseActions[button] = action;
    }

    /**
    
        Has
    
        */
    public static bool HasKey<T>(Keys k) {
        bool val = keyBindings.ContainsKey(typeof(T)) && keyBindings[typeof(T)].Contains(k);
        return val;
    }

    public static bool HasMouse<T>(int b) {
        bool val = mouseBindings.ContainsKey(typeof(T)) && mouseBindings[typeof(T)].Contains(b);
        return val;
    }

    /**
    
        On
    
        */
    public static void On<T>(Action<int, bool> handler) {
        mouseHandlers[typeof(T)] = handler;
    }

    public static void On<T>(Action<Keys, bool> handler) {
        keyHandlers[typeof(T)] = handler;
    }

    public static void On<T>(Keys[] keys, Action<Keys, bool> handler) {
        var type = typeof(T);
        if(!keyBindings.ContainsKey(type)) return;
        
        foreach(var k in keys) {
            if(!keyBindings[type].Contains(k)) {
                keyBindings[type].Add(k);
            }
        }

        keyHandlers[type] = handler;
    }

    /**
    
        Dispatch
    
        */
    public static void Dispatch(bool pressed, Keys key) {
        if(pressed) {
            if(heldKeys.Contains(key)) return;
            heldKeys.Add(key);
        } else {
            heldKeys.Remove(key);
        }
            
        foreach(var (type, keys) in keyBindings) {
            if(keys.Contains(key) && keyHandlers.TryGetValue(type, out var handler)) {
                handler(key, pressed);
            }
        }

        if(keyActions.TryGetValue(key, out var action)) {
            action(pressed);
        }
    }

    public static void Dispatch(bool pressed, int button) {
        foreach(var (type, buttons) in mouseBindings) {
            if(buttons.Contains(button) && mouseHandlers.TryGetValue(type, out var handler)) {
                handler(button, pressed);
            }
        }

        if(pressed && mouseActions.TryGetValue(button, out var action)) {
            action();
        }
    }

    /**
    
        Clear
    
        */
    public static void Clear() {
        keyBindings.Clear();
        keyHandlers.Clear();
        keyActions.Clear();
        keyActions.Clear();

        mouseBindings.Clear();
        mouseHandlers.Clear();
        mouseActions.Clear();
        registeredMouse.Clear();
        
        currentType = null;
    }
}

/**

    Player Input main class

    */
class PlayerInput {
    private Input input;
    private PlayerController playerController;
    
    private bool[] keyPressed = new bool[(int) Keys.LastKey + 1];
    private bool fKeyPressed = false;

    public PlayerInput(Input input, PlayerController playerController) {
        this.input = input;
        this.playerController = playerController;
    }

    // Get Player Controller
    public PlayerController getPlayerController() {
        return playerController;
    }

    // Set Key State
    public void setKeyState(Keys key, bool pressed) {
        int idx = (int)key;
        if(idx >= 0 && idx < keyPressed.Length) {
            keyPressed[idx] = pressed;
        }

        Mapper.Dispatch(pressed, key);
    }

    // Is Key Down
    public bool isKeyDown(Keys key) {
        int idx = (int)key;
        bool val = idx > 0 && idx < keyPressed.Length && keyPressed[idx];
        return val;
    }

    // Handle Mouse
    public void handleMouse(float xOffset, float yOffset) {
        playerController.getCamera().handleMouse(xOffset, yOffset);
    }

    // On Mouse Button
    public void onMouseButton(int button, bool pressed) {
        Mode mode = playerController.getMode();
        
        if(mode.getCurrentMode() == Modes.GETTER) {
            Mapper.Dispatch(pressed, button);
            if(pressed) mode.executeAction();
        }
    }
}