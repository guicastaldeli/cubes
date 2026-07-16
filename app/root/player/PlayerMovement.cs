namespace App.Root.Player;
using App.Root.Utils;
using System.Globalization;
using System.Reflection;

static class PlayerMovement {
    public class Data {
        [Convert("float")] [ConverterKey("gravity")] public float Gravity { get; set; } = -20.0f;
        [Convert("float")] [ConverterKey("gravityScale")] public float GravityScale { get; set; } = 3.0f;
        [Convert("float")] [ConverterKey("drag")] public float Drag { get; set; } = 0.1f;
        [Convert("float")] [ConverterKey("moveSpeed")] public float MoveSpeed { get; set; } = 20.0f;
        [Convert("float")] [ConverterKey("flySpeed")] public float FlySpeed { get; set; } = 20.0f;
        [Convert("float")] [ConverterKey("jumpForce")] public float JumpForce { get; set; } = 8.0f;
        [Convert("bool")] [ConverterKey("jumpGravityEnabled")] public bool JumpGravityEnabled { get; set; } = false;
        [Convert("float")] [ConverterKey("jumpGravity")] public float JumpGravity { get; set; } = -15.0f;
        [Convert("float")] [ConverterKey("jumpGravityScale")] public float JumpGravityScale { get; set; } = 1.0f;
        [Convert("float")] [ConverterKey("friction")] public float Friction { get; set; } = 0.9f;
        [Convert("float")] [ConverterKey("airControl")] public float AirControl { get; set; } = 0.8f;
        [Convert("float")] [ConverterKey("maxFallSpeed")] public float MaxFallSpeed { get; set; } = -30.0f;
        [Convert("float")] [ConverterKey("pullDrag")] public float PullDrag { get; set; } = 0.1f;
        [Convert("float")] [ConverterKey("buoyancy")] public float Buoyancy { get; set; } = 0.0f;
    
        public Data() {}
    }

    public static readonly Dictionary<string, PropertyInfo> configMembers = new();
    public static readonly Dictionary<string, MethodInfo> converters = typeof(Converter)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            m => m.GetCustomAttribute<ConverterKey>()!.Key,
            m => m
        );

    private static Data currentData = null!;
    private static PlayerController playerController = null!;
    private static RigidBody rigidBody = null!;

    static PlayerMovement() {
        foreach(var prop in typeof(Data).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            var key = prop.GetCustomAttribute<ConverterKey>()?.Key;
            if(key != null) configMembers[key.ToLower()] = prop;
        }
    }

    /**
     * 
     * Apply
     *
     */
    // Apply to Player
    private static void ApplyToPlayer(Data data) {
        if(playerController == null) return;

        playerController.setMoveSpeed(data.MoveSpeed);
        playerController.setFlySpeed(data.FlySpeed);
        playerController.setJumpForce(data.JumpForce);
    }

    // Apply to RigidBody
    private static void ApplyToRigidBody(Data data) {
        if(rigidBody == null) return;

        rigidBody.setGravity(data.Gravity);
        rigidBody.setGravityScale(data.GravityScale);
        rigidBody.setDrag(data.Drag);
        rigidBody.setJumpGravityEnabled(data.JumpGravityEnabled);
        rigidBody.setJumpGravity(data.JumpGravity);
        rigidBody.setJumpGravityScale(data.JumpGravityScale);
        rigidBody.setPullDrag(data.PullDrag);
        rigidBody.setBuoyancy(data.Buoyancy);
        rigidBody.setMaxFallSpeed(data.MaxFallSpeed);
        rigidBody.setFriction(data.Friction);
        rigidBody.setAirControl(data.AirControl);
    }

    // Apply
    public static void Apply(Data data) {
        currentData = data;

        ApplyToPlayer(data);
        ApplyToRigidBody(data);
    }

    /**
     * 
     * Convert
     *
     */
    public static Data? Convert(string input) {
        if(string.IsNullOrEmpty(input)) return null;

        var profile = new Data();
        var inv = CultureInfo.InvariantCulture;
        var segments = input.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach(var segment in segments) {
            var trimmed = segment.Trim();
            if(string.IsNullOrEmpty(trimmed)) continue;

            var separator = trimmed.Contains(':') ? ':' : '=';
            var parts = trimmed.Split(separator, 2);
            if(parts.Length != 2) continue;

            var key = parts[0].Trim().ToLower();
            var val = parts[1].Trim();

            if(configMembers.TryGetValue(key, out var prop)) {
                try {
                    var propType = prop.PropertyType;
                    if(propType == typeof(float)) {
                        if(float.TryParse(val, NumberStyles.Float, inv, out float result)) {
                            prop.SetValue(profile, result);
                            Console.WriteLine($"[PlayerMovement] Set {key} = {result}");
                        } else {
                            Console.WriteLine($"[PlayerMovement] Failed to parse float for {key}: '{val}'");
                        }
                    } else if(propType == typeof(bool)) {
                        if(bool.TryParse(val, out bool result)) {
                            prop.SetValue(profile, result);
                            Console.WriteLine($"[PlayerMovement] Set {key} = {result}");
                        } else {
                            Console.WriteLine($"[PlayerMovement] Failed to parse bool for {key}: '{val}'");
                        }
                    } else if(propType == typeof(int)) {
                        if(int.TryParse(val, out int result)) {
                            prop.SetValue(profile, result);
                            Console.WriteLine($"[PlayerMovement] Set {key} = {result}");
                        } else {
                            Console.WriteLine($"[PlayerMovement] Failed to parse int for {key}: '{val}'");
                        }
                    }
                } catch(Exception err) {
                    Console.WriteLine($"[PlayerMovement] Error setting {key}: {err.Message}");
                }
            } else {
                Console.WriteLine($"[PlayerMovement] Key '{key}' not found in configMembers");
            }
        }

        return profile;
    }

    /**
     * 
     * Init
     *
     */
    public static void Init(PlayerController playerController, RigidBody rigidBody) {
        PlayerMovement.playerController = playerController;
        PlayerMovement.rigidBody = rigidBody;
    }
} 