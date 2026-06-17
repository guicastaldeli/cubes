/**

    Entity Spawner class

    */
namespace App.Root.World.Entity;
using App.Root.Chunk;
using App.Root.Collider;
using App.Root.Collider.Types;
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

    /**
     * 
     * Get
     *
     */
    public Dictionary<string, object> GetData() {
        var dict = new Dictionary<string, object>();

        foreach(var field in typeof(Instance).GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            object? val = field.GetValue(this);
            if(val == null) continue;

            var keyAttr = field.GetCustomAttribute<ConverterKey>();
            var converterAttr = field.GetCustomAttribute<ConvertAttribute>();
            string dictKey = keyAttr?.Key ?? field.Name.ToLower();

            if(converterAttr != null && converters.TryGetValue(converterAttr.Converter, out var method)) {
                dict[dictKey] = method.Invoke(null, new[] { val })!;
            } else {
                dict[dictKey] = val;
            }

        }
        
        return dict;
    }

    public static List<T> Get<T>(List<Instance> instances, string key) {
        List<T> val = instances.Select(i => i.GetData())
            .Where(d => d.ContainsKey(key) && d[key] is T)
            .Select(d => (T)d[key])
            .ToList();

        return val;
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
    public static float SPAWN_AREA = (float)ChunkCoord.CHUNK_SIZE; 
    public static int VISIBLE_ENTITIES = 6;

    private Tick? tick;
    private Mesh? mesh;
    private CollisionManager collisionManager;

    private Random range = new Random();

    private float startZ;
    private float endZ;

    private const float MIN_SPEED = 1.0f;
    private const float MAX_SPEED = 10.0f;

    private const float MIN_LIFETIME = 5.0f;
    private const float MAX_LIFETIME = 20.0f;

    private Dictionary<string, List<Instance>> instances = new();
    private Dictionary<string, State> instanceStates = new();
    private Dictionary<string, List<bool>> instanceHidden = new();

    private Dictionary<string, string> entityIdToMeshType = new();
    private Dictionary<string, (MeshData data, Vector3 position)> pendingPhysics = new();

    public MeshEntitySpawner(Tick tick, Mesh mesh, CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;

        this.startZ = -SPAWN_AREA;
        this.endZ = SPAWN_AREA;

        onEvents();
        
        //MeshEntityCollider.init(mesh, collisionManager, this);
        //MeshEntityCollider.onEvents();
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
    public List<Instance> getInstances(string id) {
        List<Instance> val = instances.TryGetValue(id, out var list) ?
            list :
            new List<Instance>();
        return val;
    }

    // Register Mesh Type
    public void registerMeshType(string entityId, string meshType) {
        entityIdToMeshType[entityId] = meshType;
    }

    // Get All Instances
    public Dictionary<string, List<Instance>> getAllInstances() {
        return instances;
    }

    // Restore Instances
    public void restoreInstances(string id, List<Instance> list) {
        instances[id] = new List<Instance>(list);
        instanceStates[id] = State.SLEEP;
    }

    // Get State
    public State? getState(string entityId) {
        State? val = instanceStates.TryGetValue(entityId, out var s) ? s : (State?)null;
        return val;
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

    private void defPosition(float deltaTime, ref Instance inst) {
        float v = 0.0f;//3.0f;
        float speed = v * deltaTime;

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

    private void defRotation(float deltaTime, ref Instance inst) {
        float angle = 360.0f;
        float speed = deltaTime * 10.0f;

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
    private bool wrap(float deltaTime, ref Instance inst, string entityId, int index) {
        float l = 0.0f;
        inst.Lifetime -= deltaTime;

        if(isOutside(inst.Position, inst.BoundaryCenter)) {
            reset(ref inst, entityId, index);
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

    private void setSpawn(float deltaTime, ref Instance inst) {
        defPosition(deltaTime, ref inst);
        defRotation(deltaTime, ref inst);
    }

    /**
     * 
     * Visibility
     *
     */
    // Hide
    public void hide(string entityId, int index) {
        if(!instances.TryGetValue(entityId, out var list)) return;
        if(index < 0 || index >= list.Count) return;

        if(!instanceHidden.TryGetValue(entityId, out var hiddenList)) {
            hiddenList = Enumerable.Repeat(false, list.Count).ToList();
            instanceHidden[entityId] = hiddenList;
        }
        if(hiddenList[index]) return;

        var pos = list[index].Position;
        Console.WriteLine($"[Spawner] hide {entityId}[{index}] at ({pos.X:F1},{pos.Y:F1},{pos.Z:F1})");

        MeshEntityCollider.remove(entityId, index);
        hiddenList[index] = true;
    }

    // Show
    public void show(string entityId, int index) {
        if(!instanceHidden.TryGetValue(entityId, out var hiddenList)) return;
        if(index < 0 || index >= hiddenList.Count) return;
        if(!hiddenList[index]) return;

        var pos = instances[entityId][index].Position;
        Console.WriteLine($"[Spawner] show {entityId}[{index}] at ({pos.X:F1},{pos.Y:F1},{pos.Z:F1})");

        hiddenList[index] = false;
    }

    // Is Hidden
    public bool isHidden(string entityId, int index) {
        if(!instanceHidden.TryGetValue(entityId, out var hiddenList)) return false;
        if(index < 0 || index >= hiddenList.Count) return false;
        return hiddenList[index];
    }

    // Get Hidden Flags
    public List<bool>? getHiddenFlags(string entityId) {
        return instanceHidden.TryGetValue(entityId, out var list) ? list : null;
    }

    /**
     * 
     * Data
     *
     */
    private Dictionary<string, List<object>> getData(List<Instance> list) {
        CacheMeshEntity.CacheFields();
        CacheMeshEntity.ClearCachedData();

        foreach(var inst in list) {
            foreach(var field in CacheMeshEntity.cachedFields!) {
                object? val = field.GetValue(inst);
                if(val == null) continue;

                var (key, converter) = CacheMeshEntity.cachedFieldMeta![field];
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
    public void update(Vector3 playerPosition) {
        if(tick == null || mesh == null) return;

        float deltaTime = tick.getDeltaTime() / 5.0f;

        cleanupEntity();

        foreach(var (id, l) in instances) {
            if(!instanceStates.TryGetValue(id, out var state)) continue;
            if(state == State.ACTIVE) continue;

            int firstVisibleIndex = -1;

            for(int i = 0; i < l.Count; i++) {
                var inst = l[i];

                if(wrap(deltaTime, ref inst, id, i)) {
                    l[i] = inst;
                    continue;
                }

                setSpawn(deltaTime, ref inst);
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
    }

    // Update Data
    public void updateData() {
        CacheMeshEntity.ClearCachedByMeshTypes();

        foreach(var (id, l) in instances) {
            string meshType = entityIdToMeshType.TryGetValue(id, out var t) ? t : id;
            
            List<bool>? hiddenList = instanceHidden.TryGetValue(id, out var hl) ? hl : null;
            List<Instance> visible;
            if(hiddenList == null) {
                visible = l;
            } else {
                visible = new List<Instance>(l.Count);
                for(int i = 0; i < l.Count; i++) {
                    if(i < hiddenList.Count && hiddenList[i]) continue;
                    visible.Add(l[i]);
                }
            }

            CacheMeshEntity.CacheByMeshType(meshType, visible);
        }

        foreach(var (meshType, allInstances) in CacheMeshEntity.cachedByMeshType) {
            syncData(meshType, allInstances);
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

    /**
     * 
     * Render
     *
     */
    public void render(EntityProps entity, ChunkCoord spawnChunk, Vector3 boundaryCenter) {
        var instanceList = Enumerable.Repeat(entity, entity.Position.Count).Select(e => spawn(e, spawnChunk, boundaryCenter)).ToList();
        
        instances[entity.Id] = instanceList;
        instanceStates[entity.Id] = State.SLEEP;
        instanceHidden[entity.Id] = Enumerable.Repeat(false, instanceList.Count).ToList();
        MeshEntityCollider.create(entity, instanceList);
    }

    /**
     * 
     * On Events
     *
     */
    private void onEvents() {
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
    private void reset(ref Instance inst, string entityId, int index) {
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
    public void cleanup() {
        MeshEntityCollider.cleanup();
        instances.Clear();
        instanceHidden.Clear();
    }
    
    private void cleanupEntity() {
        if(MeshEntityCollider.colliderIds.Count == 0) return;
        if(collisionManager.getPendingRemovalsCount() == 0) return;
        
        MeshEntityCollider.cleanupRemoved();

        var removedEntities = instances.Keys
            .Where(id => !MeshEntityCollider.colliderIds.ContainsKey(id) &&
                !(instanceStates.TryGetValue(id, out var s) && s == State.HIDDEN)
            )
            .ToList();

        foreach(var rId in removedEntities) {
            instances.Remove(rId);
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
        instances.Remove(entityId);
        instanceStates.Remove(entityId);
        instanceHidden.Remove(entityId);

        entityIdToMeshType.Remove(entityId);
        mesh?.removeData(entityId);
    }
}