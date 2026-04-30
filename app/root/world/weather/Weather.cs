namespace App.Root.World.Weather;
using App.Root.Mesh;
using App.Root.Mesh.Particle;
using App.Root.Shaders;
using App.Root.Utils;
using OpenTK.Mathematics;
using NLua;

/**

    General Data helpers

    */
class WeatherEntry {
    public int Id;
    public string? Name;
    public float Frequency;
    public int Value;
}

class ParticleConfig {
    public float[] Color = { 1.0f, 1.0f, 1.0f };
    public int Amount = 10;
    public float Size = 1.0f;
    public float Speed = 1.0f;
    public float Lifetime = 1.0f;
    public float[] Vel = { 0.0f, 1.0f, 0.0f };
}

class TempConfig {
    public float R;
    public float G;
    public float B;
    public float Strength;
}

class WeatherType {
    public const string NORMAL = "NORMAL";
    public const string RAIN = "RAIN";    
    public const string SNOW = "SNOW";
}

/**

    Weather data helper

    */
class WeatherData {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/weather/WeatherData.lua");
    public static string DEFAULT_WEATHER = WeatherType.SNOW;

    private static Weather weather = null!;
    private static Lua data = null!;

    /**
    
        Init
    
        */
    public static void init(Weather weather) {
        WeatherData.weather = weather;
        data = new Lua();

        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        
        data.DoFile(DATA_PATH);
        Directory.SetCurrentDirectory(originalDir);
    }

    // Get Entries
    public static List<WeatherEntry> getEntries() {
        var res = new List<WeatherEntry>();

        var func = data["getTypes"] as LuaFunction;
        if(func == null) return res;

        var table = func.Call()[0] as LuaTable;
        if(table == null) return res;

        foreach(string key in table.Keys) {
            var row = table[key] as LuaTable;
            if(row == null) continue;

            res.Add(new WeatherEntry {
                Id = Convert.ToInt32(row["i"]),
                Name = key,
                Frequency = Convert.ToSingle(row["f"]),
                Value = Convert.ToInt32(row["v"])
            });
        }

        return res;
    }

