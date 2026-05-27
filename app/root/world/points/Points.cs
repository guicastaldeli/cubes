namespace App.Root.World.Points;
using App.Root.World.Entity;

public static class Points {
    private static int TOTAL = 0;

    // Get Total
    public static int getTotal() {
        return TOTAL;
    }

    /**
    
        Add
    
        */
    public static void Add(int xp) {
        int added = Xp.ConvertToPoints(xp);
        TOTAL += added;
        EventStream.set("points-added", TOTAL);
    }

    /**
    
        Reset
    
        */
    public static void reset() {
        TOTAL = 0;
    }
}