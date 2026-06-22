namespace App.Root.Chunk;

/**

    Chunk Priority

    */
public enum ChunkPriority {
    HIGH = 0,
    MEDIUM = 1,
    LOW = 2,
    VERY_LOW = 3
}

/**

    Chunk Priority Config

    */
class ChunkPriorityConfig {
    /**
     *
     * Distance Thresholds
     *
     */
    public float HighDistance { get; set; } = 3.0f;
    public float MediumDistance { get; set; } = 6.0f;
    public float LowDistance { get; set; } = 10.0f;
    public float MaxDistance { get; set; } = 15.0f;

    /**
     *
     * Update Intervals
     *
     */
    public int HighInterval { get; set; } = 1;
    public int MediumInterval { get; set; } = 2;
    public int LowInterval { get; set; } = 4;
    public int VeryLowInterval { get; set; } = 8;

    /**
     *
     * Entity Reduction
     *
     */
    public float HighEntityRatio { get; set; } = 1.0f;
    public float MediumEntityRatio { get; set; } = 0.5f;
    public float LowEntityRatio { get; set; } = 0.25f;
    public float VeryLowEntityRatio { get; set; } = 0.1f;
}

/**

    Chunk Priority Data

    */
static class ChunkPriorityManager {
    private static ChunkPriorityConfig _config = new ChunkPriorityConfig();
    private static Dictionary<ChunkCoord, ChunkPriorityData> _priorityCache = new();
    private static Dictionary<string, Dictionary<ChunkCoord, ChunkPriorityData>> _handlerPriorityCache = new();
    
    private static int _frameCounter = 0;

    public static ChunkPriorityConfig Config {
        get => _config;
        set => _config = value ?? new ChunkPriorityConfig();
    }

    public static void IncrementFrame() {
        int v = 1000000;
        _frameCounter++;
        if(_frameCounter > v) _frameCounter = 0;
    }

    public static int FrameCounter => _frameCounter;

    // Get Entity Ratio
    public static float GetEntityRatio(ChunkPriority priority) {
        switch(priority) {
            case ChunkPriority.HIGH:
                return _config.HighEntityRatio;
            case ChunkPriority.MEDIUM:
                return _config.MediumEntityRatio;
            case ChunkPriority.LOW:
                return _config.LowEntityRatio;
            case ChunkPriority.VERY_LOW:
                return _config.VeryLowEntityRatio;
            default:
                return 1.0f;
        }
    }

    // Get Priority Data
    public static ChunkPriorityData GetPriorityData(ChunkCoord playerChunk, ChunkCoord chunk) {
        var key = chunk;
        
        if(!_priorityCache.TryGetValue(key, out var data) || data.PlayerChunk != playerChunk) {
            var priority = CalculatePriority(playerChunk, chunk);
            
            data = new ChunkPriorityData {
                Chunk = chunk,
                PlayerChunk = playerChunk,
                Priority = priority,
                LastUpdateFrame = _frameCounter,
                ShouldUpdateThisFrame = ShouldUpdateThisFrame(priority)
            };
            
            _priorityCache[key] = data;
        }

        return data;
    }

    // Get Handler Priority
    public static ChunkPriorityData? GetHandlerPriority(string handlerId, ChunkCoord coord) {
        if(_handlerPriorityCache.TryGetValue(handlerId, out var map)) {
            ChunkPriorityData? val = map.TryGetValue(coord, out var data) ? data : null;
            return val;
        }

        return null;
    }

    /**
     *
     * Entity Reduction
     *
     */
    public static ChunkPriority CalculatePriority(ChunkCoord playerChunk, ChunkCoord chunk) {
        float dist = playerChunk.DistanceTo(chunk);

        if(dist <= _config.HighDistance) return ChunkPriority.HIGH;
        if(dist <= _config.MediumDistance) return ChunkPriority.MEDIUM;
        if(dist <= _config.LowDistance) return ChunkPriority.LOW;
        if(dist <= _config.MaxDistance) return ChunkPriority.VERY_LOW;

        return ChunkPriority.VERY_LOW;
    }

    /**
     *
     * Should Update this Frame
     *
     */
    public static bool ShouldUpdateThisFrame(ChunkPriority priority) {
        switch(priority) {
            case ChunkPriority.HIGH:
                return _frameCounter % _config.HighInterval == 0;
            case ChunkPriority.MEDIUM:
                return _frameCounter % _config.MediumDistance == 0;
            case ChunkPriority.LOW:
                return _frameCounter % _config.LowInterval == 0;
            case ChunkPriority.VERY_LOW:
                return _frameCounter % _config.VeryLowInterval == 0;
            default:
                return true;
        }
    }
    /**
     *
     * Register Handler Priority
     *
     */
    public static void RegisterHandlerPriority(string handlerId, ChunkCoord coord, ChunkPriorityData data) {
        if(!_handlerPriorityCache.ContainsKey(handlerId)) {
            _handlerPriorityCache[handlerId] = new Dictionary<ChunkCoord, ChunkPriorityData>();
        }
        _handlerPriorityCache[handlerId][coord] = data;
    }

    /**
     *
     * Clear Cache
     *
     */
    public static void ClearCache() {
        _priorityCache.Clear();
        _handlerPriorityCache.Clear();
    }
}

/**

    Chunk Priority Data

    */
public struct ChunkPriorityData {
    public ChunkCoord Chunk;
    public ChunkCoord PlayerChunk;
    public ChunkPriority Priority;
    public int LastUpdateFrame;
    public bool ShouldUpdateThisFrame;
    public float EntityRatio => ChunkPriorityManager.GetEntityRatio(Priority);

    public int GetEntityCount(int total) {
        int val = Math.Max(1, (int)(total * EntityRatio));
        return val;
    }

    public bool IsVisible {
        get {
            float dist = Chunk.DistanceTo(PlayerChunk);
            return dist <= ChunkPriorityManager.Config.MaxDistance;
        }
    }
}

/**

    Chunk Data main class

    */
class ChunkData {
    public ChunkCoord coord;
    public bool hasMore;
    public bool isGenerated = false;

    public ChunkData(ChunkCoord coord, bool hasMore = true) {
        this.coord = coord;
        this.hasMore = hasMore;
        this.isGenerated = true;
    }
}