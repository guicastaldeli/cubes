namespace App.Root.World.Env.Skybox;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Utils;
using NLua;

/**

    Color Data Helper

    */
class Color {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "utils/TimePeriod.lua");

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
    public static LuaTable? getTop() {
        if(Period.currentPeriod == null) return null;

        LuaFunction? current = data["getTopColor"] as LuaFunction;
        if(current == null) return null;

        object[] res = current.Call(Period.getName(Period.currentPeriod));
        return res[0] as LuaTable;
    }

    // Get Bottom Color
    public static LuaTable? getBottom() {
        if(Period.currentPeriod == null) return null;

        LuaFunction? current = data["getBottomColor"] as LuaFunction;
        if(current == null) return null;

        object[] res = current.Call(Period.getName(Period.currentPeriod));
        return res[0] as LuaTable;
    }

    /**

        Init
    
        */
    public static void init(Skybox skybox, TimeCycle timeCycle) {
        Color.skybox = skybox;
        Color.timeCycle = timeCycle;
        data.DoFile(DATA_PATH);
    }

    /**
    
        Update
    
        */
    public static void update() {
        colors = data["Colors"] as LuaTable;
        currentColor = getCurrent();
    }

    public static void updateColors() {
        LuaTable? newColor = getCurrent();
        if(newColor != null && currentColor != null) {
            if(Period.getName(newColor) != Period.getName(currentColor)) {
                currentColor = newColor;
                Console.WriteLine($"Period changed to: {Period.getName(currentColor)}");
            }
        } 
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

        int topColor = Convert.ToInt32(Color.getTop());
        int bottomColor = Convert.ToInt32(Color.getBottom());

        shaderProgram.setUniform("topColor", topColor);
        shaderProgram.setUniform("bottomColor", bottomColor);
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
        Color.updateColors();
    }
}