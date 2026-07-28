namespace App.Root.Screen.Main.Server;
using App.Root.Screen.Main.Client;
using App.Root.Save;
using App.Root.Utils;
using App.Root.Input;
using App.Root.UI;

class ClientDialogAction {
    private Window window;
    private ScreenController screenController;
    private ClientDialog clientDialog;
    private Network network;

    private bool isCreatingSave = false;
    
    public dynamic? elements;

    public static string[] Elements = {
        "saves_list",
        "create_save_container",
        "save_name_container",
        "save_name_input", 
        "create_save_label",
        "saves_title",
        "back_btn"
    };

    public ClientDialogAction(
        Window window,
        ScreenController screenController, 
        ClientDialog clientDialog,
        Network network
    ) {
        this.window = window;
        this.screenController = screenController;
        this.clientDialog = clientDialog;
        this.network = network;

        elements = ElementEntry.C<ScreenElement>(id => clientDialog.getElementById(id), Elements);
        registerObjects();
    }

    // Get Elements
    public dynamic getElements() {
        elements = ElementEntry.C<ScreenElement>(id => clientDialog.getElementById(id), Elements);
        return elements;
    }

    // Get Element
    public ScreenElement? getElement(string id) {
        var els = getElements();
        
        var prop = els.GetType().GetProperty(id);
        if(prop != null) return prop.GetValue(els) as ScreenElement;
        
        return null;
    }

    // Register Objects
    private void registerObjects() {
        var saveType = typeof(SaveManager);
        var typeName = saveType.Name;
        var name = WordInflector.ToPlural(typeName);

        DocParser.ReplaceObject(typeName, saveType);
        DocParser.ReplaceObject(name, saveType);

        var displayType = typeof(DisplaySave);
        DocParser.ReplaceObject(displayType.Name, displayType);
    }

    // Show Create Input
    public void showCreateInput() {
        var els = getElements();

        els.save_name_container.visible = true;
        els.create_save_container.visible = false;
        els.saves_list.visible = false;
        els.saves_title.visible = false;
        els.back_btn.visible = false; 

        string? inputId = els.save_name_input.id;
        if(!string.IsNullOrEmpty(inputId)) {
            InputField.register(inputId);
            InputField.focus(inputId);
            els.save_name_input.text = "";
        }
    }

    // Hide Create Input
    private void hideCreateInput() {
        var els = getElements();

        els.save_name_container.visible = false;
        els.create_save_container.visible = true;
        els.saves_list.visible = true;
        els.saves_title.visible = true;
        els.back_btn.visible = true;

        els.save_name_input.text = "";
    } 

    /**
     *
     * Create Save
     *
     */
    // Create Save
    [GlobalInput]
    public void createSave() {
        if(isCreatingSave) {
            cancelCreateSave();
            return;
        }

        isCreatingSave = true;
        showCreateInput();
    }

    // Confirm Create Save
    [GlobalInput]
    public void confirmCreateSave() {
        // todo
    }

    /**
     *
     * Cancel Save
     *
     */
    // Cancel Save
    [GlobalInput]
    public void cancelSave() {
        cancelCreateSave();
    }

    // Cancel Create Save
    private void cancelCreateSave() {
        isCreatingSave = false;
        hideCreateInput();
    }

    /**
     *
     * Load Save
     *
     */
    [GlobalInput]
    public void loadSave(string saveId) {
        string? saveName = SaveManager.GetSaveNameById(saveId);
        Console.WriteLine($"[ClientDialog] Loading Save... name: {saveName} ; id: {saveId}");

        var result = SaveManager.LoadSave(saveId);
        if(result.Result == SaveLoader.LoadResult.Success) {
            Console.WriteLine($"[ClientDialog] Save loaded successfully!");
            Screen.screenController.switchTo(null);
            clientDialog.getMainScreen().getMainScene().init();
        }
    }

    /**
     *
     * Delete Save
     *
     */
    [GlobalInput]
    public void deleteSave(string saveId) {
        string? saveName = SaveManager.GetSaveNameById(saveId);
        Console.WriteLine($"[ClientDialog] Deleting Save... name: {saveName} ; id: {saveId}");

        if(SaveManager.Delete(saveId)) {
            Console.WriteLine($"[ClientDialog] Deleted Save... name: {saveName} ; id: {saveId}");
            clientDialog.updateSaveList();
        } else {
            Console.WriteLine($"[ClientDialog] Failed to delete save... name: {saveName} ; id: {saveId}");
        }
    }

    // Start
    public void start() {
        clientDialog.getMainScreen().getMainScene().init();
    }

    // Back
    public void back() {
        clientDialog.hide();
        clientDialog.getMainScreen().show();
        
        network.stop();
    }
}