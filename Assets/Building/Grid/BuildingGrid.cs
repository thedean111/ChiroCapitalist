using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingGrid : MonoBehaviour
{
    //--------------------------------
    [Header("References")]
    public Transform wallRoot;
    public GameObject wall_intExt;
    public GameObject wall_intInt;
    public GameObject door;
    
    [Header("Params")]
    public float fadeTime = 0.2f;
    public int coordCheckDepthCount = 10; // how many tiles in either axis to check for valid placement
    //--------------------------------
    //=================================
    private Dictionary<Vector2Int, CellType> placeableTiles = new Dictionary<Vector2Int, CellType>();
    private Dictionary<AdjEdgeKey, Transform> walls = new Dictionary<AdjEdgeKey, Transform>();
    private float cellSize;

    enum WallType
    {
        NONE,
        INTERIOR_EXTERIOR,
        INTERIOR_INTERIOR,
        INTERFACE
    }

    //=================================

    private MeshRenderer gridMat;

    void OnEnable()
    {
        gridMat = GetComponentInChildren<MeshRenderer>();
    }

    /// <summary>
    /// Fade the grid in or out.
    /// </summary>
    /// <param name="target">Boolean value that determines the final state of the grid.</param>
    public void FadeGrid(bool target)
    {
        float t = target ? 1f : 0f;
        gridMat.material.DOFloat(t, "_Alpha", fadeTime);
    }

    /// <summary>
    /// Computes the center of the nearest grid cell given a world coordinate.
    /// </summary>
    /// <param name="pos">Some coordinate in world space.</param>
    public Vector3 ClampPosition(Vector3 pos, out Vector2Int currentCoord)
    {
        currentCoord = WorldToGrid(pos);
        return new Vector3(
            currentCoord.x  * cellSize,
            0.05f,
            currentCoord.y * cellSize
        );
    }

    /// <summary>
    /// Given a grid coordinate, return a world space position.
    /// </summary>
    /// <param name="coord">Coordinate to transform.</param>
    public Vector3 GridToWorld(Vector2Int coord)
    {
        return new Vector3(
            coord.x * cellSize,
            0,
            coord.y * cellSize
        );
    }

    /// <summary>
    /// Given a coordinate in world space, return the X-Z grid coordinate it is in
    /// </summary>
    /// <param name="pos">Position to transform.</param>
    public Vector2Int WorldToGrid(Vector3 pos)
    {
        return new Vector2Int(
            (int)Math.Floor(pos.x / cellSize),
            (int)Math.Floor(pos.z / cellSize)
        );
    }

    /// <summary>
    /// Creates the default state of the grid under the assumption it is rectangular and extends in the +X and +Z directions.
    /// </summary>
    public void Initialize()
    {
        cellSize = gridMat.material.GetFloat("_CellSize");
        int xSize = (int)(transform.localScale.x / cellSize);
        int zSize = (int)(transform.localScale.z / cellSize);
        int xPos = (int)(transform.position.x / cellSize);
        int zPos = (int)(transform.position.z / cellSize);

        for (int x = xPos; x < xSize; x++)
        {
            for (int z = zPos; z < zSize; z++)
            {
                placeableTiles.Add(new Vector2Int(x, z), CellType.NONE);
            }
        }
    }

    /// <summary>
    /// Given a coordinate and X-Z size, determine if the tile will fit on the grid
    /// </summary>
    /// <param name="coord"> Starting coordinate of the tile to be checked. </param>
    /// <param name="size"> Expanse of the tile to be checked. </param>
    public bool FitsOnGrid(Vector2Int coord, Vector2Int size)
    {
        for (int x = coord.x; x < coord.x + size.x; x++)
        {
            for (int y = coord.y; y < coord.y + size.y; y++)
            {
                if (!placeableTiles.ContainsKey(new Vector2Int(x, y)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Given a starting point and size, walk backwards until a valid position on the grid is found.
    /// </summary>
    /// <param name="coord"> Starting coordinate for the search. </param>
    /// <param name="size"> Size of the tile that needs to fit on the grid </param>
    public Vector2Int FindValidPlacementCoord(Vector2Int coord, Vector2Int size)
    {
        for (int x = coord.x; x >= coord.x - coordCheckDepthCount; x--)
        {
            for (int y = coord.y; y >= coord.y - coordCheckDepthCount; y--)
            {
                Vector2Int newCoord = new Vector2Int(x, y);
                if (FitsOnGrid(newCoord, size))
                {
                    return newCoord;
                }
            }
        }
        return Vector2Int.down;
    }

    /// <summary>
    /// Given a coordinate and a size, determine if the tile overlaps with tiles already on the grid.
    /// </summary>
    /// <param name="coord"> Starting coordinate for the search. </param>
    /// <param name="size"> Size of the tile that needs to fit on the grid. </param>
    /// <returns> False- if there is a conflict. True- if there isn't. </returns>
    public bool CanPlaceTile(Vector2Int coord, Vector2Int size, List<SpecialCell> specialCells)
    {
        // Checks for overlaps
        for (int x = coord.x; x < coord.x + size.x; x++) {
            for (int y = coord.y; y < coord.y + size.y; y++) {
                Vector2Int testCoord = new Vector2Int(x, y);
                if (placeableTiles[testCoord] != CellType.NONE) {
                    return false;
                }
            }
        }

        // Special cells have additional rules that affect placement validity
        bool allSpecialCellsSatisfied = true;
        foreach (SpecialCell sc in specialCells) {

            bool cellSatisfied = false;
            sc.effectiveCoord = WorldToGrid(sc.transform.position);
            Debug.Log("Special Cell == World Pos: " + sc.transform.position + " || Coord: " + sc.effectiveCoord);

            cellSatisfied |= sc.CheckCell(placeableTiles.GetValueOrDefault(sc.effectiveCoord + Vector2Int.up, CellType.NONE));
            cellSatisfied |= sc.CheckCell(placeableTiles.GetValueOrDefault(sc.effectiveCoord + Vector2Int.right, CellType.NONE));
            cellSatisfied |= sc.CheckCell(placeableTiles.GetValueOrDefault(sc.effectiveCoord + Vector2Int.down, CellType.NONE));
            cellSatisfied |= sc.CheckCell(placeableTiles.GetValueOrDefault(sc.effectiveCoord + Vector2Int.left, CellType.NONE));

            allSpecialCellsSatisfied &= cellSatisfied;
        }

        return allSpecialCellsSatisfied;
    }

    /// <summary>
    /// Provide some grid coordinates knowledge of the tile that is placed on them.
    /// </summary>
    /// <param name="coord"> Starting coordinate for the search. </param>
    /// <param name="tile"> Contains placement information for a tile. </param>
    public void AddTileToGrid(Vector2Int coord, Vector2Int size, CellType defaultType, List<SpecialCell> specialCells)
    {
        // For every space the tile takes up, map that coord to the tile
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int testCoord = new Vector2Int(x + coord.x, y + coord.y);
                placeableTiles[testCoord] = defaultType;
            }
        }

        // Ensure unique cells are handled
        // sc.effectiveCoord is computed before the user even clicks to place the tile
        foreach (SpecialCell sc in specialCells) {
            placeableTiles[sc.effectiveCoord] = sc.type;
            
        }

        // Update walls
        UpdateWalls(coord, size);
    }

    /// <summary>
    /// Check the cells around a full tile and update the local walls accordingly.
    /// </summary>
    /// <param name="coord"> Starting coordinate for the search. </param>
    /// <param name="size"> Size of the tile that needs to fit on the grid. </param>
    public void UpdateWalls(Vector2Int coord, Vector2Int size)
    {
        Vector2Int checkCoord = new Vector2Int();

        // Top / Bottom
        for (int x = 0; x < size.x; x++)
        {
            // Checks the bottom (-Z) adjacent cells
            checkCoord.x = coord.x + x;
            checkCoord.y = coord.y - 1;
            AdjEdgeKey edge = new AdjEdgeKey(new Vector2Int(checkCoord.x, coord.y), checkCoord);
            SpawnWall(edge, new Vector3(
                checkCoord.x * cellSize,
                0,
                coord.y * cellSize
            ), 0);

            // Checks the top (+Z) adjacent cells
            checkCoord.y = coord.y + size.y;
            edge = new AdjEdgeKey(new Vector2Int(checkCoord.x, coord.y + size.y - 1), checkCoord);
            SpawnWall(edge, new Vector3(
                (checkCoord.x + 1) * cellSize,
                0,
                (coord.y + size.y) * cellSize
            ), 180);
        }

        // Left / Right
        for (int y = 0; y < size.y; y++)
        {
            checkCoord.y = coord.y + y;
            checkCoord.x = coord.x - 1;
            AdjEdgeKey edge = new AdjEdgeKey(new Vector2Int(coord.x, checkCoord.y), checkCoord);
            SpawnWall(edge, new Vector3(
                coord.x * cellSize,
                0,
                (checkCoord.y + 1) * cellSize
            ), 90);

            checkCoord.x = coord.x + size.x;
            edge = new AdjEdgeKey(new Vector2Int(coord.x + size.x - 1, checkCoord.y), checkCoord);
            SpawnWall(edge, new Vector3(
                (coord.x + size.x) * cellSize,
                0,
                checkCoord.y * cellSize
            ), 270);

        }
    }

    /// <summary>
    /// Given two coordinates, evaluate the tiles at those coordinates to determine if a border should exist between the tiles.
    /// </summary>
    private WallType EvaluateBorder(AdjEdgeKey edge)
    {
        // If coord2 is not in the grid, then coord1 is a border tile. Place a wall on the edge between coord1 and coord2
        if (!placeableTiles.ContainsKey(edge.coord1) || !placeableTiles.ContainsKey(edge.coord2)) {
            return WallType.INTERIOR_EXTERIOR;

        // If one of the cells is empty then there needs to be a wall to enclose the space
        } else if (placeableTiles[edge.coord1] == CellType.NONE || placeableTiles[edge.coord2] == CellType.NONE) {
            return WallType.INTERIOR_EXTERIOR;

        // Cells may only interface with hallways
        } else if ((placeableTiles[edge.coord1] == CellType.INTERFACE && placeableTiles[edge.coord2] == CellType.HALLWAY) || 
                    (placeableTiles[edge.coord2] == CellType.INTERFACE && placeableTiles[edge.coord1] == CellType.HALLWAY)) {
            return WallType.INTERFACE;

        // Two hallways should always merge together
        } else if (placeableTiles[edge.coord1] == CellType.HALLWAY && placeableTiles[edge.coord2] == CellType.HALLWAY) {
            return WallType.NONE;

        // All other cases are two neighboring cells that should be divided?
        } else {
            return WallType.INTERIOR_INTERIOR;
        }


    }

    /// <summary>
    /// Helper that actually instantiates a wall given a WallType.
    /// </summary>
    private void SpawnWall(AdjEdgeKey edge, Vector3 position, float yRot)
    {
        WallType type = EvaluateBorder(edge);
        GameObject spawnObj;
        switch (type) {
            case WallType.NONE:
                Debug.Log("remove wall");
                if (walls.ContainsKey(edge)) {
                    Debug.Log("Destroying wall");
                    Destroy(walls[edge].gameObject);
                    walls.Remove(edge);
                }
                return;

            case WallType.INTERIOR_EXTERIOR:
                spawnObj = wall_intExt;
                break;

            case WallType.INTERIOR_INTERIOR:
                spawnObj = wall_intInt;
                break;

            case WallType.INTERFACE:
                spawnObj = door;
                break;

            default:
                Debug.LogWarning("Cannot resolve wall handling!");
                return;
        }

        if (walls.ContainsKey(edge)) {
            Destroy(walls[edge].gameObject);
        }
        walls[edge] = Instantiate(spawnObj, position, Quaternion.Euler(0, yRot, 0), wallRoot).transform;

    }

    class AdjEdgeKey
    {
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
    }
}