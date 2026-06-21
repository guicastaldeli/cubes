namespace App.Root.World.Skybox;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Utils;
using App.Root.Player;
using OpenTK.Mathematics;
using App.Root.Chunk;
using NLua;

/**

    Color Data Helper

    */
class Color {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/skybox/SkyboxColor.lua");

    public static Tick tick = null!;
    public static ShaderProgram shaderProgram = null!;
    public static Skybox skybox = null!;
    public static TimeCycle timeCycle = null!;

    public static Lua data = null!;
    public static LuaTable? colors = null!;
    public static LuaTable? currentColor;

    public static Vector3 prevTopColor;
    public static Vector3 prevBottomColor;
    public static Vector3 currentTopColor;
    public static Vector3 currentBottomColor;
    public static float transitionProgress = 1.0f;
    public static float transitionSpeed = 0.5f;

    // Get Current
    public static LuaTable? getCurrent() {
        LuaFunction? current = data["getCurrentColor"] as LuaFunction;
        if(current == null) return null;

        object[] res = current.Call(timeCycle.getHour());
        return res[0] as LuaTable;
    }

    // Get Top Color
    public static string? getTop() {
        if(currentColor == null) return null;

        LuaFunction? func = data["getTopColor"] as LuaFunction;
        if(func == null) return null;

        object[] res = func.Call(currentColor["name"] as string);
        return res[0] as string;
    }

    // Get Bottom Color
    public static string? getBottom() {
        if(currentColor == null) return null;

        LuaFunction? func = data["getBottomColor"] as LuaFunction;
        if(func == null) return null;

        object[] res = func.Call(currentColor["name"] as string);
        return res[0] as string;
    }

    /**
     * 
     * Init
     *
     */
    public static void init(Tick tick, ShaderProgram shaderProgram, Skybox skybox, TimeCycle timeCycle) {
        Color.tick = tick;
        Color.shaderProgram = shaderProgram;
        Color.skybox = skybox;
        Color.timeCycle = timeCycle;

        data = new Lua();
        
        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        data.DoFile(DATA_PATH);
        Directory.SetCurrentDirectory(originalDir);

        update();
        updateColorValue();
        
        reset();
    }

    /**
     * 
     * Update
     *
     */
    public static void update() {
        if(data == null) return;
        currentColor = getCurrent();
    }

    public static void updateColors() {
        prevTopColor = currentTopColor;
        prevBottomColor = currentBottomColor;

        updateColorValue();
        
        transitionProgress = 0.0f;
    }

    public static void updateColorValue() {
        string? topStr = Convert.ToString(getTop());
        string? bottomStr = Convert.ToString(getBottom());

        var top = HexToRgb.C(topStr!);
        var bottom = HexToRgb.C(bottomStr!);

        currentTopColor = new Vector3(top.r, top.g, top.b);
        currentBottomColor = new Vector3(bottom.r, bottom.g, bottom.b);
    }

    public static void updateTransition() {
        if(transitionProgress < 1.0f) {
            transitionProgress += tick.getDeltaTime() * transitionSpeed;
            if(transitionProgress > 1.0f) transitionProgress = 1.0f;
        }
    }

    /**
     *
     * Reset
     *
     */
    private static void reset() {
        prevTopColor = currentTopColor;
        prevBottomColor = currentBottomColor;
        transitionProgress = 1.0f;
    }
}

/**

    Skybox main class.

    */
[IChunked]
class Skybox : WorldHandler {
    private const string ID = "skybox";
    private const string MESH = "skybox";

    private Tick tick;
    private ShaderProgram shaderProgram;
    private Mesh mesh;
    private Camera camera;
    private TimeCycle timeCycle;
    private PlayerController playerController;

    private SkyboxStar ISkyboxStar = new SkyboxStar();

    private bool initialized = false;
    private string? lastPeriodName = null;

