using System.Text.Json;

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

    // Convert Elements
    public void convertEls() {
        var converted = new Dictionary<DataType, List<Dictionary<string, object>>>();
        foreach(var (type, list) in data) {
            converted[type] = list.Select(entry =>
                entry.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value is JsonElement je ? 
                        convertEl(je) : 
                        kvp.Value 
                )
            ).ToList();
        }
        data = converted;
    }

    private object convertEl(JsonElement el) {
        return el.ValueKind switch {
            JsonValueKind.String => el.GetString()!,
            JsonValueKind.Number => el.TryGetSingle(out var f) ? f : (object)el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => el.ToString()
        };
    }

    // Get
    public List<Dictionary<string, object>> get(DataType type) {
        return data.TryGetValue(type, out var list) ? 
            list : 
            new();
    }
}