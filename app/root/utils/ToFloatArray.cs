/**

    Helper util class to convert
    floats to array.

    */
namespace App.Root.Utils;
using NLua;

static class ToFloatArray {
    /**

        Convert

        */
    public static float[] C(LuaTable table) {
        int len = table.Values.Count;
        float[] arr = new float[len];
        for(int i = 1; i <= len; i++) {
            arr[i-1] = Convert.ToSingle(table[i]);
        }

        return arr;
    }
}