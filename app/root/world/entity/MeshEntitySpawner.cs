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

        Get
    
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
    
        Init
    
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
        collisionManager.addStaticCollider(collider);
        boundaryObject = collider;
    }

    /**
    
        Spawn Point
    
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
    
        Mesh Entity State
    
        */
    private enum State {
        SLEEP,
        ACTIVE
    }

    /**
    
        Mesh Entity main
    
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

    private Dictionary<string, (MeshData data, Vector3 position)> pendingPhysics = new();
    
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

    /**
    
        Position
    
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
    
        Rotation
    
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
    
        Speed
    
        */
    private float setSpeed() {
        float val = MIN_SPEED + 
            (float)range.NextDouble() * 
            (MAX_SPEED - MIN_SPEED);
        
        return val;
    }

    /**
    
        Color
    
        */
    private string setColor(EntityProps entity) {
        string val = entity.Color;
        return val;
    }

    /**
    
        Lifetime
    
        */
    private float setLifetime() {
        float lifetime =  MIN_LIFETIME +
            (float)range.NextDouble() * 
            (MAX_LIFETIME - MIN_LIFETIME);
        
        float val = ConvertTime.MinutesToSeconds(lifetime);
        return val;
    }

    /**
    
        Wrap
    
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
    
        Texture
    
        */
    private string? setTexture(EntityProps entity) {
        string? val = entity.Tex;
        return val;
    }

    /**
    
        Spawn
    
        */
    private Instance spawn(EntityProps entity) {
        Instance e = new Instance {
            Position = setPosition(),
            Speed = setSpeed(),
            Rotation = setRotation(),
            Lifetime = setLifetime(),
            Color = setColor(entity),
            Tex = setTexture(entity)
        };

        return e;
    }

    private void setSpawn(float deltaTime, ref Instance inst) {
        defPosition(deltaTime, ref inst);
        defRotation(deltaTime, ref inst);
    }

    /**
    
        Data
    
        */
    private Dictionary<string, List<object>> getData(List<Instance> list) {
        var result = new Dictionary<string, List<object>>();

        foreach(var inst in list) {
            foreach(var (key, val) in inst.GetData()) {
                if(!result.ContainsKey(key)) result[key] = new List<object>();
                result[key].Add(val);
            }
        }

        return result;
    }

    public void syncData(string entityId) {
        if(!instances.ContainsKey(entityId)) return;

        var list = instances[entityId];
        if(list.Count == 0) return;

        var data = getData(list);
        var renderer = mesh?.getMeshRenderer(entityId);
        if(renderer == null) return;

        bool isInit = renderer.getInstanceVboInitialized();

        string methodName = isInit
            ? nameof(MeshRenderer.updateInstanceData)
            : nameof(MeshRenderer.setInstanceData);
        var method = typeof(MeshRenderer).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length > 1);
    
        var args = method.GetParameters().Select(p => {
            string key = p.Name!.ToLower();

            if(data.TryGetValue(key, out var val)) {
                var listType = typeof(List<>).MakeGenericType(p.ParameterType.GetGenericArguments()[0]);
                var result = (IList)Activator.CreateInstance(listType)!;
                foreach(var item in val) result.Add(item);
                return (object?)result;
            }
            
            return Activator.CreateInstance(p.ParameterType);
        }).ToArray();

        method.Invoke(renderer, args);
    }

    /**
    
        Update
    
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

            syncData(id);
            if(l.Count > 0) mesh.setPosition(id, l[0].Position);

        }

        updatePhysics();
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
    
        Render
    
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

        On Events

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

            syncData(entityId);
        });

        // Instanced Place
        EventStream.on("instanced-place", (data) => {
            if(data is not (string id, MeshData meshData, Vector3 position)) return;
            pendingPhysics[id] = (meshData, position);
        });
    }

    /**
    
        Reset
    
        */
    private void reset(ref Instance inst) {        
        inst.Position = setPosition();
        inst.Speed = setSpeed();
        inst.Rotation = setRotation();
    }

    /**

        Cleanup

        */
    public void cleanup() {
        MeshEntityCollider.cleanup();
        instances.Clear();
    }
    
    private void cleanupEntity() {
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
    
        Remove
    
        */
    public void removeInstance(string entityId, int index) {
        if(!instances.ContainsKey(entityId)) return;
        if(index >= instances[entityId].Count) return;
        instances[entityId].RemoveAt(index);
    }
}