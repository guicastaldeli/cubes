/**

    Convert Time util class

    */
namespace App.Root.Utils;

public static class ConvertTime {
    /**

        Time Unit

        */
    public enum TimeUnit {
        Seconds,
        Minutes,
        Hours
    }

    private const float SECS_PER_MIN = 60.0f;
    private const float SECS_PER_HOUR = 3600.0f;
    private const float MINS_PER_HOUR = 60.0f;

    /**
    
        Convert
    
        */
    private static readonly Dictionary<(TimeUnit, TimeUnit), Func<float, float>> ConversionFactor = new() {
        [(TimeUnit.Minutes, TimeUnit.Seconds)] = val => val * SECS_PER_MIN,
        [(TimeUnit.Hours, TimeUnit.Seconds)] = val => val * SECS_PER_HOUR,
        [(TimeUnit.Hours, TimeUnit.Minutes)] = val => val * MINS_PER_HOUR,
        [(TimeUnit.Seconds, TimeUnit.Minutes)] = val => val / SECS_PER_MIN,
        [(TimeUnit.Seconds, TimeUnit.Hours)] = val => val / SECS_PER_HOUR,
        [(TimeUnit.Minutes, TimeUnit.Hours)] = val => val / MINS_PER_HOUR
    };

    public static float Convert(float val, TimeUnit from, TimeUnit to) {
        float time = from == to ? val : ConversionFactor[(from, to)](val);
        return time; 
    }

    /**
    
        Utils
    
        */
    public static float MinutesToSeconds(float min) {
        float val = Convert(min, TimeUnit.Minutes, TimeUnit.Seconds);
        return val;
    }

    public static float SecondsToMinutes(float secs) {
        float val = Convert(secs, TimeUnit.Seconds, TimeUnit.Minutes);
        return val;
    }

    public static float HoursToSeconds(float hours) {
        float val = Convert(hours, TimeUnit.Hours, TimeUnit.Seconds);
        return val;
    }

    public static float SecondsToHours(float seconds) {
        float val = Convert(seconds, TimeUnit.Seconds, TimeUnit.Hours);
        return val;
    }
}