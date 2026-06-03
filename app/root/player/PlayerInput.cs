namespace App.Root.Player;
using App.Root.Chat;
using App.Root.Mesh;
using App.Root.Player.Inventory;
using OpenTK.Windowing.GraphicsLibraryFramework;

/**

    Mapper

    */
static class Mapper {
    private static Type? currentType = null;

    private static readonly Dictionary<Type, List<Keys>> bindings = new();
    private static readonly Dictionary<Type, Action<Keys, bool>> handlers = new();
    private static readonly Dictionary<Keys, Action<bool>> keyActions = new();
    private static readonly HashSet<Keys> heldKeys = new();

    /**
    
        Set
    
        */
    public static void set<T>() {
        currentType = typeof(T);
        bindings[currentType] = new List<Keys>();
    }

    public static void set<T>(Keys k) {
        set<T>();
        key(k);
    }

    /**
    
        Key
    
        */
    public static void key(Keys k) {
        if(currentType == null) return;
        if(!bindings[currentType].Contains(k)) {
            bindings[currentType].Add(k);
        }
    }

    public static void key(Keys k, Action<bool> action) {
        if(currentType == null) return;
        if(!bindings[currentType].Contains(k)) {
            bindings[currentType].Add(k);
        }

        keyActions[k] = action;
    }

    public static void onKey<T>(Action<Keys, bool> handler) {
        handlers[typeof(T)] = handler;
    }

    public static void onKey<T>(Keys[] keys, Action<Keys, bool> handler) {
        var type = typeof(T);
        if(!bindings.ContainsKey(type)) return;

        foreach(var k in keys) {
            if(!bindings[type].Contains(k)) {
                bindings[type].Add(k);
            }
        }

        handlers[type] = handler;
    }

    public static bool hasKey<T>(Keys k) {
        var type = typeof(T);
        bool val = bindings.ContainsKey(type) &&
            bindings[type].Contains(k);

        return val;
    }

    /**
    
        Dispatch
    
        */
    public static void dispatch(Keys key, bool pressed) {
        if(pressed) {
            if(heldKeys.Contains(key)) return;
            heldKeys.Add(key);
        } else {
            heldKeys.Remove(key);
        }
        
        foreach(var (type, keys) in bindings) {
            if(keys.Contains(key) && handlers.TryGetValue(type, out var handler)) {
                handler(key, pressed);
            }
        }

        if(keyActions.TryGetValue(key, out var action)) {
            action(pressed);
        }
    }

    /**
    
        Clear
    
        */
    public static void clear() {
        bindings.Clear();
        handlers.Clear();
        keyActions.Clear();
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

        Mapper.dispatch(key, pressed);
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
    public void onMouseButton(int button) {
        Mode mode = playerController.getMode();
        
        if(mode.getCurrentMode() == Modes.GETTER) {
            MeshInteractionController meshInteractionController = playerController.getMesh().getMeshInteractionController();
            if(meshInteractionController != null) {
                if(button == 0) meshInteractionController.onBreak();
                if(button == 1) meshInteractionController.onPlace();
                mode.executeAction();
            }

            return;
        }
    }
}