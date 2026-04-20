namespace App.Root.Player;
using App.Root.Mesh;
using OpenTK.Mathematics;

/**

    Slot Extensions Helper
    to help detect arms.

    */
static class SlotExtensions {
    public static IEnumerable<Slot> GetArmSlots(this Slot slot) {
        return slot == Slot.CENTER ?
            new[] { Slot.LEFT, Slot.RIGHT } :
            new[] { slot };
    }

    public static Slot GetOppositeArm(this Slot slot) {
        return slot switch {
            Slot.LEFT => Slot.RIGHT,
            Slot.RIGHT => Slot.LEFT,
            _ => slot
        };
    }
}

/**

    Arm class helper

    */
class Arm {
    private Mesh mesh;

    public Arm(Mesh mesh) {
        this.mesh = mesh;
    }

    public Vector3 set(Slot slot, string id) {
        Vector3 offset = Vector3.Zero;
        Matrix4 rotationMatrix = Matrix4.Identity;

        switch(slot) {
            case Slot.LEFT:
                offset = new Vector3(-1.5f, -1.0f, 5.0f);
                rotationMatrix = 
                    Matrix4.CreateRotationX(0.0f) *
                    Matrix4.CreateRotationY(5.0f) *
                    Matrix4.CreateRotationZ(0.0f);

                break;
            case Slot.RIGHT:
                offset = new Vector3(1.5f, -1.0f, 5.0f); 
                rotationMatrix = 
                    Matrix4.CreateRotationX(0.0f) *
                    Matrix4.CreateRotationY(5.0f) *
                    Matrix4.CreateRotationZ(0.0f);

                break;
            case Slot.CENTER:
                rotationMatrix = 
                    Matrix4.CreateRotationX(1.0f) *
                    Matrix4.CreateRotationY(1.0f) *
                    Matrix4.CreateRotationZ(1.0f);

                break;
        }

        mesh.setRotationMatrix(id, rotationMatrix);
        return offset;
    }
}

/**

    Main Player Mesh class.

    */
class PlayerMesh {
    private Window window;
    private Camera camera;
    private PlayerController playerController;
    private Mesh mesh;

    private Arm arm;
    
    public string PLAYER_ID => PlayerController.getId();
    public static string PLAYER_MESH = "sphere";

    public Dictionary<Slot, string?> armMeshIds = new();
    private const string ARM_MESH = "rectangle";

    public PlayerMesh(
        Window window,
        Camera camera,
        PlayerController playerController, 
        Mesh mesh
    ) {
        this.window = window;
        this.camera = camera;
        this.playerController = playerController;
        this.mesh = mesh;

        this.arm = new Arm(mesh);
    }

    // Get Arm Id
    private string getArmId(Slot slot) {
        string val = $"arm-{slot}";
        return val;
    }

    /**
    
        Set 

        */
    public void set(bool local) {
        MeshRegistry.register(PLAYER_ID);

        if(!mesh.hasMesh(PLAYER_ID)) {
            window.queueOnRenderThread(() => {
                MeshData data = MeshLoader.load(PLAYER_MESH);
                mesh.add(PLAYER_ID, data);
                if(local) mesh.setVisible(PLAYER_ID, false);
            });
        }
    }

    public void setArm(Slot slot) {
        if(slot == Slot.CENTER) return;
        
        string ARM_ID = getArmId(slot);

        if(!mesh.hasMesh(ARM_ID)) {
            window.queueOnRenderThread(() => {
                MeshData data = MeshLoader.load(ARM_MESH);
                mesh.add(ARM_ID, data);
                mesh.setScale(ARM_ID, 0.5f, 0.5f, 0.5f);

                updateArmPosition(slot, ARM_ID);
                mesh.setVisible(ARM_ID, true);
                armMeshIds[slot] = ARM_ID;
            });
        } else {
            updateArmPosition(slot, ARM_ID);
            mesh.setVisible(ARM_ID, true);
            armMeshIds[slot] = ARM_ID;
        }
    }

    /**
    
        Hide 

        */
    private void hideArm(Slot slot) {
        if(slot == Slot.CENTER) return;

        string armId = getArmId(slot);
        if(mesh.hasMesh(armId)) mesh.setVisible(armId, false);
        armMeshIds[slot] = null;
    }

    public void hideArms(Slot? slot) {
        if(slot == null) return;
        foreach(var s in slot.Value.GetArmSlots()) {
            hideArm(s);
        }
    }
    

    /**
    
        Update
    
        */
    public void update() {
        if(!mesh.hasMesh(PLAYER_ID)) return;

        var pos = playerController.getPosition();
        mesh.setPosition(PLAYER_ID, pos.X, pos.Y, pos.Z);
    }

    public void updateArmPosition(Slot slot, string armId) {
        Vector3 offset = arm.set(slot, armId);

        Vector3 forward = camera.getFront();
        Vector3 right = camera.getRight();
        Vector3 up = camera.getUp();

        Vector3 pos =
            playerController.getPosition() +
            forward * offset.Z +
            right * offset.X +
            up * offset.Y;
        
        mesh.setPosition(armId, pos);
    }

    public void updateArms(Slot slot) {
        if(slot != Slot.CENTER) {
            hideArm(slot.GetOppositeArm());
        }
        foreach(var s in slot.GetArmSlots()) {
            setArm(s);
        }
    }
}