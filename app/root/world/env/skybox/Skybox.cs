namespace App.Root.World.Env.Skybox;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Utils;
using OpenTK.Mathematics;
using NLua;
using App.Root.Player;

/**

    Color Data Helper

    */
class Color {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/env/skybox/SkyboxColor.lua");

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

        Init
    
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
    }

    /**
    
        Update
    
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
}

/**

    Skybox main class.

    */
class Skybox : WorldHandler {
    private const string ID = "skybox";
    private const string MESH = "skybox";

    private Tick tick;
    private ShaderProgram shaderProgram;
    private Mesh mesh;
    private TimeCycle timeCycle;

    private bool initialized = false;
    private string? lastPeriodName = null;

    (float x, float y, float z) pos = (0.0f, 0.0f, 0.0f);
    private float size = 100.0f;

    public Skybox(
        [Inject] Tick tick,
        [Inject] ShaderProgram shaderProgram, 
        [Inject] Mesh mesh,
        [Inject] TimeCycle timeCycle
    ) {
        this.tick = tick;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
        this.timeCycle = timeCycle;

        SkyboxStar.init(tick, mesh);
        Color.init(tick, shaderProgram, this, timeCycle);
    }

    /**
    
        Set
    
        */
    private void set() {
        MeshData data = MeshDataLoader.load(MESH);
        data.shaderType = 8;

        mesh.setPosition(ID, pos.x, pos.y, pos.z);
        mesh.add(ID, data);
        mesh.setScale(ID, size);

        SkyboxStar.set();

        Color.updateColors();
    }

    /**
    
        Render

        */  
    public override void render() {
        if(!initialized) {
            set();

            initialized = true;
        }
    }

    /**
    
        Update

        */ 
    public override void update() {
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

        SkyboxStar.update();
    }

    private void updateShader() {
        shaderProgram.setUniformB("periodType", Period.getNumber(Period.getCurrent()!));
        shaderProgram.setUniform("currentHour", timeCycle.getHour());
        shaderProgram.setUniform("time", tick.getCurrentTime());

        shaderProgram.setUniformB("topColor", Color.currentTopColor.X, Color.currentTopColor.Y, Color.currentTopColor.Z);
        
        shaderProgram.setUniformB("bottomColor", Color.currentBottomColor.X, Color.currentBottomColor.Y, Color.currentBottomColor.Z);
        shaderProgram.setUniformB("prevTopColor", Color.prevTopColor.X, Color.prevTopColor.Y, Color.prevTopColor.Z);
        shaderProgram.setUniformB("prevBottomColor", Color.prevBottomColor.X, Color.prevBottomColor.Y, Color.prevBottomColor.Z);
        
        shaderProgram.setUniform("transitionProgress", Color.transitionProgress);
        shaderProgram.setUniform("starTransition", SkyboxStar.getTransitionProgress());
    }

    /**

        Skybox Star class.

        */
    private static class SkyboxStar {
        private static string STAR_ID = "star";
        private static string STAR_MESH = "quad";

        private static int STAR_COUNT = 600;
        private static float STAR_RADIUS = 25.0f;
    
        private static Tick tick = null!;
        private static Mesh mesh = null!;

        static (float x, float y, float z) pos = (0.0f, 0.0f, 0.0f);

        private static List<Vector3> originalPos = new();
        private static List<float[]> originalColors = new();
        private static List<float> currentRotations = new();
        private static List<float> speeds = new();

        private static float fieldRotation = 0.0f;
        private static float fieldRotationSpeed = 0.01f;
        private static Vector3 fieldAxis = new Vector3(1, 0, 0);

        private static int prevPeriodType = -1;
        private static float transitionProgress = 1.0f;
        private static float transitionDuration = 0.5f;
        private static bool isTransitioning = false;
        
        private static bool visible = true;
        private static bool targetVisibility = true;
        private static bool currentVisibility = true;

        /**
        
            Init
        
            */
        public static void init(Tick tick, Mesh mesh) {
            SkyboxStar.tick = tick;
            SkyboxStar.mesh = mesh;
        }

