namespace App.Root.UI;

interface UIHandler {
    void render() {}
    void render(UIElement uiElement) {}
    void handleAction(string action) {}
    void handleKeyPress(int key, int action) {}
    void handleMouseMove(int mouseX, int mouseY) {}
    void onWindowResize(int width, int height) {}
    void onShow() {}
    void onHide() {}
    void update() {}
}