namespace App.Root.Collider;
using OpenTK.Mathematics;

class BBox {
    public float minX;
    public float minY;
    public float minZ;

    public float maxX;
    public float maxY;
    public float maxZ;

    public BBox(
        float minX, 
        float minY, 
        float minZ, 
        float maxX, 
        float maxY, 
        float maxZ
    ) {
        this.minX = minX;
        this.minY = minY;
        this.minZ = minZ;
        this.maxX = maxX;
        this.maxY = maxY;
        this.maxZ = maxZ;
    }

    /**
    
        Set From Center
    
        */
    public void setFromCenter(Vector3 center, Vector3 size) {
        float hx = size.X / 2f;
        float hy = size.Y / 2f;
        float hz = size.Z / 2f;
    
        minX = center.X - hx;
        minY = center.Y - hy;
        minZ = center.Z - hz;

        maxX = center.X + hx;
        maxY = center.Y + hy;
        maxZ = center.Z + hz;
    }

    public static BBox setFromCenterI(Vector3 center, Vector3 size) {
        float hx = size.X / 2f;
        float hy = size.Y / 2f;
        float hz = size.Z / 2f;
    
        return new BBox(
            center.X - hx, center.Y - hy, center.Z - hz,
            center.X + hx, center.Y + hy, center.Z + hz
        );
    }

    /**
    
        Size
    
        */
    // Get Size X
    public float getSizeX() {
        return maxX - minX;
    }

    // Get Size Y
    public float getSizeY() {
        return maxY - minY;
    }

    // Get Size Z
    public float getSizeZ() {
        return maxZ - minZ;
    }

    /**
    
        Intersects
    
        */
    public bool intersects(BBox other) {
        return (minX <= other.maxX && maxX >= other.minX) &&
            (minY <= other.maxY && maxY >= other.minY) &&
            (minZ <= other.maxZ && maxZ >= other.minZ);
    }
}