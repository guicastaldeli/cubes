namespace App.Root.Save;
using System.Text.Json;

public static class GenerateSave {
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
    public static void CreateManifest(string saveFolder, SaveFile saveFile) {
        var manifest = new {
            save_id = saveFile.SaveId,
            save_name = saveFile.SaveName,
            created_at = saveFile.CreatedAt,
            version = saveFile.Version,
            files = Directory.GetFiles(saveFolder).Select(Path.GetFileName).ToList()
        };

        string manifestPath = SavePath.ManifestPath(saveFolder);
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
            if(obj == null) continue;

            var serialized = Data.SerializeStoreData(obj);
            if(serialized != null) {
                string filePath = Path.Combine(saveFolder, M.STORE_DATA(id));
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
    public static SaveFile CreateSave(string saveName) {
        if(string.IsNullOrEmpty(saveName)) throw new ArgumentException("Save name cannot be empty!");
        IsValidSaveName(saveName);
        
        SaveFile saveFile = new SaveFile(saveName);
        string saveId = saveFile.SaveId;
        string saveFolder = SavePath.SaveFolder(saveName);
        if(SavePath.SaveFolderExists(saveName)) throw new InvalidOperationException($"Save '{saveName}' already exists!");

        Directory.CreateDirectory(saveFolder);
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"[SaveGenerator] Created save folder: {saveFolder}; ID: {saveId}, Name: {saveName}");
        Console.ResetColor();

        string meta = M.SAVE_META;
        Data.RegisterStoreData(saveFile);
        Data.RegisterData(meta, saveFile);

        DataOutput.SetSavePath(saveFolder);
        DataOutput.SaveAll();
        SaveStoreDataToFolder(saveFolder, meta);

        CreateManifest(saveFolder, saveFile);

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[SaveGenerator] Save '{saveName}' created successfully!");
        Console.ResetColor();
        return saveFile;
    }
}