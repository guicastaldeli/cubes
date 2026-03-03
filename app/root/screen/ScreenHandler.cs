namespace App.Root.Screen;

interface ScreenHandler {
    void render() {}
    void update() {}
    void handleAction(string action) {}
    void handkeKeyPress(int key, int action) {}
    void handleMouseMove(int mosueX, int mouseY) {}
    void onWindowResize(int width, int height) {}
}