using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// The interactable grid object is in charge of managing the grid geometry along with player interaction. This grid is assumed to lie on the XZ.
/// </summary>
public class InteractableGrid : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public event Action<Vector2Int> OnHoveredCellChange;
    public Vector2Int HoveredCoord {get { return _lastHoveredCoord; }}
    public float CellSize {get { return _cellSize; }}
    public float HalfCell {get { return _halfCellSize; }}

    [Header("Params")]
    [Range(0,1f)] public float fadeTime = 0.2f;
    [Range(1, 10)] public int markerNudgeDepth = 3;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private float _cellSize; // NOTE: extracted from the grid shader
    private float _halfCellSize;
    private MeshRenderer _gridRenderer;
    private Vector2Int _lastHoveredCoord;
    private bool _gridActive = false;
    private LayerMask _gridMask;
    private Vector2Int footprint = Vector2Int.one;
    private HashSet<Vector2Int> availableCells = new();


    //*********************************************************************

    /// <summary>
    /// Unity start method.
    /// </summary>
    private void Start() {
        _gridRenderer = GetComponentInChildren<MeshRenderer>();
        _cellSize = _gridRenderer.material.GetFloat("_CellSize");
        _halfCellSize = _cellSize * 0.5f;
        _gridMask = LayerMask.GetMask("BuildingGrid");

        int xSize = (int)(transform.localScale.x / _cellSize);
        int zSize = (int)(transform.localScale.z / _cellSize);
        int xPos = (int)(transform.position.x / _cellSize);
        int zPos = (int)(transform.position.z / _cellSize);

        for (int x = xPos; x < xSize; x++)
        {
            for (int z = zPos; z < zSize; z++)
            {
                availableCells.Add(new Vector2Int(x, z));
            }
        }
    }

    /// <summary>
    /// Unity update method.
    /// </summary>
    private void Update() {
        if (!_gridActive || UIManager.Instance.IsPointerOverUI()) { return; }

        // Cast a ray from the mouse to the grid.
        // Fire an event if the hovered cell changes
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 50, _gridMask)) {
            Vector2Int hitCoord = WorldToGrid(hit.point);
            if (hitCoord != _lastHoveredCoord && FootprintOnGrid(hitCoord)) {
                _lastHoveredCoord = hitCoord;
                OnHoveredCellChange?.Invoke(hitCoord);
            }
        }
    }

    /// <summary>
    /// Update the internal marker footprint to ensure the selection always is on grid cells. Will adjust the position on update.
    /// </summary>
    public bool UpdateFootprint(Vector2Int newMarkerSize) {
        footprint = newMarkerSize;

        // If the new footprint would be off the grid, then nudge the tile until it is on the grid.
        // Since tiles always expand in the +X/+Z direction, we need to nudge backwards
        if (!FootprintOnGrid(_lastHoveredCoord)) {
            for (int x = _lastHoveredCoord.x; x >= _lastHoveredCoord.x - markerNudgeDepth; x--)
            {
                for (int y = _lastHoveredCoord.y; y >= _lastHoveredCoord.y - markerNudgeDepth; y--)
                {
                    Vector2Int newCoord = new Vector2Int(x, y);
                    if (FootprintOnGrid(newCoord))
                    {
                        _lastHoveredCoord = newCoord;
                        return true;
                    }
                }
            }

            Debug.LogWarning("Interactable Grid: Cannot compute a new position for the marker footprint that is on valid cells.");
            return false;
        }

        return false;
    }

    /// <summary>
    /// Tween the alpha value of the grid material.
    /// </summary>
    public void ToggleGrid(bool target) {
        _gridActive = target;
        _gridRenderer.material.DOFloat(target ? 1f : 0f, "_Alpha", fadeTime);
    }

    /// <summary>
    /// Convert a world position to a 2D grid coordinate.
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 pos) {
        return new Vector2Int(
            (int)Math.Floor(pos.x / _cellSize),
            (int)Math.Floor(pos.z / _cellSize)
        );
    }

    /// <summary>
    /// Given a grid coordinate, return a world space position.
    /// </summary>
    public Vector3 GridToWorld(Vector2Int coord)
    {
        return new Vector3(
            coord.x * _cellSize,
            0,
            coord.y * _cellSize
        );
    }

    /// <summary>
    /// Compute a rotated local cell coordinate based on a an original coordinate, size, and rotation.
    /// </summary>
    public Vector2Int RotateLocal(Vector2Int local, Vector2Int size, int rot) {
        int w = size.x;
        int h = size.y;
        rot = ((rot % 4) + 4) % 4;

        return rot switch
        {
            0 => new Vector2Int(local.x, local.y),
            1 => new Vector2Int(h - 1 - local.y, local.x),
            2 => new Vector2Int(w - 1 - local.x, h - 1 - local.y),
            3 => new Vector2Int(local.y, w - 1 - local.x),
            _ => local
        };
    }

    /// <summary>
    /// Give a rotation and size, return an offset from the bottom left corner that keeps the object bottom-left-aligned.
    /// </summary>
    public Vector2Int RotationOffset(int rot, Vector2Int size) {
        return rot switch
        {
            0 => Vector2Int.zero,
            1 => new Vector2Int(size.y, 0),   // 90 CW: z += width
            2 => new Vector2Int(size.x, size.y),    // 180:   x += width, z += height
            3 => new Vector2Int(0, size.x),   // 270 CW: x += height
            _ => Vector2Int.zero
        };
    }

    /// <summary>
    /// Evaluates the stored footprint at the given coord to determine if it will be on the grid still.
    /// </summary>
    private bool FootprintOnGrid(Vector2Int coord) {
        // If any of the cells don't exist in the available ones, then this is an invalid position 
        for (int x = coord.x; x < coord.x + footprint.x; x++) {
            for (int y = coord.y; y < coord.y + footprint.y; y++) {
                if (!availableCells.Contains(new Vector2Int(x, y))) {
                    return false;
                }
            }
        }

        return true;
    }
}
