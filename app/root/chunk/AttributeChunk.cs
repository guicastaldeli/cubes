namespace App.Root.Chunk;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

/**

    Chunk Attributes

    */

/**

    Chunked Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
class ChunkedAttribute : Attribute {
    public int RenderDistance { 
        get; 
    }

    public ChunkedAttribute(int renderDistance = 8) {
        RenderDistance = renderDistance;
    }

    /**
     * 
     * Register
     *
     */
    public static void R(ChunkHandler handler, List<ChunkHandler> chunkedHandlers, Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks) {
        var attr = handler.GetType().GetCustomAttribute<ChunkedAttribute>();

        if(attr != null) {
            chunkedHandlers.Add(handler);
            handlerActiveChunks[handler] = new HashSet<ChunkCoord>();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[ChunkManager] Chunked: {handler.GetType().Name}");
            Console.ResetColor();
        }
    }
}

/**

    Instance Chunked Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
class IChunkedAttribute : Attribute {
    /**
     * 
     * Register
     *
     */
    public static void R(ChunkHandler handler, List<ChunkHandler> globalHandlers) {
        var attr = handler.GetType().GetCustomAttribute<IChunkedAttribute>();
        
        if(attr != null) {
            globalHandlers.Add(handler);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[ChunkManager] Global: {handler.GetType().Name}");
            Console.ResetColor();
        }
    }
}

/**

    Scanner

    */

/**

    Scanner Attribute 

    */
[AttributeUsage(AttributeTargets.Method)]
class ScanAttribute : Attribute {}

/**

    Scanner main class

    */
static class Scanner {
    private static HashSet<object> scanned = new(); 

    // Register Handler
    private static void RegisterHandler(
        ChunkHandler handler,
        List<ChunkHandler> chunkedHandlers,
        List<ChunkHandler> globalHandlers,
        Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks
    ) {
        var type = handler.GetType();
        var declaringType = type.DeclaringType;
        bool isNested = declaringType != null;

        if(type.GetCustomAttribute<ChunkedAttribute>() != null && !chunkedHandlers.Contains(handler)) {
            chunkedHandlers.Add(handler);
            handlerActiveChunks[handler] = new HashSet<ChunkCoord>();
            
            if(isNested) {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[ChunkManager] Chunked (in {declaringType!.Name}): {type.Name}");
                Console.ResetColor();
            }
        }
        if(type.GetCustomAttribute<IChunkedAttribute>() != null && !globalHandlers.Contains(handler)) {
            globalHandlers.Add(handler);

            if(isNested) {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[ChunkManager] Global (in {declaringType!.Name}): {type.Name}");
                Console.ResetColor();
            }
        }
    }

    // Resolve Params
    private static object?[]? ResolveParams(ParameterInfo[] parameters, object parent) {
        var args = new object?[parameters.Length];
        var parentType = parent.GetType();

        for(int i = 0; i < parameters.Length; i++) {
            var paramType = parameters[i].ParameterType;

            var field = FindFieldOfType(parentType, paramType, parent);
            if(field != null) {
                args[i] = field;
                continue;
            }

            var found = FindValueOfType(parent, paramType);
            if(found != null) {
                args[i] = found;
                continue;
            }

            if(parameters[i].HasDefaultValue) {
                args[i] = parameters[i].DefaultValue;
            } else if(paramType.IsValueType) {
                args[i] = Activator.CreateInstance(paramType);
            } else {
                return null;
            }
        }

        return args;
    }

    /**
     *
     * Instantiate
     *
     */
    // Instantiate Nested
    private static void InstantiateNested(
        object parent,
        Type type,
        List<ChunkHandler> chunkedHandlers,
        List<ChunkHandler> globalHandlers,
        Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks
    ) {
        var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

        foreach(var nestedType in nestedTypes) {
            var attrChunked = nestedType.GetCustomAttribute<ChunkedAttribute>();
            var attrIChunked = nestedType.GetCustomAttribute<IChunkedAttribute>();

            if(attrChunked != null || attrIChunked != null) {
                var instance = FindInstance(parent, nestedType);

                if(instance != null && scanned.Add(instance)) {
                    if(instance is ChunkHandler ch) {
                        RegisterHandler(ch, chunkedHandlers, globalHandlers, handlerActiveChunks);
                    }
                    else if(parent is ChunkHandler parentHandler) {
                        if(attrChunked != null && !chunkedHandlers.Contains(parentHandler)) {
                            chunkedHandlers.Add(parentHandler);
                            handlerActiveChunks[parentHandler] = new HashSet<ChunkCoord>();
                            
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine($"[ChunkManager] Chunked (via {parent.GetType().Name}.{nestedType.Name}): {parent.GetType().Name}");
                            Console.ResetColor();
                        }
                        if(attrIChunked != null && !globalHandlers.Contains(parentHandler)) {
                            globalHandlers.Add(parentHandler);
                            
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.WriteLine($"[ChunkManager] Global (via {parent.GetType().Name}.{nestedType.Name}): {parent.GetType().Name}");
                            Console.ResetColor();
                        }
                    }

                    ScanRecursive(instance, nestedType, chunkedHandlers, globalHandlers, handlerActiveChunks);
                    InstantiateNested(instance, nestedType, chunkedHandlers, globalHandlers, handlerActiveChunks);
                    return;
                }

                if(typeof(ChunkHandler).IsAssignableFrom(nestedType)) {
                    var inst = TryInstantiate(parent, nestedType);
                    if(inst != null && scanned.Add(inst)) {
                        RegisterHandler(inst, chunkedHandlers, globalHandlers, handlerActiveChunks);
                        ScanRecursive(inst, nestedType, chunkedHandlers, globalHandlers, handlerActiveChunks);
                        InstantiateNested(inst, nestedType, chunkedHandlers, globalHandlers, handlerActiveChunks);
                    }
                }
            } 
        }
    }

