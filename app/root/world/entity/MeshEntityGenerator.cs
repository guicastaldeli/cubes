/**

    Mesh Entity Generator to handle
    main controller and generation.

    */
namespace App.Root.World.Entity;
using App.Root.Chunk;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.Utils;
using OpenTK.Mathematics;
using NLua;

/**

    Loader Setter Helper

    */
class Setter {
    /**
     * 
     * Set
     *
     */
    public static void set(LuaTable entities, Dictionary<string, MeshData> res) {
        foreach(var key in entities.Keys) {
            if(entities[key] is not LuaTable entry) continue;

            string? id = entry["id"] as string;
            string? loader = entry["loader"] as string;
            string? tex = entry["tex"] as string;
            if(id == null || loader == null) continue;

            MeshData content = loader switch {
                "data" => MeshDataLoader.load(id),
                "model" => MeshModelLoader.loadModel(id),
                _ => throw new Exception($"Unknown loader '{loader}' for entity '{id}'")
            };

            if(tex != null) content.texPath = tex;

            res[id] = content;
        }
    }
}

/**

    Mesh Entity Generator main class.

    */
[Chunked]
[ManagedState]
class MeshEntityGenerator : WorldHandler, IChunkUpdatable {
    private static ChunkPriorityConfig ChunkPriority => new ChunkPriorityConfig {
        HighDistance = 3.0f,
        MediumDistance = 6.0f,
        LowDistance = 10.0f,
        MaxDistance = 15.0f,

        HighEntityRatio = 1.0f,
        MediumEntityRatio = 0.5f,
        LowEntityRatio = 0.25f,
        VeryLowEntityRatio = 0.1f
    };

    private static readonly string DATA_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/entity/MeshEntity.lua");
    
    private Tick tick;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private PlayerController playerController;
    private ChunkManager chunkManager;

    private MeshEntitySpawner entitySpawner;

    private Queue<string> generationQueue = new();
    private Queue<ChunkCoord> pendingGeneration = new();
    private HashSet<ChunkCoord> pendingGenerationSet = new();
    private static Dictionary<string, MeshData>? cachedMeshTypes = null;

    private HashSet<ChunkCoord> activeChunks = new();
    private HashSet<ChunkCoord> seenChunks = new();
    private HashSet<ChunkCoord> generatedChunks = new();
    private ChunkPriorityConfig priorityConfig = ChunkPriority;
    private Vector3 lastPlayerPosition;

    private Dictionary<string, EntityProps> entityPropsById = new();

    [Poolable("gen_entity_props", typeof(PoolableDictionary<string, EntityProps>), InitialSize = 16, MaxSize = 64)] private PoolableDictionary<string, EntityProps> entityProps = null!;
    [Poolable("gen_entity_instances", typeof(PoolableDictionary<string, PoolableList<Instance>>), InitialSize = 16, MaxSize = 64)] private PoolableDictionary<string, PoolableList<Instance>> entityInstances = null!;
    [Poolable("gen_by_mesh", typeof(PoolableDictionary<string, PoolableList<Instance>>), InitialSize = 16, MaxSize = 64)] private PoolableDictionary<string, PoolableList<Instance>> byMeshType = null!;

    private bool initialized = false;

    public MeshEntityGenerator(
        [Inject] Tick tick, 
        [Inject] Mesh mesh, 
        [Inject] CollisionManager collisionManager,
        [Inject] PlayerController playerController,
        [Inject] ChunkManager chunkManager
    ) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.playerController = playerController;
        this.chunkManager = chunkManager;
    
        this.entitySpawner = new MeshEntitySpawner(tick, mesh, collisionManager);

        PoolInjector.Inject(this);
    
        chunkManager.RegisterUpdatable(this, ChunkPriority);
        
