using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This object will spawn walls based on placement rules given groups of cells.
/// </summary>
public class WallBuilder : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    [Header("References")]
    public InteractableGrid grid;
    
    [Header("Prefabs")]
    public GameObject interiorWall;
    public GameObject exteriorWall;
    public GameObject door;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    Dictionary<AdjEdgeKey, GameObject> _edges = new();
    //*********************************************************************

    /// <summary>
    /// Given a dictionary of cells, build all edges that can exist.
    /// TODO: This doesn't actually work because the input dictionary is not fully populated
    /// </summary>
    // public void RebuildAll(Dictionary<Vector2Int, CellData> cells) {
    //     HashSet<AdjEdgeKey> neededEdges = new HashSet<AdjEdgeKey>();

    //     // Extract every possible edge that needs to be generated
    //     foreach (var kv in cells) {
    //         Vector2Int coord = kv.Key;
    //         EvaluateAdjCells(coord, coord + Vector2Int.right, neededEdges, cells);
    //         EvaluateAdjCells(coord, coord + Vector2Int.up, neededEdges, cells);
    //     }

    //     // For every edge, evaluate and spawn the relevant prefab
    //     foreach (AdjEdgeKey edge in neededEdges) {
    //         Debug.Log(edge);
    //         WallType type = EvaluateEdge(edge, cells);
    //         ApplyEdge(edge, type);
    //     }

    //     List<AdjEdgeKey> toRemove = null;

    //     foreach (var kv in _edges)
    //     {
    //         if (!neededEdges.Contains(kv.Key))
    //         {
    //             toRemove ??= new List<AdjEdgeKey>();
    //             toRemove.Add(kv.Key);
    //         }
    //     }

    //     if (toRemove == null) return;

    //     for (int i = 0; i < toRemove.Count; i++)
    //         RemoveEdge(toRemove[i]);
    // }

    /// <summary>
    /// Evaluates edges on the perimeter defined by the starting coordinate and the footprint size.
    /// </summary>
    public void RebuildPerimeter(Vector2Int startCoord, Vector2Int footprint, Dictionary<Vector2Int, CellData> cells) {
        // One loop in the X-direction that will evaluate the top and bottom border
        for (int x = 0; x < footprint.x; x++) {
            // Bottom edge
            Vector2Int inTileCoord = new Vector2Int(startCoord.x + x, startCoord.y);
            Vector2Int outTileCoord = new Vector2Int(startCoord.x + x, startCoord.y - 1);
            WallType type = EvaluateEdge(inTileCoord, outTileCoord, cells);
            int flip = 1;
            if (type == WallType.Exterior && cells.ContainsKey(outTileCoord)) { flip = -1; }
            ApplyEdge(
                new AdjEdgeKey(inTileCoord, outTileCoord),
                type,
                grid.GridToWorld(inTileCoord) + (Vector3.right * grid.HalfCell),
                flip == 1 ? Vector3.zero : Vector3.up * 180
            );

            // Top edge
            inTileCoord.y = startCoord.y + footprint.y - 1;
            outTileCoord.y = startCoord.y + footprint.y;
            type = EvaluateEdge(inTileCoord, outTileCoord, cells);
            flip = 1;
            if (type == WallType.Exterior && cells.ContainsKey(outTileCoord)) { flip = -1; }
            ApplyEdge(
                new AdjEdgeKey(inTileCoord, outTileCoord),
                type,
                grid.GridToWorld(outTileCoord) + (Vector3.right * grid.HalfCell),
                flip == -1 ? Vector3.zero : Vector3.up * 180
            );
        }

        // One loop in the Y-direction that will evaluate the left and right border
        for (int y = 0; y < footprint.y; y++) {
            // Left edge
            Vector2Int inTileCoord = new Vector2Int(startCoord.x, startCoord.y + y);
            Vector2Int outTileCoord = new Vector2Int(startCoord.x - 1, startCoord.y + y);
            WallType type = EvaluateEdge(inTileCoord, outTileCoord, cells);
            int flip = 1;
            if (type == WallType.Exterior && cells.ContainsKey(outTileCoord)) { flip = -1; }
            ApplyEdge(
                new AdjEdgeKey(inTileCoord, outTileCoord),
                type,
                grid.GridToWorld(inTileCoord) + (Vector3.forward * grid.HalfCell),
                Vector3.up * 90 * flip
            );

            // Right edge
            inTileCoord.x = startCoord.x + footprint.x - 1;
            outTileCoord.x = startCoord.x + footprint.x;
            type = EvaluateEdge(inTileCoord, outTileCoord, cells);
            flip = 1;
            if (type == WallType.Exterior && cells.ContainsKey(outTileCoord)) { flip = -1; }
            ApplyEdge(
                new AdjEdgeKey(inTileCoord, outTileCoord),
                type,
                grid.GridToWorld(outTileCoord) + (Vector3.forward * grid.HalfCell),
                Vector3.up * -90 * flip
            );
        }
    }

    /// <summary>
    /// For a given edge, decide what needs to spawn.
    /// </summary>
    private WallType EvaluateEdge(Vector2Int inCoord, Vector2Int outCoord, Dictionary<Vector2Int, CellData> cells) {
        bool hasInData = cells.TryGetValue(inCoord, out var inData);
        bool hasOutdata = cells.TryGetValue(outCoord, out var outData);

        if ((hasInData && !hasOutdata) || (!hasInData && hasOutdata)) {
            return WallType.Exterior;
        }

        // Two cells that are within the same tile should not be separated
        if (inData.tileID == outData.tileID)
            return WallType.None;

        // Two hallways will always merge
        if (inData.type == CellType.Hallway && outData.type == CellType.Hallway)
            return WallType.None;

        // Any interface cell next to a hallway will spawn a door
        // TODO: How to prevent two doors from spawning on one cell?
        if ((inData.flags == CellFlags.Interface && outData.type == CellType.Hallway) || 
            (outData.flags == CellFlags.Interface && inData.type == CellType.Hallway))
            return WallType.Door;

        return WallType.Interior;
    }

    /// <summary>
    /// Handles game objects and edge dictionary management.
    /// </summary>
    private void ApplyEdge(AdjEdgeKey edge, WallType type, Vector3 position, Vector3 rotation) {
        // Destroy any existing walls
        if (type == WallType.None) {
            RemoveEdge(edge);
            return;
        }

        // Extract the desired prefab
        GameObject prefab = type switch {
            WallType.Exterior => exteriorWall,
            WallType.Interior => interiorWall,
            WallType.Door => door,
            _ => null
        };

        if (prefab == null) {
            RemoveEdge(edge);
            return;
        }

        // Destroy an already existing segment
        if (_edges.TryGetValue(edge, out GameObject segment)) {
            Destroy(segment);
        }

        // Spawn the prefab and position it properly
        _edges[edge] = Instantiate(prefab, position, Quaternion.Euler(rotation), transform);
    }

    /// <summary>
    /// For a given edge, destroy the object associated and remove the edge.
    /// </summary>
    private void RemoveEdge(AdjEdgeKey edge) {
        if (_edges.TryGetValue(edge, out GameObject obj)) {
            Destroy(obj);
            _edges.Remove(edge);
        }
    }
}

/// <summary>
/// Object that represents the edge between two cells.
/// </summary>
class AdjEdgeKey  {
    public Vector2Int coord1;
    public Vector2Int coord2;

    // coord1 is always less than coord2
    public AdjEdgeKey(Vector2Int c1, Vector2Int c2)
    {
        if (c1.sqrMagnitude < c2.sqrMagnitude)
        {
            coord1 = c1;
            coord2 = c2;
        } else
        {
            coord1 = c2;
            coord2 = c1;   
        }
    }

    public bool Equals(AdjEdgeKey obj)
    {
        return coord1 == obj.coord1 && coord2 == obj.coord2;
    }
    public override bool Equals(object obj)
    {
        return obj is AdjEdgeKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (coord1.GetHashCode() * 397) ^ coord2.GetHashCode();
        }   
    }

    public override string ToString()
    {
        return $"Coord 1: {coord1} | Coord 2: {coord2}";
    }
}