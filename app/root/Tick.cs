namespace App.Root;

class Tick {
    public static Tick? instance;

    private int TICKS_PER_SEC = 50;
    private float TICK_RATE;

    private float accumulatedTime = 0.0f;
    private float accumulatedElaptedTime = 0.0f;
    private float ELAPSED_TIME = 10_000_000.0f;
    private int tickCount = 0;

    public float deltaTime = 0.0f;
    private long lastFrameTime = 0;

    private int frameCount  = 0;
    private long lastFpsUpdateTime = 0;
    public int fps = 0;

    private bool timeUpdatedThisFrame = false;
    private bool paused = false;

    public Tick() {
        instance = this;
        TICK_RATE = 1.0f / TICKS_PER_SEC;
    }

    // Get Current Time
    public float getCurrentTime() {
        return accumulatedElaptedTime;
    }

    // Get Fps
    public int getFps() {
        return fps;
    }

    // Tick
    private void tick() {
        tickCount++;
    }

    public int getTickCount() {
        return tickCount;
    }

    public float getTickRate() {
        return TICK_RATE;
    }

    public float getTickDelta() {
        return TICK_RATE;
    }

    public float getTickBasedSpeed(float speed) {
        float val = speed * TICK_RATE;
        return val;
    }

    public void setTicksPerSec(int ticks) {
        TICKS_PER_SEC = ticks;
        TICK_RATE = 1.0f / TICKS_PER_SEC;
    }

    public int getTicksPerSec() {
        return TICKS_PER_SEC;
    }

    // Delta Time
    public float getDeltaTime() {
        return deltaTime;
    }

    public static float getDeltaTimeI() {
        if(instance != null) return instance.deltaTime;

        float fps = 0.016f;
        return fps;
    }

    // Paused
    public void setPaused(bool paused) {
        this.paused = paused;
    }

    public bool isPaused() {
        return paused;
    }

    public void togglePause() {
        paused = !paused;
    }

    // Reset
    public void reset() {
        lastFrameTime = DateTime.Now.Ticks;
        lastFpsUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        frameCount = 0;
        fps = 0;
        accumulatedTime = 0.0f;
        timeUpdatedThisFrame = false;
    }

    ///
    /// Update
    /// 
    private void updateTime() {
        if(timeUpdatedThisFrame) return;

        long currentTime = DateTime.Now.Ticks;
        if(lastFrameTime == 0) {
            lastFrameTime = currentTime;
            lastFpsUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            timeUpdatedThisFrame = true;
            deltaTime = 0.0f;
            return;
        }

        if(paused) {
            deltaTime = 0.0f;
        } else {
            float ticks = ELAPSED_TIME;
            deltaTime = (currentTime - lastFrameTime) / ticks;
            lastFrameTime = currentTime;
        }

        timeUpdatedThisFrame = true;

        long currentWallTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        frameCount++;
        if(currentWallTime - lastFpsUpdateTime >= 1000) {
            fps = frameCount;
            frameCount = 0;
            lastFpsUpdateTime = currentWallTime;
        }
    }

    public void update() {
        updateTime();
        if(paused) {
            timeUpdatedThisFrame = false;
            return;
        }

        accumulatedTime += deltaTime;
        accumulatedElaptedTime += deltaTime;
        while(accumulatedTime >= TICK_RATE) {
            tick();
            accumulatedTime -= TICK_RATE;
        }

        timeUpdatedThisFrame = false;
    }
}