/**

    Platform main class

    */
namespace App.Root.World.Platform;
using App.Root.Chunk;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Particle;
using App.Root.Physics;
using App.Root.Player;
using App.Root.Resource;
using App.Root.Utils;
using OpenTK.Mathematics;

[Chunked]
class Platform : WorldHandler {
    private Mesh mesh;
    private CollisionManager collisionManager;
    private PlatformRegistry platformRegistry;
    private PlayerController playerController;

    public const string GRID_ID = "grid";
    private const string MESH = "cube";

    (float x, float y, float z) pos = (0.0f, 0.0f, 0.0f);
    private Vector3 offset = Vector3.Zero;
    
    private const int SIZE_X = 1000;
    private const int SIZE_Y = 1;
    private const int SIZE_Z = 1000;
    private const float SPACING = 1.0f;

    private bool initialized = false;

    private Dictionary<ChunkCoord, List<string>> chunkColliders = new();

    public static Vector3? height {
        get;
        private set;
    }

    public static float? topSurfaceY {
        get;
        private set;
    }
    
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

    /**

        Temporary Meshes to test 
        raycaster for objects

        */
        private int spawnCounter = 0;

        private void spawnMesh(string meshType, Vector3 position, float scale, string texPath, string stackId) {
            string id = $"{meshType}_{spawnCounter++}";

            MeshData data = MeshDataLoader.load(meshType);
            mesh.add(id, data);
            mesh.setPosition(id, position);
            if(scale != 1.0f) mesh.setScale(id, scale);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            MeshInteractionRegistry.getInstance().register(id, State.BREAKABLE, mesh, stackId);
        }

        private void spawnGrid(string meshType, Vector3 origin, int cols, int rows, float scale = 1.0f, float spacing = 1.0f, string texPath = "world/test.jpg") {
            string stackId = $"{meshType}_wall";

            for(int r = 0; r < rows; r++) {
                for(int c = 0; c < cols; c++) {
                    float px = origin.X + c * spacing;
                    float py = origin.Y + r * spacing;
                    spawnMesh(meshType, new Vector3(px, py, origin.Z), scale, texPath, stackId);
                }
            }
        }

        public void set2() {
            string id = "cubic";
            string stackId = "cubic_stack";
            string mesht = "cube";
            MeshData data = MeshDataLoader.load(mesht);
            mesh.add(id, data);
            mesh.setPosition(id, 0.0f, 10.0f, -3.0f);
            mesh.setScale(id, 0.5f);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            string texPath = "world/test.jpg";
            int texId = TextureLoader.load(texPath);
            mesh.setTexture(id, texId, texPath);

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            //collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
            //collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
        
            MeshInteractionRegistry.getInstance().register(
                id,
                State.BREAKABLE,
                mesh,
                PhysicsType.DYNAMIC,
                stackId
            );
        }

        public void set3() {
            string id = "cubic2";
            string mesht = "sphere";
            MeshData data = MeshDataLoader.load(mesht);
            mesh.add(id, data);
            mesh.setPosition(id, 2.0f, 10.0f, -3.0f);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            string texPath = "world/test.jpg";
            int texId = TextureLoader.load(texPath);
            mesh.setTexture(id, texId, texPath);

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            //collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
            //collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
        
            MeshInteractionRegistry.getInstance().register(
                id,
                State.BREAKABLE,
                mesh,
                PhysicsType.DYNAMIC
            );
        }

        public void set4() {
            string id = "dino";
            string path = "dino.obj";

            MeshData data = MeshModelLoader.loadModel(path);
            data.isModel = true;
            data.modelPath = path;
            data.colliderShape = ColliderType.CUBE;

            mesh.add(id, data);
            mesh.setPosition(id, -3.0f, 10.0f, -3.0f);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            string texPath = "mesh/dino.png";
            int texId = TextureLoader.load(texPath);
            mesh.setTexture(id, texId, texPath);

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            //collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
            //collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
        
            MeshInteractionRegistry.getInstance().register(
                id,
                State.BREAKABLE,
                mesh,
                PhysicsType.DYNAMIC,
                meshType: path
            );
        }

        ///
        /// Particles
        /// 
        private int frameCounter = 0;
        private ParticleEntity? particleEntity = null;

