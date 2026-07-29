namespace App.Root.Chunk;
using System.Collections.Generic;

/**

    Chunk Updatable

    */
interface IChunkUpdatable {
    HashSet<ChunkCoord> GetActiveChunks();
    void UpdateChunks(Dictionary<ChunkCoord, ChunkPriorityData> priorityData, ChunkPriorityConfig config);
}

/**

    Chunk Processor

    */
class ChunkProcessor {
    private List<IChunkUpdatable> updatables = new();
    private Dictionary<IChunkUpdatable, ChunkPriorityConfig> customConfigs = new();
    private ChunkPriorityConfig defaultConfig = new ChunkPriorityConfig();

    /**
     *
     * Register
     *
     */
    public void Register(IChunkUpdatable updatable, ChunkPriorityConfig? customConfig = null) {
        if(!updatables.Contains(updatable)) {
            updatables.Add(updatable);
            if(customConfig != null) {
                customConfigs[updatable] = customConfig;
            }
        }
    }

    /**
     *
     * Unregister
     *
     */
    public void Unregister(IChunkUpdatable updatable) {
        updatables.Remove(updatable);
        customConfigs.Remove(updatable);
    }

    /**
     *
     * Process
     *
     */
    public void Process(ChunkCoord playerChunk, int frameCounter) {
        var chunkPriorities = new Dictionary<ChunkCoord, ChunkPriorityData>();

        foreach(var updatable in updatables) {
            var activeChunks = updatable.GetActiveChunks();
            
            foreach(var coord in activeChunks) {
                if(!chunkPriorities.TryGetValue(coord, out var data)) {
                    var priority = ChunkPriorityManager.CalculatePriority(playerChunk, coord);

                    data = new ChunkPriorityData {
                        Chunk = coord,
                        PlayerChunk = playerChunk,
                        Priority = priority,
                        ShouldUpdateThisFrame = ChunkPriorityManager.ShouldUpdateThisFrame(priority)
                    };

                    chunkPriorities[coord] = data;
                }
            }
        }

        foreach(var updatable in updatables) {
            var config = customConfigs.GetValueOrDefault(updatable, defaultConfig);
            updatable.UpdateChunks(chunkPriorities, config);
        }
    }

    /**
     *
     * Clear
     *
     */
    public void Clear() {
        updatables.Clear();
        customConfigs.Clear();
    }
}