namespace App.Root.Player;
using OpenTK.Mathematics;

class Camera {
    private Vector3 position;
    private Vector3 front;
    private Vector3 up;
    private Vector3 right;
    private Vector3 worldUp;

    private float yaw = -90.0f;
    private float pitch = 0.0f;
    private float fov = 45.0f;

    private float targetAngle = 0.0f;

    public Camera() {
        position = new Vector3(0.0f, 0.0f, 0.0f);
        worldUp = Vector3.UnitY;
        front = new Vector3(0.0f, 0.0f, -1.0f);
        up = Vector3.UnitY;
        right = Vector3.UnitX;
        updateVectors();
    }

    public Vector3 getFront() {
        return new Vector3(front);
    }

    public Vector3 getRight() {
        return new Vector3(right);
    }

    public Vector3 getUp() {
        return new Vector3(up);
    }

    public float getYaw() {
        return yaw;
    }

    public float getPitch() {
        return pitch;
    }

    // Position
    public void setPosition(float x, float y, float z) {
        position = new Vector3(x, y, z);
    }

    public void setPosition(Vector3 position) {
        this.position = position;
    }
    
    public Vector3 getPosition() {
        return new Vector3(position);
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

    // Handle Mouse
    public void handleMouse(float xOffset, float yOffset) {
        float sensv = 0.1f;
        yaw += xOffset * sensv;
        pitch += yOffset * sensv;
        pitch = Math.Clamp(pitch, -89.0f, 89.0f);
        updateVectors();
    }

    ///
    /// Update 
    ///
    private void updateVectors() {
        float yawRad = MathHelper.DegreesToRadians(yaw);
        float pitchRad = MathHelper.DegreesToRadians(pitch);

        front = Vector3.Normalize(new Vector3(
            MathF.Cos(yawRad) * MathF.Cos(pitchRad),
            MathF.Sin(pitchRad),
            MathF.Sin(yawRad) * MathF.Cos(pitchRad)
        ));
        right = Vector3.Normalize(Vector3.Cross(front, worldUp));
        up = Vector3.Normalize(Vector3.Cross(right, front));
    }

    private void updateRotation() {
        yaw += 5.0f * Tick.getDeltaTimeI();
        front = Vector3.Normalize(new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch))
        ));
    } 

    private void updateRotationTarget(Vector3 target) {
        float targetRadius = 5.0f;
        float targetHeight = 1.0f;
        float targetSpeed = 5.0f;

        targetAngle += targetSpeed * Tick.getDeltaTimeI();
        position = new Vector3(
            target.X + targetRadius * MathF.Cos(MathHelper.DegreesToRadians(targetAngle)),
            target.Y + targetHeight,
            target.Z + targetRadius * MathF.Sin(MathHelper.DegreesToRadians(targetAngle))
        );

        front = Vector3.Normalize(target - position);
    }

    public void update() {
        //updateRotationTarget(new Vector3(0.0f, 0.0f, -3.0f));
        //updateRotation();    
    }
}