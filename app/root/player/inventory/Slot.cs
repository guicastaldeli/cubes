
using App.Root.Mesh;

/**

    Slot class for
    the inventory resource slots.

    */
namespace App.Root.Player.Inventory;

class Slot {
    public int index;
    public int row;
    public int col;

    public int x;
    public int y;
    public int width;
    public int height;

    public string? itemId = null;
    public int count = 0;
    public int maxStack = 24;

    public bool isEmpty => count == 0 || itemId == null;
    public bool isFull => count >= maxStack;
    public PlacedMeshDef? def = null;

    public UI.UIElement el;

    public Slot(
        int index,
        int row,
        int col,
        int x,
        int y,
        int width,
        int height
    ) {
        this.index = index;
        this.row = row;
        this.col = col;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;

        el = new UI.UIElement(
            "div", "", "", "arial",
            x, y, width, height,
            1.0f, new float[]{ 1f, 1f, 1f, 1f },
            true, ""
        );
        el.hoverable = true;
        el.hoverBackgroundColor = new float[]{ 1f, 1f, 1f, 0.5f };
    }

    // Contains Point
    public bool containsPoint(int mx, int my) {
        return mx >= x && mx <= x  + width &&
            my >= y && my <= y + height;
    }

    ///
    /// Add
    /// 
    public int add(PlacedMeshDef incomingDef, int amount) {
        if(def != null && def.StackId != incomingDef.StackId) return amount;
        if(def == null) def = incomingDef;
        itemId = incomingDef.MeshType;
        
        int canAdd = maxStack - count;
        int adding = Math.Min(canAdd, amount);
        count += adding;

        return amount - adding;
    }

    ///
    /// Remove
    /// 
    public int setRemove(int amount) {
        int removing = Math.Min(count, amount);
        count -= removing;

        if(count == 0) itemId = null;

        return removing;
    }

    public void remove() {
        setRemove(1);
    }
}