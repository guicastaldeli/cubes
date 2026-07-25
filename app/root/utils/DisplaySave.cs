namespace App.Root.Utils;
using App.Root.Save;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class DisplayAttribute : Attribute {
    public string Name { get; set; }

    public DisplayAttribute(string Name) {
        this.Name = Name;
    }
}

public static class DisplaySave {
    private static Dictionary<string, Func<SaveFile, object>> displayGetters = new();

    static DisplaySave() {
        var methods = typeof(DisplaySave).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<DisplayAttribute>() != null);

        foreach(var method in methods) {
            var attr = method.GetCustomAttribute<DisplayAttribute>();
            if(attr == null) continue;

            var func = (Func<SaveFile, object>)Delegate.CreateDelegate(typeof(Func<SaveFile, object>), method);
            displayGetters[attr.Name] = func;
            Console.WriteLine($"[SaveDisplay] Registered: {attr.Name}");
        }
    }

    /**
     * Save Name
     */
    [Display("save_name")]
    public static string SaveName(SaveFile save) {
        return save.SaveName;
    }

    /**
     * Created At
     */
    [Display("created_at")]
    public static string CreatedAt(SaveFile save) {
        if(string.IsNullOrEmpty(save.CreatedAt)) return "00";
        return DateTime.TryParse(save.CreatedAt, out var time) ?
            time.ToString("yyyy-MM-dd HH:mm") :
            save.CreatedAt;
    }

    /**
     * Last Played
     */
    [Display("last_played")]
    public static string LastPlayed(SaveFile save) {
        if(string.IsNullOrEmpty(save.LastPlayed)) return "Never";

        if(DateTime.TryParse(save.LastPlayed, out var time)) {
            var diff = DateTime.Now - time;
            if(diff.TotalMinutes < 1) return "Just Now";
            if(diff.TotalSeconds < 60) return $"{(int)diff.TotalMinutes}m ago";
            if(diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if(diff.TotalDays < 7) return $"{diff.Days}d ago";
            return time.ToString("MMM dd, yyyy");
        }

        return save.LastPlayed;
    }

    /**
     * Play Time
     */
    [Display("play_time")]
    public static string PlayTime(SaveFile save) {
        float hours = save.PlayTime;
        if(hours < 0.0167f) return "< 1m";
        if(hours < 1) {
            int min = (int)(hours * 60);
            return $"{min}m";
        }
        if(hours < 24) {
            return $"{hours:F1}h";
        }

        int days = (int)(hours / 24);
        float remaining = hours % 24;
        if(remaining < 1) return $"{days}d";
        return $"{days}d {remaining:F0}h";
    }

    /**
     * Player Name
     */
    [Display("player_name")]
    public static string PlayerName(SaveFile save) {
        return save.PlayerName;
    }

    /**
     * Version
     */
    [Display("version")]
    public static string Version(SaveFile save) {
        return save.Version;
    }
}