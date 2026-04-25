namespace App.Root;
using App.Root.Info;

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

    private string kv(KeyValuePair<string, string> v) {
        return $"{v.Key}={v.Value}";
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
    
    /**
    
        Load

        */
    public void load() {
        if(!File.Exists(filePath)) return;
        foreach(var line in File.ReadAllLines(filePath)) {
            var idx = line.IndexOf("=");
            if(idx < 0) continue;

            var key = line[..idx].Trim();
            var val = line[(idx+1)..].Trim();
            
            data[key] = val;
        }
    }

    /**
    
        Save

        */
    public void save() {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllLines(filePath, data.Select(v => kv(v)));
    }
}