/**

    Mesh Interaction Controller to
    handle the Mesh Breakability...

    */
namespace App.Root.Mesh;
using App.Root.Collider;
using App.Root.Player;
using App.Root.World;
using OpenTK.Mathematics;

/**

    Main Mesh Interaction Controller
    class.

    */
class MeshInteractionController {
    private Window window;
    private Mesh mesh;
    private Input input;
    private CollisionManager collisionManager;
    private Camera camera;

    private Raycaster raycaster;
    private PlacementRaycaster placementRaycaster;

    private PlacedMeshDef? heldMesh = null;
    private int placedCounter = 0;

    public MeshInteractionController(
        Window window,
        Camera camera,
        Mesh mesh,
        Input input,
        CollisionManager collisionManager,
        Raycaster raycaster
    ) {
        this.window = window;
        this.camera = camera;
        this.mesh = mesh;
        this.input = input;
        this.collisionManager = collisionManager;
        this.raycaster = raycaster;

        this.placementRaycaster = new PlacementRaycaster(
            camera,
            collisionManager, 
            raycaster
        );
    }
    
    // Get Held Mesh
    public PlacedMeshDef? getHeldMesh() {
        return heldMesh;
    }

    // Get Mesh Half Height
    private float getMeshHalfHeight(MeshData data, Vector3 scale) {
        float[]? vertices = data.getVertices();
        if(vertices == null) return 0.5f;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for(int i = 1; i < vertices.Length; i += 3) {
            if(vertices[i] < minY) minY = vertices[i];
            if(vertices[i] > maxY) maxY = vertices[i];
        }

        float meshHeight = (maxY - minY) * scale.Y;
        
        if(data.isModel) return meshHeight;
        return meshHeight / 2.0f;
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

        var inventory = 
            input.getPlayerInputMap()
            .getInventory();
        if(inventory != null) {            
            inventory.getInventory().addItem(def);
        }

        heldMesh = def;

        WorldUpdater.getInstance().removeMesh(hit);
    }

    /**
    
        On Place
    
        */
    public void onPlace() {
        var inventoryInstance = input.getPlayerInputMap().getInventory();
        if(inventoryInstance == null) return;

        var inventory = inventoryInstance.getInventory();
        var mainSlot = inventory.getActiveSlot();
        if(mainSlot.isEmpty || mainSlot.def == null) return;

        PlacedMeshDef def = mainSlot.def;

        MeshData? data = LoadMeshData.L(
            def.MeshType, 
            def.MeshData!,
            def.ColliderShape,
            def.ColliderRadius
        );
        if(data == null) return;
        
        Vector3 scale = def.Scale ?? mesh.getDefaultScale(data);
        float halfH = getMeshHalfHeight(data, scale);
        float placementH = data.isModel ? 0.0f : halfH;

        Vector3? point = placementRaycaster.findPlacementPoint(placementH);
        if(point == null) {
            Console.WriteLine("No valid surface to place on!");
            return;
        } 

        string newId = $"{def.MeshType}_{placedCounter++}";
        
        WorldUpdater.getInstance().addMesh(
            newId, 
            def.MeshType, 
            point.Value, 
            scale, 
            def.TexId, 
            def.TexPath,
            def.PhysicsType,
            def.MeshData
        );

        mainSlot.remove();
        heldMesh = 
            mainSlot.isEmpty ? 
            null : 
            mainSlot.def;
    }
}