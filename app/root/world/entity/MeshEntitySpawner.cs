/**

    Entity Spawner class

    */
namespace App.Root.World.Entity;
using App.Root.Chunk;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Physics;
using App.Root.Utils;
using OpenTK.Mathematics;
using System.Reflection;

/**

    Entity Instance

    */
public struct Instance {
    [ConverterKey("positions")] public Vector3 Position;
    [ConverterKey("rotations")] public float Rotation;
    [ConverterKey("scales")] public float Scale;
    [Convert("rgba")] [ConverterKey("colors")] public string Color;
    [ConverterKey("texpaths")] public string? Tex;
    public float Speed;
    public float Lifetime;
    public ChunkCoord SpawnedInChunk;
    public Vector3 BoundaryCenter;

    public static readonly Dictionary<string, MethodInfo> converters = typeof(Converter)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            m => m.GetCustomAttribute<ConverterKey>()!.Key,
            m => m
        );
} 

/**

    LOD Entity

    */
public static class LODEntity {
    // Get Entities to Process
    public static int GetEntitiesToProcess(int totalCount, float quality, int minCount = 1) {
        int val = Math.Max(minCount, (int)(totalCount * quality));
        return val;
    }

    // Should Process Collisions
    public static bool ShouldProcessCollisions(LODLevel level, LODConfig config) {
        if(config.SkipCollisionsForLow) {
            bool val = (int)level < config.CollsionLODThreshold;
            return val;
        }

        return true;
    }

    /**
     * 
     * Process
     *
     */
    public static List<(int index, LODData data)> Process<T>(
        List<T> entities,
        Func<T, Vector3> getPosition,
        Vector3 playerPosition,
        LODConfig config, 
        int maxEntities = -1
    ) {
        var result = new List<(int, LODData)>();

        for(int i = 0; i < entities.Count; i++) {
            if(maxEntities > 0 && result.Count >= maxEntities) break;

            var pos = getPosition(entities[i]);
            var lodData = LODManager.getLODData(entities, i, pos, playerPosition, config);

            if(!lodData.IsVisible) continue;
            if(lodData.ShouldUpdateThisFrame) result.Add((i, lodData));
        }

        return result;
    }
}

/**

    Mesh Entity Spawner main class

    */
class MeshEntitySpawner {
    /**
     * 
     * Mesh Entity State
     *
     */
    public enum State {
        SLEEP,
        ACTIVE,
        HIDDEN
    }

    /**
     * 
     * Mesh Entity Main
     *
     */
    public static float SPAWN_AREA = 50.0f; 
    public static int VISIBLE_ENTITIES = 6;

    private Tick tick;      private float DeltaTime { get { return tick.getDeltaTime() / 5.0f; } }
    private Mesh mesh;
    private CollisionManager collisionManager;

    private Random range = new Random();

    private float startZ;
    private float endZ;

    private bool dataUsed = false;
    private bool streamUsed = false;

    private int cleanupFrame = 0;
    private const int CLEANUP_INTERVAL = 60;

    private const float MIN_SPEED = 1.0f;
    private const float MAX_SPEED = 10.0f;

    private const float MIN_LIFETIME = 5.0f;
    private const float MAX_LIFETIME = 20.0f;

    [Poolable("spawner_instances", typeof(PoolableDictionary<string, PoolableList<Instance>>), InitialSize = 32, MaxSize = 128)] private PoolableDictionary<string, PoolableList<Instance>> instances = null!;
    [Poolable("spawner_states", typeof(PoolableDictionary<string, State>), InitialSize = 32, MaxSize = 128)] private PoolableDictionary<string, State> instanceStates = null!;
    [Poolable("spawner_hidden", typeof(PoolableDictionary<string, PoolableList<bool>>), InitialSize = 32, MaxSize = 128)] private PoolableDictionary<string, PoolableList<bool>> instanceHidden = null!;
    [Poolable("spawner_chunk", typeof(PoolableDictionary<ChunkCoord, PoolableList<string>>), InitialSize = 32, MaxSize = 128)] private PoolableDictionary<ChunkCoord, PoolableList<string>> instancesByChunk = null!;

    [Poolable("spawner_mesh_types", typeof(PoolableDictionary<string, string>), InitialSize = 32, MaxSize = 128)] private PoolableDictionary<string, string> entityIdToMeshType = null!;
    private Dictionary<string, (MeshData data, Vector3 position)> pendingPhysics = new();

