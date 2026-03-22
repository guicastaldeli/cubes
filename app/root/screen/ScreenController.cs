namespace App.Root.Screen;
using App.Root;
using App.Root.Shaders;
using App.Root.Screen.Main;
using System.Collections.Generic;
using App.Root.Screen.Pause;

class ScreenController {
    public enum SCREENS {
        MAIN,
        PAUSE
    }

    public int screenWidth;
    public int screenHeight;

    public Tick tick;
    public Input input;
    public Window window;
    public ShaderProgram shaderProgram;
    public Scene? scene;
    public Root.Main main;

    public Dictionary<SCREENS, Screen> screens = new();
    public SCREENS? activeScreen = null;
    public Screen? currentScreen = null;
    public Screen? prevScreen = null;

    public bool running = false;

    public ScreenController(
        Tick tick,
        Input input,
        Window window,
        ShaderProgram shaderProgram,
        Scene scene,
        Root.Main main,
        int screenWidth,
        int screenHeight
    ) {
        this.tick = tick;
        this.input = input;
        this.window = window;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.shaderProgram = shaderProgram;
        this.main = main;

        Screen.init(
            screenWidth, 
            screenHeight,
            tick,
            input,
            window, 
            shaderProgram,
            this,
            scene
        );
        this.init();
    }

    // Screen Active
    public bool isScreenActive(SCREENS screenType) {
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

    ///
    /// Mouse
    /// 
    public string? checkClick(int mouseX, int mouseY) {
        return currentScreen?.checkClick(mouseX, mouseY);
    }

    public void handleMouseMove(int mouseX, int mouseY) {
        currentScreen?.handleMouseMove(mouseX, mouseY);
    }

    ///
    /// Switch
    /// 
    public void switchTo(SCREENS? screenType) {
        prevScreen = null;
        currentScreen = null;
        activeScreen = null;
        foreach(var screen in screens.Values) screen.setActive(false);
        if(screenType == null) return;

        if(screens.TryGetValue(screenType.Value, out var target)) {
            currentScreen = target;
            activeScreen = screenType;
            currentScreen.setActive(true);
        }
    }

    public void switchToOverlay(SCREENS screenType) {
        prevScreen = currentScreen;

        foreach(var screen in screens.Values) screen.setActive(false);
        currentScreen = null;
        activeScreen = null;

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

    ///
    /// Render
    /// 
    public void render() {
        prevScreen?.render();
        if(currentScreen != prevScreen) currentScreen?.render();
    }

    ///
    /// Update
    /// 
    public void update() {
        prevScreen?.update();
        if(currentScreen != null) currentScreen.update();
    }

    ///
    /// Init
    /// 
    public void init() {
        // Main
        screens[SCREENS.MAIN] = new MainScreen();

        //Pause
        screens[SCREENS.PAUSE] = new PauseScreen();
    }

    // Handle Window Resize
    public void handleWindowResize(int newWidth, int newHeight) {
        this.screenWidth = newWidth;
        this.screenHeight = newHeight;
        foreach(var screen in screens.Values) {
            screen.onWindowResize(newWidth, newHeight);
        }
    }
}