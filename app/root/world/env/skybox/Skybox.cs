namespace App.Root.World.Env.Skybox;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Utils;
using NLua;

/**

    Color Data Helper

    */
class Color {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/env/skybox/SkyboxColor.lua");

    public static Skybox skybox = null!;
    public static TimeCycle timeCycle = null!;

    public static Lua data = null!;
    public static LuaTable? colors = null!;
    public static LuaTable? currentColor;

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
    public static void init(Skybox skybox, TimeCycle timeCycle) {
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
        LuaTable? newColor = getCurrent();
        if(newColor == null) return;
        if(currentColor == null) return;;

        string? newName = newColor["name"] as string;
        string? currentName = currentColor["name"] as string;
        if(newName != currentName) currentColor = newColor;
    }
}

/**

    Skybox main class.

    */
class Skybox : WorldHandler {
    private const string ID = "skybox";
    private const string MESH = "skybox";

    private ShaderProgram shaderProgram;
    private Mesh mesh;
    private TimeCycle timeCycle;

    private bool initialized = false;
    private string? lastPeriodName = null;

    public Skybox(
        [Inject] ShaderProgram shaderProgram, 
        [Inject] Mesh mesh,
        [Inject] TimeCycle timeCycle
    ) {
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
        this.timeCycle = timeCycle;

        Color.init(this, timeCycle);
    }

    /**
    
        Set
    
        */
    private void set() {
        MeshData data = MeshDataLoader.load(MESH);
        data.shaderType = 8;

        mesh.setPosition(ID, 0.0f, 0.0f, 0.0f);
        mesh.add(ID, data);
        mesh.setScale(ID, 100.0f);
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
                updateColors();
            }
        }
    }

    private void updateColors() {
        string? topStr = Convert.ToString(Color.getTop());
        string? bottomStr = Convert.ToString(Color.getBottom());

        var topColor = HexToRgb.C(topStr!);
        var bottomColor = HexToRgb.C(bottomStr!);

        shaderProgram.setUniform("topColor", topColor.r, topColor.g, topColor.b);
        shaderProgram.setUniform("bottomColor", bottomColor.r, bottomColor.g, bottomColor.b);
    }
}