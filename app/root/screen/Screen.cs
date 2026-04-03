namespace App.Root.Screen;
using App.Root.Shaders;
using App.Root.Text;
using System.Collections.Generic;

class Screen : ScreenHandler {
    public static readonly string DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screen/");
    
    public static int screenWidth;
    public static int screenHeight;

    public static Tick tick = null!;
    public static Input input = null!;
    public static Window window = null!;
    public static ShaderProgram shaderProgram = null!;
    public static ScreenController screenController = null!;
    public static Scene scene = null!;

    public static TextRenderer? textRenderer;
    public bool active = false;
    public string screenName;
    public ScreenData? screenData;

    public int lastMouseX = -1;
    public int lastMouseY = -1;

    public static void init(
        int screenWidth,
        int screenHeight,
        Tick tick,
        Input input,
        Window window,
        ShaderProgram shaderProgram,
        ScreenController screenController,
        Scene scene
    ) {
        Screen.tick = tick;
        Screen.input = input;
        Screen.window = window;
        Screen.screenWidth = screenWidth;
        Screen.screenHeight = screenHeight;
        Screen.shaderProgram = shaderProgram;
        Screen.screenController = screenController;
        Screen.scene = scene;
    }

    public Screen(string filePath, string screenName) {
        this.screenName = screenName;
        try {
            textRenderer ??= new TextRenderer(shaderProgram, screenWidth, screenHeight);
            textRenderer.loadFont("arial", "arial.ttf", 16.0f);
            this.screenData = DocParser.parseScreen(
                filePath,
                screenWidth,
                screenHeight
            );
            Console.WriteLine($"Screen '{screenName}' initialized: {screenData?.elements.Count ?? 0} elements");
        } catch(Exception err) {
            Console.Error.WriteLine($"Failed to init screen '{screenName}': {err.Message}");
        }
    }

    public Scene getScene() {
        return scene;
    }

    public string? getScreenType() {
        return screenData?.screenType;
    }

    public string getScreenName() {
        return screenName;
    }

    // Active
    public void setActive(bool active) {
        resetMouse();
        resetHover();
        this.active = active;
    }

    public bool isActive() {
        return active;
    }

    // Get Element
    public ScreenElement? getElementById(string id) {
        ScreenElement? val = 
            screenData != null ? 
            DocParser.getElementById(screenData, id) : 
            null;
        return val;
    }

    public List<ScreenElement> getEleentsByType(string type) {
        List<ScreenElement> val = 
            screenData != null ? 
            DocParser.getElementsByType(screenData, type) :
            new();
        return val;
    }

    // Get Text Renderer
    public TextRenderer? getTextRenderer() {
        return textRenderer;
    }

    // Check Click
    public virtual string? checkClick(int mouseX, int mouseY) {
        if(!active || screenData == null) return null;

        var buttons = DocParser.getElementsByType(screenData, "button");
        foreach(var button in buttons) {
            if(mouseX >= button.x && mouseX <= button.x + button.width &&
               mouseY >= button.y && mouseY <= button.y + button.height
            ) {
                handleAction(button.action);
                return button.action;
            }
        }
        return null;
    }

    // Handle Mouse Move
    public virtual void handleMouseMove(int mouseX, int mouseY) {
        this.lastMouseX = mouseX;
        this.lastMouseY = mouseY;
        if(screenData == null) return;

        foreach(var el in screenData.elements) {
            if(!el.visible || !el.hoverable) continue;

            bool mouseOver = el.containsPoint(mouseX, mouseY);
            if(mouseOver && !el.isHovered) el.applyHover();
            else if(!mouseOver && el.isHovered) el.removeHover();
        }
    }

    // Handle Key Press
    public virtual void handleKeyPress(int key, int action) {}

    // Handle Action
    public virtual void handleAction(string action) {}

    // Show
    public void show() {
        resetMouse();
        resetHover();
        active = true;
    }

    // Hide
    public void hide() {
        resetHover();
        active = false;
    }

    ///
    /// Render
    /// 
    public virtual void render() {
        if(!active || textRenderer == null || screenData == null) return;

        DocParser.renderScreen(
            screenData,
            screenWidth,
            screenHeight,
            shaderProgram,
            textRenderer
        );
    }

    ///
    /// Update
    /// 
    public virtual void update() {
        if(lastMouseX >= 0 && lastMouseY >= 0) {
            handleMouseMove(lastMouseX, lastMouseY);
        }
    }

    ///
    /// Window Resize
    /// 
    public virtual void onWindowResize(int width, int height) {
        screenWidth = width;
        screenHeight = height;
        textRenderer?.updateScreenSize(width, height);

        if(screenData != null) {
            screenData = DocParser.parseScreen(
                screenData.screenType,
                width,
                height
            );
        }
    }

    /// 
    /// Reset
    /// 
    public void resetMouse() {
        lastMouseX = -1;
        lastMouseY = -1;
    }

    public void resetHover() {
        if(screenData == null) return;
        foreach(var el in screenData.elements) {
            if(el.hoverable && el.isHovered) el.removeHover();
        }
    }
}