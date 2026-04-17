
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
    public Slot slot = null!;

    private int screenWidth;
    private int screenHeight;
    private int width;
    private int height;
    private float slotWidthPct;
    private float slotHeightPct;

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
        float gapPct,
        float slotWidthPct,
        float slotHeightPct
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
        this.slotWidthPct = slotWidthPct;
        this.slotHeightPct = slotHeightPct;
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

    // Select
    public void selectSlot(int index) {
        if(index < 0 || index >= grid.slots.Count) return;

        var src = grid.slots[index];
        if(src.isEmpty) return;

        (slot.itemId, src.itemId) = (src.itemId, slot.itemId);
        (slot.count, src.count) = (src.count, slot.count);
    }

    // On Resize
    public void onResize(int w, int h) {
        screenWidth = w;
        screenHeight = h;
        build();
    }

    // Handle Mouse Move
    public void handleMouseMove(int mouseX, int mouseY) {
        foreach(var slot in grid.slots) {
            bool over = slot.el.containsPoint(mouseX, mouseY);
            if(over && !slot.el.isHovered) slot.el.applyHover();
            else if(!over && slot.el.isHovered) slot.el.removeHover();
        }
    }

    /**

        Build

        */
    private void build() {
        int edgePadding = (int)(width * edgePaddingPct);
        int topPadding = (int)(height * topPaddingPct);
        int slotWidth = (int)(width * slotWidthPct);
        int slotHeight = (int)(height * slotHeightPct);
        int gapBetween = (int)(width * gapPct);

        grid = new Grid(
            cols, rows,
            startX + edgePadding,
            startY + topPadding,
            slotWidth,
            slotHeight,
            gapBetween
        );

        int mainY = startY + topPadding + rows * (slotHeight + gapBetween) + gapBetween;
        slot = new Slot(
            -1, -1, -1,
            startX + edgePadding,
            mainY,
            slotWidth, slotHeight
        );
    }

    /**
    
        Render
    
        */
    public void render() {
        foreach(var slot in grid.slots) {
            renderSlot(slot);
        }
        if(!slot.isEmpty) {
            renderSlot(slot);
        }
    }

    private void renderSlot(Slot slot) {
        if(!slot.el.isHovered && slot.isEmpty) return;

        if(!slot.isEmpty) {
            slot.el.backgroundColor = new float[]{ 0.0f, 1.0f, 1.0f, 0.5f };
        }

        DocParser.renderUIElement(slot.el, screenWidth, screenHeight, shaderProgram);

        if(slot.count > 1 && textRenderer != null) {
            textRenderer.renderText(
                slot.count.ToString(),
                slot.x + slot.width - 12,
                slot.y + slot.height - 14,
                0.5f, new float[]{ 1f, 1f, 1f, 1f }, "arial"
            );
        }
    }
}