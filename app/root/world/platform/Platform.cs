namespace App.Root.World.Platform;
using App.Root.Chunk;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Particle;
using App.Root.Physics;
using App.Root.Player;
using App.Root.Utils;
using NLua;
using OpenTK.Mathematics;
using System.Reflection;

/**

    Platform Themes

    */
[DataInput]
[DataOutput(Path: "player_storage.ps")]
public static class PlatformThemes {
    public class Theme {
        [Convert("int32")] [ConverterKey("id")] public int? Id { get; set; }
        [Convert("string")] [ConverterKey("name")] public string? Name { get; set; }
        [Convert("string")] [ConverterKey("movement")] public string? Movement { get; set; }
        [Convert("string")] [ConverterKey("audio")] public string? Audio { get; set; }
        [Convert("int32")] [ConverterKey("top")] public int? Top { get; set; }
        [Convert("string")] [ConverterKey("particles")] public string? Particles { get; set; }
        [Convert("string")] [ConverterKey("texture")] public string? Texture { get; set; }

        public Theme() {}
    }
    
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/platform/themes/PlatformThemes.lua");

    private static Lua data = null!;
    private static List<Theme> themes = null!;

    private static readonly Dictionary<string, MethodInfo> converters = typeof(Converter)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            m => m.GetCustomAttribute<ConverterKey>()!.Key,
            m => m
        );

    private static readonly Dictionary<string, PropertyInfo> themeProps = typeof(Theme)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(t => t.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            t => t.GetCustomAttribute<ConverterKey>()!.Key,
            t => t
        );

    // Has Data
    public static bool HasData() {
        bool val = themes != null && themes.Count > 0;
        return val;
    }

    // Get Themes
    public static List<Theme> GetThemes() {
        if(themes == null) ExtractData();
        
        List<Theme> val = themes ?? new List<Theme>();
        return val;
    }

    // Get Theme
    public static Theme GetTheme(int id) {
        Theme val = GetThemes().FirstOrDefault(t => t.Id == id)!;
        return val;
    }

    public static Theme GetTheme(string name) {
        Theme val = GetThemes().FirstOrDefault(t => t.Name == name)!;
        return val;
    }
    
    /**
     *
     * Extract Data
     *
     */
    public static object ExtractData() {
        if(themes != null) return themes;

        Load();
        themes = new List<Theme>();
        if(data == null) return themes;

        try {
            var themesData = data["Themes"] as LuaTable;
            if(themesData == null) {
                Console.WriteLine("[PlatformThemes] Themes table not found!");
                return themes;
            }

            foreach(var key in themesData.Keys) {
                var themeData = themesData[key] as LuaTable;
                if(themeData != null) {
                    var theme = new Theme();

                    foreach(var field in themeProps) {
                        var fieldKey = field.Key;
                        var propInfo = field.Value;

                        if(themeData[fieldKey] != null) {
                            var val = themeData[fieldKey];
                            var attr = propInfo.GetCustomAttribute<ConvertAttribute>();
                            
                            if(attr != null) {
                                string name = attr.Converter;

                                if(converters.TryGetValue(name, out var converter)) {
                                    try {
                                        var converted = converter.Invoke(null, new[] { val });
                                        propInfo.SetValue(theme, converted);
                                    } catch(Exception err) {
                                        Console.WriteLine($"[PlatformThemes] Converter error for {fieldKey}: {err.Message}");
                                    }
                                }                                
                            }
                        }
                    }

                    themes.Add(theme);
                }
            }

            Console.WriteLine($"ExtractData() -- [PlatformThemes] Extracted {themes.Count}");
        } catch(Exception err) {
            throw new Exception($"ExtractData() -- [PlatformThemes] Error: {err.Message}");
        }

        return themes;
    }

    /**
     *
     * Save Data
     *
     */
    public static void SaveData(object data) {
        if(data is List<Theme> t) {
            themes = t;
            Console.WriteLine($"SaveData() -- [PlatformThemes] Saved {themes?.Count ?? 0} themes");
        }
    }

    /**
     *
     * Load
     *
     */
    private static void Load() {
        if(data != null) return;

        if(!File.Exists(DATA_PATH)) {
            Console.WriteLine($"[PlatformThemes] File not found: {DATA_PATH}");
            return;
        }

        data = new Lua();

        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        
        try {
            var result = data.DoFile(DATA_PATH);
            
            if(result != null && result.Length > 0) {
                var returnTable = result[0] as LuaTable;
                if(returnTable != null) {
                    var themes = returnTable["Themes"];
                    if(themes != null) {
                        data["Themes"] = themes;
                        Console.WriteLine("[PlatformThemes] Successfully loaded themes from Lua");
                    } else {
                        Console.WriteLine("[PlatformThemes] No 'Themes' key in returned table");
                    }
                }
            }
        } catch(Exception err) {
            Console.WriteLine($"[PlatformThemes] Error loading Lua file: {err.Message}");
            Console.WriteLine(err.StackTrace);
        }
        
        Directory.SetCurrentDirectory(originalDir);
    }
}

