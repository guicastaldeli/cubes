/**
    
    Lerp Util helper
    
    */
static class Lerp {
    /**
    
        Setter
    
        */
    public static float S(float a, float b, float t) {
        float val = a + (b - a) * t;
        return val;
    }
}