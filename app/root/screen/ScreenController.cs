namespace App.Root.Screen;
using App.Root.Shaders;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using App.Root.Screen.Main;

class ScreenController {
    public enum SCREENS {
        MAIN,
        PAUSE
    }

    public int screenWidth;
    public int screenHeight;
    public ShaderProgram shaderProgram;

    public Dictionary<SCREENS, Screen> screens = new();
    public SCREENS? activeScreen = null;
    public Screen? currentScreen = null;

    public ScreenController(
        ShaderProgram shaderProgram,
        int screenWidth,
        int screenHeight
    ) {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.shaderProgram = shaderProgram;

        Screen.init(
            screenWidth, 
            screenHeight, 
            shaderProgram,
            this
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
        foreach(var screen in screens.Values) screen.setActive(false);
        currentScreen = null;
        activeScreen = null;
        if(screenType == null) return;

        if(screens.TryGetValue(screenType.Value, out var target)) {
            currentScreen = target;
            activeScreen = screenType;
            currentScreen.setActive(true);
        }
    }

    ///
    /// Render
    /// 
    public void render() {
        if(currentScreen != null) currentScreen.render();
    }

    ///
    /// Update
    /// 
    public void update() {
        if(currentScreen != null) currentScreen.update();
    }

    ///
    /// Init
    /// 
    public void init() {
        // Main
        screens[SCREENS.MAIN] = new MainScreen();
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