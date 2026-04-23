/**
    
    Mesh Registry class
    for general mesh registering.
    
    */
namespace App.Root.Mesh;

static class MeshRegistry {
    private static HashSet<string> runtimeIds = new();

    // Is Runtime
    public static bool isRuntime(string id) {
        return runtimeIds.Contains(id);
    }

    /**
    
        Register
    
        */
    public static void register(string id) {
        runtimeIds.Add(id);
    }

    /**
    
        Unregister
    
        */
    public static void unregister(string id) {
        runtimeIds.Remove(id);
    }

    /**
    
        Clear
    
        */
    public static void clear() {
        runtimeIds.Clear();
    }
}