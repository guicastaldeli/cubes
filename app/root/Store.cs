namespace App.Root;

class Store {
    private readonly string filePath;
    private Dictionary<string, string> data = new();

    public Store(string filePath) {
        this.filePath = filePath;
        load();
    }

    public bool has(string key) {
        return data.ContainsKey(key);
    }

    // Get and Set Raw
    public string? getRaw(string key) {
        string? val = data.TryGetValue(key, out var v) ? v : null;
        return val;
    }

    public void setRaw(string key, string val) {
        data[key] = val;
    }

    // Typed Get and Set
    public T get<T>(InfoField<T> field) {
        var raw = getRaw(field.key);
        if(raw == null) return field.defaultValue();
        return field.deserialize(raw);
    }

    public void set<T>(InfoField<T> field, T val) {
        setRaw(field.key, field.serialize(val));
    }
    
}