        private void emitParticle() {
            ParticleController particleController = mesh.getParticleController()!;
            Random random = new Random();

            Vector3 position = new Vector3(0.0f, 10.0f, -3.0f);
            Vector3 color = new Vector3(1.0f, 1.0f, 1.0f); 
            int amount = 5;
            float size = 0.1f;
            float speed = 0.3f;
            float lifetime = 2.5f;
            Vector3 velNum = new Vector3(5.0f, 5.0f, 5.0f);

            if(particleEntity == null) {
                particleEntity = particleController.emit(
                    position,
                    color,
                    amount,
                    size,
                    speed,
                    lifetime,
                    velNum,
                    () => {
                        return new Vector3(
                            random.NextSingle(),
                            random.NextSingle(),
                            random.NextSingle()
                        );
                    }
                );
            } else {
                particleEntity.set(
                    new Vector3(0.0f, 10.0f, -3.0f),
                    true,
                    () => {
                        return new Vector3(
                            random.NextSingle(),
                            random.NextSingle(),
                            random.NextSingle()
                        );
                    }
                );
            }
        }
    /**
        ****
        ****
        ****

        */
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
        float wx = chunkOrigin.X + x * SPACING;
        float wy = chunkOrigin.Y + y * SPACING;
        float wz = chunkOrigin.Z + z * SPACING;

        if(wx < 0 || wx >= SIZE_X ||
            wy < 0 || wy >= SIZE_Y || 
            wz < 0 || wz >= SIZE_Z) return null;

        return (wx, wy, wz); 
    }

    // Set Platform Props
    private void setPlatformProps(List<string> colliderIds, Vector3 chunkOrigin, ChunkCoord coord) {
        string colliderId = $"{GRID_ID}_{coord.cx}_{coord.cz}";

        Vector3 chunkBoxCenter = new Vector3(
            chunkOrigin.X + ChunkCoord.CHUNK_SIZE * SPACING / 2.0f,
            chunkOrigin.Y + SIZE_Y * SPACING / 2.0f,
            chunkOrigin.Z + ChunkCoord.CHUNK_SIZE * SPACING / 2.0f
        );
        Vector3 chunkBoxHalf = new Vector3(
            ChunkCoord.CHUNK_SIZE * SPACING / 2.0f,
            SIZE_Y * SPACING / 2.0f,
            ChunkCoord.CHUNK_SIZE * SPACING / 2.0f
        );

        collisionManager.addStaticCollider(new StaticObject(
            chunkBoxCenter, chunkBoxHalf.X, chunkBoxHalf.Y, chunkBoxHalf.Z, colliderId
        ));

        colliderIds.Add(colliderId);
    }

    // Set Platform
    private void setPlatform(bool renderMesh = true) {
        if(!initialized) {
            height = null;
            topSurfaceY = null;

            mesh.add(GRID_ID, MESH);
            MeshInteractionRegistry.getInstance().register(
                GRID_ID,
                State.GRID,
                mesh,
                PhysicsType.RECEIVER
            );

            Vector3 size = mesh.getSize(GRID_ID);
            Vector3 half = size / 2.0f;
            topSurfaceY = half.Y;
            height = new Vector3(0, topSurfaceY.Value, 0);

            var renderer = mesh.getMeshRenderer(GRID_ID);
            if(renderer != null) renderer.isInstanced = true;

            initialized = true;
        }

        if(!ContextChunk.hasChunk) return;
        ChunkCoord coord = ContextChunk.current!.Value;
        if(chunkColliders.ContainsKey(coord)) return;

        var colliderIds = new List<string>();
        Vector3 chunkOrigin = coord.ToWorldPosition();

        var positions = new List<Vector3>();

        setPlatformProps(colliderIds, chunkOrigin, coord);

        for(int x = 0; x < ChunkCoord.CHUNK_SIZE; x++) {
            for(int z = 0; z < ChunkCoord.CHUNK_SIZE; z++) {
                for(int y = 0; y < ChunkCoord.CHUNK_SIZE; y++) {
                    var bounds = setBounds(chunkOrigin, x, y, z);
                    if(bounds == null) continue;
                    
                    positions.Add(new Vector3(bounds.Value.wx, bounds.Value.wy, bounds.Value.wz));
                }
            }
        }

        if(positions.Count == 0) return;
        chunkColliders[coord] = colliderIds;
        //Console.WriteLine($"[Platform] chunk {coord} generated with {positions.Count} positions");
        ChunkPositions.Add(GRID_ID, coord, positions);

        if(renderMesh) {
            setMesh(positions);
            //Console.WriteLine($"Platform draw calls: 1 (instanced {positions.Count} cubes)");
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
        }

        ChunkPositions.Remove(GRID_ID, coord);
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        frameCounter++;

        if(frameCounter % 10 == 0) {
            //emitParticle();
        }

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