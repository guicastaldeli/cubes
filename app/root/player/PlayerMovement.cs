namespace App.Root.Player;

using System.Reflection;
using App.Root.Utils;

public class PlayerMovement {
    [Convert("float")] [ConverterKey("gravity")] public float Gravity { get; set; } = -20.0f;
    [Convert("float")] [ConverterKey("gravityScale")] public float GravityScale { get; set; } = 3.0f;
    [Convert("float")] [ConverterKey("drag")] public float Drag { get; set; } = 0.1f;
    [Convert("float")] [ConverterKey("moveSpeed")] public float MoveSpeed { get; set; } = 20.0f;
    [Convert("float")] [ConverterKey("flySpeed")] public float FlySpeed { get; set; } = 20.0f;
    [Convert("float")] [ConverterKey("jumpForce")] public float JumpForce { get; set; } = 8.0f;
    [Convert("float")] [ConverterKey("jumpGravity")] public float JumpGravity { get; set; } = -15.0f;
    [Convert("float")] [ConverterKey("jumpGravityScale")] public float JumpGravityScale { get; set; } = 1.0f;
    [Convert("float")] [ConverterKey("groundFriction")] public float GroundFriction { get; set; } = 0.9f;
    [Convert("float")] [ConverterKey("maxFallSpeed")] public float MaxFallSpeed { get; set; } = -30.0f;
    [Convert("float")] [ConverterKey("buoyancy")] public float Buoyancy { get; set; } = 0.0f;

    public static readonly Dictionary<string, PropertyInfo> configMembers = new();
    public static readonly Dictionary<string, MethodInfo> converters = typeof(Converter)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            m => m.GetCustomAttribute<ConverterKey>()!.Key,
            m => m
        );

    public PlayerMovement() {}
    static PlayerMovement() {
        foreach(var prop in typeof(PlayerMovement).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            var key = prop.GetCustomAttribute<ConverterKey>()?.Key;
            if(key != null) configMembers[key.ToLower()] = prop;
        }
    }

    /**
     * 
     * Apply
     *
     */

    /**
     * 
     * Convert
     *
     */
    public static PlayerMovement Convert(string data) {
        var profile = new PlayerMovement();
        if(string.IsNullOrEmpty(data)) return profile;

        var lines = data.Split('\n');
        foreach(var line in lines) {
            var trimmed = line.Trim();
            if(string.IsNullOrEmpty(trimmed)) continue;

            var separator = trimmed.Contains(':') ? ':' : '=';
            var parts = trimmed.Split(separator, 2);
            if(parts.Length != 2) continue;

            var key = parts[0].Trim().ToLower();
            var val = parts[1].Trim().TrimEnd(',');

            if(configMembers.TryGetValue(key, out var prop)) {
                try {
                    var convertAttr = prop.GetCustomAttribute<ConvertAttribute>();
                    if(convertAttr != null && converters.TryGetValue(convertAttr.Converter, out var converter)) {
                        var result = converter.Invoke(null, new object[] { val });
                        prop.SetValue(profile, result);
                    }
                } catch(Exception err) {
                    Console.WriteLine($"[PlayerMovement] Error setting {key}: {err.Message}");
                }
            }
        }

        return profile;
    }
} 