namespace App.Root.Save;
using App.Root.Info;
using App.Root.Utils;

/**

    Meta Info

    */
public static class M {
    public const string SAVE_META = "SV_ST.info.meta";
    public const string SAVE_META_JSON = "SV_ST.info.meta.json";
    public const string META_NAME = "m.manifest.json";
    public static string STORE_DATA(string id) { return $"{id}.json"; }
}

/**

    Save Path

    */
public static class SavePath {
    public static string SAVES_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");

    /**
     * Get Save Folder
     */
    public static string GetSaveFolder(string saveName) { return Path.Combine(SAVES_DIR, saveName); }
    public static string GetSaveFolder(SaveFile saveFile) { return Path.Combine(SAVES_DIR, saveFile.SaveName); }

    /**
     * Get Save Meta Path
     */
    public static string GetSaveMetaPath(string saveFolder) { return Path.Combine(saveFolder, M.SAVE_META_JSON); }
    public static string GetSaveMetaPath(SaveFile saveFile) { return Path.Combine(GetSaveFolder(saveFile), M.SAVE_META_JSON); }
    public static string GetSaveMetaPathByName(string saveName) { return Path.Combine(GetSaveFolder(saveName), M.SAVE_META_JSON); }

    /**
     * Get Manifest Path
     */
    public static string GetManifestPath(string saveFolder) { return Path.Combine(saveFolder, M.META_NAME); }
    public static string GetManifestPath(SaveFile saveFile) { return Path.Combine(GetSaveFolder(saveFile), M.META_NAME); }

    /**
     * Get Store Data Path
     */
    public static string GetStoreDataPath(string saveFolder, string id) { return Path.Combine(saveFolder, M.STORE_DATA(id)); }

    /**
     * Save Folder Exists
     */
    public static bool SaveFolderExists(string saveName) { return Directory.Exists(GetSaveFolder(saveName)); }
    public static bool SaveFolderExists(SaveFile saveFile) { return Directory.Exists(GetSaveFolder(saveFile)); }

    /**
     * Ensure Saves Directory
     */
    public static void EnsureSavesDirectory() { if(!Directory.Exists(SAVES_DIR)) Directory.CreateDirectory(SAVES_DIR); }
}

/**

    Save File

    */
[StoreData(M.SAVE_META)]
[DataOutput(Path: M.SAVE_META_JSON)]
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
