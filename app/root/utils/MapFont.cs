using App.Root.Text;

namespace App.Root.Utils;

public static class MapFont {
    // Extension
    private static readonly (string a, string b) Ext = (
        ".ttf", 
        "*.ttf"
    );

    /**
    
        Normalize Key
    
        */
    private static Dictionary<string, string> keyMap = new() {
        { "-", "" },
        { "_", "" },
        { " ", "" }  
    };

    private static string Norm(string key) {
        string val = key.ToLower();
        
        foreach(var entry in keyMap) {
            val = val.Replace(entry.Key, entry.Value);
        }

        return val;
    }

    /**
    
        Resolve
    
        */
    public static string R(string key) {
        if(File.Exists(Path.Combine(FontLoader.FONT_DIR, key + Ext.a))) {
            return key + Ext.a;
        }

        var files = Directory.GetFiles(FontLoader.FONT_DIR, Ext.b);
        foreach(var file in files) {
            string fileName = Norm(Path.GetFileNameWithoutExtension(file));
            if(fileName.StartsWith(Norm(key))) {
                return Path.GetFileName(file);
            }
        }

        string val = key + Ext.a;
        return val;
    }
}