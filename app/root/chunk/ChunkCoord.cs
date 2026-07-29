namespace App.Root.Chunk;
using OpenTK.Mathematics;

/** 

    Calculate Chunk util

    */
static class CalculateChunk {
    // To Max Height
    public static float ToMaxHeight(ChunkCoord coord) {
        var (_, maxY) = ChunkCoord.GetHeightRange(coord);
        return maxY;
    }

    // To Min Height
    public static float ToMinHeight(ChunkCoord coord) {
        var (minY, _) = ChunkCoord.GetHeightRange(coord);
        return minY;
    }

    // Multiplier
    public static float Multiply(float value, float multiplier) {
        float val = value * multiplier;
        return val;
    }

    /**
     *
     * Clamp
     *
     */
    // Clamp to Chunk Height
    public static float ClampToChunkHeight(ChunkCoord coord, float rawY) {
        float min = ToMinHeight(coord);
        float max = ToMaxHeight(coord);

        float val = Math.Clamp(rawY, min, max);
        return val;
    }

    public static float ClampToChunkHeight(ChunkCoord coord, float rawY, float multiplier) {
        float min = ToMinHeight(coord);
        float max = ToMaxHeight(coord);

        float mid = (min + max) / 2.0f;
        float halfRange = (max - min) / 2.0f * multiplier;

        float stretchedMin = mid - halfRange;
        float stretchedMax = mid + halfRange;

        float val = Math.Clamp(rawY, stretchedMin, stretchedMax);
        return val;
    }

    /**
     *
     * Expand
     *
     */
    public static float Expand(ChunkCoord coord, float value, float multiplier) {
        float min = ToMinHeight(coord);
        float max = ToMaxHeight(coord);
        float mid = (min + max) / 2.0f;
        float halfRange = (max - min) / 2.0f;

        float distance = value - mid;
        float expanded = mid + (distance * multiplier);

        float expandedMin = mid - (halfRange * multiplier);
        float expandedMax = mid + (halfRange * multiplier);

        float val = Math.Clamp(expanded, expandedMin, expandedMax);
        return val;
    }
}

/** 

    Chunk Coord main class

    */
public readonly struct ChunkCoord {
    public readonly int cx;
    public readonly int cy;
    public readonly int cz;

    public const int CHUNK_SIZE = 16;
    
    public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
    public static bool operator !=(ChunkCoord a, ChunkCoord b) => !a.Equals(b);

    public ChunkCoord(int cx, int cy, int cz) {
        this.cx = cx;
        this.cy = cy;
        this.cz = cz;
    }

    // Equals
    public bool Equals(ChunkCoord other) {
        bool val =
            cx == other.cx &&
            cy == other.cy &&
            cz == other.cz;

        return val;
    }

    public override bool Equals(object? obj) {
        bool val = obj is ChunkCoord other && Equals(other);
        return val;
    }

    // Get Hash Code
    public override int GetHashCode() {
        int val = HashCode.Combine(cx, cy, cz);
        return val;
    }

    // To String
    public override string ToString() {
        string val = $"Chunk({cx},{cy},{cz})";
        return val;
    }

    // Get Min Height
    public static float GetMinHeight(ChunkCoord coord) {
        Vector3 origin = coord.ToWorldPosition();
        return origin.Y;
    }

    // Get Max Height
    public static float GetMaxHeight(ChunkCoord coord) {
        Vector3 origin = coord.ToWorldPosition();
        return origin.Y + ChunkCoord.CHUNK_SIZE;
    }

    // Get Height Range
    public static (float min, float max) GetHeightRange(ChunkCoord coord) {
        float min = GetMinHeight(coord);
        float max = min + ChunkCoord.CHUNK_SIZE;
        return (min, max);
    }

    /**
     *
     * From World Position
     *
     */
    public static ChunkCoord FromWorldPosition(float x, float y, float z) {
        ChunkCoord val = new ChunkCoord(
            (int)MathF.Floor(x / CHUNK_SIZE),
            (int)MathF.Floor(y / CHUNK_SIZE),
            (int)MathF.Floor(z / CHUNK_SIZE)
        );

        return val; 
    }

    /**
     *
     * To World Position
     *
     */
    public Vector3 ToWorldPosition() {
        Vector3 val = (
            cx * CHUNK_SIZE,
            cy * CHUNK_SIZE,
            cz * CHUNK_SIZE
        );

        return val;
    }

    /**
     *
     * Distance To
     *
     */
    public float DistanceTo(ChunkCoord other) {
        float dx = cx - other.cx;
        float dy = cy - other.cy;
        float dz = cz - other.cz;

        float val = MathF.Sqrt(
            dx * dx + 
            dy * dy + 
            dz * dz
        );

        return val;
    }
}