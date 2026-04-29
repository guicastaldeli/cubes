/**
    
    Service Container for 
    Attribute Injection.
    
    */
public class ServiceContainer {
    private Dictionary<Type, object> services = new Dictionary<Type, object>();
    private static bool activeSRegister = false;

    /**
    
        Has
    
        */
    public bool Has(Type type) {
        return services.ContainsKey(type);
    }

    /**
    
        Get
    
        */
    public object? Get(Type type) {
        return services.ContainsKey(type) ? services[type] : null;
    }
    
    public T? Get<T>() where T : class {
        return services.ContainsKey(typeof(T)) ? 
            (T)services[typeof(T)] : null;
    }

    /**
    
        Register
    
        */
    public void Register<T>(T service) where T : class {
        services[typeof(T)] = service;
    }

    public void Register<T>(Type type, T service) where T : class {
        services[type] = service;
    }

    /**
    
        SRegister
    
        */
    public void SRegister<T>(T service) where T : class {
        services[typeof(T)] = service;
    }

    public static void ActiveSRegister(bool active) {
        activeSRegister = active;
    }

    public static bool IsSRegisterActive() {
        return activeSRegister;
    }
}