        /**
        
            Set
        
            */
        public static void set() {
            MeshData data = MeshDataLoader.load(STAR_MESH);
            data.shaderType = 9;

            mesh.add(STAR_ID, data);
            mesh.setPosition(STAR_ID, pos.x, pos.y, pos.z);

            var (positions, colors, rotations) = generate();
            originalPos = new List<Vector3>(positions);
            originalColors = new List<float[]>(colors);
            currentRotations = new List<float>(rotations);
            setSpeed(rotations);

            mesh.getMeshRenderer(STAR_ID)!.isInstanced = true;
            mesh.getMeshRenderer(STAR_ID)!.setInstanceData(positions, colors, rotations);
        }

        // Get Transition Progress
        public static float getTransitionProgress() {
            return transitionProgress;
        }

        // Is Visible
        private static bool isVisible(int periodType) {
            bool val = periodType >= 1 && 
                periodType <= 4;
            return val;
        }

        // Set Speed
        private static void setSpeed(List<float> rotations) {
            var rand = new Random(123);
            speeds = new List<float>();
            for(int i = 0; i < rotations.Count; i++) {
                speeds.Add(0.2f + (float)rand.NextDouble() * 0.8f);
            }
        }

        // Start Transition
        private static void startTransition(bool appearing) {
            isTransitioning = true;
            visible = appearing;

            if(appearing) {
                transitionProgress = 0.0f;
                currentVisibility = true;
            } else {
                transitionProgress = 1.0f;
            }
        }

        /**
        
            Generate
        
            */
        public static (List<Vector3>, List<float[]>, List<float>) generate() {
            var positions = new List<Vector3>();
            var colors = new List<float[]>();
            var rotations = new List<float>();

            float angle = MathF.PI * (3.0f - MathF.Sqrt(5.0f));
            var range = new Random(42);

            for(int i = 0; i < STAR_COUNT; i++) {
                float rotation = (float)(range.NextDouble() * 2.0 * Math.PI);

                float y = 1.0f - (i / (float)(STAR_COUNT - 1)) * 2.0f;
                float radiusAtY = MathF.Sqrt(1.0f - y * y);
                float theta = angle * i;

                float x = MathF.Cos(theta) * radiusAtY;
                float z = MathF.Sin(theta) * radiusAtY;

                float brightness = 0.5f + (float)range.NextDouble() * 0.5f;
                float size = 0.1f + (float)range.NextDouble() * 1.5f;

                float depth = 1.0f + (float)(range.NextDouble() * 1.0f - 0.1f);
                float radius = STAR_RADIUS * depth;

                positions.Add(new Vector3(x, y, z) * radius);
                colors.Add(new float[] {
                    brightness, brightness, brightness,
                    size
                });
                rotations.Add(rotation);
            }

            return (positions, colors, rotations);
        }

        /**
        
            Update
        
            */
        public static void update() {
            int currentPeriod = Period.getNumber(Period.getCurrent()!);
            if(currentPeriod != prevPeriodType) {
                bool nowVisible = isVisible(currentPeriod);
                
                if(nowVisible != targetVisibility) {
                    targetVisibility = nowVisible;
                    startTransition(nowVisible);
                }

                prevPeriodType = currentPeriod;
            }

            if(isTransitioning) {
                updateTransition();
            }
            if(transitionProgress <= 0.0f && !isTransitioning && !targetVisibility) {
                return;
            }

            float multiplier = isTransitioning ? 3.0f : 1.0f;
            for(int i = 0; i < currentRotations.Count; i++) {
                currentRotations[i] += speeds[i] * tick.getDeltaTime() * multiplier;
                if(currentRotations[i] > MathF.PI * 2.0f) currentRotations[i] -= MathF.PI * 2.0f;
            }

            fieldRotation += fieldRotationSpeed * tick.getDeltaTime();
            if(fieldRotation > MathF.PI * 2.0f) fieldRotation -= MathF.PI * 2.0f;

            Matrix4 rotationMatrix = Matrix4.CreateFromAxisAngle(fieldAxis, fieldRotation);
            var rotattedPositions = new List<Vector3>();
            for(int i = 0; i < originalPos.Count; i++) {
                Vector3 pos = Vector3.TransformPosition(originalPos[i], rotationMatrix);
                rotattedPositions.Add(pos);
            }

            var renderer = mesh.getMeshRenderer(STAR_ID);
            if(renderer != null) renderer.updateInstanceData(rotattedPositions, originalColors, currentRotations);
        }

        private static void updateTransition() {
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
    }
}