    (float x, float y, float z) pos = (0.0f, 0.0f, 0.0f);
    private float size = 100.0f;

    public Skybox(
        [Inject] Tick tick,
        [Inject] ShaderProgram shaderProgram, 
        [Inject] Mesh mesh,
        [Inject] Camera camera,
        [Inject] TimeCycle timeCycle,
        [Inject] PlayerController playerController
    ) {
        this.tick = tick;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
        this.camera = camera;
        this.timeCycle = timeCycle;
        this.playerController = playerController;

        ISkyboxStar.init(tick, mesh, playerController);
        Color.init(tick, shaderProgram, this, timeCycle);
    }

    /**
     * 
     * Set
     *
     */
    private void set() {
        MeshData data = MeshDataLoader.load(MESH);
        data.shaderType = 8;
        data.shaderAddon = 1;

        mesh.setPosition(ID, pos.x, pos.y, pos.z);
        mesh.add(ID, data);
        mesh.setScale(ID, size);

        Color.update();
        Color.updateColors();

        reset();

        updateShader();
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        if(!initialized) {
            set();

            initialized = true;
        }
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        Vector3 playerPosition = playerController.getPosition();
        mesh.setPosition(ID, playerPosition.X, playerPosition.Y, playerPosition.Z);

        Color.update();

        if(Color.currentColor != null) {
            string? currentName = Color.currentColor["name"] as string;
            if(currentName != lastPeriodName) {
                lastPeriodName = currentName;
                Console.WriteLine($"*** Color changed to: {currentName} ***");

                Color.updateColors();
            }
        }

        Color.updateTransition();

        updateShader();

        ISkyboxStar.update();
    }

    public void updateShader() {
        shaderProgram.setUniform("cameraY", camera.getPosition().Y);
        
        shaderProgram.setUniformb("periodType", Period.getNumber(Period.getCurrent()!));
        shaderProgram.setUniform("currentHour", timeCycle.getHour());
        shaderProgram.setUniform("time", tick.getCurrentTime());

        shaderProgram.setUniformb("topColor", Color.currentTopColor.X, Color.currentTopColor.Y, Color.currentTopColor.Z);
        
        shaderProgram.setUniformb("bottomColor", Color.currentBottomColor.X, Color.currentBottomColor.Y, Color.currentBottomColor.Z);
        shaderProgram.setUniformb("prevTopColor", Color.prevTopColor.X, Color.prevTopColor.Y, Color.prevTopColor.Z);
        shaderProgram.setUniformb("prevBottomColor", Color.prevBottomColor.X, Color.prevBottomColor.Y, Color.prevBottomColor.Z);
        
        shaderProgram.setUniform("periodStart", Period.getStart(Period.getCurrent()!));
        shaderProgram.setUniform("periodEnd", Period.getEnd(Period.getCurrent()!));

        shaderProgram.setUniform("transitionProgress", Color.transitionProgress);
        shaderProgram.setUniform("starTransition", ISkyboxStar.getTransitionProgress());
    }

    /**
     *
     * Reset
     *
     */
    private void reset() {
        Color.prevTopColor = Color.currentTopColor;
        Color.prevBottomColor = Color.currentBottomColor;
        Color.transitionProgress = 1.0f;
    }

    /**

        Skybox Star class.

        */
    [Chunked]
    public class SkyboxStar {
        private string STAR_ID = "star";
        private string STAR_MESH = "quad";

        private const int STARS_PER_CHUNK = 40;
    
        private Tick tick = null!;
        private Mesh mesh = null!;
        private PlayerController playerController = null!;

        private Dictionary<ChunkCoord, List<Vector3>> chunkLocalPositions = new();
        private Dictionary<ChunkCoord, List<float[]>> chunkColors = new();
        private Dictionary<ChunkCoord, List<float>> chunkRotations = new();
        private Dictionary<ChunkCoord, List<float>> chunkSpeeds = new();

