namespace App.Root.UI;
using App.Root.Shaders;
using App.Root.UI.Voip;
using System.Collections.Generic;
using App.Root.Player.Inventory;

class UIController {
    public enum UIType {
        CHAT,
        VOIP,
        INVENTORY
    }

    public int screenWidth;
    public int screenHeight;
    public ShaderProgram shaderProgram;

    private Dictionary<UIType, UI> uis = new();
    private UIType? active = null;
    private UI? currentUI = null;
    private bool isVisible = false;

    public UIController(
        ShaderProgram shaderProgram,
        int screenWidth,
        int screenHeight
    ) {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.shaderProgram = shaderProgram;

        UI.init(screenWidth, screenHeight, shaderProgram, this);
        init();
    }

    // Get
    public UI? get(UIType uiType) {
        return uis.GetValueOrDefault(uiType);
    }

    public T? get<T>(UIType uiType) where T : UI {
        if(uis.TryGetValue(uiType, out var ui)) {
            return ui as T;
        }
        return null;
    }

    // Show
    public void show(UIType uiType) {
        if(active != null && active != uiType) hide();

        active = uiType;
        currentUI = uis.GetValueOrDefault(uiType);

        if(currentUI != null) {
            currentUI.onShow();
            isVisible = true;
        }
    }

    // Hide
    public void hide() {
        currentUI?.onHide();
        active = null;
        currentUI = null;
        isVisible = false;
    }
    
    // Toggle
    public void toggle(UIType uiType) {
        if(active == uiType) hide();
        else show(uiType);
    }

    public bool getIsVisible() {
        return isVisible;
    }

    public UIType? getActive() {
      return active;  
    }

    // Handle Key Press
    public bool handleKeyPress(int key, int action) {
        return currentUI != null && handleCurrentKeyPress(key, action);
    }

    private bool handleCurrentKeyPress(int key, int action) {
        currentUI?.handleKeyPress(key, action);
        return false;
    }

    // Handle Mouse Click
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

    // Handle Mouse Move
    public void handleMouseMove(int mouseX, int mouseY) {
        if(currentUI != null && isVisible) {
            currentUI.handleMouseMove(mouseX, mouseY);
        }
    }

    ///
    /// Update
    ///
    public void update() {
        foreach(var ui in uis.Values) ui.update();
    }

    ///
    /// Render
    /// 
    public void render() {
       foreach(var ui in uis.Values) ui.render();
    }

    ///
    /// Init
    ///
    private void init() {
        uis[UIType.CHAT] = new Chat.Chat();
        uis[UIType.VOIP] = new VoipUI();
        uis[UIType.INVENTORY] = new InventoryUI();
    }

    ///
    /// Window Resize
    ///
    public void onWindowResize(int width, int height) {
        screenWidth = width;
        screenHeight = height;
        foreach(var ui in uis.Values) ui.onWindowResize(width, height);
    }
}