namespace App.Root._Sync;
using System.Collections;
using System.Reflection;

/**

    Sync Timestamp attribute

    */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class SyncTimestampAttribute : Attribute {
    public SyncTimestampAttribute() {}
}

/**

    Sync Resolver main class.

    */
public class SyncResolver {
    // Get Timestamp
    private long GetTimestamp(object data) {
        var type = data.GetType();

        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            if(prop.GetCustomAttribute<SyncTimestampAttribute>() != null) {
                var value = prop.GetValue(data);
                if(value is long longValue) return longValue;
                if(value is DateTime dtValue) return dtValue.Ticks;
                if(value is DateTimeOffset dtoValue) return dtoValue.Ticks;
            }
        }
        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            if(field.GetCustomAttribute<SyncTimestampAttribute>() != null) {
                var value = field.GetValue(data);
                if(value is long longValue) return longValue;
                if(value is DateTime dtValue) return dtValue.Ticks;
                if(value is DateTimeOffset dtoValue) return dtoValue.Ticks;
            }
        }

        return DateTime.UtcNow.Ticks;
    }

    // Server Authority
    private object ServerAuthority(object existing, object incoming) {
        object val = existing;
        return val;
    }

    // Last Write Wins
    private object LastWriteWins(object existing, object incoming, long timestamp) {
        var existingTimestamp = GetTimestamp(existing);
        if(timestamp > existingTimestamp) return incoming; 

        return existing;
    }

    // Merge With Arbiter
    private object MergeWithArbiter(object existing, object incoming) {
        var type = existing.GetType();

        var merged = Activator.CreateInstance(type);
        if(merged == null) return existing;
    
        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            if(prop.CanWrite) {
                var existingValue = prop.GetValue(existing);
                var incomingValue = prop.GetValue(incoming);

                var mergedValue = MergeValue(existingValue, incomingValue, prop.PropertyType);
                prop.SetValue(merged, mergedValue);
            }
        }
        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            var existingValue = field.GetValue(existing);
            var incomingValue = field.GetValue(incoming);

            var mergedValue = MergeValue(existingValue, incomingValue, field.FieldType);
            field.SetValue(merged, mergedValue);
        }

        return merged;
    }

    // Lock Based
    private object LockBased(object existing, object incoming) {
        if(LockSync.Instance.IsLocked(existing.GetType().Name.ToLower())) {
            return existing;
        }
        return incoming;
    }

    /**
     *
     * Merge
     *
     */
    private object? MergeValue(object? existing, object? incoming, Type type) {
        if(existing == null) return incoming;
        if(incoming == null) return existing;

        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return MergeLists(existing, incoming);
        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) return MergeDictionaries(existing, incoming);
        if(type.IsPrimitive || type == typeof(string) || type == typeof(decimal)) {
            if(!Equals(existing, incoming)) return incoming;
            return existing;
        }
        if(type.IsClass) {
            try {
                var merged = Activator.CreateInstance(type);
                if(merged == null) return existing;

                foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                    if(prop.CanWrite) {
                        var existingValue = prop.GetValue(existing);
                        var incomingValue = prop.GetValue(incoming);
                        var mergedValue = MergeValue(existingValue, incomingValue, prop.PropertyType);
                        prop.SetValue(merged, mergedValue);
                    }
                }

                return merged;
            } catch {
                return incoming;
            }
        }
        
        return incoming;
    }

    // Merge Lists
    private object MergeLists(object existing, object incoming) {
        var existingList = existing as IEnumerable;
        var incomingList = incoming as IEnumerable;
        if(existingList == null) return incoming;
        if(incomingList == null) return existing;

        var result = new List<object>();
        foreach(var item in existingList) result.Add(item);
        foreach(var item in incomingList) {
            if(!result.Contains(item)) {
                result.Add(item);
            }
        }

        return result;
    }

    // Merge Dictionaries
    private object MergeDictionaries(object existing, object incoming) {
        var existingDict = existing as IDictionary;
        var incomingDict = incoming as IDictionary;
        if(existingDict == null) return incoming;
        if(incomingDict == null) return existing;

        var result = new Dictionary<object, object?>();
        foreach(var key in existingDict.Keys) result[key] = existingDict[key];
        foreach(var key in incomingDict.Keys) result[key] = incomingDict[key];

        return result;
    }

    /**
     *
     * Resolve
     *
     */
    public object? Resolve(object existing, object incoming, ConflictResolution resolution, long timestamp) {
        switch(resolution) {
            case ConflictResolution.SERVER_AUTHORITY: return ServerAuthority(existing, incoming);
            case ConflictResolution.LAST_WRITE_WINS: return LastWriteWins(existing, incoming, timestamp);
            case ConflictResolution.MERGE_WITH_ARBITER: return MergeWithArbiter(existing, incoming);
            case ConflictResolution.LOCK_BASED: return LockBased(existing, incoming);
            default: return existing;
        }
    }
}