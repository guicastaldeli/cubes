namespace App.Root.Utils;

public static class Env {
    private static string ENV_PATH = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "root", ".env", ".env"));

    private static Dictionary<string, string> values = new();
    private static bool loaded = false;

    /**
     *
     * Get
     *
     */
    public static string? Get(string key) {
        string? val = values.TryGetValue(key, out var v) ? v : null;
        return val;
    }

    public static string GetOrThrow(string key) {
        if(!values.TryGetValue(key, out var val)) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"[Env] Missing required key: {key}");
            Console.ResetColor();
            throw new Exception($"[Env] Exception: {key}");
        }

        return val;
    }

    /**
     *
     * Load
     *
     */
    public static void Load() {
        if(!File.Exists(ENV_PATH)) {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine("[Env] .env file not found!.");
            Console.ResetColor();
            return;
        }

        foreach(var line in File.ReadAllLines(ENV_PATH)) {
            if(string.IsNullOrWhiteSpace(line)) continue;
            if(line.StartsWith("#")) continue;

            int idx = line.IndexOf('=');
            if(idx < 0) continue;

            string key = line[..idx].Trim();
            string val = line[(idx + 1)..].Trim();
            if(val.StartsWith('"') && val.EndsWith('#')) {
                val = val[1..^1];
            }

            values[key] = val;
        }

        loaded = true;

        Console.BackgroundColor = ConsoleColor.Blue;
        Console.Error.WriteLine("[Env] Loaded! {values.count} variables");
        Console.ResetColor();
    }
}