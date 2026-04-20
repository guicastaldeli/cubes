
/**

    Class for all the
    player modes...

    */
using App.Root.Mesh;
namespace App.Root.Player;
using OpenTK.Mathematics;

/**

    Player Modes

    */
public enum Modes {
    NORMAL,
    GETTER
}

/**

    Slots

    */
public enum Slot {
    LEFT,
    RIGHT,
    CENTER
}

/**

    Position class helper.

    */
static class Position {
    public static Vector3 set(Slot slot) {
        Vector3 offset = Vector3.Zero;

        switch(slot) {
            case Slot.LEFT:
                offset = new Vector3(-1.5f, 0.0f, 5.0f);
                break;
            case Slot.RIGHT:
                offset = new Vector3(1.5f, 0.0f, 5.0f); 
                break;
            case Slot.CENTER:
                offset = new Vector3(0.0f, 0.0f, 5.0f);
                break;
        }

        return offset;
    }
}

/**

    Main Player Modes
    class.

    */
class Mode {
    private Window window;
    private Camera camera;
    private Mesh.Mesh mesh;
    private PlayerController playerController;

    private Modes currentMode = Modes.NORMAL;
    private Dictionary<Slot, string?> previewMeshIds = new();
    
    private Slot? activeSlot = null;
    private Slot? prevSlot = null;

    private bool leftPressed = false;
    private bool rightPressed = false;

    public Mode(
        Window window,
        Camera camera,
        Mesh.Mesh mesh, 
        PlayerController playerController
    ) {
        this.window = window;
        this.camera = camera;
        this.mesh = mesh;
        this.playerController = playerController;
    }

    // Get Current Mode
    public Modes getCurrentMode() {
        return currentMode;
    }

    // Get Current Slot
    public Slot? getCurrentSlot() {
        return activeSlot;
    }

    // On Enter Getter Mode
    private void onEnterGetterMode(Slot slot) {
        activeSlot = slot;
        updatePreview(slot);
    }

    // On Exit Getter Mode
    private void onExitGetterMode(Slot slot) {
        hidePreview(slot);
        activeSlot = null;
    }

    // Get Preview Id
    public string getPreviewId(Slot slot) {
        string val = $"pv-{slot}";
        return val;
    }

    /**

        Set

        */
    public void set(Modes mode, Slot slot) {
        if(currentMode == mode) return;

        Modes prevMode = currentMode;
        currentMode = mode;

        Console.WriteLine($"Mode changed: {prevMode} -> {currentMode} (Slot: {slot})");

        if(mode == Modes.GETTER) {
            onEnterGetterMode(slot);
        } else {
            onExitGetterMode(slot);
        }
    }

    /**

        Preview

        */
    // Update Preview
    private void updatePreview(Slot slot) {
        var invInstance = 
            playerController
                .getPlayerInputMap()
                .getInventory();
        if(invInstance != null) {
            var inv = invInstance.getInventory();
            var mainSlot = inv.getActiveSlot();
            if(!mainSlot.isEmpty && mainSlot.def != null) {
                showPreview(slot, mainSlot.def);
            } else {
                hidePreview(slot);
            }
        }
    }

    // Update Preview Position
    private void updatePreviewPosition(Slot slot, string previewId) {
        Vector3 offset = Position.set(slot);

        Vector3 forward = camera.getFront();
        Vector3 right = camera.getRight();
        Vector3 up = camera.getUp();
        Vector3 pos = 
            playerController.getPosition() +
            forward * offset.Z +
            right * offset.X +
            up * offset.Y;

        mesh.setPosition(previewId, pos);
    }

    // Show Preview
    private void showPreview(Slot slot, PlacedMeshDef def) {
        string previewId = getPreviewId(slot);
        if(!mesh.hasMesh(previewId)) {
            window.queueOnRenderThread(() => {
                MeshData data = MeshLoader.load(def.MeshType);
                
                mesh.add(previewId, data);
                if(def.Scale.HasValue) mesh.setScale(previewId, def.Scale.Value);
                mesh.setTexture(previewId, def.TexId, def.TexPath);
                
                updatePreviewPosition(slot, previewId);
                mesh.setVisible(previewId, true);
                previewMeshIds[slot] = previewId;   
            });
        } else {
            updatePreviewPosition(slot, previewId);
            mesh.setVisible(previewId, true);

            previewMeshIds[slot] = previewId;
        }
    }

    // Hide Preview
    public void hidePreview(Slot slot) {
        string previewId = getPreviewId(slot);
        if(mesh.hasMesh(previewId)) {
            mesh.setVisible(previewId, false);
        }
        previewMeshIds[slot] = null;
    }

    /**
    
        Handle Input

        */
    public void handleInput(Slot slot, bool pressed) {
        if(slot == Slot.LEFT) leftPressed = pressed;
        if(slot == Slot.RIGHT) rightPressed = pressed;

        PlayerMesh playerMesh = playerController.getPlayerMesh();

        bool bothHeld = leftPressed && rightPressed;
        if(bothHeld) {
            if(activeSlot != Slot.CENTER) {
                prevSlot = activeSlot;

                if(activeSlot.HasValue) {
                    hidePreview(activeSlot.Value);
                    playerMesh.hideArms(activeSlot.Value);
                }

                activeSlot = Slot.CENTER;
                if(currentMode == Modes.NORMAL) {
                    set(Modes.GETTER, Slot.CENTER);
                } else {
                    updatePreview(Slot.CENTER);
                    playerMesh.updateArms(Slot.CENTER);
                }
            }

            return;
        }

        if(activeSlot == Slot.CENTER && !bothHeld) {
            hidePreview(Slot.CENTER);

            Slot targetSlot = prevSlot ?? Slot.RIGHT;
            
            activeSlot = targetSlot;
            updatePreview(targetSlot);
            playerMesh.updateArms(targetSlot);

            prevSlot = null;
            return;
        }

        if(!pressed) return;

        
        if(currentMode == Modes.NORMAL) {
            set(Modes.GETTER, slot);
            playerMesh.updateArms(slot);
            return;
        }
        if(activeSlot == slot) {
            set(Modes.NORMAL, slot);
            playerMesh.hideArms(slot);
            return;
        }
        if(activeSlot.HasValue) {
            hidePreview(activeSlot.Value);
            playerMesh.hideArms(activeSlot.Value);

            activeSlot = slot;
            updatePreview(slot);
            playerMesh.updateArms(slot);
        }
    }

    /**

        Update

        */
    public void update() {
        if(currentMode != Modes.GETTER) return;

        PlayerMesh playerMesh = playerController.getPlayerMesh();

        foreach(var kvp in previewMeshIds) {
            if(kvp.Value == null) continue;
            updatePreviewPosition(kvp.Key, kvp.Value);
        }
        foreach(var kvp in playerMesh.armMeshIds) {
            if(kvp.Value == null) continue;
            playerMesh.updateArmPosition(kvp.Key, kvp.Value);
        }
    }

    /**
    
        Execute Action

        */
    public void executeAction() {
        if(currentMode != Modes.GETTER) return;
        if(!activeSlot.HasValue) return;

        updatePreview(activeSlot.Value);
    }
}

