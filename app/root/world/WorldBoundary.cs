/**

    World Boundary main class

    */
namespace App.Root.World;
using App.Root.Chunk;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Player;
using OpenTK.Mathematics;

class WorldBoundary {
    private PlayerController playerController;
    private RigidBody rigidBody;
    private CollisionManager collisionManager;

    private BoundaryObject? boundary; 

    private const float BOUNDARY_MULTIPLIER = 5.0f;
    public const float GLOBAL_MULTIPLIER = BOUNDARY_MULTIPLIER * 2.0f;

    public WorldBoundary(PlayerController playerController, RigidBody rigidBody, CollisionManager collisionManager) {
        this.playerController = playerController;
        this.rigidBody = rigidBody;
        this.collisionManager = collisionManager;

        init();
    }

    // Get Boundary Object
    public BoundaryObject? getBoundaryObject() {
        return boundary;
    }

    // On Stream
    private void onStream(BoundaryObject boundary, object distance) {
        EventStream.set("s-center", (object)boundary.getCenter());
        EventStream.set("s-distance", (object)distance);
    }

    /**
     *
     * Compute
     *
     */
    // Compute Distance
    private float computeDistance() {
        var chunks = EventStream.get<List<ChunkCoord>>("streamed-chunks");
        if(chunks == null || chunks.Count == 0) return 0.0f;

        int minCx = chunks.Min(c => c.cx);
        int minCz = chunks.Min(c => c.cz);

        int maxCx = chunks.Max(c => c.cx);
        int maxCz = chunks.Max(c => c.cz);

        float spanX = (maxCx - minCx + 1) * ChunkCoord.CHUNK_SIZE;
        float spanZ = (maxCz - minCz + 1) * ChunkCoord.CHUNK_SIZE;

        float extent = MathF.Max(spanX, spanZ) / 2.0f;
        
        float val = extent * BOUNDARY_MULTIPLIER;
        return val;
    }

    // Compute Center
    private Vector3 computeCenter() {
        var chunks = EventStream.get<List<ChunkCoord>>("streamed-chunks");
        if(chunks == null || chunks.Count == 0) return Vector3.Zero;

        float sumCx = 0;
        float sumCz = 0;

        foreach(var c in chunks) {
            sumCx += c.cx;
            sumCz += c.cz;
        }

        float avgCx = sumCx / chunks.Count;
        float avgCz = sumCz / chunks.Count;

        float worldX = avgCx * ChunkCoord.CHUNK_SIZE + ChunkCoord.CHUNK_SIZE / 2.0f;
        float worldZ = avgCz * ChunkCoord.CHUNK_SIZE + ChunkCoord.CHUNK_SIZE / 2.0f;

        Vector3 val = new Vector3(worldX, 0.0f, worldZ);
        return val; 
    }

    /**
     * 
     * Apply
     *
     */
    public void apply() {
        Vector3 pos = rigidBody.getPosition();
        
        var boundary = getBoundaryObject();
        if(boundary == null || !boundary.isActive()) return;

        boundary.setCenter(computeCenter());
        
        float distance = computeDistance();
        boundary.setDistance(distance);

        onStream(boundary, distance);

        float minHeight = boundary.getMinHeight();
        float maxHeight = boundary.getMaxHeight();

        if(pos.Y <= minHeight) {
            Vector3 spawn = playerController.setSpawnProps();
            playerController.setPosition(spawn.X, spawn.Y, spawn.Z);

            Console.WriteLine($"[WorldBoundary] spawned at {spawn}");
            return;
        }
        
        if(pos.Y >= maxHeight) {
            pos.Y = maxHeight;
            rigidBody.setPosition(pos);
            rigidBody.setVelocity(new Vector3(
                rigidBody.getVelocity().X,
                0.0f,
                rigidBody.getVelocity().Z
            ));
        }
    }

    /**
     * 
     * Init
     *
     */
    private void init() {
        this.boundary = new BoundaryObject(0.0f);
        collisionManager.addStaticCollider(boundary);
    }
}