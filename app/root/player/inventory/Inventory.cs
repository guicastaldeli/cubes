
using App.Root.Shaders;
using App.Root.UI;

/**

    Main Inventory class.

    */
namespace App.Root.Player.Inventory;

class Inventory {
    private ShaderProgram shaderProgram;

    public Grid grid;
    public Slot mainSlot;

    private int screenWidth;
    private int screenHeight;
    private int width;
    private int height;

    private const int SLOT_SIZE = 54;
    private const int PADDING = 7;
    private const int OFFSET = 7;
    private const int OFFSET_Y = 7;

    public Inventory(
        ShaderProgram shaderProgram,
        int screenWidth, 
        int screenHeight,
        int width,
        int height
    ) {
        this.shaderProgram = shaderProgram;

        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.width = width;
        this.height = height;
        
        build();
    }
    
    // Add Item
    public bool addItem(string itemId) {
        int remaining = 1;
        while(remaining > 0) {
            var slot = grid.findSlot(itemId);
            if(slot == null) return false;
            remaining = slot.add(itemId, remaining);
        }
        return true;
    }

    // Remove Item from Main Slot
    public string? removeFromMain() {
        if(mainSlot.isEmpty) return null;

        string id = mainSlot.itemId!;
        mainSlot.remove(1);

        return id;
    }

    // Select
    public void selectSlot(int index) {
        if(index < 0 || index >= grid.slots.Count) return;

        var src = grid.slots[index];
        if(src.isEmpty) return;

        (mainSlot.itemId, src.itemId) = (src.itemId, mainSlot.itemId);
        (mainSlot.count, src.count) = (src.count, mainSlot.count);
    }

    // On Resize
    public void onResize(int w, int h) {
        screenWidth = w;
        screenHeight = h;
        build();
    }

    /**

        Build

        */
    private void build() {
        int startX = (screenWidth - width) / 2;
        int startY = (screenHeight - height) / 2;

        float ef = 0.012f;
        int edgePadding = (int)(width * ef);

        int cols = 9;
        int rows = 3;

        float gbf = 0.006f;
        int gapBetween = (int)(width * gbf);
        
        int slotSize = 
            (width - (edgePadding * 2) - 
            (gapBetween * (cols - 1))) / 2;

        float tpf = 0.055f;
        int topPadding = (int)(width * tpf);

        grid = new Grid(
            cols, rows,
            startX + edgePadding,
            startY + topPadding,
            slotSize,
            gapBetween
        );

        int mainY = 
            startY + 
            topPadding + 
            rows * (slotSize + gapBetween) + 
            gapBetween;
        mainSlot = new Slot(
            -1, -1, -1,
            startX + edgePadding,
            mainY,
            slotSize, slotSize
        );
    }

    /**
    
        Render
    
        */
    public void render() {
        foreach(var slot in grid.slots) {
            if(slot.isEmpty) continue;
            renderSlot(slot);
        }
        if(!mainSlot.isEmpty) {
            renderSlot(mainSlot);
        }
    }

    private void renderSlot(Slot slot) {
        var el = new UIElement(
            "div", "", "", "arial",
            slot.x, slot.y,
            slot.width, slot.height,
            1.0f,
            new float[]{ 1.0f, 1.0f, 1.0f, 1.0f },
            true,
            ""
        );
        el.backgroundColor = new float[]{ 1.0f, 1.0f, 1.0f, 0.3f };
    }
}