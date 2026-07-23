using System.Text.Json;
using App.Root.Info;

namespace App.Root.Save;

public static class GenerateSave {
    private static string SAVES_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");

    // Get All Saves
    public static List<string> GetAllSaves() {
        if(!Directory.Exists(SAVES_DIR)) return new List<string>();

        List<string> val = Directory.GetDirectories(SAVES_DIR)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList()!;
        return val;
    }

    // Save Exists
    public static bool SaveExists(string saveName) {
        string saveFolder = Path.Combine(SAVES_DIR, saveName);
        
        bool val = Directory.Exists(saveFolder);
        return val;
    }
    
    // Create Manifest
    private static void CreateManifest(string saveFolder, string saveName) {
        var manifest = new {
            save_name = saveName,
            created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            files = Directory.GetFiles(saveFolder).Select(Path.GetFileName).ToList()
        };

        string manifestPath = Path.Combine(saveFolder, "manifest.json");
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(manifestPath, json);
    }

    /**
     *
     * Create Save
     *
     */
    public static string CreateSave(string saveName) {
        string saveFolder = Path.Combine(SAVES_DIR, saveName);
        if(Directory.Exists(saveFolder)) {
            Console.WriteLine($"[GenerateSave] Save '{saveName}' already exists!");
            return saveFolder;
        }

        Directory.CreateDirectory(saveFolder);
        Console.WriteLine($"[GenerateSave] Created save folder: {saveFolder}");
    
        var saveInfo = new SaveFile {
            SaveName = saveName,
            CreatedAt = DateTime.Now,
            LastPlayed = DateTime.Now,
            PlayerId = InfoController.UserId
        };

        SaveManager.Save(saveInfo, saveFolder);
        DataOutput.SaveAll();

        CreateManifest(saveFolder, saveName);
        Console.WriteLine($"[GenerateSave] Save '{saveName}' created successfully!");
        return saveFolder;
    }
}