
using App.Root.Mesh;
using App.Root.Screen;
using App.Root.Shaders;
using App.Root.Text;
using App.Root.UI;

/**

    Main Inventory class.

    */
namespace App.Root.Player.Inventory;

class Inventory {
    private ShaderProgram shaderProgram = null!;
    private TextRenderer textRenderer = null!;

    public Grid grid = null!;
    public Slot mainSlot = null!;

    private int screenWidth;
    private int screenHeight;
    private int width;
    private int height;

    private int startX;
    private int startY;

    private int cols;
    private int rows;
    private float edgePaddingPct;
    private float topPaddingPct;
    private float gapPct;
 
    public Inventory(
        int screenWidth,
        int screenHeight,
        int startX, 
        int startY,
        int width,
        int height,
        int cols,
        int rows,
        float edgePaddingPct,
        float topPaddingPct,
        float gapPct = 0.006f
    ) {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.width = width;
        this.height = height;
        this.startX = startX;
        this.startY = startY;
        this.cols = cols;
        this.rows = rows;
        this.edgePaddingPct = edgePaddingPct;
        this.topPaddingPct = topPaddingPct;
        this.gapPct = gapPct;
        build();
    }

    // Set Shader Program
    public void setShaderProgram(ShaderProgram shaderProgram) {
        this.shaderProgram = shaderProgram;
    }

    // Set Text Renderer
    public void setTextRenderer(TextRenderer textRenderer) {
        this.textRenderer = textRenderer;
    }
    
    // Add Item
    public bool addItem(PlacedMeshDef def) {
        int remaining = 1;
        while(remaining > 0) {
            var slot = grid.findSlot(def);
            if(slot == null) return false;
            remaining = slot.add(def, remaining);
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
        int edgePadding = (int)(width * edgePaddingPct);
        int gapBetween = (int)(width * gapPct);
        int slotSize = 
            (width - 
            (edgePadding * 2) - 
            (gapBetween * (cols - 1))) / 
            cols;
        int topPadding = (int)(height * topPaddingPct);

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
        // Slot
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
        DocParser.renderUIElement(
            el, 
            screenWidth, screenHeight, 
            shaderProgram
        );

        // Stack Count
        if(slot.count > 1 && textRenderer != null) {
            textRenderer.renderText(
                slot.count.ToString(),
                slot.x + slot.width - 12,
                slot.y + slot.height - 14,
                0.5f,
                new float[]{ 1.0f, 1.0f, 1.0f, 1.0f },
                "arial"
            );
        }
    }
}