        private Dictionary<ChunkCoord, float> chunkFieldRotation = new();
        private const float FIELD_ROTATION_SPEED = 0.02f;
        private Vector3 fieldAxis = new Vector3(1, 0, 0);

        private string prevPeriodName = "";
        private float transitionProgress = 1.0f;
        private float transitionDuration = 0.5f;
        private bool isTransitioning = false;
        
        private bool visible = true;
        private bool targetVisibility = true;
        private bool currentVisibility = true;

        private bool initialized = false;

        public SkyboxStar() {
            
        }

        // Start Transition
        private void startTransition(bool appearing) {
            isTransitioning = true;
            visible = appearing;

            if(appearing) {
                transitionProgress = 0.0f;
                currentVisibility = true;
            } else {
                transitionProgress = 1.0f;
            }
        }

        // Get Chunk Sphere Center
        private Vector3 getChunkCenter(ChunkCoord coord) {
            Vector3 origin = coord.ToWorldPosition();
            float chunkSize = ChunkCoord.CHUNK_SIZE;

            var (_, maxY) = ChunkCoord.GetHeightRange(coord);
            float spawnHeight = maxY;

            return new Vector3(
                origin.X + chunkSize / 2.0f,
                spawnHeight,
                origin.Z + chunkSize / 2.0f
            );
        }

        // Get Transition Progress
        public float getTransitionProgress() {
            return transitionProgress;
        }

        // Is Visible
        private bool isVisible() {
            return Period.isAssetPeriod();
        }

        /**
         * 
         * Init
         *
         */
        public void init(Tick tick, Mesh mesh, PlayerController playerController) {
            this.tick = tick;
            this.mesh = mesh;
            this.playerController = playerController;
        }

        /**
         * 
         * Generate
         *
         */
        private (List<Vector3>, List<float[]>, List<float>, List<float>) generate(ChunkCoord coord) {
            int seed = HashCode.Combine(coord.cx, coord.cz);
            var range = new Random(seed);

            var positions = new List<Vector3>();
            var colors = new List<float[]>();
            var rotations = new List<float>();
            var speeds = new List<float>();

            for(int i = 0; i < STARS_PER_CHUNK; i++) {
                float u = (float)range.NextDouble();
                float v = (float)range.NextDouble();

                float theta = 2.0f * MathF.PI * u;
                float phi = MathF.Acos(2.0f * v - 1.0f);

                float x = MathF.Sin(phi) * MathF.Cos(theta);
                float y = MathF.Sin(phi) * MathF.Sin(theta);
                float z = MathF.Cos(phi);

                float depthVar = 0.4f + (float)range.NextDouble() * 0.01f;
                float brightness = 0.5f + (float)range.NextDouble() * 0.5f;
                float size = 0.1f + (float)range.NextDouble() * 1.5f;
                float rotation = (float)(range.NextDouble() * 2.0f * MathF.PI);
                float speed = 0.2f + (float)range.NextDouble() * 0.8f;

                positions.Add(new Vector3(x, y, z) * depthVar);
                colors.Add(new float[] { brightness, brightness, brightness, size });
                rotations.Add(rotation);
                speeds.Add(speed);
            }

            return (positions, colors, rotations, speeds);
        }

        /**
         * 
         * Upload
         *
         */
        private void upload() {
            var allPositions = new List<Vector3>();
            var allColors = new List<float[]>();
            var allRotations = new List<float>();

            foreach(var (coord, positions) in chunkLocalPositions) {
                Vector3 center = getChunkCenter(coord);

                float fieldRotation = chunkFieldRotation.TryGetValue(coord, out var r) ? r : 0.0f;
                Matrix4 rotationMatrix = Matrix4.CreateFromAxisAngle(fieldAxis, fieldRotation);

                foreach(var pos in positions) {
                    Vector3 worldPos = center + Vector3.TransformPosition(pos * ChunkCoord.CHUNK_SIZE, rotationMatrix);
                    allPositions.Add(worldPos);
                }

                allColors.AddRange(chunkColors[coord]);
                allRotations.AddRange(chunkRotations[coord]);
            }

            var renderer = mesh.getMeshRenderer(STAR_ID);
            if(renderer == null || allPositions.Count == 0) return;

            if(renderer.getInstanceVboInitialized()) {
                renderer.updateInstanceData(allPositions, allColors, allRotations);
            } else {
                renderer.setInstanceData(allPositions, allColors, allRotations);
            }
        }

