namespace App.Root.Screen.Main.Server;
using App.Root.Screen.Main.Client;
using App.Root.Save;
using App.Root.Utils;
using App.Root.Input;

class ClientDialogAction {
    private Window window;
    private ScreenController screenController;
    private ClientDialog clientDialog;
    private Network network;

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
    }

    // Register Objects
    private void RegisterObjects() {
        var saveType = typeof(SaveManager);
        var typeName = saveType.Name;
        var name = WordInflector.ToPlural(typeName);

        DocParser.ReplaceObject(typeName, saveType);
        DocParser.ReplaceObject(name, saveType);

        var displayType = typeof(DisplaySave);
        DocParser.ReplaceObject(displayType.Name, displayType);
    }

    /**
     *
     * Create Save
     *
     */
    [GlobalInput]
    public void createSave() {
        // todo
    }

    /**
     *
     * Load Save
     *
     */
    [GlobalInput]
    public void loadSave(string saveName) {
        Console.WriteLine($"[ClientDialog] Loading save: {saveName}");

        var result = SaveManager.LoadSave(saveName);
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
    public void deleteSave(string saveName) {
        Console.WriteLine($"[ClientDialog] Deleting save: {saveName}");

        if(SaveManager.Delete(saveName)) {
            Console.WriteLine($"[ClientDialog] Deleted save: {saveName}");
            updateUI();
        } else {
            Console.WriteLine($"[ClientDialog] Failed to delete save: {saveName}");
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