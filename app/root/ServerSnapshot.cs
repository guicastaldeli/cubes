namespace App.Root;

class ServerSnapshot {
    private static ServerSnapshot? instance;
    private Dictionary<DataType, List<DataEntry>> entries = new();

    public static ServerSnapshot getInstance() {
        instance ??= new ServerSnapshot();
        return instance;
    }
    
    // Register
    public void register(DataType type, DataEntry entry) {
        if(!entries.ContainsKey(type)) entries[type] = new();
        entries[type].Add(entry);
    }

    // Snapshot
    public DataSnapshot snapshot() {
        return new DataSnapshot(entries);
    }

    // Clear
    public void clearAll() {
        entries.Clear();
    }
}

