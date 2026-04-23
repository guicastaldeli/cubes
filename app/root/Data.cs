namespace App.Root;

enum DataType {
    MESH,
    PLAYER,
    WORLD
}

class Data {
    private static Data? instance;
    private Dictionary<DataType, List<DataEntry>> entries = new();

    public static Data getInstance() {
        instance ??= new Data();
        return instance;
    }

    /**
    
        Register

        */
    public void register(DataType type, DataEntry entry) {
        if(!entries.ContainsKey(type)) entries[type] = new();
        entries[type].Add(entry);
    }

    /**
    
        Unregister

        */
    public void unregister(DataType type, DataEntry entry) {
        if(entries.TryGetValue(type, out var list)) list.Remove(entry);
    }

    /**
    
        Get

        */
    public List<DataEntry> get(DataType type) {
        return entries.TryGetValue(type, out var list) ?
            list :
            new();
    }

    /**
    
        Snapshot

        */
    // Snapshot
    public DataSnapshot snapshot() {
        return new DataSnapshot(entries);
    }

    // Apply
    public void apply(
        DataSnapshot snapshot,
        DataType type,
        Action<Dictionary<string, object>> handler
    ) {
        foreach(var entry in snapshot.get(type)) {
            handler(entry);
        }
    }

    /**
    
        Clear

        */
    public void clear(DataType type) {
        if(entries.ContainsKey(type)) entries[type].Clear();
    }

    public void clearAll() {
        entries.Clear();
    }
}