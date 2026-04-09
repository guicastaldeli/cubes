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
        this.serialize = serialized();
        this.deserialize = deserialized();
    }

    private Func<T, string> serialized() {
        return serialize ?? (v => v?.ToString() ?? "");
    }

    private Func<string, T> deserialized() {
        return deserialize ?? (s => (T)Convert.ChangeType(s, typeof(T)));
    }
}