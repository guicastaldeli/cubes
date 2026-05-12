/**

    Entity Spawner class

    */
namespace App.Root.World.Entity;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using OpenTK.Mathematics;

/**

    Entity Instance

    */
struct Instance {
    public Vector3 Position;
    public float Speed;
    public float Rotation;
    public float Lifetime;
    public string Color;
    public string? Tex;

    /**
    
        Data
    
        */
    // Main Data
    private Instance Data() {
        return this;
    }

    // Instance Data
    public (Vector3 position, float[] color, float rotation, string? tex) InstData() {
        var data = Data();
        var val = (data.Position, Converter.ToRgba(data.Color), data.Rotation, data.Tex);
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
        var collider = new BoundaryObject(EntitySpawner.SPAWN_AREA);
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

    Entity Spawner main class

    */
class EntitySpawner {
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
    
    public EntitySpawner(Tick tick, Mesh mesh, CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;

        this.startZ = -SPAWN_AREA;
        this.endZ = SPAWN_AREA;

        SpawnPoint.init(collisionManager);
        EntityCollider.init(mesh, collisionManager);
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
        float val = 
            MIN_SPEED + 
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
        float val = MIN_LIFETIME +
            (float)range.NextDouble() * 
            (MAX_LIFETIME - MIN_LIFETIME);
        return val;
    }

    /**
    
        Wrap
    
        */
    private bool wrap(float deltaTime, ref Instance inst) {
        float l = 0.0f;
        inst.Lifetime -= deltaTime;

        if(inst.Lifetime <= l || isOutside(inst.Position)) {
            reset(ref inst);
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
    private (List<Vector3>, List<float[]>, List<float>, List<string?>) getData(string id, List<Instance> list) {
        var inst = list.Select(i => i.InstData()).ToList();
        
        var positions = inst.Select(d => d.position).ToList();
        var colors = inst.Select(d => d.color).ToList();
        var rotations = inst.Select(d => d.rotation).ToList();
        var textures = inst.Select(d => d.tex).ToList();

        return (positions, colors, rotations, textures);
    }

    /**
    
        Update
    
        */
    public void update() {
        if(tick == null || mesh == null) return;

        float deltaTime = tick.getDeltaTime() / 5.0f;

        foreach(var (id, l) in instances) {
            for(int i = 0; i < l.Count; i++) {
                var inst = l[i];

                if(wrap(deltaTime, ref inst)) {
                    l[i] = inst;
                    EntityCollider.update(id, i, inst.Position);
                    continue;
                }

                setSpawn(deltaTime, ref inst);
                l[i] = inst;
                EntityCollider.update(id, i, inst.Position);
            }

            var (positions, colors, rotations, textures) = getData(id, l);
            mesh.getMeshRenderer(id)?.updateInstanceData(positions, colors, rotations, textures);
            if(l.Count > 0) mesh.setPosition(id, l[0].Position);
        }
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
        EntityCollider.create(entity, instanceList);
    }

    /**
    
        Reset
    
        */
    private void reset(ref Instance inst) {        
        inst.Position = setPosition();
        inst.Speed = setSpeed();
        inst.Rotation = setRotation();
        inst.Lifetime = setLifetime();
    }

    /**

        Cleanup

        */
    private void cleanup() {
        EntityCollider.cleanup();
        instances.Clear();
    }
}