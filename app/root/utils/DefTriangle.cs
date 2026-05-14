/**

    Def Triangle Util class...

    */
namespace App.Root.Utils;
using OpenTK.Mathematics;

public static class DefTriangle {
    public static Vector3 dir;
    public static Vector3 origin;
    public static float bdist;
    public static bool hit;   

    public static float[] verts = null!;
    public static Vector3 pos;
    public static Vector3 scale;

    /**

        Set
    
        */
    // Raycaster
    public static void r(Vector3 dir, Vector3 origin) {
        DefTriangle.dir = dir;
        DefTriangle.origin = origin;
        DefTriangle.bdist = float.MaxValue;
        DefTriangle.hit = false;
    }

    // Mesh
    public static void m(float[] verts, Vector3 pos, Vector3 scale) {
        DefTriangle.verts = verts;
        DefTriangle.pos = pos;
        DefTriangle.scale = scale;
    }

    /**
    
        Test
    
        */
    public static void t(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 edge1 = b - a;
        Vector3 edge2 = c - a;
        Vector3 h = Vector3.Cross(dir, edge2);
        float det = Vector3.Dot(edge1, h);

        float f = 1e-6f;
        if(MathF.Abs(det) < f) return;

        float invDet = 1.0f / det;
        Vector3 s = origin - a;
        float u = invDet * Vector3.Dot(s, h);
        if(u < 0 || u > 1) return;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = invDet * Vector3.Dot(dir, q);
        if(v < 0 || u + v > 1) return;

        float t = invDet * Vector3.Dot(edge2, q);
        if(t >= 0 && t < bdist) {
            bdist = t;
            hit = true;
        }
    }

    /**
    
        Get Vertices
    
        */
    public static Vector3 getVert(int i) {
        Vector3 val = new Vector3(
            verts[i*3+0] * scale.X + pos.X,
            verts[i*3+1] * scale.Y + pos.Y,
            verts[i*3+2] * scale.Z + pos.Z
        );

        return val;
    }
}