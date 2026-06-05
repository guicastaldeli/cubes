/**

    Mesh Interaction Controller to
    handle the Mesh Breakability...

    */
namespace App.Root.Mesh;

using System.Data;
using App.Root.Collider;
using App.Root.Player;
using App.Root.Player.Inventory;
using App.Root.Utils;
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

        Mapper.Set<MeshInteractionController>();
        Mapper.Mouse(0, onBreak);
        Mapper.Mouse(1, onPlace);
    }
    
    // Get Held Mesh
    public PlacedMeshDef? getHeldMesh() {
        return heldMesh;
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

        var instanced = EventStream.get<Dictionary<string, List<string>>>("stream-id");
        if(instanced != null) {
            foreach(var (entityId, colliderList) in instanced) {
                int index = colliderList.IndexOf(hit);
                if(index >= 0) {
                    mesh.removeInstance(entityId, index);
                    break;
                }
            }
        }

        bool isEntity = IsEntity.BC(def.IsEntity);
        if(isEntity) EventStream.set("instanced-break", hit);

        EventStream.set("collider-remove", hit);
                
        collisionManager.removeCollider(hit);
        collisionManager.processRemovals();
        mesh.removeData(hit);
        WorldUpdater.getInstance().removeMesh(hit);

        var inventory = Inventory.getInstance();
        if(inventory != null) {            
            inventory.addItem(def);
        }

        heldMesh = def;
    }

    /**
    
        On Place
    
        */
    public void onPlace() {
        var inventoryInstance = Inventory.getInstance();
        if(inventoryInstance == null) return;

        var mainSlot = inventoryInstance.getActiveSlot();
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
        Vector3 halfExtends = HalfMesh.HalfExtents(data, scale);

        Vector3? point = data.isModel ?
            placementRaycaster.findPlacementPoint(HalfMesh.HalfHeight(data, scale)) :
            placementRaycaster.findPlacementPoint(halfExtends);
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
            def.MeshData,
            def.Color,
            def.IsEntity
        );

        bool isEntity = IsEntity.BC(def.IsEntity);
        if(isEntity) EventStream.set("instanced-place", (newId, def.MeshData, point.Value));

        Console.WriteLine($"placement point: {point.Value}");
        mainSlot.remove();
        heldMesh = 
            mainSlot.isEmpty ? 
            null : 
            mainSlot.def;
    }
}