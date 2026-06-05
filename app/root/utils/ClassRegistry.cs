/**

    Class Register main class.

    */
namespace App.Root.Utils;
using System.Reflection;
using System.Runtime.CompilerServices;

class ClassRegistry {
    private ServiceContainer ServiceContainer;

    private static Dictionary<Type, List<Action>> afterHooks = new();
    private static Dictionary<Type, bool> registrationMode = new();

    public ClassRegistry(ServiceContainer serviceContainer) {
        this.ServiceContainer = serviceContainer;
    }

    // After
    private static void after<TParent>(Action action) {
        var type = typeof(TParent);
        if(!afterHooks.ContainsKey(type)) afterHooks[type] = new();
        afterHooks[type].Add(action);
    }

    private static void runAfter<T>() {
        var type = typeof(T);
        if(!afterHooks.TryGetValue(type, out var hooks)) return;
        foreach(var hook in hooks) hook();
        afterHooks.Remove(type);
    }
    
    /**
     * 
     * Logs
     *
     */
    private void success(string label, Type type) {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Registered [{label}]: {type.Name}");
        Console.ResetColor();
    }

    private void error(string label, Type type) {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"Failed to register [{label}]: {type.Name}");
        Console.ResetColor();
    }

    /**
     * 
     * Create Instance
     *
     */
    private T? CreateInstance<T>(Type type) where T : class {
        var constructors = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length);

        foreach(var ctor in constructors) {
            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];
            bool canResolve = true;

            for(int i = 0; i < parameters.Length; i++) {
                var param = parameters[i];
                var service = ServiceContainer.Get(param.ParameterType);

                if(service != null) {
                    args[i] = service;
                } else if(param.IsOptional) {
                    args[i] = param.DefaultValue;
                } else {
                    canResolve = false;
                    break;
                }
            }

            if(canResolve) {
                try {
                    return (T)ctor.Invoke(args);
                } catch(Exception err) {
                    Console.WriteLine($"Error creating {type.Name}: {err.Message}");
                    return null;
                }
            }
        }

        return null;
    }

    /**
     * 
     * Register
     *
     */
    // Register
    public List<T> Register<T>() where T : class {
        registrationMode[typeof(T)] = false;

        var result = new List<T>();
        var baseType = typeof(T);
        string label = typeof(T).Name;

        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsSubclassOf(baseType) &&
                t.GetCustomAttribute<ClassRegistryIgnore>() == null
            );
        foreach(var type in types) {
            var instance = CreateInstance<T>(type);
            if(instance != null) {
                result.Add(instance);
                success(label, type);
            } else {
                error(label, type);
            }
        }

        runAfter<T>();
        return result;
    }

    // Ordered Register
    public List<T> ORegister<T>() where T : class {
        registrationMode[typeof(T)] = true;
        
        var result = new List<T>();
        var baseType = typeof(T);
        string label = typeof(T).Name;

        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsSubclassOf(baseType) &&
                t.GetCustomAttribute<ClassRegistryIgnore>() == null
            ).ToList();

        var remaining = new Queue<Type>(types);
        int maxPasses = types.Count;

        while(remaining.Count > 0) {
            int countBefore = remaining.Count;
            int toProcess = remaining.Count;

            for(int i = 0; i < toProcess; i++) {
                var type = remaining.Dequeue();
                var instance = CreateInstance<T>(type);

                if(instance != null) {
                    result.Add(instance);
                    ServiceContainer.Register(type, instance);
                    success(label, type);
                } else {
                    remaining.Enqueue(type);
                }
            }

            if(remaining.Count == countBefore) break;
        }

        foreach(var type in remaining) {
            error(label, type);
        }

        runAfter<T>();
        return result;
    }

    // Parent Register
    public void PRegister<TParent, T>(Action<List<T>> onComplete) 
        where TParent : class
        where T : class {
        ClassRegistry.after<TParent>(() => {
            bool ordered = registrationMode.GetValueOrDefault(typeof(TParent), true);
            var result = ordered ? ORegister<T>() : Register<T>();
            onComplete(result);
        });
    }

    /**
     * 
     * Clear
     *
     */
    public void Clear() {
        afterHooks.Clear();
        registrationMode.Clear();
    }
}