    private const string LOD_ID = "mesh_entities";
    [LODable(LOD_ID, typeof(LODConfig), InitialSize = 32, MaxSize = 128)] private LODConfig lodConfig = null!;

    public MeshEntitySpawner(Tick tick, Mesh mesh, CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;

        this.startZ = -SPAWN_AREA;
        this.endZ = SPAWN_AREA;

        PoolInjector.Inject(this);

        onStream();
        
        MeshEntityCollider.init(mesh, collisionManager, this);
        MeshEntityCollider.onStream();

        LODInjector.Inject(this);
    }

    // Get Boundary
    public float getBoundary() {
        return SPAWN_AREA;
    }

    // Get Start Z
    public float getStartZ() {
        return startZ;
    }

    // Get End Z
    public float getEndZ() {
        return endZ;
    }

    // Is Outside
    public bool isOutside(Vector3 position, Vector3 boundaryCenter) {
        float maxDist = SPAWN_AREA;
        bool outsideX = MathF.Abs(position.X - boundaryCenter.X) > maxDist;
        bool outsideZ = MathF.Abs(position.Z - boundaryCenter.Z) > maxDist;
        return outsideX || outsideZ;
    }

    // Get Instances
    public PoolableList<Instance> getInstances(string id) {
        PoolableList<Instance> val = instances.TryGetValue(id, out var list) ? list : null!;
        return val;
    }

    // Register Mesh Type
    public void registerMeshType(string entityId, string meshType) {
        entityIdToMeshType[entityId] = meshType;
    }

    // Get All Instances
    public PoolableDictionary<string, PoolableList<Instance>> getAllInstances() {
        return instances;
    }

    // Restore Instances
    public void restoreInstances(string id, PoolableList<Instance> list) {
        instances[id] = list;
        instanceStates[id] = State.SLEEP;
    }

    // Get State
    public State? getState(string entityId) {
        State? val = instanceStates.TryGetValue(entityId, out var s) ? s : (State?)null;
        return val;
    }

    // Get Chunk Center
    private Vector3 getChunkCenter(ChunkCoord coord) {
        Vector3 worldPos = coord.ToWorldPosition();
        float chunkSize = ChunkCoord.CHUNK_SIZE;
        float halfChunk = chunkSize / 2.0f;

        var (minY, maxY) = ChunkCoord.GetHeightRange(coord);
        float centerY = (minY + maxY) / 2.0f;

        return new Vector3(
            worldPos.X + halfChunk,
            centerY,
            worldPos.Z + halfChunk
        );
    }

    /**
     * 
     * Position
     *
     */
    public List<Vector3> getPositions(string id) {
        List<Vector3> val = instances[id].Select(i => i.Position).ToList();
        return val;
    }

    private void defPosition(ref Instance inst) {
        float v = 0.0f;//3.0f;
        float speed = v * DeltaTime;

        inst.Position = new Vector3(
            inst.Position.X,
            inst.Position.Y,
            inst.Position.Z + inst.Speed * speed
        );
    }

    private Vector3 setPosition(Vector3 center) {
        float r = SPAWN_AREA;

        float x = center.X + (float)(range.NextDouble() * r * 2.0f - r);
        float y = center.Y + (float)(range.NextDouble() * r * 2.0f - r);
        float z = center.Z + (float)(range.NextDouble() * r * 2.0f - r);

        Vector3 val = new Vector3(x, y, z);
        return val;
    }

    /**
     * 
     * Rotation
     *
     */
    private float setRotation() {
        float f = 360.0f;
        float val = (float)(range.NextDouble() * f);
        return val;
    }

    private void defRotation(ref Instance inst) {
        float angle = 360.0f;
        float speed = DeltaTime * 10.0f;

        inst.Rotation = 
            (inst.Rotation + inst.Speed * speed) %
            angle;
    }

    /**
     * 
     * Speed
     *
     */
    private float setSpeed() {
        float val = MIN_SPEED + 
            (float)range.NextDouble() * 
            (MAX_SPEED - MIN_SPEED);
        
        return val;
    }

    /**
     * 
     * Color
     *
     */
    private string setColor(EntityProps entity) {
        string val = entity.Color;
        return val;
    }

