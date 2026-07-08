namespace App.Root.World.Weather;

/**

    Duration helper

    */
static class Duration {
    public const float MIN_DURATION_LOW = 60.0f;
    public const float MIN_DURATION_HIGH = 300.0f;

    public const float MAX_DURATION_LOW = 300.0f;
    public const float MAX_DURATION_HIGH = 600.0f;
}

/**

    Weather Cycle main class

    */
class WeatherCycle {
    private List<WeatherEntry> entries = new();
    private Random range = new Random();

    private string currentName = WeatherData.DEFAULT_WEATHER;
    private string prevName = WeatherData.DEFAULT_WEATHER;

    private float timer = 0.0f;
    private float duration = 0.0f;

    public event Action<string, string>? onWeatherChanged;

    public string getCurrent() {
        return currentName;
    }

    public string getPrevious() {
        return prevName;
    }

    private string weigthedRandom() {
        if(entries.Count == 0) return WeatherData.DEFAULT_WEATHER;
        
        float total = entries.Sum(e => e.Frequency);
        float roll = (float)(range.NextDouble() * total);
        float cumulative = 0.0f;

        foreach(var entry in entries) {
            cumulative += entry.Frequency;
            if(roll <= cumulative) return entry.Name!;
        }
        
        return entries[0].Name!;
    }

    public void forceWeather(string weather) {
        Console.WriteLine($"[WeatherCycle] Force weather to: {weather}");
        prevName = currentName;
        currentName = weather;
        timer = 0.0f;
        SetRandomDuration();
        onWeatherChanged?.Invoke(prevName, currentName);
    }

    private void SetRandomDuration() {
        float minDuration = Duration.MIN_DURATION_LOW + 
            (float)(range.NextDouble() * (Duration.MIN_DURATION_HIGH - Duration.MIN_DURATION_LOW));
        float maxDuration = Duration.MAX_DURATION_LOW + 
            (float)(range.NextDouble() * (Duration.MAX_DURATION_HIGH - Duration.MAX_DURATION_LOW));
        duration = minDuration + (float)(range.NextDouble() * (maxDuration - minDuration));
        Console.WriteLine($"[WeatherCycle] Next weather in {duration:F1} seconds");
    }

    private void next(bool force = false) {
        if(entries.Count == 0) {
            Console.WriteLine("[WeatherCycle] No weather entries available!");
            return;
        }
        
        string next = weigthedRandom();
        if(next == currentName && !force) next = weigthedRandom();
        
        prevName = currentName;
        currentName = next;
        timer = 0.0f;
        SetRandomDuration();

        Console.WriteLine($"[WeatherCycle] Changing weather to: {next}");
        onWeatherChanged?.Invoke(prevName, currentName);
    }

    public void init(List<WeatherEntry> entries) {
        Console.WriteLine($"[WeatherCycle] Initializing with {entries.Count} entries");
        this.entries = entries;
        if(entries.Count > 0) {
            currentName = entries[0].Name!;
            if(!Weather.debugMode) next(force: true);
        } else {
            Console.WriteLine("[WeatherCycle] WARNING: No weather entries found!");
        }
    }

    public void update(float deltaTime) {
        if(entries.Count == 0) return;
        
        timer += deltaTime;
        if(timer >= duration) {
            Console.WriteLine($"[WeatherCycle] Timer {timer:F1}s >= duration {duration:F1}s, changing weather...");
            next();
        }
    }
}