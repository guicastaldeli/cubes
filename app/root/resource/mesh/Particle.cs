/**

    Main particle mesh
    data.

    */
namespace App.Root.Resource.Mesh;

using System.Reflection;
using App.Root.Mesh;
using App.Root.Utils;
using OpenTK.Mathematics;

class Particle {
    [Convert("vec3")] [ConverterKey("position")] public Vector3 position;
    [Convert("vec3")] [ConverterKey("vel")] public Vector3 vel;
    [Convert("vec3")] [ConverterKey("color")] public Vector3 color;
    [Convert("float")] [ConverterKey("size")] public float size;
    [Convert("bool")] [ConverterKey("live")] public bool live { get; set; } = true;
    [Convert("float")] [ConverterKey("playerMovSpeed")] public float playerMovSpeed { get; set; } = 1.0f;
    [Convert("float")] [ConverterKey("playerStand")] public float playerStand { get; set; } = 1.0f;
    [Convert("float")] [ConverterKey("speed")] public float speed { get; set; } = 1.0f;
    [Convert("vec3")] [ConverterKey("velNum")] public Vector3 velNum { get; set; } = Vector3.One;
    [Convert("float")] [ConverterKey("targetY")] public float targetY { get; set; } = 1.0f;
    [Convert("bool")] [ConverterKey("enableMotion")] public bool enableMotion { get; set; } = true;
    [Convert("int")] [ConverterKey("amount")] public int amount { get; set; } = 10;
    public string id = "";
    public MeshData? cachedMeshData;
    public Vector3 basePos = Vector3.Zero;
    public Vector3 initialVel;
    public float lifetime;
    public float maxLifetime;
    public float rotation;
    public float rotationSpeed;
    public float swayPhase;
    public float swayAmplitude;
    public float swayFrequency;
    public Vector3 swayVel;

    public static readonly Dictionary<string, (PropertyInfo? prop, FieldInfo? field)> configMembers = new();
    public static readonly Dictionary<string, MethodInfo> converters = typeof(Converter)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            m => m.GetCustomAttribute<ConverterKey>()!.Key,
            m => m
        );

    static Particle() {
        foreach(var prop in typeof(Particle).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            var key = prop.GetCustomAttribute<ConverterKey>()?.Key;
            if(key != null) configMembers[key] = (prop, null);
        }

        foreach(var field in typeof(Particle).GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            var key = field.GetCustomAttribute<ConverterKey>()?.Key;
            if(key != null) configMembers[key] = (null, field);
        }
    }
    public Particle() {
        this.position = Vector3.Zero;
        this.vel = Vector3.Zero;
        this.initialVel = Vector3.Zero;
        this.color = Vector3.One;
        this.rotation = 0.0f;
        this.rotationSpeed = 0.0f;
        this.swayPhase = 0.0f;
        this.swayAmplitude = 1.0f;
        this.swayFrequency = 1.0f;
        this.swayVel = Vector3.Zero;
    }
}