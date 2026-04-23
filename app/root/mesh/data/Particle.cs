/**

    Main particle mesh
    data.

    */
namespace App.Root.Mesh.Data;
using OpenTK.Mathematics;

class Particle {
    public string id = "";
    public MeshData? cachedMeshData;

    public Vector3 position;
    public Vector3 basePos = Vector3.Zero;
    
    public Vector3 vel;
    public Vector3 initialVel;

    public Vector3 color;

    public float size;
    
    public float lifetime;
    public float maxLifetime;

    public float rotation;
    public float rotationSpeed;
    
    public float swayPhase;
    public float swayAmplitude;
    public float swayFrequency;
    public Vector3 swayVel;

    public Particle() {
        this.position = Vector3.Zero;

        this.vel = Vector3.Zero;
        this.initialVel = Vector3.Zero;

        this.color = Vector3.One;
        
        this.rotation = 0.0f;
        this.rotationSpeed = 0.0f;

        this.swayPhase = 0.0f;
        this.swayAmplitude = 1.0f;
        this.swayFrequency = 1.0f;
        this.swayVel = Vector3.Zero;
    }
}