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
    UNBREAKABLE
}

record PlacedMeshDef(
    string MeshType,
    string? TexPath,
    int TexId
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
    public void register(string id, State b, PlacedMeshDef def) {
        breakMap[id] = b;
        defMap[id] = def;
    }    

    /**
    
        Unregister

        */
    public void unregister(string id) {
        breakMap.Remove(id);
        defMap.Remove(id);
    }
}