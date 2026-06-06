/**

    Main particle general controller

    */
namespace App.Root.Particle;
using OpenTK.Mathematics;
using App.Root.Mesh;

class ParticleController {
    private Mesh mesh;

    private List<ParticleEntity> particleEntities;
    private ParticleEntity? particleEntity;

    public ParticleController(Mesh mesh) {
        this.mesh = mesh;
        this.particleEntities = new List<ParticleEntity>();
    }

    // Get Particle Entity
    public ParticleEntity? getParticleEntity() {
        return particleEntity;
    }

    public List<ParticleEntity> getParticleEntities() {
        return particleEntities;
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
        ParticleEntity entity = new ParticleEntity(mesh);
        
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

        particleEntities.Add(entity);
        return entity;
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        for(int i = particleEntities.Count - 1; i >= 0; i--) {
            particleEntities[i].update();
            if(!particleEntities[i].isActiveEntity()) {
                particleEntities[i].cleanup();
                particleEntities.RemoveAt(i);
            }
        }
    }

    /**
     * 
     * Render
     *
     */
    public void render() {
        foreach(ParticleEntity entity in particleEntities) {
            entity.render();
        }
    }

    /**
     * 
     * Cleanup
     * 
     */
    public void cleanup() {
        foreach(ParticleEntity entity in particleEntities) {
            entity.cleanup();
        }
        particleEntities.Clear();
    }
}