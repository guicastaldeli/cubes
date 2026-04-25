namespace App.Root.World.Env.Skybox;
using App.Root.Mesh;
using App.Root.Utils;

class Skybox : WorldHandler {
    private const string ID = "skybox";
    private const string MESH = "skybox";

    private Mesh mesh;

    private bool initialized = false;

    public Skybox([Inject] Mesh mesh) {
        this.mesh = mesh;
    }

    /**
    
        Set
    
        */
    private void set() {
        MeshData data = MeshDataLoader.load(MESH);

        mesh.setPosition(ID, 0.0f, 0.0f, 0.0f);
        mesh.add(ID, data);
        mesh.setScale(ID, 100.0f);
    }

    /**
    
        Render

        */  
    public override void render() {
        if(!initialized) {
            set();

            initialized = true;
        }
    }

    /**
    
        Update

        */ 
    public override void update() {
        
    }
}