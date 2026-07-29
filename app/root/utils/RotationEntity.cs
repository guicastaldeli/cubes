/**
    
    Rotation Entity helper.
    
    */
namespace App.Root.Utils;
using App.Root.World.Entity;
using OpenTK.Mathematics;

public static class RotationEntity {
    /**
     * 
     * Rotate
     *
     */
    public static Matrix4 R(EntityProps entity) {
        var rotationRad = MathHelper.DegreesToRadians(entity.Rotation);
        var rotationMatrix = Matrix4.CreateRotationY(rotationRad);
        return rotationMatrix;
    }
}