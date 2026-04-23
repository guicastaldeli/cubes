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
    
        Create
    
        */
    public void create(
        Vector3 position,
        Vector3 color,
        int amount,
        float size,
        float speed,
        float lifetime
    ) {
        particleEntity = new ParticleEntity(mesh);
        
        particleEntity.setColor(color);
        particleEntity.setSize(size);
        particleEntity.setSpeed(speed);
        particleEntity.setLifetime(lifetime);

        particleEntities.Add(particleEntity);
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