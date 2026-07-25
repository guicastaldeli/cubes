using System.Net;
using System.Text.Json;

namespace App.Root.Save;

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
        public string? SaveFolder { get; set; }
        public SaveFile? SaveInfo { get; set; }
        public bool IsCorrupted { get; set; } = false;
    }

    // Get All Saves
    public static List<SaveFile> GetAllSaves() {
        var saves = new List<SaveFile>();
        if(!Directory.Exists(GenerateSave.SAVES_DIR)) return saves;

        foreach(var dir in Directory.GetDirectories(GenerateSave.SAVES_DIR)) {
            string saveName = Path.GetFileName(dir);
            string metaPath = Path.Combine(dir, M.SAVE_META_JSON);

            if(File.Exists(metaPath)) {
                try {
                    var json = File.ReadAllText(metaPath);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if(data != null) {
                        if(data != null) {
                            var saveFile = new SaveFile();
                            Data.DeserializeStoreData(saveFile, data);
                            saves.Add(saveFile);
                        }
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
        string savesDir = GenerateSave.SAVES_DIR;
        if(!Directory.Exists(savesDir)) return new List<string>();

        List<string> val = Directory.GetDirectories(savesDir)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList()!;
        return val;
    }
    
    // Save Exists
    public static bool SaveExists(string saveName) {
        string saveFolder = Path.Combine(GenerateSave.SAVES_DIR, saveName);
        
        bool val = Directory.Exists(saveFolder);
        return val;
    }

    // Get Save Size
    public static long GetSaveSize(string saveName) {
        string saveFolder = Path.Combine(GenerateSave.SAVES_DIR, saveName);
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
    public static LoadResultInfo Load(string saveName) {
        string saveFolder = Path.Combine(GenerateSave.SAVES_DIR, saveName);
        var result = new LoadResultInfo {
            Result = LoadResult.Success,
            SaveFolder = saveFolder
        };

        if(!Directory.Exists(saveFolder)) {
            result.Result = LoadResult.NotFound;
            result.Message = $"Save '{saveName}' not found!";
            return result;
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"[SaveLoader] Loading save: {saveName}");
        Console.ResetColor();

        string manifestPath = GenerateSave.ManifestPath(saveFolder);
        if(!File.Exists(manifestPath)) {
            result.Result = LoadResult.MissingFiles;
            result.Message = "Missing manifest.json!";
            result.IsCorrupted = true;
            return result;
        }

        try {
            string metaPath = Path.Combine(saveFolder, M.SAVE_META_JSON);
            if(File.Exists(metaPath)) {
                var json = File.ReadAllText(metaPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if(data != null) {
                    var saveFile = new SaveFile();
                    Data.DeserializeStoreData(saveFile, data);
                    result.SaveInfo = saveFile;
                }
            }
        } catch(Exception ex) {
            Console.WriteLine($"[SaveLoader] Error loading save meta: {ex.Message}");
            result.IsCorrupted = true;
        }

        DataOutput.SetSavePath(saveFolder);
        try {
            DataOutput.LoadAll();
            LoadStoreDataFromFolder(saveFolder);
            DataInput.LoadAll();

            Console.WriteLine($"[SaveLoader] Loaded save: {saveName}");
            result.Message = "Save loaded successfully!";
        } catch(Exception ex) {
            result.Result = LoadResult.Corrupt;
            result.Message = $"Error loading save: {ex.Message}";
            result.IsCorrupted = true;
            Console.WriteLine($"[SaveLoader] Error: {ex.Message}");
        }

        return result;
    }

    // Load Store Data From Folder
    public static void LoadStoreDataFromFolder(string saveFolder) {
        var jsonFiles = Directory.GetFiles(saveFolder, "*.json");
        foreach(var file in jsonFiles) {
            string manifest = GenerateSave.ManifestPath(saveFolder);
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
            } catch(Exception ex) {
                Console.WriteLine($"[SaveLoader] Error loading {file}: {ex.Message}");
            }
        }
    } 

    /**
     *
     * Delete
     *
     */
    public static bool Delete(string saveName) {
        string saveFolder = Path.Combine(GenerateSave.SAVES_DIR, saveName);
        if(!Directory.Exists(saveFolder)) return false;

        try {
            Directory.Delete(saveFolder, true);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[SaveLoader] Deleted save: {saveName}");
            Console.ResetColor();
            
            return true;
        } catch(Exception ex) {
            Console.WriteLine($"[SaveLoader] Error deleting save: {ex.Message}");
            return false;
        }
    }
}