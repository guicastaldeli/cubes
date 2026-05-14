/**

    Is Entity Checker util.

    */
namespace App.Root.Utils;
using App.Root.Mesh;

public static class IsEntity {
    /**
    
        Check
    
        */
    // Default Checker
    public static void C(MeshData data, bool? isEntity) {
        data.isEntity = (isEntity == true) ? 1 : 0;
    }

    // Bool Checker
    public static bool BC(bool? isEntity) {
        bool val = isEntity == true;
        return val;
    }
}