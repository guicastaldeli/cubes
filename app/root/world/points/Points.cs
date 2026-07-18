namespace App.Root.World.Points;

using App.Root.Utils;
using App.Root.World.Entity;

[ActionConverter]
[DataOutput("player_storage.ps")]
class PointsData {
    [Convert("int32")] [ConverterKey("total")] public int Total { get; set; }

    public static PointsData? data = null;

    // Has Data
    public static bool HasData() {
        bool val = data != null;
        return val;
    }

    // Get Data
    public static PointsData GetData() {
        if(data == null) ExtractData();

        PointsData val = data ?? new PointsData();
        return val;
    }

    // Get Total
    public static int GetTotal() {
        var d = GetData();
        return d.Total;
    }

    // Save Points
    public static void SavePoints(int total) {
        if(data == null) data = new PointsData();
        data.Total = total;

        DataOutput.Save("pointsdata");
    }

    /**
     *
     * Extract Data
     *
     */
    public static object ExtractData() {
        if(data != null) return data;
        data = new PointsData();

        try {
            DataOutput.Load("pointsdata");
            Console.WriteLine("[PointsData] Loaded points data from file");
        } catch(Exception err) {
            Console.WriteLine($"[PointsData] No saved data found: {err.Message}");
        }

        return data;
    }

    /**
     *
     * Save Data
     *
     */
    public static void SaveData(object data) {
        if(data is PointsData d) {
            PointsData.data = d;
            Console.WriteLine($"SaveData() -- [PointsData] Saved total: {d.Total}");
        }
    }
}

public static class Points {
    private static int TOTAL = 0;
    private static bool initialized = false;

    // Get Total
    public static int getTotal() {
        return TOTAL;
    }
    
    // Set
    public static void Set(int amount) {
        TOTAL = amount;
        PointsData.SavePoints(TOTAL);
        EventStream.set("points-added", TOTAL);
        Console.WriteLine($"[Points] DEBUG: Set points to {amount}");
    }

    /**
     * 
     * Add
     *
     */
    public static void Add(int xp) {
        int added = Xp.ConvertToPoints(xp);
        TOTAL += added;

        PointsData.SavePoints(TOTAL);
        EventStream.set("points-added", TOTAL);
    }

    /**
     * 
     * Init
     *
     */
    public static void Init() {
        if(initialized) return;
        initialized = true;

        TOTAL = PointsData.GetTotal();
        Console.WriteLine($"[Points] Initialized with total: {TOTAL}");
    }

    /**
     * 
     * Reset
     *
     */
    public static void reset() {
        TOTAL = 0;
        PointsData.SavePoints(TOTAL);
    }
}