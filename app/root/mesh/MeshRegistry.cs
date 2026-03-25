namespace App.Root.Mesh;

static class MeshRegistry {
    private static HashSet<string> runtimeIds = new();

    public static void register(string id) {
        runtimeIds.Add(id);
    }

    public static void unregister(string id) {
        runtimeIds.Remove(id);
    }

    public static bool isRuntime(string id) {
        return runtimeIds.Contains(id);
    }
}