    /**
     * 
     * Lifetime
     *
     */
    private float setLifetime() {
        float lifetime =  MIN_LIFETIME +
            (float)range.NextDouble() * 
            (MAX_LIFETIME - MIN_LIFETIME);
        
        float val = ConvertTime.MinutesToSeconds(lifetime);
        return val;
    }

    /**
     * 
     * Wrap
     *
     */
    private bool wrap(ref Instance inst, string entityId, int index) {
        float l = 0.0f;
        inst.Lifetime -= DeltaTime;

        if(isOutside(inst.Position, inst.BoundaryCenter)) {
            reset(ref inst);
            return true;
        }

        if(inst.Lifetime <= l) {
            if(isHidden(entityId, index)) return true;

            if(!MeshEntityCollider.colliderIds.ContainsKey(entityId)) return true;
            if(index >= MeshEntityCollider.colliderIds[entityId].Count) return true;
            
            string colliderId = MeshEntityCollider.colliderIds[entityId][index];
            collisionManager.removeCollider(colliderId);
            return true;
        }

        return false;
    }

    /**
     * 
     * Texture
     *
     */
    private string? setTexture(EntityProps entity) {
        string? val = entity.Tex;
        return val;
    }

    /**
     * 
     * Spawn
     *
     */
    private Instance spawn(EntityProps entity, ChunkCoord spawnChunk, Vector3 boundaryCenter) {
        Instance e = new Instance {
            Position = setPosition(boundaryCenter),
            Speed = setSpeed(),
            Rotation = setRotation(),
            Lifetime = setLifetime(),
            Color = setColor(entity),
            Tex = setTexture(entity),
            Scale = entity.Scale,
            SpawnedInChunk = spawnChunk,
            BoundaryCenter = boundaryCenter
        };

        return e;
    }

    private void setSpawn(ref Instance inst) {
        defPosition(ref inst);
        defRotation(ref inst);
    }

    /**
     * 
     * Show
     *
     */
    public void show(string entityId, int index) {
        if(!instanceHidden.TryGetValue(entityId, out var hiddenList)) return;
        if(index < 0 || index >= hiddenList.Count) return;
        if(!hiddenList[index]) return;

        hiddenList[index] = false;

        streamUsed = true;
    }

    /**
     * 
     * Hide
     *
     */
    // Hide
    public void hide(string entityId, int index) {
        if(!instances.TryGetValue(entityId, out var list)) return;
        if(index < 0 || index >= list.Count) return;

        if(!instanceHidden.TryGetValue(entityId, out var hiddenList)) {
            hiddenList = new PoolableList<bool>();
            instanceHidden[entityId] = hiddenList;
        }
        if(hiddenList[index]) return;

        MeshEntityCollider.remove(entityId, index);
        hiddenList[index] = true;

        streamUsed = true;
    }

    // Hide All In Chunk
    private void hideAllInChunk(string entityId, ChunkCoord coord) {
        if(!instances.TryGetValue(entityId, out var instanceList)) return;

        for(int i = 0; i < instanceList.Count; i++) {
            if(instanceList[i].SpawnedInChunk == coord) {
                hide(entityId, i);
            }
        }
    }

    // Is Hidden
    public bool isHidden(string entityId, int index) {
        if(!instanceHidden.TryGetValue(entityId, out var hiddenList)) return false;
        if(index < 0 || index >= hiddenList.Count) return false;
        return hiddenList[index];
    }

    // Get Hidden Flags
    public PoolableList<bool>? getHiddenFlags(string entityId) {
        PoolableList<bool>? val = instanceHidden.TryGetValue(entityId, out var list) ? list : null;
        return val;
    }

    /**
     * 
     * Data
     *
     */
    private Dictionary<string, List<object>> getData(List<Instance> list) {
        CacheMeshEntity.CacheFields();
        CacheMeshEntity.CacheFieldGettes();
        CacheMeshEntity.ClearCachedData();

        foreach(var inst in list) {
            for(int i = 0; i < CacheMeshEntity.cachedFieldGetters!.Length; i++) {
                var getter = CacheMeshEntity.cachedFieldGetters![i];
                object val = getter(inst);

                var (key, converter) = CacheMeshEntity.cachedFieldMeta![CacheMeshEntity.cachedFields[i]];
                object finalVal = converter != null ? converter.Invoke(null, new[] { val })! : val;

                CacheMeshEntity.CacheData(key, finalVal);
            }
        }

        return CacheMeshEntity.cachedData;
    }

