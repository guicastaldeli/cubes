/**

    Main particle general controller

    */
namespace App.Root.Particle;
using OpenTK.Mathematics;
using App.Root.Mesh;
using App.Root.Chunk;

class ParticleController {
    private Mesh mesh;
    private ParticleEntity particleEntity;

    [Poolable("particle_entities", typeof(PoolableList<ParticleEntity>), InitialSize = 16, MaxSize = 128)] public PoolableList<ParticleEntity> particlePool = null!;
    [Poolable("particle_active", typeof(PoolableList<ParticleEntity>), InitialSize = 16, MaxSize = 128)] public PoolableList<ParticleEntity> activeEntities = null!;

    public ParticleController(Mesh mesh) {
        this.mesh = mesh;
        this.particleEntity = new ParticleEntity(mesh, this);
        this.particleEntity.setup();

        PoolInjector.Inject(this);
    }

    // Get Particle Entity
    public ParticleEntity getParticleEntity() {
        ParticleEntity? entity;

        if(particlePool.Count > 0) {
            entity = particlePool[0];
            particlePool.RemoveAt(0);
            
            entity.reset();
        } else {
            entity = new ParticleEntity(mesh, this);
        }

        return entity;
    }

    public List<ParticleEntity> getParticleEntities() {
        List<ParticleEntity> val = new List<ParticleEntity>(activeEntities);
        return val;
    }

    /**
     * 
     * Emit
     *
     */
    public ParticleEntity emit(
        Vector3 position,
        Vector3 color,
        int amount,
        float size,
        float speed,
        float lifetime,
        Vector3? velNum = null,
        Func<Vector3>? colorSupplier = null,
        float targetY = 0.0f,
        bool enableMotion = false,
        float spawnRadius = 0.0f
    ) {
        ParticleEntity entity = getParticleEntity();
        
        entity.setTargetY(targetY);
        entity.setColor(color);
        entity.setAmount(amount);
        entity.setSize(size);
        entity.setSpeed(speed);
        entity.setLifetime(lifetime);
        entity.setMotion(enableMotion);
        entity.setSpawnRadius(spawnRadius);
        if(velNum.HasValue) entity.setVelNum(velNum.Value);

        entity.set(position, velNum.HasValue, colorSupplier);

        activeEntities.Add(entity);
        return entity;
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        bool hasUpdates = false;

        for(int i = activeEntities.Count - 1; i >= 0; i--) {
            var entity = activeEntities[i];
            entity.update();
            
            if(!entity.IsActive()) {
                particleEntity.returnEntity(entity);
            } else if(entity.needsBufferUpdate) {
                hasUpdates = true;
                entity.needsBufferUpdate = false;
            }
        }

        if(hasUpdates && activeEntities.Count > 0) {
            particleEntity.combineAndRender();
        }
    }

    /**
     * 
     * Render
     *
     */
    public void render() {
        if(particleEntity.allPositions.Count > 0) {
            particleEntity.render();
        }
    }

    /**
     * 
     * Cleanup
     * 
     */
    public void cleanup() {
        foreach(ParticleEntity entity in activeEntities) {
            entity.cleanup();
            particlePool.Add(entity);
        }

        activeEntities.Clear();
        particleEntity.cleanupAll();
    }
}