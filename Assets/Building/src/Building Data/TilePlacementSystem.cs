using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NUnit.Framework.Constraints;
using UnityEngine;

public class TilePlacementSystem {
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public Vector2Int EffectiveSize {get; private set;}
    public bool IsValidCoord { get {return _validCoord;}}
    public Dictionary<int, TileInstance> tiles = new(); // All tile instances mapped by their id

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private InteractableGrid _grid;
    private Dictionary<Vector2Int, CellData> cells = new();
    private int uid = 0;
    private TileDefinition _selectedTileDef;
    private int _rot;
    private bool _validCoord;
    private List<int> _anchors = new();
    //---------------------------------------------------------------------

    // Constructor, Getter
    //*********************************************************************
    public TilePlacementSystem(InteractableGrid grid) { _grid = grid; }
    public Dictionary<Vector2Int, CellData> GetCells() { return cells; }
    public TileDefinition SelectedTileDef() { return _selectedTileDef; }
    //*********************************************************************

    /// <summary>
    /// Explicitly define a tile at given coordinate as an anchor tile.
    /// </summary>
    public void AddAnchor(Vector2Int coord) {
        if (!cells.ContainsKey(coord)) { return; }

        _anchors.Add(cells[coord].tileID);
    }

    /// <summary>
    /// Clean up data when setting a new tile. Not every set tile will be at 0 rotation.
    /// </summary>
    public Vector3 SetSelectedTile(TileDefinition def, int rot) { 
        _selectedTileDef = def; 

        if (def != null) {
            return UpdateRotation(rot, def.size);
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Given a rotation and base size, compute the rotated size.
    /// </summary>
    public Vector3 UpdateRotation(int rot, Vector2Int baseSize) {
        Vector2Int offset2D = _grid.RotationOffset(rot, baseSize);
        _rot = rot;

        // When rotating determine the size to use for checks
        EffectiveSize = rot switch {
            0 => baseSize,
            1 => new Vector2Int(baseSize.y, baseSize.x),
            2 => baseSize,
            3 => new Vector2Int(baseSize.y, baseSize.x),
            _ => baseSize
        };

        float w = offset2D.x * _grid.CellSize;
        float h = offset2D.y * _grid.CellSize;

       return new Vector3(w, 0, h);
    }

    /// <summary>
    /// If there is a selected tile, check if it would be valid on the input coordinate.
    /// </summary>
    public bool IsSelectionValid(Vector2Int coord) {
        _validCoord = true;

        if (_selectedTileDef == null) { return false; }

        // Does the tile size overlap with any existing tiles?
        if(CheckOverlap(coord, EffectiveSize)) {
            _validCoord = false;
            return false;
        } 
        
        // Check all the special cells now
        bool allSpecialCellsSatisfied = true;
        foreach (SpecialCellData sc in _selectedTileDef.specialCells) {
            Vector2Int rotatedLocal = _grid.RotateLocal(sc.localCoord, _selectedTileDef.size, _rot);
            Vector2Int adjustedWorld = rotatedLocal + _grid.HoveredCoord;
            allSpecialCellsSatisfied &= IsSpecialCellSatisfied(adjustedWorld, sc);
        }

        if (!allSpecialCellsSatisfied) {
            _validCoord = false;
            return false;
        }

        _validCoord = true;
        return true;
    }

    /// <summary>
    /// Without a tile definition input, the tile placement system will attempt to spawn the currently
    /// selected tile.
    /// </summary>
    public bool TrySpawn(Vector2Int coord, Transform root, float tweenTime, out TileInstance newTile) {
        newTile = null;
        if (_selectedTileDef == null || !_validCoord) { return false;}
        newTile = SpawnTile(coord, _selectedTileDef, _rot, root, tweenTime);
        return true;
    }

    /// <summary>
    /// Spawn whatever is provided to this method without performing any checks. There is a chance to overwrite data if not used properly.
    /// </summary>
    public TileInstance SpawnTile(Vector2Int coord, TileDefinition def, int rot, Transform root, float tweenTime=0.15f) {
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
            root).GetComponent<Tile>(); // parent transform
        instance.instance.transform.DOMove(spawnPos, tweenTime).SetEase(Ease.InCubic);
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
        return instance;
    }

    /// <summary>
    /// Take an existing tile instance and move it to the location at coord.
    /// </summary>
    public void UpdateInstance(Vector2Int coord, TileInstance instance) {
        UpdateInstance(coord, instance, _rot);
    }

    /// <summary>
    /// Take an existing tile instance and move it to the location at coord.
    /// </summary>
    public void UpdateInstance(Vector2Int coord, TileInstance instance, int rot) {
        Vector3 spawnPos = _grid.GridToWorld(coord + _grid.RotationOffset(rot, instance.def.size));
        instance.instance.transform.DOMove(spawnPos, 0.2f);
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
        if (sc.overrideType == CellFlags.Interface || sc.overrideType == CellFlags.ConnectSameOrAnchor) {
            if (cells.TryGetValue(worldCoord + Vector2Int.up, out var cd) && (cd.type == CellType.Hallway || ((cd.flags & CellFlags.Anchor) != 0))) return true;
            if (cells.TryGetValue(worldCoord + Vector2Int.right, out cd) && (cd.type == CellType.Hallway || ((cd.flags & CellFlags.Anchor) != 0))) return true;
            if (cells.TryGetValue(worldCoord + Vector2Int.down, out cd) && (cd.type == CellType.Hallway || ((cd.flags & CellFlags.Anchor) != 0))) return true;
            if (cells.TryGetValue(worldCoord + Vector2Int.left, out cd) && (cd.type == CellType.Hallway || ((cd.flags & CellFlags.Anchor) != 0))) return true;
            return false;

        // This special cell must be adjacent to something, no matter what it is
        } 
        // else if (sc.overrideType == CellFlags.ConnectSameOrAnchor) {
        //     if (cells.ContainsKey(worldCoord + Vector2Int.up)) return true;
        //     if (cells.ContainsKey(worldCoord + Vector2Int.right)) return true;
        //     if (cells.ContainsKey(worldCoord + Vector2Int.down)) return true;
        //     if (cells.ContainsKey(worldCoord + Vector2Int.left)) return true;
        //     return false;
        // }

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

    /// <summary>
    /// Evaluate all of the tiles present and return if there are islands or not.
    /// </summary>
    public bool IdentifyIslands(Color notIslandColor, Color islandColor) {
        if (_anchors.Count == 0) { return false; }

        // Create a temporary hash set instances to track which ones have been visited
        HashSet<int> notVisited = new HashSet<int>(tiles.Keys);

        // Search all adjacencies around the tile
        foreach (int anchor in _anchors) {
            MarkAdjacencies(anchor, ref notVisited);
        }

        // Set the color of the islands accordingly
        foreach (int tileID in tiles.Keys) {
            tiles[tileID].instance.SetOverlayColor(notVisited.Contains(tileID) ? islandColor : notIslandColor);
        }

        // There are islands if some tiles are left unvisited
        _validCoord &= notVisited.Count == 0;
        return notVisited.Count != 0;
    }

    /// <summary>
    /// Recursively visit all tiles that are connected to the provided tile and remove them from the notVisited list.
    /// </summary>
    private bool MarkAdjacencies(int tileID, ref HashSet<int> notVisited) {
        // If the provided tile has not been visited, then remove it from the HashSet
        if (notVisited.Contains(tileID)) { notVisited.Remove(tileID); }

        // These two vectors define the tiles footprint
        // NOTE: We only care about the adjacencies of tiles that haven't already been visited
        Vector2Int coord = tiles[tileID].origin;
        Vector2Int size = tiles[tileID].def.size;

        for (int x = 0; x < size.x; x++) {
            // Bottom edge
            Vector2Int outTileCoord = new Vector2Int(coord.x + x, coord.y - 1);
            if (cells.ContainsKey(outTileCoord) && notVisited.Contains(cells[outTileCoord].tileID)) {
                MarkAdjacencies(cells[outTileCoord].tileID, ref notVisited);
            }

            // Top edge
            outTileCoord.y = coord.y + size.y;
            if (cells.ContainsKey(outTileCoord) && notVisited.Contains(cells[outTileCoord].tileID)) {
                MarkAdjacencies(cells[outTileCoord].tileID, ref notVisited);
            }
        }

        // One loop in the Y-direction that will evaluate the left and right border
        for (int y = 0; y < size.y; y++) {
            // Left edge
            Vector2Int outTileCoord = new Vector2Int(coord.x - 1, coord.y + y);
            if (cells.ContainsKey(outTileCoord) && notVisited.Contains(cells[outTileCoord].tileID)) {
                MarkAdjacencies(cells[outTileCoord].tileID, ref notVisited);
            }

            // Right edge
            outTileCoord.x = coord.x + size.x;
            if (cells.ContainsKey(outTileCoord) && notVisited.Contains(cells[outTileCoord].tileID)) {
                MarkAdjacencies(cells[outTileCoord].tileID, ref notVisited);
            }
        }

        return false;
    }
}