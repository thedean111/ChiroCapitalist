using System.Collections.Generic;
using DG.Tweening;
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

    public Dictionary<Vector2Int, CellData> GetCells() { return cells; }

    /// <summary>
    /// Attempt to place a tile on the grid. Only checks for grid validity.
    /// </summary>
    public void TryPlace(Vector2Int coord, TileDefinition def, int rot, Transform root) {
        TileInstance instance = new TileInstance();
        instance.tileID = uid;
        instance.def = def;
        instance.origin = coord;
        instance.rotation = rot;
        Vector3 spawnPos = _grid.GridToWorld(coord + _grid.RotationOffset(rot, def.size));
        instance.instance = Object.Instantiate(
            def.prefab, // prefab to create
            spawnPos + Vector3.up, // world position
            Quaternion.Euler(0, rot * -90, 0), // rotation
            root); // parent transform
        instance.instance.transform.DOMove(spawnPos, 0.15f).SetEase(Ease.InCubic);
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
    /// Take an existing tile instance and move it to the location at coord.
    /// </summary>
    public void UpdateInstance(Vector2Int coord, TileInstance instance, int rot) {
        Vector3 spawnPos = _grid.GridToWorld(coord + _grid.RotationOffset(rot, instance.def.size));
        instance.instance.transform.position = spawnPos;
        instance.origin = coord;
        instance.rotation = rot;
        instance.instance.transform.rotation = Quaternion.Euler(0, rot * -90, 0);
        tiles.Add(instance.tileID, instance);

        // For each cell coordinate the tile spans, add its cell data
        for (int x = 0; x < instance.def.size.x; x++) {
            for (int y = 0; y < instance.def.size.y; y++) {
                // Situate the coordinates
                Vector2Int local = new Vector2Int(x, y);
                Vector2Int rotatedLocal = _grid.RotateLocal(local, instance.def.size, rot);
                Vector2Int world = coord + rotatedLocal;

                // Base cell data
                CellData data = new CellData();
                data.tileID = instance.tileID;
                data.type = instance.def.type;
                
                // Extract the potential flags from the local coordinate
                instance.def.GetFlags(local, out data.flags);

                // Add the cell to the dictionary
                cells.Add(world, data);
            }
        }
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

    /// <summary>
    /// Checks if the footprint provided by the input overlaps with any cells that are already established.
    /// </summary>
    public bool IsSpecialCellSatisfied(Vector2Int worldCoord, SpecialCellData sc) {
        // All interfaces must connect to a hallway
        if (sc.overrideType == CellFlags.Interface) {
            if (cells.TryGetValue(worldCoord + Vector2Int.up, out var cd) && cd.type == CellType.Hallway) return true;
            if (cells.TryGetValue(worldCoord + Vector2Int.right, out cd) && cd.type == CellType.Hallway) return true;
            if (cells.TryGetValue(worldCoord + Vector2Int.down, out cd) && cd.type == CellType.Hallway) return true;
            if (cells.TryGetValue(worldCoord + Vector2Int.left, out cd) && cd.type == CellType.Hallway) return true;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Checks the dictionary at the coordinate to see if there is cell data.
    /// </summary>
    public bool HasCellData(Vector2Int coord) {
        return cells.ContainsKey(coord);
    }

    /// <summary>
    /// Extract a tile instance from the input coordinate if it exists.
    /// </summary>
    public TileInstance GetTileInstance(Vector2Int coord) {
        if (cells.TryGetValue(coord, out CellData data)) {
            if (tiles.TryGetValue(data.tileID, out TileInstance inst)) {
                return inst;
            }
        }

        return null;
    }

    /// <summary>
    /// Remove any data and tile instances that are tied to the footprint of cells.
    /// </summary>
    public void RemoveCellFootprint(Vector2Int coord, Vector2Int size) {
        for (int x = coord.x; x < coord.x + size.x; x++) {
            for (int y = coord.y; y < coord.y + size.y; y++) {
                Vector2Int checkCoord = new Vector2Int(x, y);
                if (cells.TryGetValue(checkCoord, out CellData data)) {
                    if (tiles.ContainsKey(data.tileID)) {
                        tiles.Remove(data.tileID);
                    }
                    cells.Remove(checkCoord);
                }
            }
        }
    }
}