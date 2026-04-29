/**

    Weather Cycle to manage main
    weather updates.

    */
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

    // Get Current
    public string getCurrent() {
        return currentName;
    }

    // Get Previous
    public string getPrevious() {
        return prevName;
    }

    // Weighted Random
    private string weigthedRandom() {
        float total = entries.Sum(e => e.Frequency);
        float roll = (float)(range.NextDouble() * total);
        float cumulative = 0.0f;

        foreach(var entry in entries) {
            cumulative += entry.Frequency;
            if(roll <= cumulative) return entry.Name!;
        }
        
        return entries[0].Name!;
    }

    /**
    
        Next
    
        */
    private void next(bool force = false) {
        string next = weigthedRandom();
        if(next == currentName && !force) next = weigthedRandom();
        
        prevName = currentName;
        currentName = next;
        timer = 0.0f;

        float minDuration =
            Duration.MIN_DURATION_LOW +
            (float)(range.NextDouble() * 
            (Duration.MIN_DURATION_HIGH - Duration.MIN_DURATION_LOW));

        float maxDuration =
            Duration.MAX_DURATION_LOW +
            (float)(range.NextDouble() * 
            (Duration.MAX_DURATION_HIGH - Duration.MAX_DURATION_LOW));

        duration = 
            minDuration + 
            (float)(range.NextDouble() * 
            (maxDuration - minDuration));

        if(force || prevName != currentName) {
            onWeatherChanged?.Invoke(prevName, currentName);
        }
    }

    /**
    
        Init
    
        */
    public void init(List<WeatherEntry> entries) {
        this.entries = entries;
        next(force: true);
    }

    /**
    
        Update
    
        */
    public void update(float deltaTime) {
        timer += deltaTime;
        if(timer >= duration) next();
    }
}