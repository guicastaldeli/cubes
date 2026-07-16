/**

    Main particle entity mesh
    generator.

    */
namespace App.Root.Particle;
using App.Root.Mesh;
using App.Root.Utils;
using App.Root.Player;
using Particle = Resource.Mesh.Particle;
using OpenTK.Mathematics;
using System.Reflection;

class ParticleEntity {
    private const string MESH_TYPE = "quad";
    private const string SHARED_MESH_ID = "particle_shared";

    private const float GRAVITY_VEL = 9.8f;

    private Tick tick;      private float DeltaTime { get { return tick.getDeltaTime(); } }
    private Mesh mesh;
    private ParticleController particleController;
    
    private Random random;

    public Particle particleConfig = null!;

    private static bool sharedMeshInitialized = false;
    private static int counter = 0;
    private int updateCounter = 0;

    public List<Particle> particles = null!;
    private string id = null!;
    private string sharedMeshId = null!;

    private bool isActive;
    private bool live;

    private Vector3 position;
    private Vector3 color;
    private float size;
    private float speed;
    private int amount;
    private float lifetime;
    private float spawnRadius;

    private bool vel;
    private Vector3 velNum;

    private bool enableMotion;
    private float swayAmplitude;
    private float swayFrequency;

    private float targetY = 0.0f;

    private float playerMovSpeed;
    private float playerStand;

    private List<Vector3> instancePositions = new();
    private List<float[]> instanceColors = new();

    public bool needsBufferUpdate = false;
    private int updateInterval = 1;
    
    private float movingTimer = 0.0f;
    private const float MOVE_TIMEOUT = 0.1f;

    public List<Vector3> allPositions = new();
    private List<float[]> allColors = new();

    public ParticleEntity(Tick tick, Mesh mesh, ParticleController particleController) {
        this.tick = tick;
        this.mesh = mesh;
        this.particleController = particleController;

        this.random = new Random();

        this.particles = new List<Particle>();

        this.id = generateEntityId();
        
        this.isActive = false;
        this.live = false;
        this.position = Vector3.Zero;
        this.color = Vector3.One;
        this.size = 0.1f;
        this.speed = 1.0f;
        this.amount = 10;
        this.lifetime = 2.0f;
        this.velNum = Vector3.One;
        this.spawnRadius = 0.0f;
        this.targetY = 0.0f;
        this.vel = false;
        this.enableMotion = false;
        this.playerMovSpeed = 0.0f;
        this.playerStand = 0.0f;
    }

    // Get Particle Config
    public Particle getParticleConfig() {
        return particleConfig;
    }

    // Return Entity
    public void returnEntity(ParticleEntity entity) {
        if(entity == null) return;

        entity.reset();
        particleController.activeEntities.Remove(entity);
        particleController.particlePool.Add(entity);
    }

    // Set Shared Mesh Id
    public void setSharedMeshId(string id) {
        sharedMeshId = id;
    }

    // Needs Buffer Update
    public bool NeedsBufferUpdate() {
        return needsBufferUpdate;
    }

    /**
     * 
     * Position
     *
     */
    // Get Position
    public Vector3 getPosition() {
        return position;
    }

    // Get Instance Positions
    public List<Vector3> getInstancePositions() {
        return instancePositions;
    }

    /**
     * 
     * Set Vel Num
     *
     */
    public void setVelNum(Vector3 velNum) {
        this.velNum = velNum;
    }

    /**
     * 
     * Color
     *
     */
    // Set Color
    public void setColor(Vector3 color) {
        this.color = color;
    }

    // Get Instance Colors
    public List<float[]> getInstanceColors() {
        return instanceColors;
    }

    /**
     * 
     * Set Size
     *
     */
    public void setSize(float size) {
        this.size = size;
        mesh.setScale(SHARED_MESH_ID, size);
    }

    /**
     * 
     * Set Speed
     *
     */
    public void setSpeed(float speed) {
        this.speed = speed;
    }

    /**
     * 
     * Set Amount
     *
     */
    public void setAmount(int amount) {
        this.amount = amount;
    }

    /**
     * 
     * Set Lifetime
     *
     */
    public void setLifetime(float lifetime) {
        this.lifetime = lifetime;
    }

    /**
     * 
     * Is Active
     *
     */
    public bool IsActive() {
        return isActive;
    }
    

    /**
     * 
     * Set Spawn Radius
     *
     */
    public void setSpawnRadius(float spawnRadius) {
        this.spawnRadius = spawnRadius;
    }

    /**
     * 
     * Set Live
     *
     */
    public void setLive(bool live) {
        this.live = live;
    }

    /**
     * 
     * Set Target Y
     *
     */
    public void setTargetY(float targetY) {
        this.targetY = targetY;
    }

    /**
     * 
     * Set Motion
     *
     */
    public void setMotion(bool enable, float swayAmplitude, float swayFrequency) {
        this.enableMotion = enable;
        this.swayAmplitude = swayAmplitude;
        this.swayFrequency = swayFrequency;
    }

    public void setMotion(bool enable) {
        this.enableMotion = enable;
    }

