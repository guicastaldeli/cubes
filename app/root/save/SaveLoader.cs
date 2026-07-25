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

    /**
     *
     * Load Save
     *
     */
    public static LoadResultInfo LoadSave(string saveName) {
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
            LoadStoreFromFolder(saveFolder);
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
}