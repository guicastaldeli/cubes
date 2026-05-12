/**

    Entity Spawner class

    */
namespace App.Root.World.Entity;

using App.Root.Collider;
using App.Root.Collider.Types;
using OpenTK.Mathematics;

/**

    Spawn Point helper class.

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

        float z = dist;
        float centerX = 0.0f;
        float centerY = 0.0f;

        float d = dist * 0.05f;
        float width = d;
        float height = d;

        return (
            new Vector3(centerX, centerY, z),
            new Vector3(width, height, 0)
        );
    }
}

/**

    Entity Spawner main class.

    */
class EntitySpawner {
    public static float SPAWN_AREA = 50.0f;

    private Tick? tick;
    private Mesh.Mesh? mesh;
    private CollisionManager collisionManager;

    private Random range = new Random();

    private float startZ;
    private float endZ;

    private float minSpeed = 1.0f;
    private float maxSpeed = 10.0f;

    private Dictionary<string, List<Vector3>> positions = new();
    private Dictionary<string, List<float>> rotations = new();
    private Dictionary<string, List<float>> speeds = new();
    private Dictionary<string, string> colors = new();
    
    public EntitySpawner(Tick tick, Mesh.Mesh mesh, CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;

        this.startZ = SPAWN_AREA;
        this.endZ = -SPAWN_AREA;

        SpawnPoint.init(collisionManager);
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

    /**
    
        Speed
    
        */
    private float setSpeed() {
        float val = 
            minSpeed + 
            (float)range.NextDouble() * 
            (maxSpeed - minSpeed);
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

    // Is Outside
    public bool isOutside(Vector3 position) {
        bool val = position.Z < endZ;
        return val;
    }

    // Get Positions
    public List<Vector3> getPositions(string id) {
        List<Vector3> val = positions[id];
        return val;
    }

    /**
    
        Wrap
    
        */
    private void wrap(List<Vector3> pos) {
        for(int i = 0; i < pos.Count; i++) {
            if(isOutside(pos[i])) {
                pos[i] = setSpawn();
            }
        }
    }

    /**
    
        Spawn
    
        */
    private Vector3 setSpawn() {
        var (center, size) = SpawnPoint.get();

        float x = center.X + (float)(range.NextDouble() * size.X - size.X / 2.0f);
        float y = center.Y + (float)(range.NextDouble() * size.Y - size.Y / 2.0f);
        float z = center.Z;

        Vector3 val = new Vector3(x, y, z);
        return val;
    }

    private (List<Vector3>, List<float>, List<float>) spawn(int count) {
        List<Vector3> pos = 
            Enumerable.Range(0, count)
                .Select(_ => setSpawn())
                .ToList();
        var speed = 
            Enumerable.Range(0, count)
                .Select(_ => setSpeed())
                .ToList();
        var rotatations =
            Enumerable.Range(0, count)
                .Select(_ => setRotation())
                .ToList();
        
        var val = (pos, rotatations, speed);
        return val;
    }

    /**
    
        Update
    
        */
    public void update() {
        if(tick == null || mesh == null) return;

        float fSpeed = 3.0f;
        float deltaTime = tick.getDeltaTime();
        float gSpeed = (fSpeed * deltaTime) / 5.0f;

        float rAngle = 360.0f;
        float rotationSpeed = gSpeed * 10.0f;

        foreach(var (id, pos) in positions) {
            List<float> speed = speeds[id];
            List<float> rotation = rotations[id];
            
            for(int i = 0; i < pos.Count; i++) {
                pos[i] = new Vector3(
                    pos[i].X,
                    pos[i].Y,
                    pos[i].Z - speed[i] * gSpeed
                );
                rotation[i] = 
                    (rotation[i] + speed[i] * rotationSpeed) % 
                    rAngle;
            }

            wrap(pos);

            mesh.getMeshRenderer(id)?.updateInstanceData(
                pos,
                Converter.ToRgbaList(colors[id], pos.Count),
                rotation
            );
        }
    }

    /**
    
        Render
    
        */
    public void render(EntityProps entity) {
        var (positionsVal, rotationsVal, speedsVal) = spawn(entity.Position.Count);

        positions[entity.Id] = positionsVal;
        rotations[entity.Id] = rotationsVal;
        colors[entity.Id] = entity.Color;
        speeds[entity.Id] = speedsVal;
    }
}