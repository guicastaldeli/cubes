namespace App.Root.Save;
using System.Text.Json;

public static class GenerateSave {
    public static string SAVES_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");

    private static string Json(string id) { return $"{id}.json"; }
    public static string ManifestPath(string saveFolder) { return Path.Combine(saveFolder, "manifest.json"); }

    // Generate Default Save Name
    public static string GenerateDefaultSaveName() {
        return "default"; // TODO: New World_1, _2 etc...   
    }

    // Is Valid Save Name
    public static bool IsValidSaveName(string saveName) {
        if(string.IsNullOrEmpty(saveName)) return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        
        bool val = !saveName.Any(c => invalidChars.Contains(c));
        if(!val) throw new ArgumentException("Save name contains invalid chars!");
        return val;
    }

    // Create Manifest
    public static void CreateManifest(string saveFolder, string saveName) {
        var manifest = new {
            save_name = saveName,
            created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            version = "1.1.1",
            files = Directory.GetFiles(saveFolder).Select(Path.GetFileName).ToList()
        };

        string manifestPath = ManifestPath(saveFolder);
        var data = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(manifestPath, data);

        Console.WriteLine($"[SaveGenerator] Created manifest: {manifestPath}");
    }
 
    // Save Store Data to Folder
    public static void SaveStoreDataToFolder(string saveFolder, string meta) {
        var ids = Data.GetAllStoreDataIds();
        foreach(var id in ids) {
            if(id == meta) continue;

            var obj = Data.GetData(id);
            if(obj == null) return;

            var serialized = Data.SerializeStoreData(obj);
            if(serialized != null) {
                string filePath = Path.Combine(saveFolder, Json(id));
                var data = JsonSerializer.Serialize(serialized, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, data);

                Console.WriteLine($"[SaveGenerator] Saved StoreData {id} to {filePath}");
            }
        }
    }

    /**
     *
     * Create Save
     *
     */
    public static string CreateSave(string saveName) {
        if(string.IsNullOrEmpty(saveName)) throw new ArgumentException("Save name cannot be empty!");

        IsValidSaveName(saveName);

        string saveFolder = Path.Combine(SAVES_DIR, saveName);
        if(Directory.Exists(saveFolder)) throw new InvalidOperationException($"Save '{saveName}' already exists!");

        Directory.CreateDirectory(saveFolder);
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"[SaveGenerator] Created save folder: {saveFolder}");
        Console.ResetColor();

        SaveFile saveFile = new SaveFile(saveName);
        string meta = M.SAVE_META;
        Data.RegisterStoreData(saveFile);
        Data.RegisterData(meta, saveFile);

        DataOutput.SetSavePath(saveFolder);

        DataOutput.SaveAll();
        SaveStoreDataToFolder(saveFolder, meta);
        CreateManifest(saveFolder, saveName);

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[SaveGenerator] Save '{saveName}' created successfully!");
        Console.ResetColor();
        return saveFolder;
    }
}