    public void syncData(string meshType, List<Instance> allInstances) {
        if(allInstances.Count == 0) return;

        var renderer = mesh?.getMeshRenderer(meshType);
        if(renderer == null) return;

        if(CacheMeshEntity.cachedUpdateMethod == null || CacheMeshEntity.cachedSetMethod == null) {
            var methods = typeof(MeshRenderer).GetMethods();
            CacheMeshEntity.Sync(methods);
        }

        bool isInit = renderer.getInstanceVboInitialized();
        var method = isInit ? CacheMeshEntity.cachedUpdateMethod! : CacheMeshEntity.cachedSetMethod;
        var paramTypes = isInit ? CacheMeshEntity.cachedUpdateParamTypes! : CacheMeshEntity.cachedSetParamTypes!;

        var data = getData(allInstances);

        CacheMeshEntity.CacheArgs(meshType, paramTypes, out var cached);
        CacheMeshEntity.CacheParamNames(cached.lists, data);

        method!.Invoke(renderer, cached.args);
    }

    /**
     * 
     * Update
     *
     */
    // Update
    public void update() {
        if(tick == null || mesh == null) return;

        /*
        cleanupEntity();

        foreach(var (id, l) in instances) {
            if(!instanceStates.TryGetValue(id, out var state)) continue;
            if(state == State.ACTIVE) continue;

            int firstVisibleIndex = -1;

            for(int i = 0; i < l.Count; i++) {
                var inst = l[i];

                if(wrap(DeltaTime, ref inst, id, i)) {
                    l[i] = inst;
                    continue;
                }

                setSpawn(DeltaTime, ref inst);
                l[i] = inst;

                bool hidden = isHidden(id, i);
                if(!hidden) {
                    MeshEntityCollider.update(id, i, inst.Position);
                    if(firstVisibleIndex < 0) firstVisibleIndex = i;
                }
            }

            if(firstVisibleIndex >= 0) mesh.setPosition(id, l[firstVisibleIndex].Position);
        }

        updateData();
        updatePhysics();
        */
    }

    // Update Data
    public void updateData() {
        CacheMeshEntity.ClearCachedByMeshTypes();
        var ids = new Dictionary<string, List<string>>();

        foreach(var (id, l) in instances) {
            string meshType = entityIdToMeshType.TryGetValue(id, out var t) ? t : id;
            List<bool>? hiddenList = instanceHidden.TryGetValue(id, out var hl) ? hl : null;
            List<string>? colliderIds = MeshEntityCollider.colliderIds.TryGetValue(id, out var cid) ? cid : null;

            List<Instance> visible;
            List<string> visibleIds;

            if(hiddenList == null) {
                visible = l;
                visibleIds = new List<string>(l.Count);
                for(int i = 0; i < l.Count; i++) {
                    visibleIds.Add(colliderIds != null && i < colliderIds.Count ? colliderIds[i] : "");
                }
            } else {
                visible = new List<Instance>(l.Count);
                visibleIds = new List<string>(l.Count);
                for(int i = 0; i < l.Count; i++) {
                    if(i < hiddenList.Count && hiddenList[i]) continue;
                    visible.Add(l[i]);
                    visibleIds.Add(colliderIds != null && i < colliderIds.Count ? colliderIds[i] : "");
                }
            }

            CacheMeshEntity.CacheByMeshType(meshType, visible);
            if(!ids.ContainsKey(meshType)) ids[meshType] = new();
            ids[meshType].AddRange(visibleIds);
        }

        Dictionary<string, int> idMap = new Dictionary<string, int>();
        Dictionary<string, string> meshTypesMap = new Dictionary<string, string>(MeshCollider.instancedMeshTypes);
        Dictionary<string, Vector3> positionsMap = new Dictionary<string, Vector3>(MeshCollider.instancedPositions);

        foreach(var (meshType, allInstances) in CacheMeshEntity.cachedByMeshType) {
            syncData(meshType, allInstances);

            if(!ids.TryGetValue(meshType, out var id)) continue;

            var renderer = mesh?.getMeshRenderer(meshType);
            renderer?.setInstanceIds(id);
            
            for(int i = 0; i < id.Count; i++) {
                if(!string.IsNullOrEmpty(id[i])) idMap[id[i]] = i;
            }
        }

        if(streamUsed) {
            EventStream.set("stream-ids", (object)idMap);
            EventStream.set("stream-mesh-types", (object)meshTypesMap);
            EventStream.set("stream-positions", (object)positionsMap);

            streamUsed = false;
        }
    }

