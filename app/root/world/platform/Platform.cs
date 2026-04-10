namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Resource;
using OpenTK.Mathematics;

class Platform : WorldHandler {
    private Mesh mesh;
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
    
    public Platform(Mesh mesh, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    } 

    // Set Position
    private void setPosition() {
        offset = new Vector3(x, y, z);
    }

    /**

        Temporary Cube to test 
        raycaster for objects

        */
    public void set2() {
        string id = "cubic";
        string mesht = "cube";
        MeshData data = MeshLoader.load(mesht);
        mesh.add(id, data);
        mesh.setPosition(id, 0.0f, 3.0f, -3.0f);

        var renderer = mesh.getMeshRenderer(id);
        if(renderer != null) renderer.isInteractive = true;

        string texPath = "env/test.jpg";
        int texId = TextureLoader.load(texPath);
        mesh.setTexture(id, texId, texPath);

        collisionManager.addStaticCollider(new StaticObject(mesh.getBBox(id), id));
    }

    ///
    /// Set
    /// 
    private void setMesh(List<Vector3> positions) {
        var renderer = mesh.getMeshRenderer(GRID_ID);
        if(renderer != null) {
            renderer.isInstanced = true;
            renderer.setInstancePositions(positions);
        }
    }

    private void set(bool renderMesh = true) {
        setPosition();
        MeshRegistry.register(GRID_ID);

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

        if(renderMesh) {
            setMesh(positions);
            Console.WriteLine($"Platform draw calls: 1 (instanced {positions.Count} cubes)");
        } else {
            mesh.remove(GRID_ID);
        }

        initialized = true;
    }

    public void setClient() {
        if(initialized) return;
        set(renderMesh: false);
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
            set2();
            initialized = true;
        }
    }

    ///
    /// Update
    /// 
    public override void update() {
        
    }
}