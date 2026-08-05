namespace App.Root._Sync;
using System.Collections.Concurrent;

public class SyncDispatcher {
    private static SyncDispatcher? instance;
    public static SyncDispatcher I => instance ??= new SyncDispatcher();

    private ConcurrentQueue<Action> queue = new();

    private bool isRunning = true;

    /**
     *
     * Enqueue
     *
     */
    public void Enqueue(Action action) {
        queue.Enqueue(action);
    }

    /**
     *
     * Process
     *
     */
    public void Process() {
        while(queue.TryDequeue(out var action)) {
            try {
                action();
            } catch(Exception err) {
                Console.WriteLine($"[SyncDispatcher] Error: {err.Message}");
            }
        }
    }

    /**
     *
     * Stop
     *
     */
    public void Stop() {
        isRunning = false;
    }
}