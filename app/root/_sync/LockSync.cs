namespace App.Root._Sync;
using System.Collections.Concurrent;

public class LockSync {
    private static LockSync? instance;
    public static LockSync Instance => instance ??= new LockSync();

    private ConcurrentDictionary<string, string> locks = new();

    public bool IsLocked(string dataId) {
        if(string.IsNullOrEmpty(dataId)) return false;

        bool val = locks.ContainsKey(dataId);
        return val;
    }
}