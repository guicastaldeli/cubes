
using App.Root.Mesh;
using App.Root.Screen;
using App.Root.Shaders;
using App.Root.Text;

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
    
    private int activeSlotIndex = -1;
 
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

    // Handle Mouse Move
    public void handleMouseMove(int mouseX, int mouseY) {
        foreach(var s in grid.slots) {
            bool over = s.el.containsPoint(mouseX, mouseY);
            if(over && !s.el.isHovered) s.el.applyHover();
            else if(!over && s.el.isHovered) s.el.removeHover();
        }

        if(!slot.isEmpty) {
            slot.x = mouseX - slot.width / 2;
            slot.y = mouseY - slot.height / 2;
            slot.el.x = slot.x;
            slot.el.y = slot.y;
        }
    }

    // Handle Mouse Click
    public void handleMouseClick(int mouseX, int mouseY) {
        var clicked = grid.getSlotAt(mouseX, mouseY);
        if(clicked == null) return;

        (slot.itemId, clicked.itemId) = (clicked.itemId, slot.itemId);
        (slot.count, clicked.count) = (clicked.count, slot.count);
        (slot.def, clicked.def) = (clicked.def, slot.def);
    }

    // Active Slot
    public Slot getActiveSlot() {
        int startIndex = (rows - 1) * cols;
        int slotIndex = 
            activeSlotIndex >= 0 ? 
            activeSlotIndex : 
            startIndex;
        if(slotIndex >= 0 && slotIndex < grid.slots.Count) {
            return grid.slots[slotIndex];
        }
        return grid.slots[startIndex];
    }

    public void setActiveSlot(int index) {
        activeSlotIndex = index;
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
        foreach(var s in grid.slots) {
            renderSlot(s);
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