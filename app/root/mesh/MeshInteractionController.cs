/**

    Mesh Interaction Controller to
    handle the Mesh Breakability...

    */
namespace App.Root.Mesh;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Player;
using App.Root.World.Platform;
using OpenTK.Mathematics;

/**

    Helper Shape class of Meshes
    to help Controller detect 
    the collider type...

    */
static class Types {
    public const string CUBE = "cube";
    public const string SPHERE = "sphere";
    public const string TRIANGLE = "triangle";
}

class Shape {
    public Mesh mesh = null!;
    public MeshData data = null!;
    public CollisionManager collisionManager = null!;

    public Shape(Mesh mesh, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager; 
    }

    /**

        Update

        */
    public Shape update(MeshData data, string id) {
        if(data == null) return this;

        switch(data.colliderShape) {
            case Types.CUBE:
                collisionManager.addStaticCollider(new StaticObject(mesh.getBBox(id), id));
                break;
            case Types.SPHERE:
                collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
                break;
            case Types.TRIANGLE:
                collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
                break;
        }

        return this;
    }
}

/**

    Main Mesh Interaction Controller
    class.

    */
class MeshInteractionController {
    private Window window;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private Camera camera;

    private Raycaster raycaster;
    private PlacementRaycaster placementRaycaster;

    private PlacedMeshDef? heldMesh = null;
    private int placedCounter = 0;

    private Shape shape = null!;

    public MeshInteractionController(
        Window window,
        Camera camera,
        Mesh mesh,
        CollisionManager collisionManager,
        Raycaster raycaster
    ) {
        this.window = window;
        this.camera = camera;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.raycaster = raycaster;

        this.placementRaycaster = new PlacementRaycaster(
            camera, 
            mesh, 
            collisionManager, 
            raycaster
        );

        this.shape = new Shape(mesh, collisionManager);
    }
    
    // Get Held Mesh
    public PlacedMeshDef? getHeldMesh() {
        return heldMesh;
    }

    // Get Mesh Half Height
    private float getMeshHalfHeight(MeshData data) {
        float[]? vertices = data.getVertices();
        if(vertices == null) return 0.5f;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for(int i = 1; i < vertices.Length; i += 3) {
            if(vertices[i] < minY) minY = vertices[i];
            if(vertices[i] > maxY) maxY = vertices[i];
        }

        float val = (maxY - minY) / 2.0f;
        return val;
    }

    /**
    
        On Break
    
        */
    public void onBreak() {
        string? hit = raycaster.cast();
        if(hit == null) return;
        if(!MeshInteractionRegistry.getInstance().isBreakable(hit)) return;

        PlacedMeshDef? def = MeshInteractionRegistry.getInstance().getDef(hit);
        if(def == null) return;

        window.queueOnRenderThread(() => {
            mesh.remove(hit);
            collisionManager.removeCollider(hit);
        });

        MeshInteractionRegistry.getInstance().unregister(hit);
        MeshRegistry.unregister(hit);

        heldMesh = def;
    }

    /**
    
        On Place
    
        */
    public void onPlace() {
        if(heldMesh == null) return;

        MeshData data = MeshLoader.load(heldMesh.MeshType);
        float halfH = getMeshHalfHeight(data);

        Vector3? point = placementRaycaster.findPlacementPoint(halfH);
        if(point == null) {
            Console.WriteLine("No valid surface to place on!");
            return;
        } 

        string newId = $"{heldMesh.MeshType}_{placedCounter++}";
        PlacedMeshDef def = heldMesh;
        Vector3 placePos = point.Value;

        window.queueOnRenderThread(() => {
            MeshData data = MeshLoader.load(def.MeshType);
            mesh.add(newId, data);
            mesh.setPosition(newId, placePos);
            
            if(def.TexId > 0) {
                mesh.setTexture(
                    newId, 
                    def.TexId, 
                    def.TexPath ?? ""
                );
            }

            var renderer = mesh.getMeshRenderer(newId);
            if(renderer != null) renderer.isInteractive = true;

            shape.update(data, newId);

            MeshInteractionRegistry.getInstance().register(
                newId,
                State.BREAKABLE,
                def
            );
        });

        heldMesh = null;
    }
}