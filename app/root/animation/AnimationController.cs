namespace App.Root.Animation;

class AnimationController {
    private Tick tick;

    private Dictionary<string, Animation> animations = new();

    public AnimationController(Tick tick) {
        this.tick = tick;
    }

    
}