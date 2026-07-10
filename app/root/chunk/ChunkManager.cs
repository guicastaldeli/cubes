namespace App.Root.Chunk;
using App.Root.Mesh;
using App.Root.Player;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

/**

    Chunk Handler class

    */
abstract class ChunkHandler : IChunkUpdatable {
    [Scan] public virtual void render() => Route();
    [Scan] public virtual void unrender() => Route();
    [Scan] public virtual void update() => Route();

    protected virtual void Route([CallerMemberName] string? name = null) {}

    public virtual HashSet<ChunkCoord> GetActiveChunks() {
        return new HashSet<ChunkCoord>();
    }

    public virtual void UpdateChunks(Dictionary<ChunkCoord, ChunkPriorityData> priorityData, ChunkPriorityConfig config) {
        foreach(var (coord, data) in priorityData) {
            if(data.ShouldUpdateThisFrame) {
                ContextChunk.Set(coord);
                update();
                ContextChunk.Clear();
            }
        }
    }
}

/**

    Chunk Positions class

    */
static class ChunkPositions {
    private static Dictionary<string, Dictionary<ChunkCoord, List<Vector3>>> handlerPositions = new();
    private static Dictionary<string, bool> handlerUsed = new();
    private static Dictionary<string, List<Vector3>> mergedCache = new();

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
        mergedCache.Remove(handlerId);
    }

    /**
     *
     * Remove
     *
     */
    public static void Remove(string handlerId, ChunkCoord coord) {
        if(handlerPositions.TryGetValue(handlerId, out var map)) map.Remove(coord);
        handlerUsed[handlerId] = true;
        mergedCache.Remove(handlerId);
    }

    /**
     *
     * Get Merged
     *
     */
    public static List<Vector3> GetMerged(string handlerId) {
        if(mergedCache.TryGetValue(handlerId, out var cached)) return cached;
        if(!handlerPositions.TryGetValue(handlerId, out var map)) return new();

        var merged = map.Values.SelectMany(p => p).ToList();
        mergedCache[handlerId] = merged;
        return merged;
    }
} 

/**

    Chunk Manager main class

    */
[ManagedState]
class ChunkManager {
    public static bool USE_FILE { get; set; } = false;

    public const int RENDER_DISTANCE = 8;
    private const int MAX_LOAD_PER_FRAME = 2;
    private const int EX_MAX_LOAD_PER_FRAME = 4;
    private const float SAVE_INTERVAL = 10.0f;

    private Tick tick;
    private Window window;
    [SkipReset] private PlayerController playerController = null!;

    private List<ChunkHandler> chunkedHandlers = new();
    private List<ChunkHandler> globalHandlers = new();

    private Dictionary<ChunkCoord, ChunkData> chunkDataMap = new();
    private HashSet<ChunkCoord> activeChunks = new();
    private Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks = new();

    private Queue<ChunkCoord> loadQueue = new();
    private Queue<ChunkCoord> unloadQueue = new();

    private ChunkCoord lastPlayerChunk = new ChunkCoord(int.MaxValue, 0, int.MaxValue);

    private ChunkProcessor chunkProcessor = new ChunkProcessor();

    private Queue<(ChunkCoord coord, Task<ChunkData> task)> loadingTasks = new();
    private HashSet<ChunkCoord> loadingChunks = new();
    private bool isLoading = false;

    private HashSet<ChunkCoord> usedChunks = new();
    private float saveTimer = 0;

    private bool initialized = false;
    private bool readyEmitted = false;

    public ChunkManager(Window window, Tick tick) {
        this.window = window;
        this.tick = tick;

        StateManager.Register(this);

        Console.WriteLine($"ChunkManager -- initialized!! Use file??: {USE_FILE}");
    }

