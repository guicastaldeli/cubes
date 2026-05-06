/**

    Entity Spawner class

    */
namespace App.Root.World.Entity;
using OpenTK.Mathematics;

class EntitySpawner {
    private const float SPAWN_AREA = 50.0f;

    private Tick? tick;
    private Mesh.Mesh? mesh;

    private Random range = new Random();

    private float startZ;
    private float endZ;

    private float minSpeed = 1.0f;
    private float maxSpeed = 10.0f;

    private Dictionary<string, List<Vector3>> positions = new();
    private Dictionary<string, float> rotations = new();
    private Dictionary<string, string> colors = new();
    private Dictionary<string, List<float>> speeds = new();
    
    public EntitySpawner(Tick tick, Mesh.Mesh mesh) {
        this.tick = tick;
        this.mesh = mesh;
    }
    public EntitySpawner() {
        this.startZ = SPAWN_AREA;
        this.endZ = -SPAWN_AREA;
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
        float b = SPAWN_AREA * 2.0f;
        
        float x = (float)(range.NextDouble() * b - SPAWN_AREA);
        float y = 0.0f;
        float z = (float)(range.NextDouble() * b - SPAWN_AREA);

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

        foreach(var (id, pos) in positions) {
            List<float> speed = speeds[id];
            
            for(int i = 0; i < pos.Count; i++) {
                pos[i] = new Vector3(
                    pos[i].X,
                    pos[i].Y,
                    pos[i].Z - speed[i] * tick.getDeltaTime()
                );
            }

            wrap(pos);

            mesh.getMeshRenderer(id)?.updateInstanceData(
                pos,
                Converter.ToRgbaList(colors[id], positions.Count),
                Converter.ToRotationList(rotations[id], positions.Count)
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