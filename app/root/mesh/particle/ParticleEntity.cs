/**

    Main particle entity mesh
    generator.

    */
namespace App.Root.Mesh.Particle;
using Particle = Data.Particle;
using App.Root.Mesh;
using OpenTK.Mathematics;

class ParticleEntity {
    private const string MESH_TYPE = "quad";

    private const float GRAVITY_VEL = 9.8f;

    private Mesh mesh;
    private Random random;

    public List<Particle> particles;
    private string id = "";

    private bool isActive;
    private Vector3 position;
    private Vector3 color;
    private float size;
    private float speed;
    private int amount;
    private float lifetime;

    private bool vel;
    private Vector3 velNum;

    private bool enableMotion;
    private float swayAmplitude;
    private float swayFrequency;

    private Vector3 RotationScratch = Vector3.Zero;
    private float[] ColorBuffer = new float[16];

    public ParticleEntity(Mesh mesh) {
        this.mesh = mesh;
        this.random = new Random();

        this.particles = new List<Particle>();

        this.isActive = false;
        this.position = Vector3.Zero;
        this.color = Vector3.One;
        this.size = 0.1f;
        this.speed = 1.0f;
        this.amount = 10;
        this.lifetime = 2.0f;
        this.velNum = Vector3.One;
    }

    // Generate Id
    private string generateId() {
        string val = 
            "p_" + 
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 
            "_" + 
        random.Next(100000);

        return val;
    }

    /**
    
        Set Vel Num
    
        */
    public void setVelNum(Vector3 velNum) {
        this.velNum = velNum;
    }

    /**
    
        Set Color
    
        */
    public void setColor(Vector3 color) {
        this.color = color;
    }

    /**
    
        Set Size
    
        */
    public void setSize(float size) {
        this.size = size;
    }

    /**
    
        Set Speed
    
        */
    public void setSpeed(float speed) {
        this.speed = speed;
    }

    /**
    
        Set Amount
    
        */
    public void setAmount(int amount) {
        this.amount = amount;
    }

    /**
    
        Set Lifetime
    
        */
    public void setLifetime(float lifetime) {
        this.lifetime = lifetime;
    }

    /**
    
        Is Active
    
        */
    public bool isActiveEntity() {
        return isActive;
    }

    /**
    
        Set Motion
    
        */
    public void setMotion(bool enable, float swayAmplitude, float swayFrequency) {
        this.enableMotion = enable;
        this.swayAmplitude = swayAmplitude;
        this.swayFrequency = swayFrequency;
    }

    /**
    
        Emit
    
        */
    public void emit(Vector3 position, bool vel, Func<Vector3>? colorSupplier = null) {
        this.id = generateId();

        this.vel = vel;
        this.position = position;
        this.isActive = true;

        for(int i = 0; i < amount; i++) {
            Particle particle = new Particle();
            particle.id = id;

            particle.position = new Vector3(position);

            if(!enableMotion) {
                particle.vel = new Vector3(
                    ((float)random.NextDouble() - 0.5f) * 2.0f * speed,
                    (float)random.NextDouble() * 3.0f * speed,
                    ((float)random.NextDouble() - 0.5f) * 2.0f * speed
                );
            }

            if(colorSupplier != null) {
                particle.color = colorSupplier();
            } else {
                particle.color = new Vector3(this.color);
            }

            particle.size = size;
            particle.lifetime = lifetime;
            particle.maxLifetime = lifetime;

            particles.Add(particle);
            createMesh(particle);

            particle.cachedMeshData = mesh.getData(particle.id);
        }       
    }

    public void emit(Vector3 position, bool vel) {
        emit(position, vel, null);
    }

    /**
    
        Create Mesh
    
        */
    public void createMesh(Particle particle) {
        try {
            mesh.add(particle.id, MESH_TYPE);
            mesh.setPosition(particle.id, particle.position);

            MeshData? data = mesh.getData(particle.id);
            if(data != null) {
                data.shaderType = 7;

                float[] colors = new float[16];
                for(int i = 0; i < 16; i += 4) {
                    colors[i] = particle.color.X;
                    colors[i+1] = particle.color.Y;
                    colors[i+2] = particle.color.Z;
                    colors[i+3] = 1.0f;
                }
                data.setColors(colors);
                mesh.getMeshRenderer(particle.id)?.updateColors(colors);
            }

            mesh.setScale(particle.id, particle.size);
        } catch(Exception err) {
            Console.Error.WriteLine("Failed to create mesh: " + err.Message);
            Console.Error.WriteLine(err.StackTrace);
        }
    }

    /**
    
        Update
    
        */
    public void update() {
        if(!isActive) return;

        float deltaTime = Tick.getDeltaTimeI();

        for(int i = particles.Count - 1; i >= 0; i--) {
            Particle particle = particles[i];
            particle.lifetime -= lifetime;
            if(particle.lifetime <= 0) {
                mesh.remove(particle.id);
                particles.RemoveAt(i);
                continue;
            }

            if(!enableMotion) {
                particle.vel.Y -= 
                    GRAVITY_VEL * 
                    deltaTime * 
                    speed;
            }
            if(vel) {
                particle.position += new Vector3(
                    particle.vel.X * deltaTime * velNum.X,
                    particle.vel.Y * deltaTime * velNum.Y,
                    particle.vel.Z * deltaTime * velNum.Z
                );
            } else {
                particle.position += new Vector3(
                    particle.vel.X * deltaTime,
                    particle.vel.Y * deltaTime,
                    particle.vel.Z * deltaTime
                );
            }

            mesh.setPosition(particle.id, particle.position);

            if(particle.cachedMeshData != null) {
                if(enableMotion) {
                    particle.rotation += particle.rotationSpeed * deltaTime;
                    RotationScratch.X = 0;
                    RotationScratch.Y = 0;
                    RotationScratch.Z = particle.rotation;
                    particle.cachedMeshData.setRotation(RotationScratch);
                }

                float alpha = particle.lifetime / particle.maxLifetime;
                for(int c = 0; c < 16; c++) {
                    ColorBuffer[c] = particle.color.X;
                    ColorBuffer[c+1] = particle.color.Y;
                    ColorBuffer[c+2] = particle.color.Z;
                    ColorBuffer[c+3] = alpha;
                }
                particle.cachedMeshData.setColors(ColorBuffer);
            }
        }

        if(particles.Count == 0) {
            isActive = false;
        }
    }

    /**
    
        Render
    
        */
    public void render() {
        if(!isActive) return;

        foreach(Particle p in particles) {
            mesh.renderId(p.id);
        }
    }

    /**
    
        Cleanup
    
        */
    public void cleanup() {
        foreach(Particle p in particles) {
            mesh.remove(p.id);
        }
        particles.Clear();
    }
}