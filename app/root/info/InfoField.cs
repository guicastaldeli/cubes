namespace App.Root.Info;

class InfoField<T> {
    public readonly string key;
    public readonly Func<T> defaultValue;
    public readonly Func<T, string> serialize;
    public readonly Func<string, T> deserialize;

    public InfoField(
        string key,
        Func<T> defaultValue,
        Func<T, string>? serialize = null,
        Func<string, T>? deserialize = null
    ) {
        this.key = key;
        this.defaultValue = defaultValue;
        this.serialize = serialized(serialize);
        this.deserialize = deserialized(deserialize);
    }

    private static Func<T, string> serialized(Func<T, string>? serialize) {
        return serialize ?? (v => v?.ToString() ?? "");
    }

    private static Func<string, T> deserialized(Func<string, T>? deserialize) {
        return deserialize ?? (s => (T)Convert.ChangeType(s, typeof(T)));
    }
}