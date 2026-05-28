/**

    Ease Out animation class.

    */
namespace App.Root.Animation;

public static class EaseOut {
    public static float OutQuad(float t) {
        return 1.0f - (1.0f - t) * (1.0f - t);
    }

    public static float OutCubic(float t) {
        return 1.0f - MathF.Pow(1.0f - t, 3f);
    }

    public static float OutQuart(float t) {
        return 1.0f - MathF.Pow(1.0f - t, 4f);
    }

    public static float OutSine(float t) {
        return MathF.Sin(t * MathF.PI / 2.0f);
    }

    public static float OutExpo(float t) {
        return t == 1.0f ? 1.0f : 1.0f - MathF.Pow(2.0f, -10.0f * t);
    }

    public static float InOutQuad(float t) {
        return t < 0.5f ? 2.0f * t * t : 1.0f - MathF.Pow(-2.0f * t + 2.0f, 2.0f) / 2.0f;
    }

    public static float InOutCubic(float t) {
        return t < 0.5f ? 4f * t * t * t : 1.0f - MathF.Pow(-2.0f * t + 2.0f, 3f) / 2.0f;
    }

    public static float InOutSine(float t) {
        return -(MathF.Cos(MathF.PI * t) - 1.0f) / 2.0f;
    }

    public static float OutBounce(float t) {
        if(t < 1.0f / 2.75f) return 7.5625f * t * t;
        if(t < 2.0f / 2.75f) { 
            t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; 
        }
        if(t < 2.5f / 2.75f) { 
            t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; 
        }
        t -= 2.625f / 2.75f; 
        return 7.5625f * t * t + 0.984375f;
    }

    public static float OutElastic(float t) {
        if(t == 0 || t == 1) return t;
        return MathF.Pow(2.0f, -10.0f * t) * 
            MathF.Sin((t - 0.075f) * 
            (2.0f * MathF.PI) / 0.3f) + 1.0f;
    }
}
