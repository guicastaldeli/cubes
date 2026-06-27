namespace App.Root.World.Platform;
using App.Root.Chunk;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Physics;
using App.Root.Player;
using App.Root.Utils;
using OpenTK.Mathematics;

/**

    Platform main class

    */
[Chunked]
class Platform : WorldHandler {
    private Mesh mesh;
    private CollisionManager collisionManager;
    private PlatformRegistry platformRegistry;
    private PlayerController playerController;

    public const string GRID_ID = "grid";
    private const string MESH = "cube";

    private const int SIZE_X = ChunkCoord.CHUNK_SIZE;
    private const int SIZE_Y = ChunkCoord.CHUNK_SIZE;
    private const int SIZE_Z = ChunkCoord.CHUNK_SIZE;
    private const float SPACING = 1.0f;

    private Dictionary<ChunkCoord, List<string>> chunkColliders = new();
    private HashSet<ChunkCoord> allGeneratedChunks = new();
    private Vector3 offset = Vector3.Zero;

    private bool initialized = false;

    private const bool DEBUG_EXPANSION = true;

    public static float? Top { get; private set; }
    
    public Platform(
        [Inject] Window window,
        [Inject] Mesh mesh, 
        [Inject] CollisionManager collisionManager, 
        [Inject] PlayerController playerController
    ) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.playerController = playerController;
        this.platformRegistry = new PlatformRegistry(window, mesh, collisionManager, this, playerController);
    
