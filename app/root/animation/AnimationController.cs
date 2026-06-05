namespace App.Root.Animation;

public static class AnimationController {
    private static Dictionary<string, Animation> animations = new();
    private static Dictionary<string, float> offsets = new();

    /**
     * 
     * Play
     *
     */
    public static void Play(
        string id,
        float start,
        float end,
        float duration,
        Action<float> onUpdate,
        Func<float, float>? type = null,
        Action? onComplete = null
    ) {
        animations[id] = new Animation {
            start = start,
            end = end,
            duration = duration,
            elapsed = 0.0f,
            active = true,
            type = type,
            onUpdate = onUpdate,
            onComplete = onComplete
        };
    }

    /**
     * 
     * Stop
     *
     */
    public static void Stop(string id) {
        animations.Remove(id);
    }

    /**
     * 
     * Update
     *
     */
    public static void Update() {
        float deltaTime = Tick.getDeltaTimeI();

        foreach(var (id, anim) in animations) {
            if(!anim.active) continue;

            anim.elapsed += deltaTime;
            float t = Math.Clamp(anim.elapsed / anim.duration, 0.0f, 1.0f);
            float eased = anim.type!(t);
            float val = anim.start + (anim.end - anim.start) * eased;
        
            anim.onUpdate?.Invoke(val);

            if(t >= 1.0f) {
                anim.active = false;
                anim.onComplete?.Invoke();
            }
        }
    }
}