/**

    Class Register main class.

    */
namespace App.Root.Utils;
using System.Reflection;

class ClassRegistry {
    private ServiceContainer ServiceContainer;

    public ClassRegistry(ServiceContainer serviceContainer) {
        this.ServiceContainer = serviceContainer;
    }

    /**
    
        Create Instance
    
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
    
        Register
    
        */
    public List<T> Register<T>() where T : class {
        var result = new List<T>();
        var baseType = typeof(T);

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

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Registered: {type.Name}");
                Console.ResetColor();
            } else {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Failed to register: {type.Name}");
                Console.ResetColor();
            }
        }

        return result;
    }

    /**
    
        Clear
    
        */
    public void Clear() {
        
    }
}