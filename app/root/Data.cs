namespace App.Root;

/**
    
    Data Type

    */
enum DataType {
    MESH,
    PLAYER,
    WORLD
}

/**
    
    Data Entry interface

    */
interface DataEntry {
    string? getId() => null;
    Dictionary<string, object> serialize();
}

/**
    
    Main Data class

    */
[ManagedState]
static class Data {
    private static Dictionary<DataType, List<DataEntry>> entries = new();

    static Data() {
        StateManager.SRegister(typeof(Data));
    }

    /**
     * 
     * Register
     *
     */
    public static void Register(DataType type, DataEntry entry) {
        if(!entries.ContainsKey(type)) entries[type] = new();
        entries[type].Add(entry);
    }

    /**
     * 
     * Unregister
     *
     */
    public static void Unregister(DataType type, DataEntry entry) {
        if(entries.TryGetValue(type, out var list)) list.Remove(entry);
    }

    /**
     * 
     * Get
     *
     */
    public static List<DataEntry> Get(DataType type) {
        List<DataEntry> val = entries.TryGetValue(type, out var list) ? list : new();
        return val;
    }

    /**
     * 
     * Snapshot
     *
     */
    // Snapshot
    public static DataSnapshot Snapshot() {
        DataSnapshot val = new DataSnapshot(entries);
        return val;
    }

    // Apply Snapshot
    public static void ApplySnapshot(DataSnapshot snapshot, DataType type, Action<Dictionary<string, object>> handler) {
        foreach(var entry in snapshot.get(type)) {
            handler(entry);
        }
    }

    /**
     * 
     * Clear
     *
     */
    // Clear
    public static void Clear(DataType type) {
        if(entries.ContainsKey(type)) entries[type].Clear();
    }

    // Clear All
    public static void ClearAll() {
        entries.Clear();
    }
}