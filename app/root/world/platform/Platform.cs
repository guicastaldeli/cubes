namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Collider.Types;
using OpenTK.Mathematics;

class Platform : WorldHandler {
    private Mesh.Mesh mesh;
    private CollisionManager collisionManager;

    private string GRID_ID = "grid";
    private string MESH = "cube";

    private float x = 0.0f;
    private float y = 0.0f;
    private float z = 0.0f;
    private Vector3 offset = Vector3.Zero;
    
    private int sizeX = 10;
    private int sizeY = 3;
    private int sizeZ = 10;
    private float spacing = 1.0f;

    private bool initialized = false;
    
    public Platform(Mesh.Mesh mesh, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    } 

    // Set Position
    private void setPosition() {
        offset = new Vector3(x, y, z);
    }

    // Set
    private void set() {
        setPosition();

        List<Vector3> positions = new();
        float offsetX = -(sizeX / 2.0f) * spacing + offset.X;
        float offsetZ = -(sizeZ / 2.0f) * spacing + offset.Z;
        
        mesh.add(GRID_ID, MESH);
        Vector3 size = mesh.getSize(GRID_ID);
        Vector3 half = size / 2.0f;

        height = getHeight();

        for(int x = 0; x < sizeX; x++) {
            for(int y = 0; y < sizeY; y++) {
                for(int z = 0; z < sizeZ; z++) {
                    string id = $"cube_{x}_{y}_{z}";

                    float px = offsetX + x * spacing;
                    float py = (y * spacing) + offset.Y;
                    float pz = offsetZ + z * spacing;
                    positions.Add(new Vector3(px, py, pz));

                    collisionManager.addStaticCollider(new StaticObject(
                        new Vector3(px, py, pz), 
                        half.X, half.Y, half.Z,
                        id
                    ));
                }
            }
        }

        var renderer = mesh.getMeshRenderer(GRID_ID);
        if(renderer != null) {
            renderer.isInstanced = true;
            renderer.setInstancePositions(positions);
        }

        Console.WriteLine($"Platform draw calls: 1 (instanced {positions.Count} cubes)");
    }

    // Height
    public Vector3 getHeight() {
        Vector3 meshSize = mesh.getSize(GRID_ID);
        float topY = offset.Y + (sizeY * spacing) + (meshSize.Y / 2.0f);
        Vector3 res = new Vector3(offset.X, topY, offset.Z); 
        return res;
    }

    public static Vector3? height {
        get;
        private set;
    }

    ///
    /// Render
    /// 
    public override void render() {
        if(!initialized) {
            set();
            initialized = true;
        }
    }

    ///
    /// Update
    /// 
    public override void update() {
        
    }
}