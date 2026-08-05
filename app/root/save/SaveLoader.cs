namespace App.Root.Save;
using System.Text.Json;

public static class SaveLoader {
    public enum LoadResult {
        Success,
        NotFound,
        InvalidVersion,
        Corrupt,
        MissingFiles
    }

    public class LoadResultInfo {
        public LoadResult Result { get; set; }
        public string? Message { get; set; }
        public string? Save { get; set; }
        public SaveFile? SaveInfo { get; set; }
        public bool IsCorrupted { get; set; } = false;
    }

    // Get All Saves
    public static List<SaveFile> GetAllSaves() {
        var saves = new List<SaveFile>();
        if(!Directory.Exists(SavePath.SAVES_DIR)) return saves;

        foreach(var dir in Directory.GetDirectories(SavePath.SAVES_DIR)) {
            string metaPath = SavePath.SaveMetaPath(dir);
            if(File.Exists(metaPath)) {
                try {
                    var json = File.ReadAllText(metaPath);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if(data != null) {
                        var saveFile = new SaveFile();
                        Data.DeserializeStoreData(saveFile, data);
                        saves.Add(saveFile);
                    }
                } catch(Exception err) {
                    Console.WriteLine($"Skipped: {err}");
                }
            }
        }

        List<SaveFile> val = saves.OrderByDescending(s => s.LastPlayed).ToList();
        return val;
    }

    // Get Save Names
    public static List<string> GetSaveNames() {
        string savesDir = SavePath.SAVES_DIR;
        if(!Directory.Exists(savesDir)) return new List<string>();

        List<string> val = Directory.GetDirectories(savesDir)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList()!;
        return val;
    }
    
    // Save Exists
    public static bool SaveExists(string saveId) {
        string? folder = SaveManager.GetSaveFolderById(saveId);
        bool val = !string.IsNullOrEmpty(folder);
        return val;
    }

    // Get Save Size
    public static long GetSaveSize(string saveName) {
        string saveFolder = Path.Combine(SavePath.SAVES_DIR, saveName);
        if(!Directory.Exists(saveFolder)) return 0;

        long val = Directory.GetFiles(saveFolder, "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
        return val;
    }

    /**
     *
     * Load
     *
     */
    // Load
    public static LoadResultInfo Load(string saveId) {
        var result = new LoadResultInfo();
        
        string? save = SaveManager.GetSaveFolderById(saveId);
        string? saveName = SaveManager.GetSaveNameById(saveId);
        if(string.IsNullOrEmpty(save)) {
            result.Result = LoadResult.NotFound;
            result.Message = $"Save ID '{saveId}' not found!";
            return result;
        }
        result.Save = save;

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"[SaveLoader] Loading Save... Name: {saveName} ; Id: {saveId}");
        Console.ResetColor();

        if(!string.IsNullOrEmpty(saveName)) SavePath.SetCurrentSave(saveName);

        string metaPath = Path.Combine(save, M.SAVE_META_JSON);
        if(!File.Exists(metaPath)) {
            result.Result = LoadResult.MissingFiles;
            result.Message = "Missing meta";
            result.IsCorrupted = true;
            return result;
        }

        try {
            var json = File.ReadAllText(metaPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if(data != null) {
                var file = new SaveFile();
                Data.DeserializeStoreData(file, data);
                result.SaveInfo = file;
                Console.WriteLine($"[SaveLoader] Loading save: {file.SaveName} (ID: {saveId})");
            }
        } catch(Exception err) {
            result.Result = LoadResult.Corrupt;
            result.Message = $"Error loading meta: {err.Message}";
            result.IsCorrupted = true;
            return result;
        }

        string manifestPath = SavePath.ManifestPath(save);
        if(!File.Exists(manifestPath)) {
            result.Result = LoadResult.MissingFiles;
            result.Message = "Missing manifest meta";
            result.IsCorrupted = true;
            return result;
        }

        try {
            DataOutput.LoadAll();
            LoadStoreDataFromFolder(save);
            DataInput.LoadAll();

            EventStream.set("save-loaded", (object)true);

            result.Result = LoadResult.Success;
            result.Message = "Save loaded successfully!";
            Console.WriteLine($"[SaveLoader] Loaded save: {saveId}");
        } catch(Exception err) {
            result.Result = LoadResult.Corrupt;
            result.Message = $"Error loading data: {err.Message}";
            result.IsCorrupted = true;
            Console.WriteLine($"[SaveLoader] Error: {err.Message}");
        }

        return result;
    }

    // Load Store Data From Folder
    public static void LoadStoreDataFromFolder(string save) {
        var jsonFiles = Directory.GetFiles(save, "*.json");
        foreach(var file in jsonFiles) {
            string manifest = SavePath.ManifestPath(save);
            string meta = M.SAVE_META_JSON;

            string fileName = Path.GetFileName(file);
            if(fileName == manifest || fileName == meta) continue;

            try {
                string id = Path.GetFileNameWithoutExtension(file);

                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if(data == null) continue;

                var obj = Data.GetData(id);
                if(obj != null) {
                    Data.DeserializeStoreData(obj, data);
                    Console.WriteLine($"[SaveLoader] Restored StoreData {id}");
                }
            } catch(Exception err) {
                Console.WriteLine($"[SaveLoader] Error loading {file}: {err.Message}");
            }
        }
    } 

    /**
     *
     * Delete
     *
     */
    public static bool Delete(string saveId) {
        string? save = SaveManager.GetSaveFolderById(saveId);
        if(string.IsNullOrEmpty(save)) return false;

        try {
            Directory.Delete(save, true);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[SaveLoader] Deleted save: {saveId}");
            Console.ResetColor();

            return true;
        } catch(Exception err) {
            Console.WriteLine($"[SaveLoader] Error deleting save: {err.Message}");
            return false;
        }
    }
}