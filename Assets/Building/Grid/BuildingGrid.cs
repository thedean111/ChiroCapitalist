using UnityEngine;
using DG.Tweening;
using System;

public class BuildingGrid : MonoBehaviour
{
    //---
    public float fadeTime = 0.2f;
    //---

    private MeshRenderer gridMat;

    void OnEnable()
    {
        gridMat = GetComponent<MeshRenderer>();
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
        float halfCellSize = cellSize * 0.5f;
        currentCoord = new Vector2Int(
            (int)(Math.Floor(pos.x / cellSize) * cellSize),
            (int)(Math.Floor(pos.z / cellSize) * cellSize)
        );
        return new Vector3(
            currentCoord.x + halfCellSize,
            0.05f,
            currentCoord.y + halfCellSize
        );
        
    }
}
