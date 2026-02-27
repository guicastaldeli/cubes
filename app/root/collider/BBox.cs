namespace App.Root.Collider;

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

    public float getSizeX() {
        return maxX - minX;
    }

    public float getSizeY() {
        return maxY - minY;
    }

    public float getSizeZ() {
        return maxZ - minZ;
    }

    public bool intersects(BBox other) {
        return (minX <= other.maxX && maxX >= other.minX) &&
            (minY <= other.maxY && maxY >= other.minY) &&
            (minZ <= other.maxZ && maxZ >= other.minZ);
    }
}