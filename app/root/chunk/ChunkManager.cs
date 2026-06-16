namespace App.Root.Chunk;
using App.Root.Mesh;
using App.Root.Player;
using OpenTK.Mathematics;
using System.Reflection;

/**

    Chunk Handler class

    */
abstract class ChunkHandler {
    public virtual void render() {}
    public virtual void unrender() {}
    public virtual void update() {}
}

/**

    Chunk Positions class

    */
static class ChunkPositions {
    private static Dictionary<string, Dictionary<ChunkCoord, List<Vector3>>> handlerPositions = new();
    private static Dictionary<string, bool> handlerUsed = new();

    // Is Used
    public static bool IsUsed(string handlerId) {
        bool val = handlerUsed.TryGetValue(handlerId, out var used) && used;
        return val;
    }

    // Clear Used
    public static void ClearUsed(string handlerId) {
        handlerUsed[handlerId] = false;
    }

    /**
     *
     * Add
     *
     */
    public static void Add(string handlerId, ChunkCoord coord, List<Vector3> positions) {
        if(!handlerPositions.ContainsKey(handlerId)) handlerPositions[handlerId] = new();
        handlerPositions[handlerId][coord] = positions;
        handlerUsed[handlerId] = true;
    }

    /**
     *
     * Remove
     *
     */
    public static void Remove(string handlerId, ChunkCoord coord) {
        if(handlerPositions.TryGetValue(handlerId, out var map)) map.Remove(coord);
        handlerUsed[handlerId] = true;
    }

    /**
     *
     * Get Merged
     *
     */
    public static List<Vector3> GetMerged(string handlerId) {
        if(!handlerPositions.TryGetValue(handlerId, out var map)) return new();
        
        List<Vector3> val = map.Values.SelectMany(p => p).ToList();
        return val;
    }
} 

/**

    Chunk Manager main class

    */
class ChunkManager {
    private const int RENDER_DISTANCE = 2;
    private const int MAX_LOAD_PER_FRAME = 2;

    private Window window;
    private Camera camera;
    private Mesh mesh;
    private PlayerController playerController;

    private List<ChunkHandler> chunkedHandlers = new();
    private List<ChunkHandler> globalHandlers = new();

    private Dictionary<ChunkCoord, ChunkData> chunkDataMap = new();
    private HashSet<ChunkCoord> activeChunks = new();
    private Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks = new();

    private Queue<ChunkCoord> loadQueue = new();
    private Queue<ChunkCoord> unloadQueue = new();

    private ChunkCoord lastPlayerChunk = new ChunkCoord(int.MaxValue, 0, int.MaxValue);

    private bool initialized = false;

    public ChunkManager(Window window, Camera camera, Mesh mesh, PlayerController playerController) {
        this.window = window;
        this.camera = camera;
        this.mesh = mesh;
        this.playerController = playerController;
    }

    // Get Active Chunks
    public HashSet<ChunkCoord> getActiveChunks() {
        return activeChunks;
    }

    // Get Chunk Data
    public ChunkData? getChunkData(ChunkCoord coord) {
        ChunkData? val = chunkDataMap.TryGetValue(coord, out var data) ? data : null;
        return val;
    }

    // Recalculate Chunks
    private void recalculateChunks(ChunkCoord playerChunk) {
        //Console.WriteLine($"[ChunkManager] recalculating chunks for player at {playerChunk}");
        var shouldBeActive = new HashSet<ChunkCoord>();

        for(int dx = -RENDER_DISTANCE; dx <= RENDER_DISTANCE; dx++) {
            for(int dz = -RENDER_DISTANCE; dz <= RENDER_DISTANCE; dz++) {
                var coord = new ChunkCoord(
                    playerChunk.cx + dx,
                    0,
                    playerChunk.cz + dz
                );

                float dist = MathF.Sqrt(dx * dx + dz * dz);
                if(dist <= RENDER_DISTANCE) shouldBeActive.Add(coord);
            }
        }

        queueLoads(shouldBeActive);
        queueUnloads(shouldBeActive);
    }

