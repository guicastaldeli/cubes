/**

    Entity Spawner class

    */
namespace App.Root.World.Entity;

using App.Root.Collider;
using App.Root.Collider.Types;
using OpenTK.Mathematics;

/**

    Entity Area helper class.

    */
static class EntityArea {
    private static CollisionManager collisionManager = null!;
    private static BoundaryObject boundaryObject = null!;

    /**
    
        Init
    
        */
    public static void init(CollisionManager collisionManager) {
        EntityArea.collisionManager = collisionManager;
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
    public static (Vector3 center, Vector3 size) getSpawnPoint() {
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
    private Dictionary<string, float> rotations = new();
    private Dictionary<string, string> colors = new();
    private Dictionary<string, List<float>> speeds = new();
    
    public EntitySpawner(Tick tick, Mesh.Mesh mesh, CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;

        this.startZ = SPAWN_AREA;
        this.endZ = -SPAWN_AREA;

        EntityArea.init(collisionManager);
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

    // Speed
    public EntitySpawner setSpeed(float min, float max) {
        this.minSpeed = min;
        this.maxSpeed = max;
        return this;
    }

    private float randomSpeed() {
        float val = 
            minSpeed + 
            (float)range.NextDouble() * 
            (maxSpeed - minSpeed);
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
                pos[i] = spawnEntity();
            }
        }
    }

    /**
    
        Spawn
    
        */
    private Vector3 spawnEntity() {
        var (center, size) = EntityArea.getSpawnPoint();

        float x = center.X + (float)(range.NextDouble() * size.X - size.X / 2.0f);
        float y = center.Y + (float)(range.NextDouble() * size.Y - size.Y / 2.0f);
        float z = center.Z;

        Vector3 val = new Vector3(x, y, z);
        return val;
    }

    private (List<Vector3>, List<float>) spawn(int count) {
        List<Vector3> pos = 
            Enumerable.Range(0, count)
                .Select(_ => spawnEntity())
                .ToList();
        var speed = 
            Enumerable.Range(0, count)
                .Select(_ => randomSpeed())
                .ToList();
        
        var val = (pos, speed);
        return val;
    }

    /**
    
        Update
    
        */
    public void update() {
        if(tick == null || mesh == null) return;

        float fSpeed = 3.0f;
        float deltaTime = tick.getDeltaTime();
        float gSpeed = fSpeed * deltaTime;

        foreach(var (id, pos) in positions) {
            List<float> speed = speeds[id];
            
            for(int i = 0; i < pos.Count; i++) {
                pos[i] = new Vector3(
                    pos[i].X,
                    pos[i].Y,
                    pos[i].Z - speed[i] * gSpeed
                );
            }

            wrap(pos);

            mesh.getMeshRenderer(id)?.updateInstanceData(
                pos,
                Converter.ToRgbaList(colors[id], pos.Count),
                Converter.ToRotationList(rotations[id], pos.Count)
            );
        }
    }

    /**
    
        Render
    
        */
    public void render(EntityProps entity) {
        var (positionsVal, speedsVal) = spawn(entity.Position.Count);
        string colorsVal = entity.Color;
        float rotationsVal = entity.Rotation;

        positions[entity.Id] = positionsVal;
        rotations[entity.Id] = rotationsVal;
        colors[entity.Id] = colorsVal;
        speeds[entity.Id] = speedsVal;
    }
}