    // Try Instantiate
    private static ChunkHandler? TryInstantiate(object parent, Type nestedType) {
        var existing = FindInstance(parent, nestedType);
        if(existing is ChunkHandler ch) return ch;

        var ctors = nestedType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(var ctor in ctors) {
            var parameters = ctor.GetParameters();
            if(parameters.Length == 0) {
                try {
                    ChunkHandler val = (ChunkHandler)ctor.Invoke(null);
                    return val;
                } catch {
                    Console.WriteLine("Next constructor (params)...");
                }
            }

            var args = ResolveParams(parameters, parent);
            if(args != null) {
                try {
                    ChunkHandler val = (ChunkHandler)ctor.Invoke(args);
                    return val;
                } catch {
                    Console.WriteLine("Next constructor (args)...");
                }
            }
        }

        try {
            ChunkHandler val = (ChunkHandler)Activator.CreateInstance(nestedType, true)!;
        } catch {
            Console.WriteLine("Cant instantiate...");
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ChunkManager] Cannot instantiate nested: {parent.GetType().Name}.{nestedType.Name} - no resolvable constructor");
        Console.ResetColor();
        
        return null;
    }

    /**
     *
     * Find
     *
     */
    // Find Instance
    private static object? FindInstance(object parent, Type nestedType) {
        var fields = parent.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(var field in fields) {
            if(field.FieldType == nestedType) {
                return field.GetValue(parent);
            }
        }

        var props = parent.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(var prop in props) {
            if(prop.PropertyType == nestedType && prop.CanRead) {
                return prop.GetValue(parent);
            }
        }

        return null;
    }

    // Find Field of Type
    private static object? FindFieldOfType(Type parentType, Type targetType, object instance) {
        var fields = parentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(var field in fields) {
            if(targetType.IsAssignableFrom(field.FieldType)) {
                object? val = field.GetValue(instance);
                return val;
            }
        }

        return null;
    }

    // Find Value of Type
    private static object? FindValueOfType(object parent, Type targetType) {
        if(parent == null) return null;

        var type = parent.GetType();

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(var field in fields) {
            if(targetType.IsAssignableFrom(field.FieldType)) {
                object? val = field.GetValue(parent);
                return val;
            }
        }

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(var prop in props) {
            if(targetType.IsAssignableFrom(prop.PropertyType) && prop.CanRead) {
                object? val = prop.GetValue(parent);
                return val;
            }
        }

        return null;
    }

    /**
     *
     * Scan
     *
     */
    // Scan
    public static void Scan(
        ChunkHandler handler, 
        List<ChunkHandler> chunkedHandlers, 
        List<ChunkHandler> globalHandlers,
        Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks
    ) {
        scanned.Clear();
        ScanRecursive(handler, handler.GetType(), chunkedHandlers, globalHandlers, handlerActiveChunks);
        InstantiateNested(handler, handler.GetType(), chunkedHandlers, globalHandlers, handlerActiveChunks);
    }

    // Scan Recursive
    private static void ScanRecursive(
        object instance,
        Type type,
        List<ChunkHandler> chunkedHandlers,
        List<ChunkHandler> globalHandlers,
        Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks
    ) {
        if(instance == null || !scanned.Add(instance)) return;

        if(instance is ChunkHandler handler && instance != null) {
            RegisterHandler(handler, chunkedHandlers, globalHandlers, handlerActiveChunks);
        }

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach(var field in fields) {
            var value = field.GetValue(instance);
            if(value == null || scanned.Contains(value)) continue;

            if(value is ChunkHandler child) {
                RegisterHandler(child, chunkedHandlers, globalHandlers, handlerActiveChunks);
                ScanRecursive(child, child.GetType(), chunkedHandlers, globalHandlers, handlerActiveChunks);
            }
            if(value is IEnumerable en && value is not string) {
                foreach(var item in en) {
                    if(item is ChunkHandler itemHandler && !scanned.Contains(itemHandler)) {
                        RegisterHandler(itemHandler, chunkedHandlers, globalHandlers, handlerActiveChunks);
                        ScanRecursive(itemHandler, itemHandler.GetType(), chunkedHandlers, globalHandlers, handlerActiveChunks);
                    }
                }
            }
        }
    }

    /**
    
        Nested Handler
    
        */
    private class NestedHandler : ChunkHandler {
        private object target;
        private Dictionary<string, MethodInfo?> methods = new();

        public NestedHandler(object target) {
            this.target = target;
            var t = target.GetType();
        
            var method = typeof(ChunkHandler).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach(var m in method) {
                if(m.GetCustomAttribute<ScanAttribute>() != null) {
                    methods[m.Name] = t.GetMethod(
                        m.Name,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        Type.EmptyTypes,
                        null
                    );
                }
            }
        }

        protected override void Route([CallerMemberName] string? name = null) {
            if(name != null && methods.TryGetValue(name, out var method)) {
                method?.Invoke(target, null);
            }   
        }
    }
}