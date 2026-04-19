
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
    RIGHT
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
    public Modes getCurrent() {
        return currentMode;
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
                showPreview(slot, mainSlot.def.MeshType);
            } else {
                hidePreview(slot);
            }
        }
    }

    // Update Preview Position
    private void updatePreviewPosition(Slot slot, string previewId) {
        Vector3 leftSlot = new Vector3(-0.4f, -0.3f, -0.6f);
        Vector3 rightSlot = new Vector3(0.4f, -0.3f, -0.6f); 
        Vector3 offset = 
            slot == Slot.LEFT ?
                leftSlot :
                rightSlot;

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
    private void showPreview(Slot slot, string meshType) {
        string previewId = getPreviewId(slot);
        if(!mesh.hasMesh(previewId)) {
            window.queueOnRenderThread(() => {
                MeshData data = MeshLoader.load(meshType);
                mesh.add(previewId, data);
                mesh.setScale(previewId, 0.5f);
                
                updatePreviewPosition(slot, previewId);
                mesh.setVisible(previewId, true);
            });

            previewMeshIds[slot] = previewId;   
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
    public void handleInput(Slot slot) {
        if(currentMode == Modes.NORMAL) {
            set(Modes.GETTER, slot);
            return;
        }
        if(activeSlot == slot) {
            set(Modes.NORMAL, slot);
            return;
        }
        if(activeSlot.HasValue) {
            hidePreview(activeSlot.Value);
        }

        set(Modes.GETTER, slot);
    }

    /**

        Update

        */
    public void update() {
        if(currentMode != Modes.GETTER) return;

        foreach(var kvp in previewMeshIds) {
            if(kvp.Value == null) continue;
            updatePreviewPosition(kvp.Key, kvp.Value);
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

