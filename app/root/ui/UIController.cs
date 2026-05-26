namespace App.Root.UI;
using App.Root.Shaders;
using System.Collections.Generic;
using App.Root.Player;
using System.Reflection;

class UIController {
    public int screenWidth;
    public int screenHeight;
    
    public ShaderProgram shaderProgram;
    public Mesh.Mesh mesh;
    private PlayerController? playerController = null!;

    private Dictionary<string, UI> uis = new();
    private string? active = null;
    private UI? currentUI = null;
    private bool isVisible = false;

    public UIController(
        ShaderProgram shaderProgram,
        Mesh.Mesh mesh,
        int screenWidth,
        int screenHeight
    ) {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;

        UI.init(screenWidth, screenHeight, this, shaderProgram, mesh);
        init();
    }
    
    // Player Controller
    public void setPlayerController(PlayerController playerController) {
        this.playerController = playerController;
    }

    public PlayerController? getPlayerController() {
        return playerController;
    }

    /**
    
        Get
    
        */
    public UI? get(string uiType) {
        return uis.GetValueOrDefault(uiType);
    }

    public T? get<T>(string uiType) where T : UI {
        if(uis.TryGetValue(uiType, out var ui)) {
            return ui as T;
        }
        return null;
    }

    /**
    
        Show
    
        */
    public void show(string uiType) {
        if(active != null && active != uiType) hide();

        active = uiType;
        currentUI = uis.GetValueOrDefault(uiType);

        if(currentUI != null) {
            currentUI.onShow();
            isVisible = true;
        }
    }

    /**
    
        Hide
    
        */
    public void hide() {
        currentUI?.onHide();
        active = null;
        currentUI = null;
        isVisible = false;
    }
    
    /**
    
        Toggle
    
        */
    public void toggle(string uiType) {
        if(active == uiType) {
            hide();
        } else {
            show(uiType);
        }
    }

    public bool getIsVisible() {
        return isVisible;
    }

    public string? getActive() {
        return active;  
    }

    /**
    
        Handle
    
        */
    // Key Press
    public bool handleKeyPress(int key, int action) {
        return currentUI != null && handleCurrentKeyPress(key, action);
    }

    // Current Key Press
    private bool handleCurrentKeyPress(int key, int action) {
        currentUI?.handleKeyPress(key, action);
        return false;
    }

    // Mouse Click
    public bool handleMouseClick(int mouseX, int mouseY, int button, int action) {
        if(!isVisible || currentUI == null) return false;

        if(button == 0 && action == 1) {
            string? clicked = currentUI.checkClick(mouseX, mouseY);
            if(clicked != null) {
                currentUI.handleAction(clicked);
                return true;
            }

            currentUI.handleMouseClick(mouseX, mouseY);
            return true;
        }
        return false;
    }

    // Mouse Move
    public void handleMouseMove(int mouseX, int mouseY) {
        if(currentUI != null && isVisible) {
            currentUI.handleMouseMove(mouseX, mouseY);
        }
    }

    /**
    
        On Window Resize
    
        */
    public void onWindowResize(int width, int height) {
        screenWidth = width;
        screenHeight = height;
        foreach(var ui in uis.Values) ui.onWindowResize(width, height);
    }

    /**
    
        Generate
    
        */
    public void generate() {
        foreach(var ui in uis.Values) {
            if(ui.enableGeneration) ui.generate();
        }
    }

    /**
    
        Update
    
        */
    public void update() {
        foreach(var ui in uis.Values) ui.update();
    }

    /**
    
        Render
    
        */
    public void render() {
        mesh.renderOrto();
        foreach(var ui in uis.Values) ui.render();
    }

    /**
    
        Init
    
        */
    private void init() {
        var baseType = typeof(UI);
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
                var instance = (UI)ctor.Invoke(null);
                uis[instance.uiName] = instance;

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine($"Registered UI!: {instance.uiName}");
                Console.ResetColor();
            }
        }
    }
}