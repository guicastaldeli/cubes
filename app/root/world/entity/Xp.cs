namespace App.Root.World.Entity;

public static class Xp {
    private static readonly Random range = new Random();

    private const int MIN_XP = 1;
    private const int MAX_XP = 99;
    
    private const int MIN_POINTS = 0;
    private const int MAX_POINTS = 999;

    /**
     * 
     * Range
     *
     */
    public static int Range() {
        int val = range.Next(1, 100);
        return val;
    }

    /**
     * 
     * Convert to Points
     *
     */
    public static int ConvertToPoints(int xp) {
        float normalized = (float)(xp - MIN_XP) / (MAX_XP - MIN_XP);
        
        float f = 1.5f;
        float curved = MathF.Pow(normalized, f);

        int val = Math.Clamp((int)(MIN_POINTS + curved * (MAX_POINTS - MIN_POINTS)), MIN_POINTS, MAX_POINTS);
        return val;
    }
}