namespace App.Root._Sync;

public static class SyncPacketTypes {
    private static Dictionary<string, Type> registeredTypes = new();
    private static Dictionary<string, DataSyncAttribute> registeredAttributes = new();

    private static bool isInitialized = false;
}