    // Update Physics
    private void updatePhysics() {
        foreach(var (id, (meshData, position)) in pendingPhysics) {
            if(!PhysicsRegistry.getInstance().has(id)) {
                PhysicsRegistry.getInstance().register(id, meshData, PhysicsType.DYNAMIC);
            }
                
            var entry = PhysicsRegistry.getInstance().getEntry(id);
            entry?.physicsBody?.setPosition(position);
        }

        pendingPhysics.Clear();
    }

    // Update Entity
    private void updateEntity(ref Instance inst, string entityId, int index) {
        if(wrap(ref inst, entityId, index)) return;
        setSpawn(ref inst);
    }

    // Update Chunks
    public void UpdateChunks(Dictionary<ChunkCoord, ChunkPriorityData> priorityData, ChunkPriorityConfig config, Vector3 playerPosition) {
        if(tick == null || mesh == null) return;

        if(cleanupFrame++ % CLEANUP_INTERVAL == 0) cleanupEntity();

        var lodConfig = LODManager.getConfig(LOD_ID);
        LODManager.IncrementFrame();

        foreach(var (coord, priorityInfo) in priorityData) {
            if(!instancesByChunk.TryGetValue(coord, out var chunkInstances)) continue;
            if(chunkInstances.Count == 0) continue;

            Vector3 chunkCenter = getChunkCenter(coord);

            var lodData = LODManager.getLODData(this, coord.GetHashCode(), chunkCenter, playerPosition, lodConfig);
            if(!lodData.IsVisible) {
                foreach(var entityId in chunkInstances) {
                    hideAllInChunk(entityId, coord);
                }
                continue;
            }
            if(!lodData.ShouldUpdateThisFrame) {
                continue;
            }
            
            int entitiesToProcess = LODEntity.GetEntitiesToProcess(chunkInstances.Count, lodData.Quality);
            processChunkEntities(coord, chunkInstances, entitiesToProcess, priorityInfo, lodData);

            dataUsed = true;
        }

        if(dataUsed) {
            updateData();
            updatePhysics();
            dataUsed = false;
        }
    }

    /**
     * 
     * Process
     *
     */
    private void processChunkEntities(ChunkCoord coord, PoolableList<string> entityIds, int entitiesToProcess, ChunkPriorityData priorityInfo, LODData lodData) {
        int processed = 0;

        foreach(var entityId in entityIds) {
            if(processed >= entitiesToProcess) break;
            if(!instances.TryGetValue(entityId, out var instanceList)) continue;

            for(int i = 0; i < instanceList.Count; i++) {
                var inst = instanceList[i];
                if(inst.SpawnedInChunk != coord) continue;

                updateEntity(ref inst, entityId, i);
                instanceList[i] = inst;

                bool shouldUpdateCollisions = 
                    priorityInfo.Priority == ChunkPriority.HIGH &&
                    !lodData.ShouldUpdateThisFrame;
                if(shouldUpdateCollisions) MeshEntityCollider.update(entityId, i, inst.Position);

                processed++;
            }
        }
    }

    /**
     * 
     * Render
     *
     */
    public void render(EntityProps entity, ChunkCoord spawnChunk, Vector3 boundaryCenter) {
        var instanceList = new PoolableList<Instance>();
        for(int i = 0; i < entity.Position.Count; i++) instanceList.Add(spawn(entity, spawnChunk, boundaryCenter));
        instances[entity.Id] = instanceList;
        instanceStates[entity.Id] = State.SLEEP;

        var hiddenList = new PoolableList<bool>();
        for(int i = 0; i < instanceList.Count; i++) hiddenList.Add(false);
        instanceHidden[entity.Id] = hiddenList;
        
        if(!instancesByChunk.ContainsKey(spawnChunk)) instancesByChunk[spawnChunk] = new PoolableList<string>();
        instancesByChunk[spawnChunk].Add(entity.Id);

        MeshEntityCollider.create(entity, instanceList);

        streamUsed = true;
    }

