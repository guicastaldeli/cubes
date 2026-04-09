namespace App.Root.Mesh;

static class MeshRegistry {
    private static HashSet<string> runtimeIds = new();

    // Register
    public static void register(string id) {
        runtimeIds.Add(id);
    }

    // Unregister
    public static void unregister(string id) {
        runtimeIds.Remove(id);
    }

    // Is Runtime
    public static bool isRuntime(string id) {
        return runtimeIds.Contains(id);
    }

    // Clear
    public static void clear() {
        runtimeIds.Clear();
    }
}