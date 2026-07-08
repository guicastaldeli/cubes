/**

    Ease In animation class.

    */
namespace App.Root.Animation;

public static class EaseIn {
    public static float Linear(float t) {
        return t;
    }

    public static float InQuad(float t) {
        return t * t;
    }

    public static float InCubic(float t) {
        return t * t * t;
    }

    public static float InQuart(float t) {
        return t * t * t * t;
    }

    public static float InSine(float t) {
        return 1f - MathF.Cos(t * MathF.PI / 2.0f);
    }

    public static float InExpo(float t) {
        return t == 0.0f ? 0.0f : MathF.Pow(2.0f, 1.0f * t - 10.0f);
    }
}
