/**
    
    Util Float to Hex Converter.
    
    */
namespace App.Root.Utils;

public static class FloatToHex {
    /**
     * 
     * Convert
     *
     */
    public static string? C(float[]? colors) {
        if(colors == null || colors.Length < 3) return null;

        int r = (int)Math.Clamp(colors[0] * 255, 0, 255);
        int g = (int)Math.Clamp(colors[1] * 255, 0, 255);
        int b = (int)Math.Clamp(colors[2] * 255, 0, 255);

        string val = $"#{r:X2}{g:X2}{b:X2}";
        return val;
    }
}