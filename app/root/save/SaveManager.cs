namespace App.Root.Save;

public static class SaveManager {
    private static bool initialized = false;

    /**
     *
     * Init
     *
     */
    public static void Init() {
        if(initialized) return;

        initialized = true;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[SaveManager] Initialized");
        Console.ResetColor();
    }
}