    /**
     * 
     * On Events
     *
     */
    private void onStream() {
        // Instanced Break
        EventStream.on("instanced-break", (data) => {
            if(data is not string colliderId) return;

            string? entityId = MeshEntityCollider.colliderToEntity.TryGetValue(colliderId, out var eid) ? eid : null;
            if(entityId == null) return;
            if(!instances.ContainsKey(entityId)) return;
            if(!instanceStates.ContainsKey(entityId)) return;

            var ids = MeshEntityCollider.colliderIds.TryGetValue(entityId, out var list) ? list : null;
            if(ids == null) return;

            int index = ids.IndexOf(colliderId);
            if(index < 0 || index >= instances[entityId].Count) return;

            instances[entityId].RemoveAt(index);
            if(instanceHidden.TryGetValue(entityId, out var hl) && index < hl.Count) hl.RemoveAt(index);
            ids.RemoveAt(index);

            MeshEntityCollider.colliderToEntity.Remove(colliderId);
            MeshCollider.removeInstanced(colliderId);

            string meshType = entityIdToMeshType.TryGetValue(entityId, out var t) ? t : entityId;
            var allInstances = instances.Where(kvp => entityIdToMeshType.TryGetValue(kvp.Key, out var mt) && mt == meshType).SelectMany(kvp => kvp.Value).ToList();
            syncData(meshType, allInstances);

            streamUsed = true;
        });

        // Instanced Place
        EventStream.on("instanced-place", (data) => {
            if(data is not (string id, MeshData meshData, Vector3 position)) return;
            pendingPhysics[id] = (meshData, position);
        });
    }

    /**
     * 
     * Reset
     *
     */
    private void reset(ref Instance inst) {
        Vector3 p = inst.Position;
        Vector3 c = inst.BoundaryCenter;
        float r = SPAWN_AREA;

        if(p.X - c.X > r) p.X = c.X - r;
        else if(p.X - c.X < -r) p.X = c.X + r;
        if(p.Z - c.Z > r) p.Z = c.Z - r;
        else if(p.Z - c.Z < -r) p.Z = c.Z + r;

        inst.Position = p;
    }

    /**
     * 
     * Cleanup
     *
     */
    // Cleanup
    public void cleanup() {
        MeshEntityCollider.cleanup();
        instances.Clear();
        instanceHidden.Clear();
    }
    
    // Cleanup Entity
    private void cleanupEntity() {
        if(MeshEntityCollider.colliderIds.Count == 0) return;
        if(collisionManager.getPendingRemovalsCount() == 0) return;

        var hiddenEntities = new HashSet<string>();
        foreach(var (entityId, hiddenList) in instanceHidden) {
            if(hiddenList.Any(h => h == true)) {
                hiddenEntities.Add(entityId);
            }
        } 
        
        MeshEntityCollider.cleanupRemoved();

        var removedEntities = instances.Keys
            .Where(id => !MeshEntityCollider.colliderIds.ContainsKey(id) &&
                !hiddenEntities.Contains(id) &&
                !(instanceStates.TryGetValue(id, out var s) && s == State.HIDDEN) &&
                instances[id].All(inst => inst.Lifetime <= 0)
            )
            .ToList();

        foreach(var rId in removedEntities) {
            instances.Remove(rId);
            instanceStates.Remove(rId);
            instanceHidden.Remove(rId);
            mesh?.removeData(rId);
            mesh?.remove(rId);
        }
    }

    /**
     * 
     * Remove
     *
     */
    // Remove Instance
    public void removeInstance(string entityId, int index) {
        if(!instances.ContainsKey(entityId)) return;
        if(index >= instances[entityId].Count) return;
        instances[entityId].RemoveAt(index);
    }

    // Remove Entity
    public void removeEntity(string entityId) {
        foreach(var (chunk, list) in instancesByChunk) {
            if(list.Remove(entityId)) {
                if(list.Count == 0) {
                    instancesByChunk.Remove(chunk);
                }
                break;
            }
        }

        instances.Remove(entityId);
        instanceStates.Remove(entityId);
        instanceHidden.Remove(entityId);

        entityIdToMeshType.Remove(entityId);
        mesh?.removeData(entityId);
    }
}