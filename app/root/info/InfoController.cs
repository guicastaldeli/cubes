namespace App.Root.Info;

class InfoController {
    private static readonly string INFO_DIR = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
            "..", "..", "..", "root", ".INFO-DATA")
    );
    private static readonly string META_FILE = Path.Combine(INFO_DIR, "inf.meta.dat");
    
    private static InfoController? instance;
    
    public static InfoController getInstance() {
        return instance!;
    }

    private readonly Store store;
    public readonly UserInfo userInfo;

    private InfoController() {
        this.store = new Store(META_FILE);
        this.userInfo = new UserInfo(store);
        
        userInfo.ensureDefaults();
    }

    // Get Store
    public Store getStore() {
        return store;
    }

    // Get User Info
    public UserInfo getUserInfo() {
        return userInfo;
    }

    // Init
    public static void init() {
        instance = new InfoController();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[InfoController] ID: {instance.userInfo.getId()}");
        Console.WriteLine($"[InfoController] USERNAME: {instance.userInfo.getUsername()}");
        Console.ResetColor();
    }
}