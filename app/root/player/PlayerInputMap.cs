using OpenTK.Windowing.GraphicsLibraryFramework;

namespace App.Root.Player;

class PlayerInputMap {
    private PlayerController playerController;
    
    private bool[] keyPressed = new bool[(int) Keys.LastKey + 1];

    public PlayerInputMap(PlayerController playerController) {
        this.playerController = playerController;
    }

    // Set Key State
    public void setKeyState(Keys key, bool pressed) {
        int idx = (int)key;
        if(idx >= 0 && idx < keyPressed.Length) {
            keyPressed[idx] = pressed;
        }
    }

    // Handle Mouse
    public void handleMouse(float xOffset, float yOffset) {
        playerController.getCamera().handleMouse(xOffset, yOffset);
    }

    // Keyboard Callback
    public void keyboardCallback() {
        playerController.updatePosition(PlayerController.MovDir.FORWARD, keyPressed[(int)Keys.W]);
        playerController.updatePosition(PlayerController.MovDir.BACKWARD, keyPressed[(int)Keys.S]);
        playerController.updatePosition(PlayerController.MovDir.LEFT, keyPressed[(int)Keys.A]);
        playerController.updatePosition(PlayerController.MovDir.RIGHT, keyPressed[(int)Keys.D]);
        playerController.updatePosition(PlayerController.MovDir.UP, keyPressed[(int)Keys.Space]);
    }
}