        StateManager.Register(this);
    }

    // Get Active Chunks
    public override HashSet<ChunkCoord> GetActiveChunks() {
        return activeChunks;
    }

    /**
     * 
     * Load
     *
     */
    public static Dictionary<string, MeshData> load() {
        if(cachedMeshTypes != null) return cachedMeshTypes;

        using Lua data = new Lua();

        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        
        data.DoFile(DATA_FILE);
        
        Directory.SetCurrentDirectory(originalDir);
        
        if(data["Entities"] is not LuaTable entities) {
            throw new Exception("MeshEntity.lua err!");
        }

        var res = new Dictionary<string, MeshData>();
        Setter.set(entities, res);

        cachedMeshTypes = res;
        return res;
    }

    /**
     * 
     * Generate
     *
     */
    private void generateSource(EntityProps entity, MeshData data, ChunkCoord spawnChunk, Vector3 boundaryCenter) {
        MeshData meshData = MeshEntityFactory.clone(data);
        meshData.isEntity = 1;
        meshData.entityType = "mesh";

        if(!mesh.hasMesh(entity.MeshType)) {
            mesh.add(entity.MeshType, meshData);
            mesh.setScale(entity.MeshType, entity.Scale);
            mesh.setColor(entity.MeshType, entity.Color);
            mesh.setRotationMatrix(entity.MeshType, RotationEntity.R(entity));
            if(entity.TexId.HasValue && entity.TexId > 0) mesh.setTexture(entity.MeshType, entity.TexId.Value, entity.Tex!);

            var renderer = mesh.getMeshRenderer(entity.MeshType);
            if(renderer != null) {
                renderer.isInstanced = true;
                renderer.isInteractive = true;
            }
        }
        
        entitySpawner.render(entity, spawnChunk, boundaryCenter);
    }

    private void generate(Dictionary<string, MeshData> meshTypes, ChunkCoord spawnChunk, Vector3 boundaryCenter, bool setInitialized = false) {
        entityProps.Clear();
        entityInstances.Clear();
        byMeshType.Clear();

        foreach(var (type, data) in meshTypes) {
            foreach(var entity in MeshEntityFactory.generate(data, type)) {
                generateSource(entity, data, spawnChunk, boundaryCenter);
                entityProps[entity.Id] = entity;
                entityPropsById[entity.Id] = entity;
                
                var instances = entitySpawner.getInstances(entity.Id);
                entityInstances[entity.Id] = instances;
                entitySpawner.registerMeshType(entity.Id, entity.MeshType);

                if(!byMeshType.ContainsKey(entity.MeshType)) byMeshType[entity.MeshType] = new PoolableList<Instance>();
                byMeshType[entity.MeshType].AddRange(instances);
            }
        }

        MeshEntityFactory.setEvent(MeshEntityCollider.colliderIds, entityProps, entityInstances);
        if(setInitialized) initialized = true;
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        ChunkCoord coord = ContextChunk.current!.Value;
        activeChunks.Add(coord);

        Vector3 chunkCenter = coord.ToWorldPosition();

        if(!initialized) {
            seenChunks.Add(coord);
            generatedChunks.Add(coord);

            var meshTypes = load();
            generate(meshTypes, coord, chunkCenter, setInitialized: true);
            return;
        }

        if(!seenChunks.Contains(coord) && !pendingGenerationSet.Contains(coord)) {
            seenChunks.Add(coord);
            pendingGenerationSet.Add(coord);
            pendingGeneration.Enqueue(coord);
            return;
        }
    }

    /**
     * 
     * Unrender
     *
     */
    public override void unrender() {
        ChunkCoord coord = ContextChunk.current!.Value;
        activeChunks.Remove(coord);
        
        foreach(var (entityId, instanceList) in entitySpawner.getAllInstances()) {
            for(int i = 0; i < instanceList.Count; i++) {
                if(entitySpawner.isHidden(entityId, i)) continue;

                var inst = instanceList[i];
                if(inst.SpawnedInChunk != coord) continue;

                entitySpawner.hide(entityId, i);
            }
        }
    }

    /**
     * 
     * Update
     *
     */
    // Update
    public override void update() {
        Vector3 playerPosition = playerController.getCamera().getPosition();

        updateVisibility(playerPosition);

        if(pendingGeneration.Count > 0) {
            var c = pendingGeneration.Dequeue();
            pendingGenerationSet.Remove(c);

            var meshTypes = load();
            Vector3 chunkCenter = c.ToWorldPosition();
            generate(meshTypes, c, chunkCenter);
        }

        while(generationQueue.Count > 0) {
            string meshType = generationQueue.Dequeue();
            var meshTypes = load();

            if(meshTypes.TryGetValue(meshType, out MeshData? meshData)) {
                Dictionary<string, MeshData> data = new Dictionary<string, MeshData> { [meshType] = meshData }; 
                
                var coord = ChunkCoord.FromWorldPosition(playerPosition.X, playerPosition.Y, playerPosition.Z);
                Vector3 chunkCenter = coord.ToWorldPosition();

                generate(data, coord, chunkCenter);
            }
        }
    }

    // Update Visibility
    private void updateVisibility(Vector3 playerPosition) {
        int visibleRange = MeshEntitySpawner.VISIBLE_ENTITIES;
        int visibleUnits = visibleRange * ChunkCoord.CHUNK_SIZE;

        foreach(var (entityId, instanceList) in entitySpawner.getAllInstances()) {
            List<int>? toShow = null;

            for(int i = 0; i < instanceList.Count; i++) {
                var inst = instanceList[i];
                if(inst.Lifetime <= 0) continue;

                float dx = MathF.Abs(inst.Position.X - playerPosition.X);
                float dz = MathF.Abs(inst.Position.Z - playerPosition.Z);

                bool insideRange = dx <= visibleUnits && dz <= visibleUnits;
                bool isHidden = entitySpawner.isHidden(entityId, i);

                if(insideRange && isHidden) {
                    toShow ??= new List<int>();
                    toShow.Add(i); 
                } else if(!insideRange && !isHidden) {
                    entitySpawner.hide(entityId, i);
                }
            }

            if(toShow == null) {
                continue;
            }
            if(entityPropsById.TryGetValue(entityId, out var props)) {
                var subset = toShow.Select(i => instanceList[i]).ToList();
                MeshEntityCollider.create(props, subset);
            }

            foreach(var idx in toShow) {
                entitySpawner.show(entityId, idx);
            }
        }
    }

    // Update Chunks
    public override void UpdateChunks(Dictionary<ChunkCoord, ChunkPriorityData> priorityData, ChunkPriorityConfig config) {
        priorityConfig = config ?? ChunkPriority;

        Vector3 playerPosition = playerController.getCamera().getPosition();
        lastPlayerPosition = playerPosition;

        entitySpawner.UpdateChunks(priorityData, config!, playerPosition);
    }
}