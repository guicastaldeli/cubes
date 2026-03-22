namespace App.Root;

class ServerSnapshot {
    private static ServerSnapshot? instance;

    public static ServerSnapshot getInstance() {
        instance ??= new ServerSnapshot();
        return instance;
    }

    private Dictionary<DataType, List<DataEntry>> entries = new();

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

