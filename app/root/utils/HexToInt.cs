/**
    
    Util Hex to Int Converter.
    
    */
static class HexToInt {
    /**

        Convert

        */
    public static int C(string hex) {
        hex = hex.Replace("#", "");

        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        int a = 255;

        int val = (r << 24) | (g << 16) | (b << 8) | a;
        return val;
    }
}