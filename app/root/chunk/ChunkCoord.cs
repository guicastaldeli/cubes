namespace App.Root.Chunk;
using OpenTK.Mathematics;

readonly struct ChunkCoord {
    public readonly int cx;
    public readonly int cy;
    public readonly int cz;

    public const int SIZE = 16;
    
    public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
    public static bool operator !=(ChunkCoord a, ChunkCoord b) => !a.Equals(b);

    public ChunkCoord(int cx, int cy, int cz) {
        this.cx = cx;
        this.cy = cy;
        this.cz = cz;
    }

    // Quals
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

    /**
     *
     * From World Position
     *
     */
    public static ChunkCoord FromWorldPosition(float x, float y, float z) {
        ChunkCoord val = new ChunkCoord(
            (int)MathF.Floor(x / SIZE),
            (int)MathF.Floor(y / SIZE),
            (int)MathF.Floor(z / SIZE)
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
            cx * SIZE,
            cy * SIZE,
            cz * SIZE
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