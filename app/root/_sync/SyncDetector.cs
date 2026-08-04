namespace App.Root._Sync;
using App.Root._Binary;
using System.Reflection;

public class SyncDetetor {
    private Dictionary<string, object> usedFlags = new();
    private Dictionary<string, byte[]> snapshots = new();

    public int SnapshotCount => snapshots.Count;

    // Get Data Id
    private string GetDataId(object data) {
        var type = data.GetType();
        var attr = type.GetCustomAttribute<DataSyncAttribute>();
        return attr?.Id ?? type.Name.ToLower();
    }

    // Compare Byte Arrays
    private bool CompareByteArrays(byte[] a, byte[] b) {
        if(a.Length != b.Length) return false;
        for(int i = 0; i < a.Length; i++) {
            if(a[i] != b[i]) return false;
        }

        return true;
    }

    // Get All Fields
    public Dictionary<string, object> GetAllFields(object data) {
        var fields = new Dictionary<string, object>();
        var type = data.GetType();

        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            var attr = prop.GetCustomAttribute<SyncFieldAttribute>();
            if(attr != null && !attr.Ignore) {
                var value = prop.GetValue(data);
                if(value != null) fields[attr.Key ?? prop.Name] = value;
            }
        }
        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            var attr = field.GetCustomAttribute<SyncFieldAttribute>();
            if(attr != null && !attr.Ignore) {
                var value = field.GetValue(data);
                if(value != null) fields[attr.Key ?? field.Name] = value;
            }
        }

        return fields;
    }

    // Should Sync Field
    private bool ShouldSyncField(object? current, object? prev, float threshold) {
        if(threshold <= 0) return false;

        if(current is IComparable currentComparable && prev is IComparable prevComparable) {
            try {
                double currentDouble = Convert.ToDouble(currentComparable);
                double prevDouble = Convert.ToDouble(prevComparable);
                return Math.Abs(currentDouble - prevDouble) >= threshold;
            } catch {
                return !Equals(current, prev);
            }
        }

        return !Equals(current, prev);
    }

    // Has Changed
    public bool HasChanged(object data) {
        string id = GetDataId(data);
        return HasChanged(id, data);
    }

    public bool HasChanged(string id, object data) {
        using var writer = new BinaryWriter();
        writer.WriteObject(data);

        var currentState = writer.GetBytes();
        if(snapshots.TryGetValue(id, out var prevState)) {
            bool val = !CompareByteArrays(prevState, currentState);
            return val;
        }

        return true;
    }

    /**
     *
     * Get Delta
     *
     */
    public Dictionary<string, object>? GetDelta(object data) {
        string id = GetDataId(data);
        return GetDelta(id, data);
    }

    public Dictionary<string, object>? GetDelta(string id, object data) {
        if(!HasChanged(id, data)) return null;

        var delta = new Dictionary<string, object>();

        if(!snapshots.TryGetValue(id, out var prevState)) {
            return GetAllFields(data);
        }

        using var reader = new BinaryReader(prevState);
        
        var prevData = reader.ReadObject();
        if(prevData == null) return GetAllFields(data);
    
        var type = data.GetType();
        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            var attr = prop.GetCustomAttribute<SyncFieldAttribute>();
            if(attr != null && !attr.Ignore) {
                var currentValue = prop.GetValue(data);
                var prevValue = prop.GetValue(prevData);

                if(!Equals(currentValue, prevValue)) {
                    if(ShouldSyncField(currentValue, prevValue, attr.DeltaThreshold)) {
                        delta[attr.Key ?? prop.Name] = currentValue!;
                    }
                }
            }
        }
        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            var attr = field.GetCustomAttribute<SyncFieldAttribute>();
            if(attr != null && !attr.Ignore) {
                var currentValue = field.GetValue(data);
                var prevValue = field.GetValue(prevData);

                if(!Equals(currentValue, prevValue)) {
                    if(ShouldSyncField(currentValue, prevValue, attr.DeltaThreshold)) {
                        delta[attr.Key ?? field.Name] = currentValue!;
                    }
                }
            }
        }

        UpdateSnapshot(id, data);
        
        Dictionary<string, object>? val = delta.Count > 0 ? delta : null;
        return val;
    }

    /**
     *
     * Update Snapshot
     *
     */
    public void UpdateSnapshot(object data) {
        string id = GetDataId(data);
        UpdateSnapshot(id, data);
    }

    public void UpdateSnapshot(string id, object data) {
        using var writer = new BinaryWriter();
        writer.WriteObject(data);
        snapshots[id] = writer.GetBytes();
    }

    /**
     *
     * Clear
     *
     */
    // Clear
    public void Clear() {
        usedFlags.Clear();
        snapshots.Clear();
    }

    // Mark Clean
    public void MarkClean(object data) {
        string id = GetDataId(data);
        usedFlags[id] = false;
        UpdateSnapshot(id, data);
    }
}