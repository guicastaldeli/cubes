namespace App.Root.World.Weather;
using App.Root.Mesh;
using App.Root.Utils;
using NLua;

/**

    Weather data helper.

    */
class WeatherData {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/weather/WeatherData.lua");

    private Lua data;
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