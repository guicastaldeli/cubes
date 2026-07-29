namespace App.Root.Chunk;

static class ContextChunk {
    [ThreadStatic]
    private static ChunkCoord? _current;

    public static ChunkCoord? current {
        get => _current;
        set => _current = value;
    }

    public static bool hasChunk => _current.HasValue;

    /**
     *
     * Set
     *
     */
    public static void Set(ChunkCoord coord) {
        _current = coord;
    }

    /**
     *
     * Clear
     *
     */
    public static void Clear() {
        _current = null;
    }
}