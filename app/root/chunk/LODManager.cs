namespace App.Root.Chunk;
using OpenTK.Mathematics;

public static class LODManager {
    private static readonly Dictionary<string, LODConfig> configs = new();
    private static readonly Dictionary<object, Dictionary<int, LODData>> lodCache = new();

    private static int _frameCounter = 0;
    private static readonly object _lock = new();

    public static int FrameCounter => _frameCounter;

    public static void IncrementFrame() {
        _frameCounter++;
    }

    // Calculate Level
    public static LODLevel calculateLevel(float distance, LODConfig config) {
        if(distance < config.UltraDistance) return LODLevel.ULTRA;
        if(distance < config.HighDistance) return LODLevel.HIGH;
        if(distance < config.MediumDistance) return LODLevel.MEDIUM;
        if(distance < config.LowDistance) return LODLevel.LOW;
        if(distance < config.VeryLowDistance) return LODLevel.VERY_LOW;
        if(distance < config.CullDistance) return LODLevel.VERY_LOW;
        return LODLevel.CULLED;
    }

    // Should Update this Frame
    public static bool shouldUpdateThisFrame(LODLevel level, LODConfig config) {
        int interval = getUpdateInterval(level, config);
        bool val = interval == 1 || (_frameCounter % interval == 0);
        return val;
    }

    /**
     *
     * Register Config
     *
     */
    public static void registerConfig(string id, LODConfig config) {
        lock(_lock) {
            configs[id] = config;
        }
    }

    /**
     *
     * Get
     *
     */
    // Get Quality
    public static float getQuality(LODLevel level, LODConfig config) {
        switch(level) {
            case LODLevel.ULTRA: return config.UltraQuality;
            case LODLevel.HIGH: return config.HighQuality;
            case LODLevel.MEDIUM: return config.MediumQuality;
            case LODLevel.LOW: return config.LowQuality;
            case LODLevel.VERY_LOW: return config.VeryLowQuality;
            case LODLevel.CULLED: return 0;
            default: return 1.0f;
        }
    }

    // Get Update Interval
    public static int getUpdateInterval(LODLevel level, LODConfig config) {
        switch(level) {
            case LODLevel.ULTRA: return config.UltraUpdateInterval;
            case LODLevel.HIGH: return config.HighUpdateInterval;
            case LODLevel.MEDIUM: return config.MediumUpdateInterval;
            case LODLevel.LOW: return config.LowUpdateInterval;
            case LODLevel.VERY_LOW: return config.VeryLowUpdateInterval;
            case LODLevel.CULLED: return int.MaxValue;
            default: return 1;
        }
    }
    
    // Get LOD Data
    public static LODData getLODData(Vector3 position, Vector3 playerPosition, LODConfig config) {
        float distance = Vector3.Distance(position, playerPosition);

        var level = calculateLevel(distance, config);
        var quality = getQuality(level, config);
        var interval = getUpdateInterval(level, config);
        var shouldUpdate = shouldUpdateThisFrame(level, config);
        var isVisible = level != LODLevel.CULLED;
        var skipCollisions = config.SkipCollisionsForLow && (int)level >= config.CollsionLODThreshold;
        var entitiesToProcess = Math.Max(1, (int)(quality * 10));

        return new LODData {
            Level = level,
            Distance = distance,
            Quality = quality,
            UpdateInterval = interval,
            ShouldUpdateThisFrame = shouldUpdate,
            IsVisible = isVisible,
            ShouldSkipCollisions = skipCollisions,
            EntitiesToProcess = entitiesToProcess
        };
    }

    /**
     *
     * Clear
     *
     */
    public static void clearCache() {
        lock(_lock) {
            lodCache.Clear();
        }
    }
}