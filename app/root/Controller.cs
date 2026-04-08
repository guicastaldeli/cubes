/*

    Global controller for main
    configurations of the program
    instances, etc...

    */
namespace App.Root;

using System.Dynamic;
using System.Reflection;
using System.Runtime.Serialization;

/**

    Instances

    */
public enum Instance {
    [EnumMember(Value = "debug")] 
    DEBUG,
    [EnumMember(Value = "dev")] 
    DEV,
    [EnumMember(Value = "prod")] 
    PROD
}

public record InstanceMap {
    public Instance DEBUG {
        get;
    } = Instance.DEBUG;

    public Instance DEV {
        get;
    } = Instance.DEV;

    public Instance PROD {
        get;
    } = Instance.PROD;
}

class Controller {
    private static Instance? current = Instance.PROD;
    private static readonly InstanceMap instanceMap = new();

    private static string? error = null;

    private Main main;

    public Controller(string[] args) {
        init(args);
        this.main = new Main();
    }

    // Get Instance
    public static Dictionary<string, Instance> getInstances() {
        Dictionary<string, Instance> val = Enum.GetValues<Instance>()
            .ToDictionary(i => 
                i.GetType()
                .GetField(i.ToString())!
                .GetCustomAttribute<EnumMemberAttribute>()!
                .Value!, i => i
            );

        return val;
    }

    public static InstanceMap getInstance() {
        return instanceMap;
    }

    public static bool getInstance(Instance instance) {
        bool val = current == instance;
        return val;
    }

    // Get Current
    public static Instance? getCurrent() {
        return current;
    }

    public static string? getCurrentName() {
        string val = current?
            .GetType()
            .GetField(current.ToString()!)!
            .GetCustomAttribute<EnumMemberAttribute>()!
            .Value!;
        
        return val;
    }

    /**

        Init

        */
    private void init(string[] args) {
        string? mode = args.FirstOrDefault()?.ToLower();
        var instances = getInstances();
        
        if(hasError(mode, instances, out var res)) return;
        current = res;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[Controller] Instance: {current}");
        Console.ResetColor();
    }

    /**

        Run

        */
    public void run() {
        main.run();
    }

    // Error
    public static bool hasError(
        string? mode,
        Dictionary<string, Instance> instances,
        out Instance? res  
    ) {
        if(mode == null || !instances.TryGetValue(mode, out var match)) {
            error = $"FATAL ERR.: unknown instance '{mode}'";
            res = null;
            return true;
        }

        res = match;
        return false;
    }

    public static string? getError() {
        return error;
    }
}