    // Set Player Controller
    public void setPlayerController(PlayerController playerController) {
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
        var shouldBeActive = new HashSet<ChunkCoord>();

        for(int dx = -RENDER_DISTANCE; dx <= RENDER_DISTANCE; dx++) {
            for(int dz = -RENDER_DISTANCE; dz <= RENDER_DISTANCE; dz++) {
                var coord = new ChunkCoord(
                    playerChunk.cx + dx,
                    0,
                    playerChunk.cz + dz
                );

                float dist = MathF.Sqrt(dx * dx + dz * dz);
                if(dist <= RENDER_DISTANCE) {
                    shouldBeActive.Add(coord);

                    if(!activeChunks.Contains(coord) &&
                        chunkDataMap.ContainsKey(coord) &&
                        !loadQueue.Contains(coord)) {
                        loadQueue.Enqueue(coord);
                    }
                }
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

    // Check Ready
    private void checkReady() {
        if(readyEmitted) return;
        if(loadQueue.Count > 0) return;
        if(activeChunks.Count == 0) return;

        readyEmitted = true;
        Console.WriteLine($"[ChunkManager] {activeChunks.Count} chunks loaded — emitting scene-ready");
        EventStream.set("chunk-ready", true);
    }

    // Mark Used
    private void markUsed(ChunkCoord coord) {
        usedChunks.Add(coord);
    }

    /**
     * 
     * Process Queues
     *
     */
    // Process Load Queue
    private void processLoadQueue() {
        int limit = loadQueue.Count > 0 && chunkDataMap.ContainsKey(loadQueue.Peek()) ? EX_MAX_LOAD_PER_FRAME : MAX_LOAD_PER_FRAME;
        
        int loaded = 0;
        while(loadQueue.Count > 0 && loaded < limit) {
            var coord = loadQueue.Dequeue();
            if(!activeChunks.Contains(coord) && !loadingChunks.Contains(coord)) {
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
        if(chunkDataMap.TryGetValue(coord, out var existingData)) {
            activeChunks.Add(coord);

            foreach(var handler in chunkedHandlers) {
                ContextChunk.Set(coord);
                handler.render();
                ContextChunk.Clear();
                handlerActiveChunks[handler].Add(coord);
            }

            loadingChunks.Remove(coord);
            return;
        }

        isLoading = true;
        loadingChunks.Add(coord);

        Task.Run(() => {
            var data = ChunkGenerator.generate(coord);
            return data;
        }).ContinueWith(task => {
            window.queueOnRenderThread(() => {
                if(!chunkDataMap.TryGetValue(coord, out var existing)) {
                    chunkDataMap[coord] = task.Result;
                    activeChunks.Add(coord);
                    markUsed(coord);

                    foreach(var handler in chunkedHandlers) {
                        ContextChunk.Set(coord);
                        handler.render();
                        ContextChunk.Clear();
                        handlerActiveChunks[handler].Add(coord);
                    }
                }

                loadingChunks.Remove(coord);
                isLoading = false;
            });
        });
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

        loadingChunks.Remove(coord);
        if(chunkDataMap.ContainsKey(coord)) chunkDataMap.Remove(coord);
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
            Scanner.Scan(handler, chunkedHandlers, globalHandlers, handlerActiveChunks);
        }
    }

    /**
     * 
     * Save
     *
     */
    public void save() {
        if(!USE_FILE) return;

        Console.WriteLine("[ChunkManager] Saving all chunks...");
        SerializeChunk.save(chunkDataMap);
        usedChunks.Clear();
    }

    public void savedUsedChunks() {
        if(!USE_FILE) return;
        if(usedChunks.Count == 0) return;

        //Console.WriteLine($"[ChunkManager] Saving {usedChunks.Count} used chunks...");

        try {
            /*var allChunks = SerializeChunk.load();

            int saved = 0;
            foreach(var coord in usedChunks) {
                if(chunkDataMap.TryGetValue(coord, out var data)) {
                    allChunks![coord] = data;
                    saved++;
                }
            }*/

            SerializeChunk.save(chunkDataMap);
            usedChunks.Clear();

            //Console.WriteLine($"[ChunkManager] Saved {saved} chunks successfully");
        } catch(Exception err) {
            Console.Error.WriteLine($"[ChunkManager] Failed to save used chunks: {err.Message}");
        }
    }

    /**
     * 
     * Render
     *
     */
    public void render() {
        if(!initialized) {
            //Console.WriteLine($"[ChunkManager] render() — loading from file, chunkDataMap: {chunkDataMap.Count}");
            chunkDataMap = SerializeChunk.load()!;
            initialized = true;
           
            if(chunkDataMap.Count > 0) {
                foreach(var coord in chunkDataMap.Keys) {
                    loadQueue.Enqueue(coord);
                }
            }
        }
        //Console.WriteLine($"[ChunkManager] render() tick — loadQueue: {loadQueue.Count}, active: {activeChunks.Count}");

        foreach(var handler in globalHandlers) {
            handler.render();
        }

        processLoadQueue();
        checkReady();
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        if(!initialized) return;

        float deltaTime = tick.getDeltaTime();

        ChunkPriorityManager.IncrementFrame();

        foreach(var handler in globalHandlers) {
            handler.update();
        }
        foreach(var handler in chunkedHandlers) {
            if(handlerActiveChunks[handler].Count > 0) {
                handler.update();
            }
        }

        Vector3 playerPos = playerController.getPosition();
        ChunkCoord playerChunk = ChunkCoord.FromWorldPosition(playerPos.X, playerPos.Y, playerPos.Z);

        chunkProcessor.Process(playerChunk, ChunkPriorityManager.FrameCounter);
        
        if(playerChunk != lastPlayerChunk) {
            lastPlayerChunk = playerChunk;
            recalculateChunks(playerChunk);
        }

        processUnloadQueue();
        processLoadQueue();

        checkReady();
 
        saveTimer += deltaTime;
        if(saveTimer >= SAVE_INTERVAL) {
            saveTimer = 0;
            savedUsedChunks();
        }
    }

    /**
     * 
     * Register Updatable
     *
     */
    public void RegisterUpdatable(IChunkUpdatable updatable, ChunkPriorityConfig? customConfig = null) {
        chunkProcessor.Register(updatable, customConfig);
    }

    /**
     * 
     * Unregister Updatable
     *
     */
    public void UnregisterUpdatable(IChunkUpdatable updatable) {
        chunkProcessor.Unregister(updatable);
    }
}