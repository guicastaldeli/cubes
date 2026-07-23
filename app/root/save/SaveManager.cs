namespace App.Root.Save;

public static class SaveManager {
    public class SaveInfo {
        public Type Type { get; set; }
        public string Section { get; set; }
        public string FileName { get; set; }
        public List<SaveFieldInfo> Fields { get; set; } = new();
    }
    
    private static Dictionary<string, SaveInfo> saveRegistry = new();

    private static bool initialized = false;
}