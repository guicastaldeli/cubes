namespace App.Root.Chunk;

class ChunkGenerator {
    /**
     *
     * Generate
     *
     */
    public static ChunkData generate(ChunkCoord coord) {
        ChunkData val = new ChunkData(coord, hasMore: true);
        return val;
    }
}