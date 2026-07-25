namespace App.Root.Save;

public static class SaveManager {
    private static string currentSaveName = "";
    private static string currentSavePath = "";

    private static bool initialized = false;

    // Get ALl Saves
    public static List<SaveFile> GetAllSaves() {
        List<SaveFile> val = SaveLoader.GetAllSaves();
        return val;
    }

    // Get Save Names
    public static List<string> GetSaveNames() {
        List<string> val = SaveLoader.GetSaveNames();
        return val;
    }

    // Save Exists
    public static bool SaveExists(string saveName) {
        bool val = SaveLoader.SaveExists(saveName);
        return val;
    }

    // Get Current Save Name
    public static string GetCurrentSaveName() {
        string val = currentSaveName;
        return val;
    }

    // Get Current Save Path
    public static string GetCurrentSavePath() {
        string val = currentSavePath;
        return val;
    }

    // Get Current Save File
    public static SaveFile? GetCurrentSaveFile() {
        SaveFile? val = Data.GetData<SaveFile>(M.SAVE_META);
        return val;
    }

    /**
     *
     * Create Save
     *
     */
    public static string CreateSave(string saveName) {
        var path = GenerateSave.CreateSave(saveName);
        currentSaveName = saveName;
        currentSavePath = path;
        return path;
    }

    /**
     *
     * Load Save
     *
     */
    public static SaveLoader.LoadResultInfo LoadSave(string saveName) {
        var result = SaveLoader.Load(saveName);
        if(result.Result == SaveLoader.LoadResult.Success) {
            currentSaveName = saveName;
            currentSavePath = result.SaveFolder ?? "";
            Console.WriteLine($"[SaveManager] Loaded save: {saveName}");
        } else {
            Console.WriteLine($"[SaveManager] Failed to load save: {result.Message}");
        }

        return result;
    }

    /**
     *
     * Save
     *
     */
    public static void Save() {
        if(string.IsNullOrEmpty(currentSaveName)) {
            Console.WriteLine("[SaveManager] No save loaded! Create a save first.");
            return;
        }

        GenerateSave.SaveStoreDataToFolder(currentSavePath, M.SAVE_META);
        DataOutput.SaveAll();

        var saveFile = Data.GetData<SaveFile>(M.SAVE_META);
        if(saveFile != null) {
            saveFile.UpdateLastPlayed();
            Data.RegisterData(M.SAVE_META, saveFile);
            DataOutput.Save(M.SAVE_META);
        }

        GenerateSave.CreateManifest(currentSavePath, currentSaveName);
        Console.WriteLine($"[SaveManager] Saved to: {currentSaveName}");
    }

    /**
     *
     * Delete
     *
     */
    public static bool Delete(string saveName) {
        bool val = SaveLoader.Delete(saveName);
        return val;
    }

    /**
     *
     * Init
     *
     */
    public static void Init() {
        if(initialized) return;

        if(!Directory.Exists(GenerateSave.SAVES_DIR)) {
            Directory.CreateDirectory(GenerateSave.SAVES_DIR);
        }

        initialized = true;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[SaveManager] Initialized");
        Console.ResetColor();
    }
}