        init();
    }

    // Set Client
    public void setClient() {
        if(initialized) return;
        setPlatform(renderMesh: false);
    }

    // Height
    public Vector3 getHeight() {
        Vector3 meshSize = mesh.getSize(GRID_ID);
        float topY = offset.Y + (SIZE_Y * SPACING) + (meshSize.Y / 2.0f);
        Vector3 res = new Vector3(offset.X, topY, offset.Z); 
        return res;
    }

    // Calculate Top
    private float calculateTop() {
        var topBounds = setBounds(Vector3.Zero, 0, SIZE_Z - 1, 0);
        if(topBounds.HasValue) return topBounds.Value.wy + SPACING / 2.0f;

        float centerOffsetY = (ChunkCoord.CHUNK_SIZE - SIZE_Z) / 2.0f;
        
        float val = centerOffsetY + (SIZE_Y - 1) * SPACING + SPACING / 2.0f;
        return val;
    }

    /**
     * 
     * On Stream
     *
     */
    private void onStream() {
        if(Top.HasValue) EventStream.set("stream-top", (object)Top.Value);
        EventStream.set("streamed-chunks", (object)new List<ChunkCoord>(allGeneratedChunks));
    }

    /**
     * 
     * Set
     *
     */
    // Set Mesh
    private void setMesh(List<Vector3> positions) {
        var renderer = mesh.getMeshRenderer(GRID_ID);
        if(renderer != null) {
            renderer.isInstanced = true;
            renderer.setInstancePositions(positions);
        }
    }

    // Set Bounds
    private (float wx, float wy, float wz)? setBounds(Vector3 chunkOrigin, int x, int y, int z) {
        float centerOffsetX = (ChunkCoord.CHUNK_SIZE - SIZE_X) / 2.0f;
        float centerOffsetY = (ChunkCoord.CHUNK_SIZE - SIZE_Y) / 2.0f;
        float centerOffsetZ = (ChunkCoord.CHUNK_SIZE - SIZE_Z) / 2.0f;

        float wx = chunkOrigin.X + centerOffsetX + x * SPACING;
        float wy = chunkOrigin.Y + centerOffsetY + y * SPACING;
        float wz = chunkOrigin.Z + centerOffsetZ + z * SPACING;

        return (wx, wy, wz);
    }

    // Set Platform Collider
    private void setPlatformCollider(List<string> colliderIds, ChunkCoord coord, List<Vector3> positions) {
        if(positions.Count == 0) return;

        string colliderId = $"{GRID_ID}_{coord.cx}_{coord.cz}";

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        foreach(var pos in positions) {
            if(pos.X < minX) minX = pos.X;
            if(pos.Y < minY) minY = pos.Y;
            if(pos.Z < minZ) minZ = pos.Z;
            if(pos.X > maxX) maxX = pos.X;
            if(pos.Y > maxY) maxY = pos.Y;
            if(pos.Z > maxZ) maxZ = pos.Z;
        }

        Vector3 boxCenter = new Vector3(
            (minX + maxX) / 2.0f,
            (minY + maxY) / 2.0f,
            (minZ + maxZ) / 2.0f
        );
        Vector3 boxHalf = new Vector3(
            (maxX - minX) / 2.0f + SPACING / 2.0f,
            (maxY - minY) / 2.0f + SPACING / 2.0f,
            (maxZ - minZ) / 2.0f + SPACING / 2.0f
        );

        collisionManager.addStaticCollider(new StaticObject(
            boxCenter,
            boxHalf.X, boxHalf.Y, boxHalf.Z,
            colliderId
        ));

        colliderIds.Add(colliderId);
    }

    // Set Platform
    private void setPlatform(bool renderMesh = true) {
        if(!initialized) {
            Top = null;

            mesh.add(GRID_ID, MESH);
            MeshInteractionRegistry.getInstance().register(
                GRID_ID,
                State.GRID,
                mesh,
                PhysicsType.RECEIVER
            );

            Top = calculateTop();

            EventStream.set("stream-top", (object)Top.Value);

            var renderer = mesh.getMeshRenderer(GRID_ID);
            if(renderer != null) renderer.isInstanced = true;

            initialized = true;
        }

        if(DEBUG_EXPANSION && chunkColliders.Count > 0) return;

        if(!ContextChunk.hasChunk) return;
        ChunkCoord coord = ContextChunk.current!.Value;
        if(chunkColliders.ContainsKey(coord)) return;

        var colliderIds = new List<string>();
        Vector3 chunkOrigin = coord.ToWorldPosition();

        var positions = new List<Vector3>();

        for(int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Y; z++) {
                for(int y = 0; y < SIZE_Z; y++) {
                    var bounds = setBounds(chunkOrigin, x, y, z);
                    if(bounds == null) continue;
                    
                    positions.Add(new Vector3(bounds.Value.wx, bounds.Value.wy, bounds.Value.wz));
                }
            }
        }

        if(positions.Count == 0) return;

        setPlatformCollider(colliderIds, coord, positions);
        chunkColliders[coord] = colliderIds;
        allGeneratedChunks.Add(coord);
        ChunkPositions.Add(GRID_ID, coord, positions);

        onStream();

        if(renderMesh) {
            setMesh(positions);
        } else {
            mesh.remove(GRID_ID);
        }
    }

    /**
     * 
     * Merge
     *
     */
    private void merge() {
       // Console.WriteLine($"[Platform] merge() called - IsUsed: {ChunkPositions.IsUsed(GRID_ID)}");
        
        if(ChunkPositions.IsUsed(GRID_ID)) {
            var merged = ChunkPositions.GetMerged(GRID_ID);
            //Console.WriteLine($"[Platform] uploading {merged.Count} positions to GPU");
            mesh.getMeshRenderer(GRID_ID)?.setInstancePositions(merged);
            ChunkPositions.ClearUsed(GRID_ID);
        }
    }

    /**
     * 
     * Load
     *
     */
    private void load() {
        if(!initialized) {
            /*
            platformRegistry.render();
            set2();
            set3();
            set4();

            spawnGrid("cube", new Vector3(4f, 3f, -3f), 5, 3);
            */
        }

        setPlatform();
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        load();
        merge();
    }

    /**
     * 
     * Unrender
     *
     */
    public override void unrender() {
        if(!ContextChunk.hasChunk) return;
        ChunkCoord coord = ContextChunk.current!.Value;

        if(chunkColliders.TryGetValue(coord, out var colliderIds)) {
           foreach(var id in colliderIds) collisionManager.removeCollider(id);
           collisionManager.processRemovals();
           chunkColliders.Remove(coord); 

           onStream();
        }

        ChunkPositions.Remove(GRID_ID, coord);
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        platformRegistry.update();
    }

    /**
     * 
     * Init
     *
     */
    private void init() {
        platformRegistry.init();
    }
}