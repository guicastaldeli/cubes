namespace App.Root.Screen;
using App.Root;
using App.Root.Shaders;
using App.Root.Input;
using App.Root.Scene;
using System.Collections.Generic;
using System.Reflection;
using App.Root.Chunk;

[ManagedState]
class ScreenController {
    public int screenWidth;
    public int screenHeight;

    private Tick tick;
    private Input input;
    private Window window;
    private ShaderProgram shaderProgram;
    private MainScene? scene;
    private Network network;

    public Dictionary<string, Screen> screens = new();
    public string? activeScreen = null;
    public Screen? currentScreen = null;
    public Screen? prevScreen = null;

    public bool running = false;

    public ScreenController(
        Tick tick,
        Input input,
        Window window,
        ShaderProgram shaderProgram,
        MainScene scene,
        Network network,
        int screenWidth,
        int screenHeight
    ) {
        this.tick = tick;
        this.input = input;
        this.window = window;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.shaderProgram = shaderProgram;
        this.network = network;

        Screen.init(
            screenWidth, 
            screenHeight,
            tick,
            input,
            window, 
            shaderProgram,
            this,
            scene,
            network
        );
        this.init();

        StateManager.Register(this);
    }

    // Screen Active
    public bool isScreenActive(string screenType) {
        return screens.TryGetValue(screenType, out var screen) && screen.isActive();
    }

    // Handle Key Press
    public void handleKeyPress(int key, int action) {
        if(currentScreen != null) {
            currentScreen.handleKeyPress(key, action);
            return;
        }
    }

    // Is Running
    public bool isRunning() {
        return running;
    }

    // Mouse
    public string? checkClick(int mouseX, int mouseY) {
        return currentScreen?.checkClick(mouseX, mouseY);
    }

    public void handleMouseMove(int mouseX, int mouseY) {
        currentScreen?.handleMouseMove(mouseX, mouseY);
    }

    // Switch 
    public void switchTo(string? screenType) {
        prevScreen = null;
        currentScreen = null;
        activeScreen = null;
        foreach(var screen in screens.Values) screen.setActive(false);
        if(screenType == null) return;

        if(screens.TryGetValue(screenType, out var target)) {
            currentScreen = target;
            activeScreen = screenType;
            currentScreen.setActive(true);
        }
    }

    public void switchToOverlay(string screenType) {
        prevScreen = currentScreen;

        if(screens.TryGetValue(screenType, out var target)) {
            currentScreen = target;
            activeScreen = screenType;
            currentScreen.setActive(true);
            prevScreen?.setActive(true);
        }
    }

    public void closeOverlay() {
        currentScreen = prevScreen;
        prevScreen = null;
    }

    /**
     * 
     * Get
     *
     */
    public Screen? get(string screenName) {
        return screens.GetValueOrDefault(screenName);
    }

    public T? get<T>(string screenName) where T : Screen {
        if(screens.TryGetValue(screenName, out var screen)) {
            return screen as T;
        }
        return null;
    }

    /**
     * 
     * Register
     *
     */
    public void register(Screen screen, Screen? parent = null) {
        screens[screen.screenName] = screen;

        if(parent != null) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Registered Screen! -- via [{parent.screenName}]: {screen.screenName}");
            Console.ResetColor();
        } else {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"Registered Screen!: {screen.screenName}");
            Console.ResetColor();
        }

        Console.ResetColor();
    }
 
    /**
     * 
     * Render
     *
     */
    public void render() {
        prevScreen?.render();
        if(currentScreen != prevScreen) currentScreen?.render();
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        prevScreen?.update();
        if(currentScreen != null) currentScreen.update();
    }

    /**
     * 
     * Init
     *
     */
    public void init() {
        var baseType = typeof(Screen);
        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsSubclassOf(baseType)
            );

        foreach(var type in types) {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if(ctor != null) {
                var instance = (Screen)ctor.Invoke(null);
                screens[instance.screenName] = instance;

                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"Registered Screen!: {instance.screenName}");
                Console.ResetColor();
            }
        }
    }

    // Handle Window Resize
    public void onWindowResize(int newWidth, int newHeight) {
        this.screenWidth = newWidth;
        this.screenHeight = newHeight;
        foreach(var screen in screens.Values) {
            screen.onWindowResize(newWidth, newHeight);
        }
    }
}