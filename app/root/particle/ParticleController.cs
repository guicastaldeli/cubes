/**

    Main particle general controller

    */
namespace App.Root.Particle;
using OpenTK.Mathematics;
using App.Root.Mesh;
using App.Root.Chunk;

class ParticleController {
    private Mesh mesh;

    [Poolable("particle_entities", typeof(PoolableList<ParticleEntity>), InitialSize = 16, MaxSize = 128)] private PoolableList<ParticleEntity> particlePool = null!;
    [Poolable("particle_active", typeof(PoolableList<ParticleEntity>), InitialSize = 16, MaxSize = 128)] private PoolableList<ParticleEntity> activeEntities = null!;

    public ParticleController(Mesh mesh) {
        this.mesh = mesh;

        PoolInjector.Inject(this);
    }

    // Get Particle Entity
    public ParticleEntity? getParticleEntity() {
        ParticleEntity? val = activeEntities.Count > 0 ? activeEntities[0] : null;
        return val;
    }

    public List<ParticleEntity> getParticleEntities() {
        List<ParticleEntity> val = new List<ParticleEntity>(activeEntities);
        return val;
    }

    // Return Entity
    private void returnEntity(ParticleEntity entity) {
        entity.cleanup();
        activeEntities.Remove(entity);
        particlePool.Add(entity);
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
        ParticleEntity entity;
        if(particlePool.Count > 0) {
            entity = particlePool[0];
            particlePool.RemoveAt(0);
            entity.reset();
        } else {
            entity = new ParticleEntity(mesh);
            entity.setup();
        }
        
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
        for(int i = activeEntities.Count - 1; i >= 0; i--) {
            var entity = activeEntities[i];
            entity.update();
            if(!entity.IsActive()) returnEntity(entity);
        }
    }

    /**
     * 
     * Render
     *
     */
    public void render() {
        foreach(ParticleEntity entity in activeEntities) {
            entity.render();
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
    }
}