
using NLua;

/**
    
    Util Table to Float Converter
    
    */
public static class TableToFloat {
    /**

        Convert

        */
    public static float[] T(LuaTable? t, int count) {
        var arr = new float[count];
        if(t == null) return arr;
        
        for(int i = 0; i < count; i++) {
            arr[i] = Convert.ToSingle(t[(long)(i+1)]);
        }
        
        return arr;
    }
}