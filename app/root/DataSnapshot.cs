namespace App.Root;

class DataSnapshot {
    public Dictionary<DataType, List<Dictionary<string, object>>> data {
        get;
        set;
    } = new();

    public DataSnapshot() {}
    public DataSnapshot(Dictionary<DataType, List<DataEntry>> entries) {
        foreach(var (type, list) in entries) {
            data[type] = list.Select(e => e.serialize()).ToList();
        }
    }

    public List<Dictionary<string, object>> get(DataType type) {
        return data.TryGetValue(type, out var list) ? 
            list : 
            new();
    }
}