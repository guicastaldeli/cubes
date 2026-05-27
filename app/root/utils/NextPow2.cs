namespace App.Root.Utils;

public static class NextPow2 {
    /**
    
        Round up to next power of 2
    
        */
    public static int R(int v) {
        int p = 1;
        while(p < v) p *= 2;
        return p;
    }    
}