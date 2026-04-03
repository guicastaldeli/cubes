namespace App.Root.UI;
using App.Root.Shaders;
using App.Root.Text;
using App.Root.Screen;
using System.Collections.Generic;

class UI : UIHandler {
    public static readonly string DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui/");

    public static int screenWidth;
    public static int screenHeight;
    public static ShaderProgram shaderProgram = null!;
    public static UIController uiController = null!;

    public TextRenderer? textRenderer;
    public string uiName;
    public string filePath;
    public UIData? uiData;
    
    public bool visible = false;

    public int lastMouseX = -1;
    public int lastMouseY = -1;

    public static void init(
        int screenWidth,
        int screenHeight,
        ShaderProgram shaderProgram,
        UIController uiController
    ) {
        UI.screenWidth = screenWidth;
        UI.screenHeight = screenHeight;
        UI.shaderProgram = shaderProgram;
        UI.uiController = uiController;
    }

    public UI(string filePath, string uiName) {
        this.filePath = filePath;
        this.uiName = uiName;
        try {
            this.textRenderer = new TextRenderer(shaderProgram, screenWidth, screenHeight);
            this.textRenderer.loadFont("arial", "arial.ttf", 16.0f);

            this.uiData = DocParser.parseUI(filePath, screenWidth, screenHeight);

            Console.WriteLine($"UI '{uiName}' initialized: {uiData?.elements.Count ?? 0} elements");
        } catch(Exception err) {
            Console.Error.WriteLine($"Failed to init UI '{uiName}': {err.Message}");
        }        
    }

    public TextRenderer? getTextRenderer() {
        return textRenderer;
    }

    // On Show
    public virtual void onShow() {
        visible = true;
    }

    // On Hide
    public virtual void onHide() {
        visible = false;
    }

    public List<UIElement> getElementsByType(string type) {
        List<UIElement>? val = 
            uiData != null ?
            DocParser.getElementsByType(uiData, type) :
            null;
        return val!;
    }

    public UIElement? getElementById(string id) {
        UIElement? val = 
            uiData != null ?
            DocParser.getElementById(uiData, id) :
            null;
        return val;
    }

    // Check Click
    public string? checkClick(int mouseX, int mouseY) {
        if(!visible || uiData == null) return null;

        var buttons = DocParser.getElementsByType(uiData, "button");
        foreach(var button in buttons) {
            if(mouseX >= button.x && mouseX <= button.x + button.width &&
               mouseY >= button.y && mouseY <= button.y + button.height
            ) {
                return button.action;
            }
        }
        return null;
    }

    // Handle Action
    public virtual void handleAction(string action) {}

    // Handle Key Press
    public virtual void handleKeyPress(int key, int action) {}

    // Handle Mouse Move
    public virtual void handleMouseMove(int mouseX, int mouseY) {
        this.lastMouseX = mouseX;
        this.lastMouseY = mouseY;
        if(uiData == null) return;

        foreach(var el in uiData.elements) {
            if(!el.visible || !el.hoverable) continue;

            bool mouseOver = el.containsPoint(mouseX, mouseY);
            if(mouseOver && !el.isHovered) el.applyHover();
            else if(!mouseOver && el.isHovered) el.removeHover();
        }
    }

    ///
    /// Render
    ///
    public virtual void render() {
        if(!visible || textRenderer == null || uiData == null) return;

        DocParser.renderUI(
            uiData,
            screenWidth,
            screenHeight,
            shaderProgram,
            textRenderer
        );
    }

    public virtual void render(UIElement element) {
        if(!element.visible || textRenderer == null) return;

        if(element.type == "div" || element.type == "button") {
            DocParser.renderUIElement(element, screenWidth, screenHeight, shaderProgram);
        }
        if((element.type == "button" || element.type == "label") && !string.IsNullOrEmpty(element.text)) {
            if(element.hasShadow) {
                textRenderer.renderTextWithShadow(
                    element.text, element.x, element.y, 
                    element.scale, 
                    element.color,
                    element.shadowOffsetX, element.shadowOffsetY, 
                    element.shadowBlur,
                    element.shadowColor, 
                    element.fontFamily
                );
            } else {
                textRenderer.renderText(
                    element.text, 
                    element.x, element.y, 
                    element.scale, 
                    element.color, 
                    element.fontFamily
                );
            }
        }
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
        UI.screenWidth = width;
        UI.screenHeight = height;
        textRenderer?.updateScreenSize(width, height);

        if(uiData != null) { 
            uiData = DocParser.parseUI(filePath, width, height);
        }
    }
}
