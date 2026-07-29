namespace App.Root.Save;
using System.Text.Json;

public static class SaveManager {
    public static string SaveId { get { return "save_id"; } }
    public static string SaveName { get { return "save_name"; } }

    private static string currentSaveId = "";
    private static string currentSaveName = "";
    private static string currentSavePath = "";

    private static bool initialized = false;

    // Get All Saves
    public static List<SaveFile> GetAllSaves() {
        List<SaveFile> val = SaveLoader.GetAllSaves();
        return val;
    }

    // Get Save Name by Id
    public static string? GetSaveNameById(string saveId) {
        if(string.IsNullOrEmpty(saveId)) return null;
        if(!Directory.Exists(SavePath.SAVES_DIR)) return null;

        foreach(var dir in Directory.GetDirectories(SavePath.SAVES_DIR)) {
            string metaPath = SavePath.SaveMetaPath(dir);
            if(!File.Exists(metaPath)) continue;

            try {
                var json = File.ReadAllText(metaPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if(data != null && data.TryGetValue(SaveId, out var obj)) {
                    string id = obj.ToString() ?? "";
                    if(id == saveId) {
                        if(data.TryGetValue(SaveName, out var name)) {
                            return name.ToString();
                        }

                        return Path.GetFileName(dir);
                    }
                }
            } catch(Exception err) {
                Console.WriteLine(err);
            }
        }

        return null;
    }

    // Get Save Folder by Id
    public static string? GetSaveFolderById(string saveId) {
        if(string.IsNullOrEmpty(saveId)) return null;
        if(!Directory.Exists(SavePath.SAVES_DIR)) return null;

        foreach(var dir in Directory.GetDirectories(SavePath.SAVES_DIR)) {
            string metaPath = SavePath.SaveMetaPath(dir);
            if(!File.Exists(metaPath)) continue;

            try {
                var json = File.ReadAllText(metaPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if(data != null && data.TryGetValue(SaveId, out var obj)) {
                    string id = obj.ToString() ?? "";
                    if(id == saveId) return dir;
                }
            } catch(Exception err) {
                Console.WriteLine(err);
            }
        }

        return null;
    }

    // Get Save File by Id
    private static SaveFile? GetSaveFileById(string saveId) {
        string? saveFolder = GetSaveFolderById(saveId);
        if(string.IsNullOrEmpty(saveFolder)) return null;

        string metaPath = SavePath.SaveMetaPath(saveFolder);
        if(!File.Exists(metaPath)) return null;

        try {
            var json = File.ReadAllText(metaPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if(data != null) {
                var file = new SaveFile();
                Data.DeserializeStoreData(file, data);
                return file;
            }
        } catch(Exception err) {
            Console.WriteLine(err);
        }

        return null;
    }

    // Save Exists
    public static bool SaveExists(string saveId) {
        bool val = SaveLoader.SaveExists(saveId);
        return val;
    }

    // Get Current Save Id
    public static string GetCurrentSaveId() {
        string val = currentSaveId;
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
        if(string.IsNullOrEmpty(currentSaveId)) return null;
        
        SaveFile? val = GetSaveFileById(currentSaveId);
        return val;
    }

    public static bool HasSaves() {
        bool val = GetAllSaves().Count > 0;
        return val;
    }

    /**
     *
     * Create Save
     *
     */
    public static string CreateSave(string saveName) {
        var file = GenerateSave.CreateSave(saveName);
        
        string saveId = file.SaveId;
        currentSaveId = saveId;
        currentSaveName = saveName;
        currentSavePath = SavePath.SaveFolder(file);

        return saveId;
    }

    /**
     *
     * Load Save
     *
     */
    public static SaveLoader.LoadResultInfo LoadSave(string saveId) {
        var result = SaveLoader.Load(saveId);
        if(result.Result == SaveLoader.LoadResult.Success) {
            currentSaveId = saveId;
            currentSavePath = result.Save ?? "";
            currentSaveName = GetSaveNameById(saveId) ?? "";

            SavePath.SetCurrentSave(currentSaveName);

            Console.WriteLine($"[SaveManager] Loaded Save... name: {currentSaveName} ; id: {currentSaveId}");
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
        if(string.IsNullOrEmpty(currentSaveId)) {
            Console.WriteLine("[SaveManager] No save loaded! Create a save first.");
            return;
        }

        var saveFile = GetSaveFileById(currentSaveId);
        if(saveFile == null) {
            Console.WriteLine("[SaveManager] Save file not found!");
            return;
        }

        string saveFolder = SavePath.SaveFolder(saveFile);
        currentSavePath = saveFolder;

        GenerateSave.SaveStoreDataToFolder(currentSavePath, M.SAVE_META);
        DataOutput.SaveAll();
        
        saveFile.UpdateLastPlayed();
        Data.RegisterData(M.SAVE_META, saveFile);
        DataOutput.Save(M.SAVE_META);

        GenerateSave.CreateManifest(saveFolder, saveFile);
        Console.WriteLine($"[SaveManager] Saved to: {currentSaveName}");
    }

    /**
     *
     * Delete
     *
     */
    public static bool Delete(string saveId) {
        bool success = SaveLoader.Delete(saveId);
        if(success && currentSaveId == saveId) {
            currentSaveId = "";
            currentSaveName = "";
            currentSavePath = "";
        }

        return success;
    }

    /**
     *
     * Init
     *
     */
    public static void Init() {
        if(initialized) return;

        SavePath.EnsureSavesDirectory();
        initialized = true;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[SaveManager] Initialized");
        Console.ResetColor();
    }
}