        /**
         * 
         * Update
         *
         */
        // Update
        public void update() {
            string currentPeriod = Period.currentPeriod != null ? Period.getName(Period.currentPeriod) : "";
            if(currentPeriod != prevPeriodName) {
                bool nowVisible = isVisible();
                
                if(nowVisible != targetVisibility) {
                    targetVisibility = nowVisible;
                    startTransition(nowVisible);
                }

                prevPeriodName = currentPeriod;
            }

            if(isTransitioning) updateTransition();
            if(transitionProgress <= 0.0f && !isTransitioning && !targetVisibility) return;

            float deltaTime = tick.getDeltaTime();
            float multiplier = isTransitioning ? 3.0f : 1.0f;
            float tp = MathF.PI * 2.0f;

            foreach(var (coord, rotations) in chunkRotations) {
                var speeds = chunkSpeeds[coord];
                for(int i = 0; i < rotations.Count; i++) {
                    rotations[i] += speeds[i] * deltaTime * multiplier;
                    if(rotations[i] > tp) rotations[i] -= tp;
                }
            }

            foreach(var coord in chunkLocalPositions.Keys.ToList()) {
                chunkFieldRotation[coord] -= FIELD_ROTATION_SPEED * deltaTime;
                if(chunkFieldRotation[coord] < 0.0f) chunkFieldRotation[coord] += tp;
            }

            upload();
        }

        // Update Transition
        private void updateTransition() {
            float deltaTime = tick.getDeltaTime();

            if(visible) {
                transitionProgress += deltaTime / transitionDuration;
                if(transitionProgress >= 1.0f) {
                    transitionProgress = 1.0f;
                    isTransitioning = false;
                    currentVisibility = true;
                }
            } else {
                transitionProgress -= deltaTime / transitionDuration;
                if(transitionProgress <= 0.0f) {
                    transitionProgress = 0.0f;
                    isTransitioning = false;
                    currentVisibility = false;
                }
            }
        }

        /**
         * 
         * Render
         *
         */
        public void render() {
            if(!ContextChunk.hasChunk) return;
            ChunkCoord coord = ContextChunk.current!.Value;

            if(!initialized) {
                MeshData data = MeshDataLoader.load(STAR_MESH);
                data.shaderType = 9;
                
                mesh.add(STAR_ID, data);
                mesh.setPosition(STAR_ID, 0.0f, 0.0f, 0.0f);
                
                var renderer = mesh.getMeshRenderer(STAR_ID);
                if(renderer != null) renderer.isInstanced = true;

                initialized = true;
            }

            if(chunkLocalPositions.ContainsKey(coord)) return;

            var (localPositions, colors, rotations, speeds) = generate(coord);
            chunkLocalPositions[coord] = localPositions;
            chunkColors[coord] = colors;
            chunkRotations[coord] = rotations;
            chunkSpeeds[coord] = speeds;
            chunkFieldRotation[coord] = 0.0f;

            upload();
        }

        /**
         * 
         * Unrender
         *
         */
        public void unrender() {
            if(!ContextChunk.hasChunk) return;
            ChunkCoord coord = ContextChunk.current!.Value;

            chunkLocalPositions.Remove(coord);
            chunkColors.Remove(coord);
            chunkRotations.Remove(coord);
            chunkSpeeds.Remove(coord);
            chunkFieldRotation.Remove(coord);

            upload();
        }
    }
}

