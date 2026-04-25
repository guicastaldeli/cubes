
/**

    Grid class for
    the inventory resource grid.

    */
using App.Root.Mesh;
namespace App.Root.Player.Inventory;

class Grid {
    public List<Slot> slots = new();
    public int cols;
    public int rows;

    public Grid(
        int cols, int rows,
        int startX, int startY,
        int slotWidth, int slotHeight,
        int padding
    ) {
        this.cols = cols;
        this.rows = rows;
        int index = 0;
        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                int x = startX + c * (slotWidth + padding);
                int y = startY + r * (slotHeight + padding);
                slots.Add(new Slot(index++, r, c, x, y, slotWidth, slotHeight));
            }
        }
    }

    // Find Slot
    public Slot? findSlot(PlacedMeshDef def) {
        for(int r = rows - 1; r >= 0; r--) {
           for(int c = 0; c < cols; c++) {
                var s = slots[r * cols + c];
                
                bool sameStack =
                    s.def?.StackId != null &&
                    s.def.StackId == def.StackId;
                
                bool sameType = 
                    s.def?.MeshType == def.MeshType;

                if(sameStack && sameType && !s.isFull) return s;
            }
        }
        for(int r = rows - 1; r >= 0; r--) {
            for(int c = 0; c < cols; c++) {
                var s = slots[r * cols + c];
                if(s.isEmpty) return s;
            }
        }
        return null;
    }

    public Slot? findOccupiedSlot(PlacedMeshDef def) {
        return slots.FirstOrDefault(s => 
            s.def?.MeshType == def.MeshType &&
            (def.StackId == null || s.def?.StackId == def.StackId) && 
            s.count > 0
        );
    }

    // Get Slot At
    public Slot? getSlotAt(int mx, int my) {
        Slot? val = slots.FirstOrDefault(s => s.containsPoint(mx, my));
        return val;
    }
}