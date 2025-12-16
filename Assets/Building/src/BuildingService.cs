using UnityEngine;
using DG.Tweening;

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

    [Header("References")]
    public InteractableGrid grid;
    public GridPlacementMarker gridMarker;
    public TilePreviewer previewer;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private bool _validCoord;
    private bool _active = false;
    private TileDefinition _selectedTileDefinition = null;
    private TilePlacementSystem _placement;
    
    //*********************************************************************

    /// <summary>
    /// Unity awake method.
    /// </summary>
    private void Awake() {
        if (Instance == null) { Instance = this; }
        _placement = new TilePlacementSystem(grid);
    }

    /// <summary>
    /// Unity start method.
    /// </summary>
    private void Start() {
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
    /// Logic to execute when the player selects an unlocked tile from the UI.
    /// </summary>
    public void SelectTile(TileDefinition selection) {
        _selectedTileDefinition = selection;
        previewer.SetPrefab(selection.prefab);
    }

    /// <summary>
    /// This is called when the player executes the input that interacts with the current cell. This does nothing while not in build mode.
    /// </summary>
    public void InteractCell() {
        if (!_active) { return; }
        Debug.Log(_selectedTileDefinition + " " + _validCoord);

        if (_selectedTileDefinition != null && _validCoord) { 
            // TODO: Add tile rotation
            _placement.TryPlace(grid.HoveredCoord, _selectedTileDefinition, 0, transform);
            CheckSelectionValidity(grid.HoveredCoord);
        }
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
    }

    /// <summary>
    /// If there is a selected tile, check if it would be valid on the input coordinate.
    /// </summary>
    private void CheckSelectionValidity(Vector2Int coord) {
        if (_selectedTileDefinition == null) { return; }

        bool overlaps = _placement.CheckOverlap(coord, _selectedTileDefinition.size);
        previewer.SetValid(!overlaps);

        _validCoord = !overlaps;
    }
}
