/**

    Class for all the
    player modes...

    */
namespace App.Root.Player;

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
    private Modes currentMode = Modes.NORMAL;
    private Dictionary<Slot, string?> previewMeshIds = new();

    // On Enter Getter Mode
    private void onEnterGetterMode() {
        updatePreview();
    }

    // On Exit Getter Mode
    private void onExitGetterMode() {
        hidePreview(Slot.LEFT);
        hidePreview(Slot.RIGHT);
    }

    /**

        Set

        */
    public void set(Modes mode) {
        if(currentMode == mode) return;

        Modes prevMode = currentMode;
        currentMode = mode;

        if(mode == Modes.GETTER) {
            onEnterGetterMode();
        } else {
            onExitGetterMode();
        }
    }

    /**

        Toggle

        */
    public void toggle() {
        set(currentMode == Modes.NORMAL ?
            Modes.GETTER :
            Modes.NORMAL
        );
    }
}

