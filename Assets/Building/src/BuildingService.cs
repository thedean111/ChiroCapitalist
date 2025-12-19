using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

/// <summary>
/// Building service for the game. Helps route functionality between different objects and performs basic validation.
/// This is a singleton because there is one service.
/// </summary>
public class BuildingService : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public static BuildingService Instance { get; private set; }

    public bool holdingTile {get; private set;} // In edit mode and currently holding a tile

    [Header("Game Object References")]
    public InteractableGrid grid;
    public GridPlacementMarker gridMarker;
    public TilePreviewer previewer;
    public WallBuilder wallBuilder;

    [Header("Audio Effects")]
    public SoundDefinition placementSound;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private bool _validCoord;
    private bool _active = false;
    private TileDefinition _selectedTileDefinition = null;
    private TilePlacementSystem _placement;
    private int _rot;
    private Vector2Int _effectiveSize;
    private bool _editing;
    
    //*********************************************************************

    /// <summary>
    /// Unity awake method.
    /// </summary>
    private void Awake() {
        if (Instance == null) { Instance = this; }
        _placement = new TilePlacementSystem(grid);
        _rot = 0;
    }

    /// <summary>
    /// Unity start method.
    /// </summary>
    private void Start() {
        wallBuilder.grid = grid;

        ToggleEdit(true);

        if (grid != null) {
            if (gridMarker != null) {
                grid.OnHoveredCellChange += UpdateGridMarker;
            } else {
                Debug.LogWarning("Building Service: grid marker is null.");
            }

            grid.OnHoveredCellChange += CheckSelectionValidity;

        } else {
            Debug.LogWarning("Building Service: interactable grid is null.");
        }
    }

    /// <summary>
    /// Performs any logic necessary for toggling the service on and off.
    /// </summary>
    public void Toggle() {
        _active = !_active;
        if (_active) Activate();
        else Deactivate();
    }

    /// <summary>
    /// Performs any logic necessary for toggling edit mode on and off.
    /// </summary>
    public void ToggleEdit(bool status) {
        _editing = status;

        if (_editing) {
            previewer.Hide();
            _selectedTileDefinition = null;
            gridMarker.Resize(Vector2Int.one);
            UIManager.Instance.ActivateEditButton(true);
        } else {
            UIManager.Instance.ActivateEditButton(false);
        }
    }

    /// <summary>
    /// Logic to execute when the player selects an unlocked tile from the UI.
    /// </summary>
    public void SelectTile(TileDefinition selection) {
        if (_editing) ToggleEdit(false);
        _selectedTileDefinition = selection;
        _effectiveSize = _selectedTileDefinition.size;
        gridMarker.Resize(selection.size);

        if (grid.UpdateFootprint(selection.size)) {
            UpdateGridMarker(grid.HoveredCoord);
        }

        previewer.SetPrefab(selection.prefab);
        _rot = 0;
        CheckSelectionValidity(grid.HoveredCoord);
    }

    /// <summary>
    /// This is called when the player executes the input that interacts with the current cell. This does nothing while not in build mode.
    /// </summary>
    public void InteractCell() {
        if (!_active || UIManager.Instance.IsPointerOverUI()) { return; }

        if (_selectedTileDefinition != null && _validCoord) {
            _placement.TryPlace(grid.HoveredCoord, _selectedTileDefinition, _rot, transform);
            SoundManager.Instance.PlayBuildEffect(placementSound);
            wallBuilder.RebuildPerimeter(grid.HoveredCoord, _effectiveSize, _placement.GetCells());
            CheckSelectionValidity(grid.HoveredCoord);
        }

        if (_editing && _placement.HasCellData(grid.HoveredCoord)) {
            UIManager.Instance.ToggleEditPopup(true);
            CameraController.Instance.ForceCameraPosition(grid.GridToWorld(grid.HoveredCoord));
        }
    }

    /// <summary>
    /// Will attempt to stop the edit. Depending
    /// </summary>
    public void TryStopEdit() {

    }

    /// <summary>
    /// If a tile is selected, increment its rotation.
    /// </summary>
    public void RotateSelection() {
        if (_selectedTileDefinition == null) { return; }

        _rot = (_rot + 1) % 4;
        Vector2Int offset2D = grid.RotationOffset(_rot, _selectedTileDefinition.size);

        // When rotating determine the size to use for checks
        _effectiveSize = _rot switch {
            0 => _selectedTileDefinition.size,
            1 => new Vector2Int(_selectedTileDefinition.size.y, _selectedTileDefinition.size.x),
            2 => _selectedTileDefinition.size,
            3 => new Vector2Int(_selectedTileDefinition.size.y, _selectedTileDefinition.size.x),
            _ => _selectedTileDefinition.size
        };
        gridMarker.Resize(_effectiveSize);

        float w = offset2D.x * grid.CellSize;
        float h = offset2D.y * grid.CellSize;

        Vector3 offset = new Vector3(w, 0, h);

        if (grid.UpdateFootprint(_effectiveSize)) {
            UpdateGridMarker(grid.HoveredCoord);
        }

        previewer.SetLocalPositionRotation(offset, Vector3.up * _rot * -90);
        CheckSelectionValidity(grid.HoveredCoord);
    }

    /// <summary>
    /// Updates the grid marker with the new coordinate.
    /// </summary>
    private void UpdateGridMarker(Vector2Int newCoord) {
        gridMarker.Move(grid.GridToWorld(newCoord));
    }

    /// <summary>
    /// Turns the service on.
    /// </summary>
    private void Activate() {
        grid.ToggleGrid(true);
        UIManager.Instance.ToggleBuildUI(true);
    }

    /// <summary>
    /// Turns the service off.
    /// </summary>
    private void Deactivate() {
        grid.ToggleGrid(false);
        gridMarker.Reset();
        UIManager.Instance.ToggleBuildUI(false);
        if (_selectedTileDefinition != null) {
            previewer.Hide();
            _selectedTileDefinition = null;
        }
    }

    /// <summary>
    /// If there is a selected tile, check if it would be valid on the input coordinate.
    /// </summary>
    private void CheckSelectionValidity(Vector2Int coord) {
        if (_selectedTileDefinition == null) { return; }

        _validCoord = true;


        if(_placement.CheckOverlap(coord, _effectiveSize)) {
            _validCoord = false;
            previewer.SetValid(_validCoord);
            return;
        } 
        
        // Check all the special cells now
        bool allSpecialCellsSatisfied = true;
        foreach (SpecialCellData sc in _selectedTileDefinition.specialCells) {
            Vector2Int rotatedLocal = grid.RotateLocal(sc.localCoord, _selectedTileDefinition.size, _rot);
            Vector2Int adjustedWorld = rotatedLocal + grid.HoveredCoord;
            allSpecialCellsSatisfied &= _placement.IsSpecialCellSatisfied(adjustedWorld, sc);
        }

        if (!allSpecialCellsSatisfied) {
            _validCoord = false;
            previewer.SetValid(_validCoord);
            return;
        }

        previewer.SetValid(_validCoord);
    }
}
