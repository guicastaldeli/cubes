namespace App.Root;
using OpenTK.Mathematics;

class Camera {
    private Vector3 position;
    private Vector3 front;
    private Vector3 up;

    private float yaw = -90.0f;
    private float pitch = 0.0f;
    private float fov = 45.0f;

    public Camera() {
        position = new Vector3(0.0f, 0.0f, 0.0f);
        front = new Vector3(0.0f, 0.0f, -1.0f);
        up = Vector3.UnitY;
    }

    // Get View
    public Matrix4 getView() {
        return Matrix4.LookAt(position, position + front, up);
    }

    // Get Projection
    public Matrix4 getProjection() {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(fov),
            (float)Window.WIDTH / Window.HEIGHT,
            0.1f,
            100.0f
        );
    }

    // Rotate
    public void rotate(float deltaX, float deltaY) {
        yaw += deltaX;
        pitch += deltaY;
        pitch = Math.Clamp(pitch, -89.0f, 89.0f);

        front = Vector3.Normalize(new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch))
        ));
    }

    ///
    /// Update 
    ///
    private void updateRotation() {
        yaw += 0.5f;
        front = Vector3.Normalize(new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch))
        ));
    } 

    public void update() {
        updateRotation();    
    }
}