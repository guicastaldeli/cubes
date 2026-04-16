
using OpenTK.Mathematics;

/**

    Mesh Interaction Registry to
    handle the Mesh Breakability...

    */
namespace App.Root.Mesh;

/**

    Mesh state for
    breakability

    */
enum State {
    BREAKABLE,
    UNBREAKABLE,
    GRID
}

record PlacedMeshDef(
    string MeshType,
    string TexPath,
    int TexId,
    Vector3? Scale = null,
    string? InstanceId = null
);

/**

    Main mesh interaction registry
    class.

    */
class MeshInteractionRegistry {
    private static MeshInteractionRegistry? instance;
    
    private Dictionary<string, State> breakMap = new();
    private Dictionary<string, PlacedMeshDef> defMap = new();
    
    public static MeshInteractionRegistry getInstance() {
        return instance ??= new();
    }

    // Is Breakable
    public bool isBreakable(string id) {
        bool val = 
            breakMap.TryGetValue(id, out var b) &&
            b == State.BREAKABLE;
        return val; 
    }

    // Is Grid
    public bool isGrid(string id) {
        if(!breakMap.TryGetValue(id, out var b)) {
            return true;
        }
        return b == State.GRID;
    }

    // Get Def
    public PlacedMeshDef? getDef(string id) {
        PlacedMeshDef? val = 
            defMap.TryGetValue(id, out var d) ?
            d :
            null;
        return val;
    }

    /**
    
        Register

        */
    public void setRegister(string id, State b, PlacedMeshDef def) {
        breakMap[id] = b;
        defMap[id] = def;
    }    

    public void register(string id, State state, Mesh mesh) {
        var renderer = mesh.getMeshRenderer(id);
        if(renderer == null) return;

        var data = mesh.getData(id);

        string meshType = data?.meshType ?? id;
        string? texPath = renderer.getTexPath();
        int texId = renderer.getTexId();
        Vector3? scale = 
            (renderer != null && renderer.isScaled()) ? 
            renderer.getScale() : 
            null;

        setRegister(id, state, new PlacedMeshDef(
            meshType, 
            texPath, 
            texId, 
            scale,
            id
        ));
    }

    /**
    
        Unregister

        */
    public void unregister(string id) {
        breakMap.Remove(id);
        defMap.Remove(id);
    }
}