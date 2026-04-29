namespace App.Root.World.Weather;

using System.Runtime.CompilerServices;
using App.Root.Mesh;
using App.Root.Utils;
using NLua;

/**

    General Data helpers

    */
class WeatherEntry {
    public int Id;
    public float Frequency;
    public int ShaderAddon;
}

class ParticleConfig {
    public float[] Color = { 1.0f, 1.0f, 1.0f };
    public int Amount = 10;
    public float Size = 1.0f;
    public float Speed = 1.0f;
    public float Lifetime = 1.0f;
    public float[] Vel = { 0.0f, -1.0f, 0.0f };
}

class TempConfig {
    public float R;
    public float G;
    public float B;
    public float Strength;
}

/**

    Weather data helper

    */
class WeatherData {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/weather/WeatherData.lua");

    private Lua data = null!;

    /**
    
        Init
    
        */
    public void init() {
        data = new Lua();

        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        
        data.DoFile(DATA_PATH);
        Directory.SetCurrentDirectory(originalDir);
    }

    // Get Entries
    public List<WeatherEntry> getEntries() {
        var res = new List<WeatherEntry>();

        var func = data["getTypes"] as LuaFunction;
        if(func == null) return res;

        var table = func.Call()[0] as LuaTable;
        if(table == null) return res;

        foreach(string key in table.Keys) {
            var row = table[key] as LuaTable;
            if(row == null) continue;

            res.Add(new WeatherEntry {
                Id = Convert.ToInt32(row["id"]),
                Frequency = Convert.ToSingle(row["f"]),
                ShaderAddon = Convert.ToInt32(row["s"])
            });
        }

        return res;
    }

    // Get Particle Config
    public ParticleConfig? getParticleConfig(string weatherName) {
        var func = data["getParticle"] as LuaFunction;
        if(func == null) return null;

        var res = func.Call(weatherName);
        if(res == null || res.Length == 0) return null;

        var t = res[0] as LuaTable;
        if(t == null) return null;

        var colorTable = t["color"] as LuaTable;
        var velTable = t["vel"] as LuaTable;

        return new ParticleConfig {
            Color = TableToFloat.T(colorTable, 3),
            Amount = Convert.ToInt32(t["amount"]),
            Size = Convert.ToSingle(t["size"]),
            Speed = Convert.ToSingle(t["speed"]),
            Lifetime = Convert.ToSingle(t["lifetime"]),
            Vel = TableToFloat.T(velTable, 3),
        };
    }

    // Get Temp Config
    public TempConfig? getTempConfig(int addonId) {
        if(addonId <= 0) return null;

        var func = data["getTemp"] as LuaFunction;
        if(func == null) return null;

        var res = func.Call(addonId);
        if(res == null || res.Length == 0) return null;

        var t = res[0] as LuaTable;
        if(t == null) return null;

        return new TempConfig {
            R = Convert.ToSingle(t["r"]),
            G = Convert.ToSingle(t["g"]),
            B = Convert.ToSingle(t["b"]),
            Strength = Convert.ToSingle(t["strength"])
        };
    }
}

class Weather : WorldHandler {
    private Tick tick;
    private Mesh mesh;

    public Weather([Inject] Tick tick, [Inject] Mesh mesh) {
        this.tick = tick;
        this.mesh = mesh;
    }

    /**
    
        Set
    
        */
    private void set() {
        
    }


    /**
    
        Render
    
        */
    public override void render() {
        base.render();
    }

    /**
    
        Update
    
        */
    public override void update() {
        base.update();
    }
}