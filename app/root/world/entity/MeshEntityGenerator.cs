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
class MeshEntityGenerator : WorldHandler {
    private static readonly string DATA_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/entity/MeshEntity.lua");
    
    private Tick tick;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private PlayerController playerController;
    private MeshEntitySpawner entitySpawner;

    private Queue<string> generationQueue = new();

    private Dictionary<string, EntityProps> entityPropsById = new();
    private HashSet<ChunkCoord> seenChunks = new();

    private bool initialized = false;

    public MeshEntityGenerator(
        [Inject] Tick tick, 
        [Inject] Mesh mesh, 
        [Inject] CollisionManager collisionManager,
        [Inject] PlayerController playerController
    ) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.playerController = playerController;
    
        this.entitySpawner = new MeshEntitySpawner(tick, mesh, collisionManager);
    
        MeshEntityCollider.onEntityRemoved += meshType => {
            generationQueue.Enqueue(meshType);
        };

        StateManager.Register(this);
    }

    // Instance Chunk
    private ChunkCoord instanceChunk(Instance inst) {
        ChunkCoord val = ChunkCoord.FromWorldPosition(
            inst.Position.X, 
            inst.Position.Y, 
            inst.Position.Z
        );

        return val;
    }

    /**
     * 
     * Load
     *
     */
    public static Dictionary<string, MeshData> load() {
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

    private void generate(Dictionary<string, MeshData> meshTypes,Vector3 boundaryCenter, bool setInitialized = false) {
        ChunkCoord spawnChunk = ContextChunk.current ?? default;

        var entityProps = new Dictionary<string, EntityProps>();
        var entityInstances = new Dictionary<string, List<Instance>>();
        var byMeshType = new Dictionary<string, List<Instance>>();

        foreach(var (type, data) in meshTypes) {
            foreach(var entity in MeshEntityFactory.generate(data, type)) {
                generateSource(entity, data, spawnChunk, boundaryCenter);
                entityProps[entity.Id] = entity;
                entityPropsById[entity.Id] = entity;
                
                var instances = entitySpawner.getInstances(entity.Id);
                entityInstances[entity.Id] = instances;
                entitySpawner.registerMeshType(entity.Id, entity.MeshType);

                if(!byMeshType.ContainsKey(entity.MeshType)) byMeshType[entity.MeshType] = new();
                byMeshType[entity.MeshType].AddRange(instances);
            }
        }

        foreach(var (meshType, allInstances) in byMeshType) {
            entitySpawner.syncData(meshType, allInstances);
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
        Vector3 chunkCenter = coord.ToWorldPosition();

        if(!initialized) {
            seenChunks.Add(coord);
            var meshTypes = load();
            generate(meshTypes, chunkCenter, setInitialized: true);
            return;
        }

        if(!seenChunks.Contains(coord)) {
            seenChunks.Add(coord);
            var meshTypes = load();
            generate(meshTypes, chunkCenter, setInitialized: !initialized);
            return;
        }

        foreach(var (entityId, instanceList) in entitySpawner.getAllInstances()) {
            List<int>? indicesToShow = null;

            for(int i = 0; i < instanceList.Count; i++) {
                if(!entitySpawner.isHidden(entityId, i)) continue;

                var inst = instanceList[i];
                var chunk = instanceChunk(inst);
                if(chunk != coord) continue;
                if(inst.Lifetime <= 0) continue;

                indicesToShow ??= new List<int>();
                indicesToShow.Add(i);
            }

            if(indicesToShow == null) continue;

            if(entityPropsById.TryGetValue(entityId, out var props)) {
                var subset = indicesToShow.Select(i => instanceList[i]).ToList();
                MeshEntityCollider.create(props, subset);
            }

            foreach(var idx in indicesToShow) {
                entitySpawner.show(entityId, idx);
            }
        }
    }

    /**
     * 
     * Unrender
     *
     */
    public override void unrender() {
        ChunkCoord coord = ContextChunk.current!.Value;

        foreach(var (entityId, instanceList) in entitySpawner.getAllInstances()) {
            for(int i = 0; i < instanceList.Count; i++) {
                if(entitySpawner.isHidden(entityId, i)) continue;

                var inst = instanceList[i];
                var chunk = instanceChunk(inst);
                if(chunk != coord) continue;

                entitySpawner.hide(entityId, i);
            }
        }
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        Vector3 playerPosition = playerController.getCamera().getPosition();
        entitySpawner.update(playerPosition);

        while(generationQueue.Count > 0) {
            string meshType = generationQueue.Dequeue();
            var meshTypes = load();

            if(meshTypes.TryGetValue(meshType, out MeshData? data)) {
                var coord = ChunkCoord.FromWorldPosition(playerPosition.X, playerPosition.Y, playerPosition.Z);
                Vector3 chunkCenter = coord.ToWorldPosition();
                generate(new Dictionary<string, MeshData> { [meshType] = data }, chunkCenter);
            }
        }
    }
}