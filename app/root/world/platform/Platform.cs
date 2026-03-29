namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Collider.Types;
using OpenTK.Mathematics;

class Platform : WorldHandler {
    private Mesh.Mesh mesh;
    private CollisionManager collisionManager;

    private string GRID_ID = "grid";
    private string MESH = "cube";
    
    private int sizeX = 10;
    private int sizeY = 3;
    private int sizeZ = 10;
    private float spacing = 4.0f;
    
    public Platform(Mesh.Mesh mesh, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    } 

    // Set
    private void set() {
        List<Vector3> positions = new();
        float offsetX = -(sizeX / 2.0f) * spacing;
        float offsetZ = -(sizeZ / 2.0f) * spacing;
        
        
        mesh.add(GRID_ID, MESH);
        Vector3 size = mesh.getSize(GRID_ID);
        Vector3 half = size / 2.0f;

        for(int x = 0; x < sizeX; x++) {
            for(int y = 0; y < sizeY; y++) {
                for(int z = 0; z < sizeZ; z++) {
                    string id = $"cube_{x}_{y}_{z}";

                    float px = offsetX + x * spacing;
                    float py = y * spacing;
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

    ///
    /// Render
    /// 
    public override void render() {
        set();
    }

    ///
    /// Update
    /// 
    public override void update() {
        
    }
}