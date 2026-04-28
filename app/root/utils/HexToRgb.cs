/**
    
    Util Hex to Rgb Converter.
    
    */
class HexToRgb {
    public static (float r, float g, float b) C(string hex) {
        float f = 255.0f;
        
        hex = hex.TrimStart('#');
    
        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        
        return (r / f, g / f, b / f);
    }
}