/**

    Main particle general controller

    */
namespace App.Root.Mesh.Particle;
using OpenTK.Mathematics;

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
    
        Emit
    
        */
    public ParticleEntity emit(
        Vector3 position,
        Vector3 color,
        int amount,
        float size,
        float speed,
        float lifetime,
        Vector3? velNum = null,
        Func<Vector3>? colorSupplier = null
    ) {
        ParticleEntity entity = new ParticleEntity(mesh);
        
        entity.setColor(color);
        entity.setAmount(amount);
        entity.setSize(size);
        entity.setSpeed(speed);
        entity.setLifetime(lifetime);
        if(velNum.HasValue) entity.setVelNum(velNum.Value);

        entity.set(position, velNum.HasValue, colorSupplier);

        particleEntities.Add(entity);
        return entity;
    }

    /**
    
        Update
    
        */
    public void update() {
        List<ParticleEntity> entitiesToRemove = new List<ParticleEntity>();
        
        foreach(ParticleEntity entity in particleEntities) {
            entity.update();
            if(!entity.isActiveEntity()) {
                entitiesToRemove.Add(entity);
            }
        }
        foreach(ParticleEntity entity in entitiesToRemove) {
            particleEntities.Remove(entity);
        }
    }

    /**
    
        Render
    
        */
    public void render() {
        foreach(ParticleEntity entity in particleEntities) {
            entity.render();
        }
    }

    /**
    
        Cleanup
    
        */
    public void cleanup() {
        foreach(ParticleEntity entity in particleEntities) {
            entity.cleanup();
        }
        particleEntities.Clear();
    }
}