/**

    Platform main class

    */
[Chunked]
class Platform : WorldHandler {
    private Mesh mesh;
    private CollisionManager collisionManager;
    private PlatformRegistry platformRegistry;
    private PlayerController playerController;

    public const string GRID_ID = "grid";
    private const string MESH = "cube";

    private const int SIZE_X = ChunkCoord.CHUNK_SIZE;
    private const int SIZE_Y = ChunkCoord.CHUNK_SIZE;
    private const int SIZE_Z = ChunkCoord.CHUNK_SIZE;
    private const float SPACING = 1.0f;

    private Dictionary<ChunkCoord, List<string>> chunkColliders = new();
    private HashSet<ChunkCoord> allGeneratedChunks = new();
    private Vector3 offset = Vector3.Zero;

    private bool initialized = false;

    private const bool DEBUG_EXPANSION = true;

    public static float? Top { get; private set; }
    
    public Platform(
        [Inject] Window window,
        [Inject] Mesh mesh, 
        [Inject] CollisionManager collisionManager, 
        [Inject] PlayerController playerController
    ) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.playerController = playerController;
        this.platformRegistry = new PlatformRegistry(window, mesh, collisionManager, this, playerController);
    
        init();
    }

    // Set Client
    public void setClient() {
        if(initialized) return;
        setPlatform(renderMesh: false);
    }

    // Height
    public Vector3 getHeight() {
        Vector3 meshSize = mesh.getSize(GRID_ID);
        float topY = offset.Y + (SIZE_Y * SPACING) + (meshSize.Y / 2.0f);
        Vector3 res = new Vector3(offset.X, topY, offset.Z); 
        return res;
    }

    // Calculate Top
    private float calculateTop() {
        var topBounds = setBounds(Vector3.Zero, 0, SIZE_Z - 1, 0);
        if(topBounds.HasValue) return topBounds.Value.wy + SPACING / 2.0f;

        float centerOffsetY = (ChunkCoord.CHUNK_SIZE - SIZE_Z) / 2.0f;
        
        float val = centerOffsetY + (SIZE_Y - 1) * SPACING + SPACING / 2.0f;
        return val;
    }

    /**
     * 
     * On Stream
     *
     */
    private void onStream() {
        if(Top.HasValue) EventStream.set("stream-top", (object)Top.Value);
        EventStream.set("streamed-chunks", (object)new List<ChunkCoord>(allGeneratedChunks));
    }

    /**
     * 
     * Set
     *
     */
    // Set Mesh
    private void setMesh(List<Vector3> positions) {
        var renderer = mesh.getMeshRenderer(GRID_ID);
        if(renderer != null) {
            renderer.isInstanced = true;
            renderer.setInstancePositions(positions);
        }
    }

    // Set Bounds
    private (float wx, float wy, float wz)? setBounds(Vector3 chunkOrigin, int x, int y, int z) {
        float centerOffsetX = (ChunkCoord.CHUNK_SIZE - SIZE_X) / 2.0f;
        float centerOffsetY = (ChunkCoord.CHUNK_SIZE - SIZE_Y) / 2.0f;
        float centerOffsetZ = (ChunkCoord.CHUNK_SIZE - SIZE_Z) / 2.0f;

        float wx = chunkOrigin.X + centerOffsetX + x * SPACING;
        float wy = chunkOrigin.Y + centerOffsetY + y * SPACING;
        float wz = chunkOrigin.Z + centerOffsetZ + z * SPACING;

        return (wx, wy, wz);
    }

    // Set Platform Collider
    private void setPlatformCollider(List<string> colliderIds, ChunkCoord coord, List<Vector3> positions) {
        if(positions.Count == 0) return;

        string colliderId = $"{GRID_ID}_{coord.cx}_{coord.cz}";

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        foreach(var pos in positions) {
            if(pos.X < minX) minX = pos.X;
            if(pos.Y < minY) minY = pos.Y;
            if(pos.Z < minZ) minZ = pos.Z;
            if(pos.X > maxX) maxX = pos.X;
            if(pos.Y > maxY) maxY = pos.Y;
            if(pos.Z > maxZ) maxZ = pos.Z;
        }

        Vector3 boxCenter = new Vector3(
            (minX + maxX) / 2.0f,
            (minY + maxY) / 2.0f,
            (minZ + maxZ) / 2.0f
        );
        Vector3 boxHalf = new Vector3(
            (maxX - minX) / 2.0f + SPACING / 2.0f,
            (maxY - minY) / 2.0f + SPACING / 2.0f,
            (maxZ - minZ) / 2.0f + SPACING / 2.0f
        );

        collisionManager.addStaticCollider(new StaticObject(
            boxCenter,
            boxHalf.X, boxHalf.Y, boxHalf.Z,
            colliderId
        ));

        colliderIds.Add(colliderId);
    }

    // Set Platform
    private void setPlatform(bool renderMesh = true) {
        if(!initialized) {
            Top = null;

            mesh.add(GRID_ID, MESH);
            MeshInteractionRegistry.getInstance().register(
                GRID_ID,
                State.GRID,
                mesh,
                PhysicsType.RECEIVER
            );

            Top = calculateTop();

            EventStream.set("stream-top", (object)Top.Value);

            var renderer = mesh.getMeshRenderer(GRID_ID);
            if(renderer != null) renderer.isInstanced = true;

            initialized = true;
        }

        if(DEBUG_EXPANSION && chunkColliders.Count > 0) return;

        if(!ContextChunk.hasChunk) return;
        ChunkCoord coord = ContextChunk.current!.Value;
        if(chunkColliders.ContainsKey(coord)) return;

        var colliderIds = new List<string>();
        Vector3 chunkOrigin = coord.ToWorldPosition();

        var positions = new List<Vector3>();

        for(int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Y; z++) {
                for(int y = 0; y < SIZE_Z; y++) {
                    var bounds = setBounds(chunkOrigin, x, y, z);
                    if(bounds == null) continue;
                    
                    positions.Add(new Vector3(bounds.Value.wx, bounds.Value.wy, bounds.Value.wz));
                }
            }
        }

        if(positions.Count == 0) return;

        setPlatformCollider(colliderIds, coord, positions);
        chunkColliders[coord] = colliderIds;
        allGeneratedChunks.Add(coord);
        ChunkPositions.Add(GRID_ID, coord, positions);

        onStream();

        if(renderMesh) {
            setMesh(positions);
        } else {
            mesh.remove(GRID_ID);
        }
    }

    /**
     * 
     * Merge
     *
     */
    private void merge() {
       // Console.WriteLine($"[Platform] merge() called - IsUsed: {ChunkPositions.IsUsed(GRID_ID)}");
        
        if(ChunkPositions.IsUsed(GRID_ID)) {
            var merged = ChunkPositions.GetMerged(GRID_ID);
            //Console.WriteLine($"[Platform] uploading {merged.Count} positions to GPU");
            mesh.getMeshRenderer(GRID_ID)?.setInstancePositions(merged);
            ChunkPositions.ClearUsed(GRID_ID);
        }
    }

    /**
     * 
     * Load
     *
     */
    private void load() {
        if(!initialized) {
            /*
            platformRegistry.render();
            set2();
            set3();
            set4();

            spawnGrid("cube", new Vector3(4f, 3f, -3f), 5, 3);
            */
        }

        setPlatform();
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        load();
        merge();
    }

    /**
     * 
     * Unrender
     *
     */
    public override void unrender() {
        if(!ContextChunk.hasChunk) return;
        ChunkCoord coord = ContextChunk.current!.Value;

        if(chunkColliders.TryGetValue(coord, out var colliderIds)) {
           foreach(var id in colliderIds) collisionManager.removeCollider(id);
           collisionManager.processRemovals();
           chunkColliders.Remove(coord); 

           onStream();
        }

        ChunkPositions.Remove(GRID_ID, coord);
    }

    private int frameCounter = 0;
        private ParticleEntity? particleEntity = null;

        private void emitParticle() {
            ParticleController particleController = mesh.getParticleController()!;
            Random random = new Random();

            Vector3 position = new Vector3(0.0f, 10.0f, -3.0f);
            Vector3 color = new Vector3(1.0f, 1.0f, 1.0f); 
            int amount = 5;
            float size = 0.1f;
            float speed = 0.3f;
            float lifetime = 2.5f;
            Vector3 velNum = new Vector3(5.0f, 5.0f, 5.0f);

            if(particleEntity == null) {
                particleEntity = particleController.emit(
                    position,
                    color,
                    amount,
                    size,
                    speed,
                    lifetime,
                    velNum,
                    () => {
                        return new Vector3(
                            random.NextSingle(),
                            random.NextSingle(),
                            random.NextSingle()
                        );
                    }
                );
            } else {
                particleEntity.set(
                    new Vector3(0.0f, 10.0f, -3.0f),
                    true,
                    () => {
                        return new Vector3(
                            random.NextSingle(),
                            random.NextSingle(),
                            random.NextSingle()
                        );
                    }
                );
            }
        }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        frameCounter++;

        /*if(frameCounter % 10 == 0) {
            emitParticle();
        }*/
        platformRegistry.update();
    }

    /**
     * 
     * Init
     *
     */
    private void init() {
        platformRegistry.init();
        test();
    }

    private void test() {
        Console.WriteLine("=== TESTING PLATFORM THEMES ===");

        var themes = PlatformThemes.GetThemes();
        Console.WriteLine($"Total themes loaded: {themes.Count}");

        foreach(var theme in themes) {
            Console.WriteLine($"  Theme:");
            var props = typeof(PlatformThemes.Theme).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(var prop in props) {
                var value = prop.GetValue(theme);
                Console.WriteLine($"        {prop.Name} = {value ?? "null"}");
            }
            Console.WriteLine("");
        }
    }
}