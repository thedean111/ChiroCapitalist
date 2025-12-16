using System.Collections.Generic;
using UnityEngine;

public class TilePlacementSystem {
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private InteractableGrid _grid;
    private Dictionary<Vector2Int, CellData> cells = new(); // Every cell on the grid that has data
    private Dictionary<int, TileInstance> tiles = new(); // All tile instances mapped by their id
    private int uid = 0;
    //*********************************************************************

    public TilePlacementSystem(InteractableGrid grid) {
        _grid = grid;
    }

    /// <summary>
    /// Attempt to place a tile on the grid. Only checks for grid validity.
    /// </summary>
    public void TryPlace(Vector2Int coord, TileDefinition def, int rot, Transform root) {
        TileInstance instance = new TileInstance();
        instance.tileID = uid;
        instance.def = def;
        instance.origin = coord;
        instance.instance = Object.Instantiate(def.prefab, _grid.GridToWorld(coord), Quaternion.identity, root);

        tiles.Add(uid, instance);

        // For each cell coordinate the tile spans, add its cell data
        for (int x = 0; x < def.size.x; x++) {
            for (int y = 0; y < def.size.y; y++) {
                // Situate the coordinates
                Vector2Int local = new Vector2Int(x, y);
                Vector2Int rotatedLocal = _grid.RotateLocal(local, def.size, rot);
                Vector2Int world = coord + rotatedLocal;

                // Base cell data
                CellData data = new CellData();
                data.tileID = uid;
                data.type = def.type;
                
                // Extract the potential flags from the local coordinate
                def.GetFlags(local, out data.flags);

                // Add the cell to the dictionary
                cells.Add(world, data);
            }
        }

        uid++;
    }

    /// <summary>
    /// Checks if the footprint provided by the input overlaps with any cells that are already established.
    /// </summary>
    public bool CheckOverlap(Vector2Int coord, Vector2Int size) {
        for (int x = coord.x; x < coord.x + size.x; x++) {
            for (int y = coord.y; y < coord.y + size.y; y++) {
                if (cells.ContainsKey(new Vector2Int(x, y))) { return true; }
            }
        }

        return false;
    }
}