namespace App.Root.Chunk;

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