    // Get Particle Config
    public static ParticleConfig? getParticleConfig(string weatherName) {
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
    public static TempConfig? getTempConfig(int addonId) {
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

    // Get Min Height
    public static void setMinHeight() {
        LuaFunction? func = data["setMinHeight"] as LuaFunction;
        if(func == null) return;

        func.Call(weather.getWorldMinHeight());
    }
}

/**

    Weather main class.

    */
class Weather : WorldHandler {
    private Tick tick;
    private Mesh mesh;
    private ShaderProgram shaderProgram;
    private World world;

    private WeatherCycle weatherCycle;
    private string currentWeather = WeatherData.DEFAULT_WEATHER;

    private TempConfig? currentTemp;
    private TempConfig? prevTemp;
    private float tempTransition = 1.0f;
    private float tempTransSpeed = 0.5f;

    private ParticleEntity? partActiveEmitter;
    private float partEmitTimer = 0.0f;
    private float partEmitInterval = 0.08f;

    private bool initialized = false;

    public Weather([Inject] Tick tick, [Inject] Mesh mesh, [Inject] ShaderProgram shaderProgram, [Inject] World world) {
        ServiceContainer.ActiveSRegister(true);
        
        this.tick = tick;
        this.mesh = mesh;
        this.shaderProgram = shaderProgram;
        this.world = world;

        this.weatherCycle = new WeatherCycle();
    
        ServiceContainer.ActiveSRegister(false);
    }

    // On Weather Changed
    private void onWeatherChanged(string prev, string next) {
        Console.WriteLine($"*** Weather changed: {prev} → {next} ***");
        currentWeather = next;

        prevTemp = currentTemp;
        currentTemp = WeatherData.getTempConfig(getWeatherValue(next));
        tempTransition = 0.0f;

        stopPartEmitter();
        if(next != WeatherData.DEFAULT_WEATHER) startPartEmitter(next);
    }

    // Get Weather Value
    private int getWeatherValue(string name) {
        int val = WeatherData.getEntries()
            .FirstOrDefault(e => e.Name == name)
            ?.Value ?? 0;
        return val;
    }

    /**
    
        World Height
    
        */
    public float getWorldMinHeight() {
        var boundary = world.getWorldBoundary().getBoundaryObject();
        if(boundary != null) {
            float minHeight = boundary.getMinHeight();
            return minHeight;
        }

        return 0.0f;
    }

    public float getWorldMaxHeight() {
        var boundary = world.getWorldBoundary().getBoundaryObject();
        if(boundary != null) {
            float maxHeight = boundary.getMaxHeight();
            return maxHeight;
        }

        return 0.0f;
    }

    /**
    
        Set
    
        */
    private void set() {
        WeatherData.init(this);
        WeatherData.setMinHeight();

        var entries = WeatherData.getEntries();
        weatherCycle.init(entries);
        weatherCycle.onWeatherChanged += onWeatherChanged;
    }

    /**
    
        Particle Emitter
    
        */
    // Start Emitter
    private void startPartEmitter(string weatherName) {
        partEmitTimer = partEmitInterval;
    }

    // Stop Emitter
    private void stopPartEmitter() {
        partActiveEmitter = null;
        partEmitTimer = 0.0f;
    } 

    // Set Position
    private Vector3 setPosition() {
        float height = getWorldMaxHeight();
        Vector3 val = new Vector3(0.0f, height, 0.0f);
        return val;
        
    }

    private float setPositionf() {
        return World.WORLD_BOUNDARY;
    }

    /**
    
        Render
    
        */
    public override void render() {
        //base.render();
    }

    /**
    
        Update
    
        */
    // Update
    public override void update() {
        if(!initialized) {
            set();
            initialized = true;
        }

        float deltaTime = tick.getDeltaTime();

        weatherCycle.update(deltaTime);
        updateTempTransition(deltaTime);
        updateShader();
        updateParticles(deltaTime);
    }

    // Update Temp Transition
    private void updateTempTransition(float deltaTime) {
        if(tempTransition < 1.0f) {
            tempTransition = Math.Min(1.0f, tempTransition + deltaTime * tempTransSpeed);
        }
    }

    // Update Shader
    private void updateShader() {
        float r = 0.0f;
        float g = 0.0f;
        float b = 0.0f;
        float strength = 0.0f;

        if(currentTemp != null) {
            float prevR = prevTemp?.R ?? 0.0f;
            float prevG = prevTemp?.G ?? 0.0f;
            float prevB = prevTemp?.B ?? 0.0f;
            float prevStrength = prevTemp?.Strength ?? 0.0f;

            r = Lerp.S(prevR, currentTemp.R, tempTransition);
            g = Lerp.S(prevG, currentTemp.G, tempTransition);
            b = Lerp.S(prevB, currentTemp.B, tempTransition);
            strength = Lerp.S(prevStrength, currentTemp.Strength, tempTransition); 
        } else if(prevTemp != null) {
            r = Lerp.S(prevTemp.R, 0.0f, tempTransition);
            g = Lerp.S(prevTemp.G, 0.0f, tempTransition);
            b = Lerp.S(prevTemp.B, 0.0f, tempTransition);
            strength = Lerp.S(prevTemp.Strength, 0.0f, tempTransition);
        }

        shaderProgram.setUniformb("weatherTemp", r, g, b);
        shaderProgram.setUniform("weatherStrength", strength);
    }

    // Update Particles
    private void updateParticles(float deltaTime) {
        if(currentWeather == WeatherType.NORMAL) return;
    
        partEmitTimer += deltaTime;
        if(partEmitTimer < partEmitInterval) return;
        partEmitTimer = 0.0f;

        var config = WeatherData.getParticleConfig(currentWeather);
        if(config == null) return;

        var controller = mesh.getParticleController();
        if(controller == null) return;

        if(partActiveEmitter == null) {
            partActiveEmitter = controller.emit(
                position: setPosition(),
                color: new Vector3(config.Color[0], config.Color[1], config.Color[2]),
                amount: config.Amount,
                size: config.Size,
                speed: config.Speed,
                lifetime: config.Lifetime,
                velNum: new Vector3(config.Vel[0], config.Vel[1], config.Vel[2]),
                targetY: getWorldMinHeight(),
                enableMotion: true,
                spawnRadius: setPositionf()
            );
        } else {
            partActiveEmitter.set(setPosition(), true, getWorldMinHeight());
        }
    }
}