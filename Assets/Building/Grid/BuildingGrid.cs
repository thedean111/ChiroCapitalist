using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class BuildingGrid : MonoBehaviour
{
    //--------------------------------
    public float fadeTime = 0.2f;
    //--------------------------------
    //=================================
    private Dictionary<Vector2Int, int> placeableTiles = new Dictionary<Vector2Int, int>();

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
        float cellSize = gridMat.material.GetFloat("_CellSize");
        currentCoord = new Vector2Int(
            (int)Math.Floor(pos.x / cellSize),
            (int)Math.Floor(pos.z / cellSize)
        );
        return new Vector3(
            currentCoord.x  * cellSize,
            0.05f,
            currentCoord.y * cellSize
        );
    }

    /// <summary>
    /// Creates the default state of the grid under the assumption it is rectangular and extends in the +X and +Z directions.
    /// </summary>
    public void Initialize()
    {
        float cellSize = gridMat.material.GetFloat("_CellSize");
        int xSize = (int)(transform.localScale.x / cellSize);
        int zSize = (int)(transform.localScale.z / cellSize);
        int xPos = (int)(transform.position.x / cellSize);
        int zPos = (int)(transform.position.z / cellSize);

        for (int x = xPos; x < xSize; x++)
        {
            for (int z = zPos; z < zSize; z++)
            {
                Debug.Log("Tile: " + x + ", " + z);
                placeableTiles.Add(new Vector2Int(x, z), 0);
            }
        }
    }

    /// <summary>
    /// Given a coordinate and X-Z size, determine if the tile will fit on the grid
    /// </summary>
    /// <param name="coord"> Starting coordinate of the tile to be checked. </param>
    /// <param name="size"> Expanse of the tile to be checked. </param>
    public bool IsValidPlacement(Vector2Int coord, Vector2Int size)
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
}