    // Queue Loads
    private void queueLoads(HashSet<ChunkCoord> shouldBeActive) {
        foreach(var coord in shouldBeActive) {
            if(!activeChunks.Contains(coord)) loadQueue.Enqueue(coord);
        }
    }

    // Queue Unloads
    private void queueUnloads(HashSet<ChunkCoord> shouldBeActive) {
        foreach(var coord in activeChunks) {
            if(!shouldBeActive.Contains(coord)) unloadQueue.Enqueue(coord);
        }
    }

    /**
     * 
     * Process Queues
     *
     */
    // Process Load Queue
    private void processLoadQueue() {
        int loaded = 0;
        while(loadQueue.Count > 0 && loaded < MAX_LOAD_PER_FRAME) {
            var coord = loadQueue.Dequeue();
            if(!activeChunks.Contains(coord)) {
                loadChunk(coord);
                loaded++;
            }
        }
    }
    
    // Process Unload Queue
    private void processUnloadQueue() {
        while(unloadQueue.Count > 0) {
            var coord = unloadQueue.Dequeue();
            if(activeChunks.Contains(coord)) {
                window.queueOnRenderThread(() => unloadChunk(coord));
            }
        }
    }

    /**
     * 
     * Load Chunk
     *
     */
    private void loadChunk(ChunkCoord coord) {
        Console.WriteLine($"[ChunkManager] loadChunk called for {coord}");
        //Console.WriteLine($"[TRACE] StackTrace:\n{Environment.StackTrace}");
        if(!chunkDataMap.TryGetValue(coord, out var data)) {
            data = ChunkGenerator.generate(coord);
            chunkDataMap[coord] = data;
        }

        activeChunks.Add(coord);

        foreach(var handler in chunkedHandlers) {
            ContextChunk.Set(coord);
            handler.render();

            ContextChunk.Clear();
            
            handlerActiveChunks[handler].Add(coord);
        }

        //Console.WriteLine($"[ChunkManager] Loaded {coord}");
    }

    /**
     * 
     * Unload Chunk
     *
     */
    private void unloadChunk(ChunkCoord coord) {
        if(!activeChunks.Contains(coord)) return;
        activeChunks.Remove(coord);

        foreach(var handler in chunkedHandlers) {
            ContextChunk.Set(coord);
            handler.unrender();

            ContextChunk.Clear();
            handlerActiveChunks[handler].Remove(coord);
        }

        //Console.WriteLine($"[ChunkManager] Unloaded {coord}");
    }

    /**
     * 
     * Register Handlers
     *
     */
    public void registerHandlers(List<ChunkHandler> handlers) {
        chunkedHandlers.Clear();
        globalHandlers.Clear();
        handlerActiveChunks.Clear();
        
        foreach(var handler in handlers) {
            ChunkedAttribute.R(handler, chunkedHandlers, handlerActiveChunks);
            IChunkedAttribute.R(handler, globalHandlers);
        }
    }

    /**
     * 
     * Save
     *
     */
    public void save() {
        SerializeChunk.save(chunkDataMap);
    }

    /**
     * 
     * Render
     *
     */
    public void render() {
        if(!initialized) {
            chunkDataMap = SerializeChunk.load();
            initialized = true;
            //Console.WriteLine($"[ChunkManager] Initialized with {chunkDataMap.Count} saved chunks.");
        }

        foreach(var handler in globalHandlers) {
            handler.render();
        }

        processLoadQueue();
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        if(!initialized) return;

        foreach(var handler in globalHandlers) {
            handler.update();
        }
        foreach(var handler in chunkedHandlers) {
            if(handlerActiveChunks[handler].Count > 0) {
                handler.update();
            }
        }

        Vector3 playerPos = playerController.getCamera().getPosition();
        ChunkCoord playerChunk = ChunkCoord.FromWorldPosition(playerPos.X, playerPos.Y, playerPos.Z);

        if(playerChunk != lastPlayerChunk) {
            lastPlayerChunk = playerChunk;
            recalculateChunks(playerChunk);
        }

        processUnloadQueue();
        processLoadQueue();
    }
}