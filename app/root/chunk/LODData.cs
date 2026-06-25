namespace App.Root.Chunk;

/**

    LOD Attribute

    */
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public class LODableAttribute : Attribute {
    public string Id { get; }
    public Type ConfigType { get; }
    public int InitialSize { get; set; } = 32;
    public int MaxSize { get; set; } = 256;

    public LODableAttribute(string Id, Type ConfigType) {
        this.Id = Id;
        this.ConfigType = ConfigType;
    }
}

/**

    LODable Interface

    */
public interface ILODable {
    string GetLODConfigId();
    void UpdateLOD(Dictionary<int, LODData> lodData, LODConfig config);
}

/**

    LOD Level

    */
public enum LODLevel {
    ULTRA = 0,
    HIGH = 1,
    MEDIUM = 2,
    LOW = 3,
    VERY_LOW = 4,
    CULLED = 5
}

/**

    LOD Config

    */
public class LODConfig {
    public bool SkipCollisionsForLow { get; set; } = true;
    public int CollsionLODThreshold { get; set; } = (int)LODLevel.MEDIUM;

    /**
     *
     * Distance
     *
     */
    public float UltraDistance { get; set; } = 10.0f;
    public float HighDistance { get; set; } = 20.0f;
    public float MediumDistance { get; set; } = 30.0f;
    public float LowDistance { get; set; } = 40.0f;
    public float VeryLowDistance { get; set; } = 50.0f;
    public float CullDistance { get; set; } = 60.0f;

    /**
     *
     * Quality
     *
     */
    public float UltraQuality { get; set; } = 1.0f;
    public float HighQuality { get; set; } = 0.8f;
    public float MediumQuality { get; set; } = 0.4f;
    public float LowQuality { get; set; } = 0.2f;
    public float VeryLowQuality { get; set; } = 0.05f;

    /**
     *
     * Update Interval
     *
     */
    public int UltraUpdateInterval { get; set; } = 1;
    public int HighUpdateInterval { get; set; } = 2;
    public int MediumUpdateInterval { get; set; } = 4;
    public int LowUpdateInterval { get; set; } = 8;
    public int VeryLowUpdateInterval { get; set; } = 16;
}

/**

    LOD Data

    */
public class LODData {
    public LODLevel Level { get; set; }
    public float Distance { get; set; }
    public float Quality { get; set; }
    public int UpdateInterval { get; set; }
    public bool ShouldUpdateThisFrame { get; set; }
    public bool IsVisible { get; set; }
    public bool ShouldSkipCollisions { get; set; }
    public int EntitiesToProcess { get; set; }
}