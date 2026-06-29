/**
    
    Attribute Converter class.
    
    */
namespace App.Root.Utils;

/**

    Converter Attribute

    */
[AttributeUsage(AttributeTargets.Field)]
public class ConvertAttribute : Attribute {
    public string Converter { get; }

    public ConvertAttribute(string converter) {
        this.Converter = converter;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
public class ConverterKey : Attribute {
    public string Key { get; }

    public ConverterKey(string key) {
        this.Key = key;
    }
}

/**

    Converter main class

    */
public static class Converter {
    /**
     * 
     * To Int
     *
     */
    [ConverterKey("int")]
    public static int ToInt(object i) {
        int val = Convert.ToInt32(i);
        return val;
    }

    /**
     * 
     * To Float
     *
     */
    [ConverterKey("float")]
    public static float ToFloat(object f) {
        float val = Convert.ToSingle(f);
        return val;
    }

    /**
     * 
     * To Double
     *
     */
    [ConverterKey("double")]
    public static double ToDouble(object d) {
        double val = Convert.ToDouble(d);
        return val;
    }

    /**
     * 
     * To Bool
     *
     */
    [ConverterKey("bool")]
    public static bool ToBool(object b) {
        bool val = Convert.ToBoolean(b);
        return val;
    }

    /**
     * 
     * To String
     *
     */
    [ConverterKey("string")]
    public static string ToString(object s) {
        string val = s?.ToString() ?? "";
        return val;
    }

    /**
     * 
     * To Long
     *
     */
    [ConverterKey("long")]
    public static long ToLong(object l) {
        long val = Convert.ToInt64(l);
        return val;
    }

    /**
     * 
     * To Decimal
     *
     */
    [ConverterKey("decimal")]
    public static decimal ToDecimal(object d) {
        decimal val = Convert.ToDecimal(d);
        return val;
    }

    /**
     * 
     * To Byte
     *
     */
    [ConverterKey("byte")]
    public static byte ToByte(object b) {
        byte val = Convert.ToByte(b);
        return val;
    }

    /**
     * 
     * To Char
     *
     */
    [ConverterKey("char")]
    public static char ToChar(object c) {
        char val = Convert.ToChar(c);
        return val;
    }

    /**
     * 
     * To DateTime
     *
     */
    [ConverterKey("dateTime")]
    public static DateTime ToDateTime(object dt) {
        DateTime val = Convert.ToDateTime(dt);
        return val;
    }

    /**
     * 
     * To Rgba
     *
     */
    [ConverterKey("rgba")]
    public static float[] ToRgba(string hex) {
        var (r, g, b) = HexToRgb.C(hex);
        float a = 1.0f;

        float[] val = new float[] { r, g, b, a };
        return val;
    }

    /**
     * 
     * To Rgba List
     *
     */
    [ConverterKey("rgbaList")]
    public static List<float[]> ToRgbaList(string hex, int count) {
        var (r, g, b) = HexToRgb.C(hex);
        float a = 1.0f;
        float[] rgba = new float[] { r, g, b, a };

        List<float[]> val = Enumerable.Repeat(rgba, count).ToList();
        return val;
    }

    /**
     * 
     * To Rotation List
     *
     */
    [ConverterKey("rotationList")]
    public static List<float> ToRotationList(float rotation, int count) {
        List<float> val = Enumerable.Repeat(rotation, count).ToList();
        return val;
    }

    /**
     * 
     * To Texture
     *
     */
    // Tex Path
    [ConverterKey("texPath")]
    public static List<string?> ToTexPath(string? texPath, int count) {
        List<string?> val = Enumerable.Repeat(texPath, count).ToList();
        return val;
    }

    // Tex Id
    [ConverterKey("texId")]
    public static List<int>? ToTexId(int? texId, int count) {
        if(texId == null) return null;
        
        List<int> val = Enumerable.Repeat(texId.Value, count).ToList();
        return val;
    }
}