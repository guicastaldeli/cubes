namespace App.Root.Player;
using App.Root._Sync;
using App.Root.Info;
using App.Root.Mesh;
using OpenTK.Mathematics;

/**

    Slot Extensions Helper
    to help detect slots.

    */
static class SlotExtensions {
    public static IEnumerable<Slot> GetSlots(this Slot slot) {
        return slot == Slot.CENTER ?
            new[] { Slot.LEFT, Slot.RIGHT } :
            new[] { slot };
    }

    public static Slot GetOppositeSlot(this Slot slot) {
        return slot switch {
            Slot.LEFT => Slot.RIGHT,
            Slot.RIGHT => Slot.LEFT,
            _ => slot
        };
    }
}

/**

    Slot class helper

    */
static class SetSlot {
    private static Camera camera = null!;
    private static Mesh mesh = null!;

    public static Dictionary<Slot, string?> slotMeshIds = new();
    public const string SLOT_MESH = "rectangle";

    /**
     *
     * Init
     *
     */
    public static void Init(Camera camera, Mesh mesh) {
        SetSlot.camera = camera;
        SetSlot.mesh = mesh;
    }

    /**
     *
     * Set
     *
     */
    public static Vector3 Set(Slot slot, string id) {
        Vector3 offset = Vector3.Zero;
        Matrix4 rotationMatrix = Matrix4.Identity;

        float posX = 1.5f;
        float posY = 1.0f;
        float posZ = 3.0f;

        (float a, float b) rotationX = (0.0f, 1.0f);
        (float a, float b) rotationY = (5.0f, 1.0f);
        (float a, float b) rotationZ = (0.0f, 1.0f);

        switch(slot) {
            case Slot.LEFT:
                offset = new Vector3(-posX, -posY, posZ);
                rotationMatrix = 
                    Matrix4.CreateRotationX(rotationX.a) *
                    Matrix4.CreateRotationY(-rotationY.a) *
                    Matrix4.CreateRotationZ(rotationZ.a);
                break;
            case Slot.RIGHT:
                offset = new Vector3(posX, -posY, posZ); 
                rotationMatrix = 
                    Matrix4.CreateRotationX(rotationX.a) *
                    Matrix4.CreateRotationY(rotationY.a) *
                    Matrix4.CreateRotationZ(rotationZ.a);
                break;
            case Slot.CENTER:
                rotationMatrix = 
                    Matrix4.CreateRotationX(rotationX.b) *
                    Matrix4.CreateRotationY(rotationY.b) *
                    Matrix4.CreateRotationZ(rotationZ.b);
                break;
        }

        Vector3 forward = camera.getFront();
        Vector3 right = camera.getRight();
        Vector3 up = camera.getUp();
        Matrix4 cameraRotation = new Matrix4(
            new Vector4(right, 0.0f),
            new Vector4(up, 0.0f),
            new Vector4(-forward, 0.0f),
            new Vector4(0, 0, 0, 1.0f)
        );

        Matrix4 rotation = rotationMatrix * cameraRotation;
        mesh.setRotationMatrix(id, rotation);
        return offset;
    }
}

/**

    Main Player Mesh class.

    */
class PlayerMesh {
    private const string PLAYER_MESH_DATA = "player_mesh_data"; 
    
    [StoreData(PLAYER_MESH_DATA)]
    [DataSync]
    public class PlayerData {
        [StoreField("player_id")] public string PlayerId { get; set; } = InfoController.UserId;
        [StoreField("mesh_type")] public string PlayerMesh { get; set; } = "sphere";
        [StoreField("visible")] public bool Visible { get; set; } = true;
    }

    private Window window;
    private Camera camera;
    private PlayerController playerController;
    private Mesh mesh;

    private PlayerData Data;

    public PlayerMesh(Window window, Camera camera, PlayerController playerController, Mesh mesh) {
        this.window = window;
        this.camera = camera;
        this.playerController = playerController;
        this.mesh = mesh;

        this.Data = new PlayerData();
        set(false);

        SetSlot.Init(camera, mesh);

        SyncManager.I.RegisterSync(PLAYER_MESH_DATA, Data);
    }

    // Get Slot Id
    private string getSlotId(Slot slot) {
        string val = $"slot-{slot}";
        return val;
    }

    /**
     * 
     * Set
     *
     */
    // Set
    public void set(bool local) {
        MeshRegistry.register(Data.PlayerId);

        if(!mesh.hasMesh(Data.PlayerId)) {
            window.queueOnRenderThread(() => {
                MeshData data = MeshDataLoader.load(Data.PlayerMesh);
                
                mesh.add(Data.PlayerId, data);
                if(local) mesh.setVisible(Data.PlayerId, Data.Visible);
            });
        }
    }

    // Set Slot Mesh
    public void setSlotMesh(Slot slot) {
        if(slot == Slot.CENTER) return;
        
        string SLOT_ID = getSlotId(slot);
        if(!mesh.hasMesh(SLOT_ID)) {
            window.queueOnRenderThread(() => {
                MeshData data = MeshDataLoader.load(SetSlot.SLOT_MESH);
                mesh.add(SLOT_ID, data);
                mesh.setScale(SLOT_ID, 0.5f, 0.5f, 0.5f);

                var renderer = mesh.getMeshRenderer(SLOT_ID);
                if(renderer != null) renderer.renderOnTop = true;

                updateSlotPosition(slot, SLOT_ID);
                mesh.setVisible(SLOT_ID, true);
                SetSlot.slotMeshIds[slot] = SLOT_ID;
            });
        } else {
            updateSlotPosition(slot, SLOT_ID);
            mesh.setVisible(SLOT_ID, true);
            SetSlot.slotMeshIds[slot] = SLOT_ID;
        }
    }

    /**
     * 
     * Hide
     *
     */
    private void hideSlot(Slot slot) {
        if(slot == Slot.CENTER) return;

        string id = getSlotId(slot);
        if(mesh.hasMesh(id)) mesh.setVisible(id, false);
        SetSlot.slotMeshIds[slot] = null;
    }

    public void hideSlots(Slot? slot) {
        if(slot == null) return;
        foreach(var s in slot.Value.GetSlots()) {
            hideSlot(s);
        }
    }
    

    /**
     * 
     * Update
     *
     */
    // Update
    public void update() {
        if(!mesh.hasMesh(Data.PlayerId)) {
            Console.WriteLine("ID NULL!!!!!!");
        }

        var pos = playerController.getPosition();
        mesh.setPosition(Data.PlayerId, pos.X, pos.Y - 1.5f, pos.Z);

        SyncManager.I.TriggerSync(PLAYER_MESH_DATA);
    }

    // Update Slot Position
    public void updateSlotPosition(Slot slot, string slotId) {
        Vector3 offset = SetSlot.Set(slot, slotId);

        Vector3 forward = camera.getFront();
        Vector3 right = camera.getRight();
        Vector3 up = camera.getUp();

        Vector3 pos =
            playerController.getPosition() +
            forward * offset.Z +
            right * offset.X +
            up * offset.Y;
        
        mesh.setPosition(slotId, pos);
    }

    // Update Slots
    public void updateSlots(Slot slot) {
        if(slot != Slot.CENTER) {
            hideSlot(slot.GetOppositeSlot());
        }
        foreach(var s in slot.GetSlots()) {
            setSlotMesh(s);
        }
    }
}