    /**
     * 
     * Set Player Move Speed
     *
     */
    public void setPlayerMovSpeed(float playerMovSpeed) {
        this.playerMovSpeed = playerMovSpeed;
    }

    /**
     * 
     * Set Player Stand
     *
     */
    public void setPlayerStand(float playerStand) {
        this.playerStand = playerStand;
    }

    /**
     * 
     * Set Update Interval
     *
     */
    public void setUpdateInterval(int interval) {
        this.updateInterval = Math.Max(1, interval);
    }

    /**
     * 
     * Generate Id
     *
     */
    // Generate Entity Id
    private string generateEntityId() {
        string val = $"PART_{Guid.NewGuid():N}";
        return val;
    }

    // Generate Index Id
    public string generateParticleId(int i) {
        string val = $"{id}_p_{i}";
        return val;
    }

    /**
     * 
     * Convert
     *
     */
    public static Particle convert(string data) {
        var particle = new Particle();
        if(string.IsNullOrEmpty(data)) return particle;

        var lines = data.Split('\n');
        foreach(var line in lines) {
            var trimmed = line.Trim();
            if(string.IsNullOrEmpty(trimmed)) continue;

            var separator = trimmed.Contains(':') ? ':' : '=';
            var parts = trimmed.Split(separator, 2);
            if(parts.Length != 2) continue;

            var key = parts[0].Trim().ToLower();
            var val = parts[1].Trim().TrimEnd(',');

            if(Particle.configMembers.TryGetValue(key, out var member)) {
                try {
                    var convertAttr = 
                        member.prop?.GetCustomAttribute<ConvertAttribute>() ??
                        member.field?.GetCustomAttribute<ConvertAttribute>();
                    if(convertAttr != null && Particle.converters.TryGetValue(convertAttr.Converter, out var converter)) {
                        var result = converter.Invoke(null, new object[] { val });
                        if(member.prop != null) {
                            member.prop.SetValue(particle, result);
                        } else if(member.field != null) {
                            member.field.SetValue(particle, result);
                        }
                    }
                } catch(Exception err) {
                    Console.WriteLine($"[Particle] Error setting {key}: {err.Message}");
                }
            } 
        }

        return particle;
    }

    /**
     * 
     * Setup
     *
     */
    public void setup() {
        if(sharedMeshInitialized) return;

        MeshData data = MeshDataLoader.load(MESH_TYPE);
        data.shaderType = 7;

        float[] colors = new float[16];
        for(int i = 0; i < 16; i += 4) {
            colors[i] = 1.0f;
            colors[i+1] = 1.0f;
            colors[i+2] = 1.0f;
            colors[i+3] = 1.0f;
        }
        data.setColors(colors);

        mesh.add(SHARED_MESH_ID, data);
        var renderer = mesh.getMeshRenderer(SHARED_MESH_ID);
        if(renderer != null) renderer.isInstanced = true;
        mesh.setScale(SHARED_MESH_ID, size);

        sharedMeshInitialized = true;
    }

