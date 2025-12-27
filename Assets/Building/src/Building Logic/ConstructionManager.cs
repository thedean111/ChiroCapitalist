using UnityEngine;

/// <summary>
/// Common functionality for all construction-related services.
/// </summary>
public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance {get; private set;}

    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    [Header("Construction Services")]
    public BuildingService placementService;
    public EditService editService;

    [Header("Game Object References")]
    public InteractableGrid grid;
    public GridPlacementMarker gridMarker;
    public TilePreviewer previewer;
    public TilePreviewer movePreviewer;
    public WallBuilder wallBuilder;

    [Header("Anchors")]
    public TileDefinition mainOffice;

    [Header("Audio Effects")]
    public SoundDefinition placementSound;
    
    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private TilePlacementSystem _tilePlacer;
    private int _rot = 0;
    //---------------------------------------------------------------------

    /// <summary>
    /// Unity awake method.
    /// </summary>
    private void Awake() {
        if (Instance == null) { Instance = this; }
        _tilePlacer = new TilePlacementSystem(grid);
        _rot = 0;
    }

    /// <summary>
    /// Unity start method.
    /// </summary>
    private void Start() {
        // BOOTSTRAP all data with good objects.
        wallBuilder.grid = grid;

        placementService.grid = grid;
        placementService.tilePlacer = _tilePlacer;
        placementService.gridMarker = gridMarker;
        placementService.previewer = previewer;
        placementService.wallBuilder = wallBuilder;

        editService.grid = grid;
        editService.tilePlacer = _tilePlacer;
        editService.gridMarker = gridMarker;
        editService.previewer = previewer;
        editService.movePreviewer = movePreviewer;
        editService.wallBuilder = wallBuilder;

        // Ensure the main office is loaded by default
        // TODO: This will eventually need to be re-worked to integrate with saving and loading.
        LoadDefaultMainOffice();

        // Setup all the callback for hovered grid cell change
        if (grid == null || gridMarker == null) { Debug.LogWarning("Grid objects not set properly!"); }
        else { 
            grid.OnHoveredCellChange += UpdateCellFeedback;
            grid.OnMouseEnterGrid += () => editService.ToggleTileWorldPreview(true);
            grid.OnMouseExitGrid += () => editService.ToggleTileWorldPreview(false);
        }
    }

    /// <summary>
    /// When starting the scene, load the default anchors associated with this game object.
    /// </summary>
    private void LoadDefaultMainOffice() {
        if (mainOffice == null) { return; }

        Vector2Int mainOfficeCoord = new Vector2Int(-2, 3);
        _tilePlacer.SpawnTile(mainOfficeCoord, mainOffice, 0, transform, 0);
        _tilePlacer.AddAnchor(mainOfficeCoord);
        wallBuilder.RebuildPerimeter(mainOfficeCoord, mainOffice.size, _tilePlacer.GetCells());
    }

    /// <summary>
    /// General updates to happen when the mouse moves to a new coordinate.
    /// </summary>
    private void UpdateCellFeedback(Vector2Int newCoord) {
        CheckMarkerValidity(newCoord);
        gridMarker.Move(grid.GridToWorld(newCoord));
    }

    /// <summary>
    /// Check the held tile definition, or lack thereof, and set the preview tint accordingly.
    /// </summary>
    public bool CheckMarkerValidity(Vector2Int coord) {
        bool validMarkerPosition = _tilePlacer.IsSelectionValid(coord);
        previewer.SetTint(validMarkerPosition ? TilePreviewState.Valid : TilePreviewState.Invalid);
        return validMarkerPosition;
    }

    /// <summary>
    /// If a tile is selected, increment its rotation.
    /// </summary>
    public void RotateSelection() {
        TileDefinition def = _tilePlacer.SelectedTileDef();
        if (def == null) { return; }

        _rot = (_rot + 1) % 4;

        previewer.SetLocalPositionRotation( _tilePlacer.UpdateRotation(_rot, def.size), Vector3.up * _rot * -90);
        gridMarker.Resize(_tilePlacer.EffectiveSize);
        grid.UpdateFootprint(_tilePlacer.EffectiveSize);
        CheckMarkerValidity(grid.HoveredCoord);

        if (editService.Active) { editService.EditOnCellChange(grid.HoveredCoord); }
    }

    /// <summary>
    /// Logic to execute when the player selects an unlocked tile from the UI.
    /// </summary>
    public void SelectTile(TileDefinition selection) {
        // As far as the services are concerned, it doesn't really matter what tile is selected. 
        // The tilePlacer will be the centralized knowledge of what tile the player is holding.
        _tilePlacer.SetSelectedTile(selection, 0);
        gridMarker.Resize(selection.size);
        grid.UpdateFootprint(selection.size);
        previewer.SetPrefab(selection.prefab);
        _rot = 0;
        CheckMarkerValidity(grid.HoveredCoord);
    }


}
