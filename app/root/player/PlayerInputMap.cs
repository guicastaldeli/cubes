namespace App.Root.Player;

using App.Root.Chat;
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

        setMode(key, pressed);
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
        Mode mode = playerController.getMode();
        
        if(mode.getCurrentMode() == Modes.GETTER) {
            MeshInteractionController meshInteractionController = playerController.getMesh().getMeshInteractionController();
            if(meshInteractionController != null) {
                if(button == 0) meshInteractionController.onBreak();
                if(button == 1) meshInteractionController.onPlace();
                mode.executeAction();
            }

            return;
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
        if(ChatController.getInstance().isOpen()) return;
        
        bool pauseOverlay = input.onPauseOverlayOpen();
        if(pauseOverlay) return;

        bool kv = key == Keys.I;
        if(kv) {
            var uiController = input.getUIController();
            uiController.toggle(UI.UIController.UIType.INVENTORY);

            bool isActive = uiController.getActive() == UI.UIController.UIType.INVENTORY;
            Action action = isActive ? () =>
                input.unlockMouse() : () =>
                input.lockMouse();
            action();

        }
    }

    // Is Inventory Open
    public bool isInventoryOpen() {
        var uiController = input.getUIController();
        var inventoryUI = uiController.get<InventoryUI>(UI.UIController.UIType.INVENTORY);
        return inventoryUI!.isOpen();
    }

    /**
    
        Mode

        */
    private void setMode(Keys key, bool pressed) {
        Mode mode = playerController.getMode();
        
        if(key == Keys.Q) {
            mode.handleInput(Slot.LEFT, pressed);
        }
        else if(key == Keys.E) {
            mode.handleInput(Slot.RIGHT, pressed);
        }
    }
}