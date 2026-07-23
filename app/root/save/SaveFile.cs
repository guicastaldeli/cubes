namespace App.Root.Save;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class SaveFieldAttribute : Attribute {
    public string? Key { get; set; }
    public bool Ignore { get; set; } = false;

    public SaveFieldAttribute() {}
    public SaveFieldAttribute(string Key) {
        this.Key = Key;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SaveDataAttribute : Attribute {
    public string? Section { get; set; }
    public string? FileName { get; set; }

    public SaveDataAttribute() {}
    public SaveDataAttribute(string Section) {
        this.Section = Section;
    }
}

[SaveData(Section = "info", FileName = "info.if")]
public class SaveFile {
    [SaveField("save_name")] public string SaveName { get; set; } = "";
    [SaveField("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    [SaveField("last_played")] public DateTime LastPlayed { get; set; } = DateTime.Now;
    [SaveField("play_time")] public float PlayTime { get; set; } = 0.0f;
    [SaveField("player_id")] public string PlayerId { get; set; } = ""; 
}