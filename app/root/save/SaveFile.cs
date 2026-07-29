namespace App.Root.Save;
using App.Root.Info;
using App.Root.Utils;

/**

    Meta Info

    */
public static class M {
    // Main Files
    public const string SAVE_META = "SV_ST.info.meta";
    public const string SAVE_META_JSON = "SV_ST.info.meta.json";
    public const string META_NAME = "m.manifest.json";
    public static string STORE_DATA(string id) { return $"{id}.json"; }

    // Save Internal Files
    public const string PLAYER_STORAGE = "player_storage.ps";
}

/**

    Save Path

    */
public static class SavePath {
    public static string SAVES_DIR => DefaultPath();

    public static string? currentSaveName = null;
    public static string? currentSaveFolder = null;

    /**
     * Default Path
     */
    public static string SDefaultPath() { return DataOutput.DataOutputInfo.PATH_DIR; }
    public static string DefaultPath() { return Path.Combine(SDefaultPath(), "saves"); }

    /**
     * Get Save Folder
     */
    public static string SaveFolder(string saveName) { return Path.Combine(SAVES_DIR, saveName); }
    public static string SaveFolder(SaveFile saveFile) { return Path.Combine(SAVES_DIR, saveFile.SaveName); }

    /**
     * Get Save Meta Path
     */
    public static string SaveMetaPath() {
        if(!string.IsNullOrEmpty(currentSaveFolder)) return Path.Combine(currentSaveFolder, M.SAVE_META_JSON);
        return Path.Combine(DefaultPath(), M.SAVE_META_JSON);
    }
    public static string SaveMetaPath(string saveFolder) { return Path.Combine(saveFolder, M.SAVE_META_JSON); }
    public static string SaveMetaPath(SaveFile saveFile) { return Path.Combine(SaveFolder(saveFile), M.SAVE_META_JSON); }
    public static string SaveMetaPathByName(string saveName) { return Path.Combine(SaveFolder(saveName), M.SAVE_META_JSON); }

    /**
     * Get Manifest Path
     */
    public static string ManifestPath() {
        if(!string.IsNullOrEmpty(currentSaveFolder)) return Path.Combine(currentSaveFolder, M.META_NAME);
        return Path.Combine(DefaultPath(), M.META_NAME);
    }
    public static string ManifestPath(string saveFolder) { return Path.Combine(saveFolder, M.META_NAME); }
    public static string ManifestPath(SaveFile saveFile) { return Path.Combine(SaveFolder(saveFile), M.META_NAME); }

    /**
     * Get Store Data Path
     */
    public static string StoreDataPath(string saveFolder, string id) { return Path.Combine(saveFolder, M.STORE_DATA(id)); }

    /**
     * Save Folder Exists
     */
    public static bool SaveFolderExists(string saveName) { return Directory.Exists(SaveFolder(saveName)); }
    public static bool SaveFolderExists(SaveFile saveFile) { return Directory.Exists(SaveFolder(saveFile)); }

    /**
     * Ensure Saves Directory
     */
    public static void EnsureSavesDirectory() { if(!Directory.Exists(SAVES_DIR)) Directory.CreateDirectory(SAVES_DIR); }

    /**
     * Get Player Storage
     */
    public static string PlayerStorage() {
        if(!string.IsNullOrEmpty(currentSaveFolder)) return Path.Combine(currentSaveFolder, M.PLAYER_STORAGE);
        return Path.Combine(DefaultPath(), M.PLAYER_STORAGE);
    }
}

/**

    Save File

    */
[StoreData(M.SAVE_META)]
[DataOutput(typeof(SavePath), nameof(SavePath.SaveMetaPath))]
public class SaveFile {
    [StoreField("save_name")] [Convert("string")] [ConverterKey("save_id")] public string SaveId { get; set; } = "";
    [StoreField("save_name")] [Convert("string")] [ConverterKey("save_name")] public string SaveName { get; set; } = "";
    [StoreField("created_at")] [Convert("string")] [ConverterKey("created_at")] public string CreatedAt { get; set; } = "";
    [StoreField("last_played")] [Convert("string")] [ConverterKey("last_played")] public string LastPlayed { get; set; } = "";
    [StoreField("version")] [Convert("string")] [ConverterKey("version")] public string Version { get; set; } = "";
    [StoreField("play_time")] [Convert("string")] [ConverterKey("play_time")] public float PlayTime { get; set; } = 0.0f;
    
    [StoreField("player_id")] [Convert("string")] [ConverterKey("player_id")] public string PlayerId { get; set; } = "";
    [StoreField("player_name")] [Convert("string")] [ConverterKey("player_name")] public string PlayerName { get; set; } = "";

    public SaveFile() {
        this.SaveId = Guid.NewGuid().ToString();
    }
    public SaveFile(string SaveName) : this() {
        this.SaveName = SaveName;
        this.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.Version = "";
        this.PlayerId = InfoController.UserId ?? "";
        this.PlayerName = InfoController.Username ?? "";
    }

    // Update Last Played
    public void UpdateLastPlayed() {
        this.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /**
     *
     * Extract Data
     *
     */
    public static object ExtractData() {
        var data = Data.GetData<SaveFile>(M.SAVE_META);
        if(data != null) return data;

        object val = new SaveFile();
        return val;
    }

    /**
     *
     * Save Data
     *
     */
    public static void SaveData(object data) {
        if(data is SaveFile saveFile) {
            Data.RegisterData(M.SAVE_META, saveFile);
            DataOutput.Save(M.SAVE_META);
        }
    }
}
