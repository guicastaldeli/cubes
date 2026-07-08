namespace App.Root.Chunk;
using System.Reflection;

public static class LODInjector {
    private static readonly Dictionary<object, bool> injected = new();

    /**
     *
     * Inject
     *
     */
    public static void Inject(object target) {
        if(target == null) return;
        if(injected.ContainsKey(target)) return;

        var type = target.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach(var field in fields) ProcessField(field);
        foreach(var prop in properties) ProcessProperty(prop);

        injected[target] = true;
    }

    /**
     *
     * Process
     *
     */
    // Process Field
    private static void ProcessField(FieldInfo field) {
        var attr = field.GetCustomAttribute<LODableAttribute>();
        if(attr == null) return;

        var configType = attr.ConfigType;
        var config = Activator.CreateInstance(configType) as LODConfig;
        if(config != null) LODManager.registerConfig(attr.Id, config);

        Console.WriteLine($"[LODInjector] Registered LOD config '{attr.Id}' for field {field.Name}");
    }

    // Process Property
    private static void ProcessProperty(PropertyInfo prop) {
        if(!prop.CanWrite) return;

        var attr = prop.GetCustomAttribute<LODableAttribute>();
        if(attr == null) return;

        var configType = attr.ConfigType;
        var config = Activator.CreateInstance(configType) as LODConfig;
        if(config != null) LODManager.registerConfig(attr.Id, config);

        Console.WriteLine($"[LODInjector] Registered LOD config '{attr.Id}' for property {prop.Name}");
    }
}