namespace App.Root.World.Entity;

public static class XpRegistry {
    private static Dictionary<string, int> map = new();

    /**
    
        Get
    
        */
    public static int? Get(string id) {
        int? val = map.TryGetValue(id, out var xp) ? xp : null;
        return val;
    }

    /**
    
        Register
    
        */
    public static void Register(string id, int xp) {
        map[id] = xp;
    }

    /**
    
        Remove
    
        */
    public static void Remove(string id) {
        map.Remove(id);
    }


    /**
    
        Clear
    
        */
    public static void Clear() {
        map.Clear();
    }
}