    /**
     * 
     * Set
     *
     */
    public void set(Vector3 position, bool vel, float targetY = 0.0f, Func<Vector3>? colorSupplier = null) {
        /*particles.Clear();
        instancePositions.Clear();
        instanceColors.Clear();*/
    
        this.targetY = targetY;
        this.vel = vel;
        this.position = position;
        this.isActive = true;

        float startY = position.Y;
        float distance = startY - targetY;

        float updatedLifetime = lifetime / speed;
        float reqTargetY = -distance / updatedLifetime;

        for(int i = 0; i < amount; i++) {
            Particle particle = new Particle();
            particle.id = generateParticleId(i);
            particle.position = new Vector3(position);

            if(enableMotion) {
                particle.position = new Vector3(
                    position.X + ((float)random.NextDouble() - 0.5f) * 2.0f * spawnRadius,
                    position.Y,
                    position.Z + ((float)random.NextDouble() - 0.5f) * 2.0f * spawnRadius
                );

                updateVel(particle, speed, reqTargetY);
            } else {
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
            particle.lifetime = updatedLifetime;
            particle.maxLifetime = updatedLifetime;

            particles.Add(particle);

            instancePositions.Add(particle.position);
            instanceColors.Add(new float[] {
                particle.color.X,
                particle.color.Y,
                particle.color.Z,
                1.0f
            });
        }       

        needsBufferUpdate = true;
        updateCounter = 0;
    }

    public void set(Vector3 position, bool vel, Func<Vector3>? colorSupplier) {
        set(position, vel, this.targetY, colorSupplier);
    }

    public void set(Vector3 position, bool vel) {
        set(position, vel, this.targetY, null);
    }


    /**
     * 
     * Update
     *
     */
    // Update
    public void update() {
        if(!isActive) return;

        bool needsUpdate = false;

        for(int i = particles.Count - 1; i >= 0; i--) {
            Particle particle = particles[i];
            particle.lifetime -= DeltaTime;

            if(particle.lifetime <= 0 || particle.position.Y <= targetY) {
                particles.RemoveAt(i);
                instancePositions.RemoveAt(i);
                instanceColors.RemoveAt(i);

                needsUpdate = true;
                continue;
            }

            if(!enableMotion) {
                particle.vel.Y -= 
                    GRAVITY_VEL * 
                    DeltaTime * 
                    speed;
            }
            if(vel) {
                particle.position += new Vector3(
                    particle.vel.X * DeltaTime,
                    particle.vel.Y * DeltaTime,
                    particle.vel.Z * DeltaTime
                );

                instancePositions[i] = particle.position;
                needsUpdate = true;
            }

            float alpha = particle.lifetime / particle.maxLifetime;
            instanceColors[i][0] = particle.color.X;
            instanceColors[i][1] = particle.color.Y;
            instanceColors[i][2] = particle.color.Z;
            instanceColors[i][3] = alpha;

            if(needsUpdate) {
                needsBufferUpdate = true;
            }
        }

        if(particles.Count == 0) {
            isActive = false;
        }
    }

    // Update Movement
    public void updateMovement(Particle config, PlayerController playerController, ref Vector3 lastPlayerPos, ref bool isMoving) {
        if(config == null || !config.live) return;

        Vector3 playerPos = playerController.getPosition();
        Vector2 horizontalDelta = new Vector2(playerPos.X - lastPlayerPos.X, playerPos.Z - lastPlayerPos.Z);
        lastPlayerPos = playerPos;

        float t = 0.02f;

        if(horizontalDelta.Length > t) {
            movingTimer = MOVE_TIMEOUT;
        } else {
            movingTimer -= DeltaTime;
        }

        bool _isMoving = movingTimer > 0.0f;

        if(_isMoving != isMoving) {
            isMoving = _isMoving;
            updateSpeed(isMoving, config);
        }
    }

    // Update Speed
    public void updateSpeed(bool isMoving, Particle config) {
        var newSpeed = config.speed * (isMoving ? config.playerMovSpeed : config.playerStand);
        var newLifetime = newSpeed > 0 ? config.lifetime / newSpeed : config.lifetime;

        Console.WriteLine($"MOVING... {isMoving}");

        setSpeed(newSpeed);
        setLifetime(newLifetime);

        float startY = position.Y;
        float distance = startY - targetY;
        float updatedLifetime = newSpeed > 0 ? newLifetime : 1.0f;
        float reqTargetY = newSpeed > 0 ? -distance / updatedLifetime : 0.0f;

        foreach(var particle in particles) {
            particle.vel = new Vector3(updateVel(particle, newSpeed, reqTargetY));
        }
        needsBufferUpdate = true;
    }

    // Update Vel
    private Vector3 updateVel(Particle particle, float speed, float target) {
        Vector3 val = particle.vel = new Vector3(
            velNum.X * speed,
            target,
            velNum.Z * speed
        );
        return val;
    }

    /**
     * 
     * Apply to Entity
     *
     */
    public void applyToEntity(ParticleEntity entity) {
        entity.setLive(live);
        entity.setTargetY(targetY);
        entity.setColor(color);
        entity.setAmount(amount);
        entity.setSize(size);
        entity.setSpeed(speed);
        entity.setLifetime(lifetime);
        entity.setMotion(enableMotion);
        entity.setSpawnRadius(spawnRadius);
        entity.setVelNum(velNum);
        entity.setPlayerMovSpeed(playerMovSpeed);
        entity.setPlayerStand(playerStand);
    }

    /**
     * 
     * Render
     *
     */
    // Render
    public void render() {
        if(allPositions.Count == 0) return;
        mesh.renderId(SHARED_MESH_ID);
    }

    // Combine And Render
    public void combineAndRender() {
        allPositions.Clear();
        allColors.Clear();

        foreach(var entity in particleController.activeEntities) {
            if(!entity.isActive) continue;

            var positions = entity.getInstancePositions();
            var colors = entity.getInstanceColors();

            if(positions.Count > 0) {
                allPositions.AddRange(positions);
                allColors.AddRange(colors);
            }
        }

        if(allPositions.Count == 0) return;

        var renderer = mesh.getMeshRenderer(SHARED_MESH_ID);
        if(renderer != null) renderer.setInstanceData(allPositions, allColors);
    }

    /**
     * 
     * Cleanup
     *
     */
    // Cleanup
    public void cleanup() {
        particles.Clear();
        instancePositions.Clear();
        instanceColors.Clear();

        isActive = false;
        needsBufferUpdate = false;
        live = false;
        updateCounter = 0;
    }

    // Cleanup All
    public void cleanupAll() {
        allPositions.Clear();
        allColors.Clear();
    }

    /**
     * 
     * Reset
     *
     */
    public void reset() {
        particles.Clear();
        instancePositions.Clear();
        instanceColors.Clear();

        isActive = false;
        live = false;
        needsBufferUpdate = false;
        updateCounter = 0;
        position = Vector3.Zero;
        color = Vector3.One;
        size = 0.1f;
        speed = 1.0f;
        amount = 10;
        lifetime = 2.0f;
        velNum = Vector3.One;
        spawnRadius = 0.0f;
        targetY = 0.0f;
        playerMovSpeed = 0.0f;
        playerStand = 0.0f;
        
        vel = false;
        enableMotion = false;

        id = generateEntityId();
    }
}