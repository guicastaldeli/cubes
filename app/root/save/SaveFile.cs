using App.Root.Info;
using App.Root.Utils;

namespace App.Root.Save;

public static class M {
    public const string SAVE_META = "SV_ST.info.meta";
    public const string SAVE_META_JSON = "SV_ST.info.meta.json";
}

[StoreData(M.SAVE_META)]
[DataOutput(Path: M.SAVE_META_JSON)]
public class SaveFile {
    [StoreField("save_name")] [Convert("string")] [ConverterKey("save_name")] public string SaveName { get; set; } = "";
    [StoreField("created_at")] [Convert("string")] [ConverterKey("created_at")] public string CreatedAt { get; set; } = "";
    [StoreField("last_played")] [Convert("string")] [ConverterKey("last_played")] public string LastPlayed { get; set; } = "";
    [StoreField("version")] [Convert("string")] [ConverterKey("version")] public string Version { get; set; } = "";
    [StoreField("play_time")] [Convert("string")] [ConverterKey("play_time")] public string PlayTime { get; set; } = "";
    
    [StoreField("player_id")] [Convert("string")] [ConverterKey("player_id")] public string PlayerId { get; set; } = "";
    [StoreField("player_name")] [Convert("string")] [ConverterKey("player_name")] public string PlayerName { get; set; } = "";

    public SaveFile() {}
    public SaveFile(string SaveName) {
        this.SaveName = SaveName;
        this.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.Version = "";
        this.PlayerId = InfoController.UserId ?? "";
        this.PlayerName = InfoController.Username ?? "";
    }

    // Update Last Played
    public void UpdateLastPlayed() {
        LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
