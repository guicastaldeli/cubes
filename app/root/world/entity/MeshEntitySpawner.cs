/**

    Entity Spawner class

    */
namespace App.Root.World.Entity;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Physics;
using App.Root.Utils;
using OpenTK.Mathematics;
using System.Collections;
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

    public static readonly Dictionary<string, MethodInfo> converters =
        typeof(Converter)
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

    Spawn Point helper class

    */
static class SpawnPoint {
    private static CollisionManager collisionManager = null!;
    private static BoundaryObject boundaryObject = null!;

    /**
     * 
     * Init
     *
     */
    public static void init(CollisionManager collisionManager) {
        SpawnPoint.collisionManager = collisionManager;
        if(isInit()) initCollider();
    }

    private static bool isInit() {
        bool val = collisionManager != null;
        return val;   
    }

    private static void initCollider() {
        var collider = new BoundaryObject(MeshEntitySpawner.SPAWN_AREA);
        collisionManager.addInteractionCollider(collider);
        boundaryObject = collider;
    }

    /**
     * 
     * Spawn Point
     *
     */
    public static (Vector3 center, Vector3 size) get() {
        var dist = boundaryObject.getBoundaryDistance();

        float minY = boundaryObject.getMinHeight();
        float maxY = boundaryObject.getMaxHeight();

        float centerX = 0.0f;
        float centerY = (minY + maxY) / 2.0f;
        float z = -dist;

        float width = maxY - minY;
        float height = maxY - minY;

        return (
            new Vector3(centerX, centerY, z),
            new Vector3(width, height, 0)
        );
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
    private enum State {
        SLEEP,
        ACTIVE
    }

    /**
     * 
     * Mesh Entity Main
     *
     */
    public static float SPAWN_AREA = 50.0f;

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

    private Dictionary<string, string> entityIdToMeshType = new();
    private Dictionary<string, (MeshData data, Vector3 position)> pendingPhysics = new();

    private static MethodInfo? cachedUpdateMethod = null;
    private static MethodInfo? cachedSetMethod = null;
    private static string[]? cachedParamNames = null;
    private static Type[]? cachedUpdateParamTypes = null;
    private static Type[]? cachedSetParamTypes = null;
    private static FieldInfo[]? cachedFields = null;
    private static Dictionary<FieldInfo, (string key, MethodInfo? converter)>? cachedFieldMeta = null;
    private Dictionary<string, List<object>> cachedData = new();
    private Dictionary<string, List<Instance>> cachedByMeshType = new();
    private Dictionary<string, (object?[] args, IList[] lists)> cachedArgsByMeshType = new();

    public MeshEntitySpawner(Tick tick, Mesh mesh, CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;

        this.startZ = -SPAWN_AREA;
        this.endZ = SPAWN_AREA;

        SpawnPoint.init(collisionManager);

        onEvents();
        
        MeshEntityCollider.init(mesh, collisionManager, this);
        MeshEntityCollider.onEvents();
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
    public bool isOutside(Vector3 position) {
        bool val = position.Z > endZ;
        return val;
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
        float v = 3.0f;
        float speed = v * deltaTime;

        inst.Position = new Vector3(
            inst.Position.X,
            inst.Position.Y,
            inst.Position.Z + inst.Speed * speed
        );
    }

    private Vector3 setPosition() {
        var (center, size) = SpawnPoint.get();

        float x = center.X + (float)(range.NextDouble() * size.X - size.X / 2.0f);
        float y = center.Y + (float)(range.NextDouble() * size.Y - size.Y / 2.0f);
        float z = center.Z;

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

        if(isOutside(inst.Position)) {
            reset(ref inst);
            return true;
        }

        if(inst.Lifetime <= l) {
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
    private Instance spawn(EntityProps entity) {
        Instance e = new Instance {
            Position = setPosition(),
            Speed = setSpeed(),
            Rotation = setRotation(),
            Lifetime = setLifetime(),
            Color = setColor(entity),
            Tex = setTexture(entity),
            Scale = entity.Scale
        };

        return e;
    }

    private void setSpawn(float deltaTime, ref Instance inst) {
        defPosition(deltaTime, ref inst);
        defRotation(deltaTime, ref inst);
    }

    /**
     * 
     * Data
     *
     */
    private Dictionary<string, List<object>> getData(List<Instance> list) {
        if(cachedFields == null) {
            cachedFields = typeof(Instance).GetFields(BindingFlags.Public | BindingFlags.Instance);
            cachedFieldMeta = new();
            foreach(var field in cachedFields) {
                var keyAttr = field.GetCustomAttribute<ConverterKey>();
                var converterAttr = field.GetCustomAttribute<ConvertAttribute>();
                string key = keyAttr?.Key ?? field.Name.ToLower();
                MethodInfo? converter = null;
                if(converterAttr != null) Instance.converters.TryGetValue(converterAttr.Converter, out converter);
                cachedFieldMeta[field] = (key, converter);
            }
        }

        foreach(var l in cachedData.Values) l.Clear();

        foreach(var inst in list) {
            foreach(var field in cachedFields) {
                object? val = field.GetValue(inst);
                if(val == null) continue;

                var (key, converter) = cachedFieldMeta![field];
                object finalVal = converter != null ? converter.Invoke(null, new[] { val })! : val;

                if(!cachedData.ContainsKey(key)) cachedData[key] = new List<object>();
                cachedData[key].Add(finalVal);
            }
        }

        return cachedData;
    }

    public void syncData(string meshType, List<Instance> allInstances) {
        if(allInstances.Count == 0) return;

        var renderer = mesh?.getMeshRenderer(meshType);
        if(renderer == null) return;

        if(cachedUpdateMethod == null || cachedSetMethod == null) {
            var methods = typeof(MeshRenderer).GetMethods();
            cachedUpdateMethod = methods.First(m => m.Name == nameof(MeshRenderer.updateInstanceData) && m.GetParameters().Length > 1);
            cachedSetMethod = methods.First(m => m.Name == nameof(MeshRenderer.setInstanceData) && m.GetParameters().Length > 1);
            cachedParamNames = cachedUpdateMethod.GetParameters().Select(p => p.Name!.ToLower()).ToArray();
            cachedUpdateParamTypes = cachedUpdateMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            cachedSetParamTypes = cachedSetMethod.GetParameters().Select(p => p.ParameterType).ToArray();
        }

        bool isInit = renderer.getInstanceVboInitialized();
        var method = isInit ? cachedUpdateMethod! : cachedSetMethod;
        var paramTypes = isInit ? cachedUpdateParamTypes! : cachedSetParamTypes!;

        var data = getData(allInstances);

        if(!cachedArgsByMeshType.TryGetValue(meshType, out var cached)) {
            var lists = cachedParamNames!.Select((key, i) => {
                var elemType = paramTypes[i].GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elemType);
                return (IList)Activator.CreateInstance(listType)!;
            }).ToArray();

            var args = lists.Cast<object?>().ToArray();
            cached = (args, lists);
            cachedArgsByMeshType[meshType] = cached;
        }

        for(int i = 0; i < cachedParamNames!.Length; i++) {
            string key = cachedParamNames[i];
            cached.lists[i].Clear();
            if(data.TryGetValue(key, out var val)) {
                foreach(var item in val) cached.lists[i].Add(item);
            }
        }

        method.Invoke(renderer, cached.args);
    }

    /**
     * 
     * Update
     *
     */
    // Update
    public void update() {
        if(tick == null || mesh == null) return;

        float deltaTime = tick.getDeltaTime() / 5.0f;

        cleanupEntity();

        foreach(var (id, l) in instances) {
            if(instanceStates.TryGetValue(id, out var state) && state == State.ACTIVE) continue;

            for(int i = 0; i < l.Count; i++) {
                var inst = l[i];

                if(wrap(deltaTime, ref inst, id, i)) {
                    l[i] = inst;
                    continue;
                }

                setSpawn(deltaTime, ref inst);
                l[i] = inst;
                MeshEntityCollider.update(id, i, inst.Position);
            }

            if(l.Count > 0) mesh.setPosition(id, l[0].Position);
        }

        updateData();
        updatePhysics();
    }

    // Update Data
    private void updateData() {
        foreach(var l in cachedByMeshType.Values) l.Clear();

        foreach(var (id, l) in instances) {
            string meshType = entityIdToMeshType.TryGetValue(id, out var t) ? t : id;
            if(!cachedByMeshType.ContainsKey(meshType)) cachedByMeshType[meshType] = new();
            cachedByMeshType[meshType].AddRange(l);
        }

        foreach(var (meshType, allInstances) in cachedByMeshType) {
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
    public void render(EntityProps entity) {
        var instanceList = 
            Enumerable.Repeat(entity, entity.Position.Count)
                .Select(spawn)
                .ToList();
        
        instances[entity.Id] = instanceList;
        instanceStates[entity.Id] = State.SLEEP;
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
    private void reset(ref Instance inst) {        
        inst.Position = setPosition();
        inst.Speed = setSpeed();
        inst.Rotation = setRotation();
    }

    /**
     * 
     * Cleanup
     *
     */
    public void cleanup() {
        MeshEntityCollider.cleanup();
        instances.Clear();
    }
    
    private void cleanupEntity() {
        if(MeshEntityCollider.colliderIds.Count == 0) return;
        if(collisionManager.getPendingRemovalsCount() == 0) return;
        
        MeshEntityCollider.cleanupRemoved();

        var removedEntities = instances.Keys
            .Where(id => !MeshEntityCollider.colliderIds.ContainsKey(id))
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
    public void removeInstance(string entityId, int index) {
        if(!instances.ContainsKey(entityId)) return;
        if(index >= instances[entityId].Count) return;
        instances[entityId].RemoveAt(index);
    }
}