/**

    Animation Props class.

    */
namespace App.Root.Animation;

class Animation {
    public float start;
    public float end;
    public float duration;
    public float elapsed;
    public bool active;
    public Func<float, float>? type;
    public Action<float>? onUpdate;
    public Action? onComplete;
}