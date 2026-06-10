namespace App.Root.Chunk;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.World;
using OpenTK.Mathematics;
using System.Reflection;

/**

    Chunk Handler class

    */
abstract class ChunkHandler {
    public virtual void onChunkLoad(ChunkCoord coord, ChunkData data) {}
    public virtual void onChunkUnload(ChunkCoord coord) {}
}

/**

    Chunk Manager main class

    */
class ChunkManager {
    private const int RENDER_DISTANCE = 8;
    private const int MAX_LOAD_PER_FRAME = 2;

    private Window window;
    private Camera camera;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private PlayerController playerController;

    private Dictionary<ChunkCoord, ChunkData> chunkDataMap = new();
    private HashSet<ChunkCoord> activeChunks = new();

    private Queue<ChunkCoord> loadQueue = new();
    private Queue<ChunkCoord> unloadQueue = new();
    private List<ChunkHandler> chunkedHandlers = new();

    private ChunkCoord lastPlayerChunk = new ChunkCoord(int.MaxValue, 0, int.MaxValue);

    private bool initialized = false;

    public ChunkManager(
        Window window, 
        Camera camera, 
        Mesh mesh, 
        CollisionManager collisionManager, 
        PlayerController playerController
    ) {
        this.window = window;
        this.camera = camera;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.playerController = playerController;
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
                if(dist <= RENDER_DISTANCE) shouldBeActive.Add(coord);
            }
        }

        queueLoads(shouldBeActive);
        queueUnloads(shouldBeActive);
    }

    // Queue Loads
    private void queueLoads(HashSet<ChunkCoord> shouldBeActive) {
        foreach(var coord in shouldBeActive) {
            if(!activeChunks.Contains(coord)) {
                loadQueue.Enqueue(coord);
            }
        }
    }

    // Queue Unloads
    private void queueUnloads(HashSet<ChunkCoord> shouldBeActive) {
        foreach(var coord in activeChunks) {
            if(!shouldBeActive.Contains(coord)) {
                unloadQueue.Enqueue(coord);
            }
        }
    }

    // Get Active Chunjs
    public HashSet<ChunkCoord> getActiveChunks() {
        return activeChunks;
    }

    // Get Chunk Data
    public ChunkData? getChunkData(ChunkCoord coord) {
        ChunkData? val = chunkDataMap.TryGetValue(coord, out var data) ? data : null;
        return val;
    }

    /**
     * 
     * Process Queues
     *
     */
    private void processQueues() {
        while(unloadQueue.Count > 0) {
            var coord = unloadQueue.Dequeue();
            unloadChunk(coord);
        }

        int loaded = 0;
        while(loadQueue.Count > 0 && loaded < MAX_LOAD_PER_FRAME) {
            var coord = loadQueue.Dequeue();
            if(!activeChunks.Contains(coord)) {
                loadChunk(coord);
                loaded++;
            }
        }
    }

    /**
     * 
     * Load Chunk
     *
     */
    private void loadChunk(ChunkCoord coord) {
        if(!chunkDataMap.TryGetValue(coord, out var data)) {
            data = ChunkGenerator.generate(coord);
            chunkDataMap[coord] = data;
        }

        activeChunks.Add(coord);

        window.queueOnRenderThread(() => {
            foreach(var handler in chunkedHandlers) {
                handler.onChunkLoad(coord, data);
            }
        });

        Console.WriteLine($"[ChunkManager] Loaded {coord}");
    }

    /**
     * 
     * Unload Chunk
     *
     */
    private void unloadChunk(ChunkCoord coord) {
        if(!activeChunks.Contains(coord)) return;
        activeChunks.Remove(coord);

        window.queueOnRenderThread(() => {
            foreach(var handler in chunkedHandlers) {
                handler.onChunkUnload(coord);
            }
        });

        Console.WriteLine($"[ChunkManager] Unloaded {coord}");
    }

    /**
     * 
     * Register Handlers
     *
     */
    public void registerHandlers(List<ChunkHandler> handlers) {
        chunkedHandlers.Clear();
        
        foreach(var handler in handlers) {
            var attr = handler.GetType().GetCustomAttribute<ChunkedAttribute>();
            if(attr != null) {
                chunkedHandlers.Add(handler);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[ChunkManager] Registered chunked handler: {handler.GetType().Name}");
                Console.ResetColor();
            }
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
            Console.WriteLine($"[ChunkManager] Initialized with {chunkDataMap.Count} saved chunks.");
        }
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        if(!initialized) return;

        Vector3 playerPos = playerController.getCamera().getPosition();
        ChunkCoord playerChunk = ChunkCoord.FromWorldPosition(playerPos.X, playerPos.Y, playerPos.Z);

        if(playerChunk != lastPlayerChunk) {
            lastPlayerChunk = playerChunk;
            recalculateChunks(playerChunk);
        }

        processQueues();
    }
}