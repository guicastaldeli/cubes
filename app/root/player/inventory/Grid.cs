/**

    Grid class for
    the inventory resource grid.

    */
namespace App.Root.Player.Inventory;

class Grid {
    public List<Slot> slots = new();
    public int cols;
    public int rows;

    public Grid(
        int cols,
        int rows,
        int startX,
        int startY,
        int slotSize,
        int padding
    ) {
        this.cols = cols;
        this.rows = rows;

        int index = 0;
        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                int x = startX + c * (slotSize + padding);
                int y = startY + r * (slotSize + padding);
                slots.Add(new Slot(index++, r, c, x, y, slotSize, slotSize));
            }
        }
    }

    // Find Slot
    public Slot? findSlot(string itemId) {
        var partial = slots.FirstOrDefault(s => s.itemId == itemId && !s.isFull);
        if(partial != null) return partial;
        return slots.FirstOrDefault(s => s.isEmpty);
    }

    // Get Slot At
    public Slot? getSlotAt(int mx, int my) {
        Slot? val = slots.FirstOrDefault(s => s.containsPoint(mx, my));
        return val;
    }
}