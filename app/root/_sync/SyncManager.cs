namespace App.Root._Sync;

/**

    Conflict Resoltion

    */
public enum ConflictResolution {
    SERVER_AUTHORITY,
    LAST_WRITE_WINS,
    MERGE_WITH_ARBITER,
    LOCK_BASED
}

/**

    Sync Mode

    */
public enum SyncMode {
    BROADCAST,
    PRIVATE,
    PERSISTENT
}

/**

    Sync Ignore Attribute

    */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
public class SyncIgnoreAttribute : Attribute {
    public SyncIgnoreAttribute() {}
}

/**

    Sync Field Attribute

    */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class SyncFieldAttribute : Attribute {
    public string? Key { get; set; }
    public bool Ignore { get; set; } = false;
    public float DeltaThreshold { get; set; } = 0.0f;
    public bool AlwaysSync { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;

    public SyncFieldAttribute() {}
    public SyncFieldAttribute(string Key) {
        this.Key = Key;
    }
}

/**

    Data Sync Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DataSyncAttribute : Attribute {
    public string? Id { get; set; }
    public SyncMode Mode { get; set; } = SyncMode.BROADCAST;
    public ConflictResolution Resolution { get; set; } = ConflictResolution.SERVER_AUTHORITY;
    public bool EnableDelta { get; set; } = true;
    public bool ValidateAlways { get; set; } = true;
    public string? RequiredPermission { get; set; }
    public float SyncThreshold { get; set; } = 0.0f;

    public DataSyncAttribute() {}
    public DataSyncAttribute(string Id) {
        this.Id = Id;
    } 
}

/**

    Sync Manager main class.

    */
public class SyncManager {
    
}