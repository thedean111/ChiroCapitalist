using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

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

    [Header("Params")]
    [Range(0,1f)] public float fadeTime = 0.2f;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private float _cellSize; // NOTE: extracted from the grid shader
    private MeshRenderer _gridRenderer;
    private Vector2Int _lastHoveredCoord;
    private bool _gridActive = false;
    private LayerMask _gridMask;

    //*********************************************************************

    /// <summary>
    /// Unity start method.
    /// </summary>
    private void Start() {
        _gridRenderer = GetComponentInChildren<MeshRenderer>();
        _cellSize = _gridRenderer.material.GetFloat("_CellSize");
        _gridMask = LayerMask.GetMask("BuildingGrid");
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
            if (hitCoord != _lastHoveredCoord) {
                _lastHoveredCoord = hitCoord;
                OnHoveredCellChange?.Invoke(hitCoord);
            }
        }
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
    /// Compute the rotated world coordinate of a cell from its local space
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
}
