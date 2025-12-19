using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.Interactions;

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
    public TilePreviewer movePreviewer;
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
    private Vector2Int _focusedCell;
    private TileInstance _movingInstance;
    
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
            grid.UpdateFootprint(Vector2Int.one);

            UIManager.Instance.ActivateEditButton(true);
            UIManager.Instance.ClearTileListSelection();
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

        // This is for placing a new tile
        if (!holdingTile && _selectedTileDefinition != null && _validCoord) {
            _placement.TryPlace(grid.HoveredCoord, _selectedTileDefinition, _rot, transform);
            SoundManager.Instance.PlayBuildEffect(placementSound);
            wallBuilder.RebuildPerimeter(grid.HoveredCoord, _effectiveSize, _placement.GetCells());
            CheckSelectionValidity(grid.HoveredCoord);

        // If the player picked up a tile this flag turns to true, attempt to place it at the mouse position.
        } else if (holdingTile) {
            ConfirmEdit();
        
        // The player is interacting with an arbitrary cell
        } else  if (_editing && _placement.HasCellData(grid.HoveredCoord)) {
            _focusedCell = grid.HoveredCoord;
            UIManager.Instance.ToggleEditPopup(true);
            CameraController.Instance.ForceCameraPosition(grid.GridToWorld(grid.HoveredCoord));
        }

    }

    /// <summary>
    /// Attempt to delete the focused tile.
    /// TODO: Check for anchor chain breakage before confirming a deletion.
    /// </summary>
    public void DeleteFocusedTile() {
        if (!_editing) { return; }

        TileInstance tile = _placement.GetTileInstance(_focusedCell);
        if (tile == null) {
            Debug.LogWarning("BuildingService: Attempting to delete a tile that doesn't exist!");
            return;
        }

        _placement.RemoveCellFootprint(tile.origin, tile.def.size);
        wallBuilder.RebuildPerimeter(tile.origin, tile.def.size, _placement.GetCells());

        // TODO: If this tile is tied to other game objects (Office-doctor-patient), reallocate resources properly
        Destroy(tile.instance);

        UIManager.Instance.ToggleEditPopup(false);
    }

    /// <summary>
    /// Pick up the tile that is currently focused.
    /// </summary>
    public void PickupTile() {
        if (!_editing) { return; }

        // Get the tile definition at the selected cell
        TileInstance tile = _placement.GetTileInstance(_focusedCell);
        if (tile == null) {
            Debug.LogWarning("BuildingService: Attempting to pick up a tile that doesn't exist!");
            return;
        }
        UIManager.Instance.ToggleEditPopup(false);

        // Holding tile in the context of editing, but treat the movement like a normal placement
        holdingTile = true;
        _selectedTileDefinition = tile.def;
        _movingInstance = tile;

        // Use the placement previewer to visualize where the tile will be moved to
        previewer.SetPrefab(tile.def.prefab);
        Vector3 localPos = UpdateEffectiveSize(_rot, tile.def.size);
        _rot = tile.rotation;
        previewer.SetLocalPositionRotation(localPos, Vector3.up * _rot * -90);
        grid.UpdateFootprint(_selectedTileDefinition.size);
        gridMarker.Resize(_selectedTileDefinition.size);

        // Use the move previewer to show the original tile location
        movePreviewer.SetPrefab(tile.def.prefab);
        movePreviewer.transform.position = tile.instance.transform.position;
        movePreviewer.transform.rotation = tile.instance.transform.rotation;
        movePreviewer.SetTint(TilePreviewState.Pending_Move);

        // Hide the instance of the tile
        tile.instance.SetActive(false);

        // Capture the initial state of the tile and then remove it from the grid
        _placement.RemoveCellFootprint(tile.origin, tile.def.size);
        wallBuilder.RebuildPerimeter(tile.origin, tile.def.size, _placement.GetCells());
    }

    /// <summary>
    /// If a tile is currently being moved, cancel the move and restore the original state.
    /// </summary>
    public void CancelEdit() {
        if (!holdingTile) { return; }

        // Add the original instance back to the placement dictionary data
        _placement.UpdateInstance(_movingInstance.origin, _movingInstance, _movingInstance.rotation);

        // Reset the original instance
        // TODO: Play some undo sound
        _movingInstance.instance.SetActive(true);
        wallBuilder.RebuildPerimeter(_movingInstance.origin, _movingInstance.def.size, _placement.GetCells());

        // Hide previews        
        movePreviewer.Hide();
        previewer.Hide();

        // Set internal data
        holdingTile = false;
        _selectedTileDefinition = null;
        _movingInstance = null;
        gridMarker.Resize(Vector2Int.one);
    }

    /// <summary>
    /// If a tile is currently being moved, place it down on the current grid coordinate--assuming its a valid position.
    /// </summary>
    public void ConfirmEdit() {
        if (_editing && _selectedTileDefinition != null) {
            if (!_validCoord) {
                Debug.LogWarning("Cannot move tile to an invalid coordinate!");
            }

            // Update the existing instance and build new walls
            // TODO: Play a special sound for moving instances
            _placement.UpdateInstance(grid.HoveredCoord, _movingInstance, _rot);
            _movingInstance.instance.SetActive(true);
            SoundManager.Instance.PlayBuildEffect(placementSound);
            wallBuilder.RebuildPerimeter(grid.HoveredCoord, _effectiveSize, _placement.GetCells());
            
            // Hide previews        
            movePreviewer.Hide();
            previewer.Hide();

            // Update flags
            _movingInstance = null;
            _selectedTileDefinition = null;
            holdingTile = false;
            gridMarker.Resize(Vector2Int.one);
        }
    }

    /// <summary>
    /// If a tile is selected, increment its rotation.
    /// </summary>
    public void RotateSelection() {
        if (_selectedTileDefinition == null) { return; }

        _rot = (_rot + 1) % 4;

        gridMarker.Resize(_effectiveSize);

        if (grid.UpdateFootprint(_effectiveSize)) {
            UpdateGridMarker(grid.HoveredCoord);
        }

        previewer.SetLocalPositionRotation(
            UpdateEffectiveSize(_rot, _selectedTileDefinition.size),
             Vector3.up * _rot * -90);

        CheckSelectionValidity(grid.HoveredCoord);
    }

    /// <summary>
    /// Given a rotation and base size, compute the rotated size.
    /// </summary>
    private Vector3 UpdateEffectiveSize(int rot, Vector2Int baseSize) {
        Vector2Int offset2D = grid.RotationOffset(_rot, baseSize);

        // When rotating determine the size to use for checks
        _effectiveSize = _rot switch {
            0 => baseSize,
            1 => new Vector2Int(baseSize.y, baseSize.x),
            2 => baseSize,
            3 => new Vector2Int(baseSize.y, baseSize.x),
            _ => baseSize
        };

        float w = offset2D.x * grid.CellSize;
        float h = offset2D.y * grid.CellSize;

       return new Vector3(w, 0, h);
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
            previewer.SetTint(TilePreviewState.Invalid);
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
            previewer.SetTint(TilePreviewState.Invalid);
            return;
        }

        previewer.SetTint(TilePreviewState.Valid);
    }
}
