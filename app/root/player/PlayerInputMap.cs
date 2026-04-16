namespace App.Root.Player;
using App.Root.Mesh;
using App.Root.Player.Inventory;
using OpenTK.Windowing.GraphicsLibraryFramework;

class PlayerInputMap {
    private Input input;
    private PlayerController playerController;
    
    private bool[] keyPressed = new bool[(int) Keys.LastKey + 1];
    private bool fKeyPressed = false;

    public PlayerInputMap(Input input, PlayerController playerController) {
        this.input = input;
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
        playerController.updatePosition(PlayerController.MovDir.DOWN, keyPressed[(int)Keys.LeftShift]);
        if(keyPressed[(int)Keys.F] && !fKeyPressed) {
            playerController.toggleFlyMode();
            fKeyPressed = true;
        } else if(!keyPressed[(int)Keys.F]) {
            fKeyPressed = false;
        }
    }

    // On Mouse Button
    public void onMouseButton(int button) {
        MeshInteractionController meshInteractionController = playerController.getMesh().getMeshInteractionController();
        if(meshInteractionController != null) {
            if(button == 0) meshInteractionController.onBreak();
            if(button == 1) meshInteractionController.onPlace();
        }
    }

    /**
    
        Inventory

        */
    // Get Inventory
    public InventoryUI? getInventory() {
        InventoryUI? val = 
            input.getUIController()
            .get<InventoryUI>(UI.UIController.UIType.INVENTORY);
        return val;
    }

    // Open Inventory
    public void openInventory(Keys key) {
        if(key == Keys.E) {
            if(input.getUIController().getActive() == UI.UIController.UIType.INVENTORY) {
                input.unlockMouse();
            } else {
                input.lockMouse();
            }
        }
    }
}