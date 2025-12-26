using UnityEngine;

public class EditService : ServiceState
{
    public static EditService Instance {get; private set;}

    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    [Header("Game Object References")]
    public InteractableGrid grid;
    public GridPlacementMarker gridMarker;
    public TilePreviewer previewer;
    public TilePreviewer movePreviewer;
    public WallBuilder wallBuilder;
    public TilePlacementSystem tilePlacer;

    [Header("Params")]
    public string hologramColorProperty = "_BaseColor";
    public string hologramLayerName = "Hologram";
    public Color baseHologramTint;
    public Color activeHologramTint;
    public Color islandColor;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private int _holoLayerMask;
    private TileInstance _payload = null;
    private Vector2Int _lastCoordWhileMoving;
    private Vector2Int _originalEditCoord;
    //---------------------------------------------------------------------


    void Awake() {
        if (Instance == null) { Instance = this;}
        _holoLayerMask = LayerMask.NameToLayer(hologramLayerName);
    }

    /// <summary>
    /// Performs any logic necessary for toggling the service on and off.
    /// </summary>
    public override void Toggle(bool status) {
        if (!CanToggle(status)) { return; }
        base.Toggle(status);

        // Ensure the grid is on and toggle the 
        if (Active) {
            UIManager.Instance.ToggleEditServiceUI(true);
            grid.ToggleGrid(true);
            ToggleOverlays(true);
            wallBuilder.ToggleWalls(false);
            grid.OnHoveredCellChange += EditOnCellChange;

        } else {
            grid.OnHoveredCellChange -= EditOnCellChange;
            CancelEdit();
            UIManager.Instance.ToggleEditServiceUI(false);
            grid.ToggleGrid(false);
            ToggleOverlays(false);
            wallBuilder.ToggleWalls(true);
            gridMarker.Reset();
            if (tilePlacer.SelectedTileDef() != null) {
                previewer.Toggle(false);
                tilePlacer.SetSelectedTile(null, 0);
            }
        }
    }

    /// <summary>
    /// This is called when the player executes the input that interacts with the current cell. This does nothing while not in build mode.
    /// </summary>
    public void InteractCell() {
        if (!Active || UIManager.Instance.IsPointerOverUI()) { return; }

        // If there is currently no payload then the player wants to attempt picking up whatever is at 
        // the current cell coordinate
        if (_payload == null) {
            PickupTile();

        // If there is a payload then the player wants the place what they are holding at the current cell
        // coordinate
        } else {
            PlaceTile();
        }
    }

    /// <summary>
    /// Restore the original state of the active payload.
    /// </summary>
    public void CancelEdit() {
        if (_payload == null) { return; }

        // Hide the previews and show the original tile again
        previewer.Toggle(false);
        movePreviewer.Toggle(false);
        _payload.instance.gameObject.SetActive(true);

        // Remove whatever was being shown to the player
        tilePlacer.RemoveCellFootprint(_lastCoordWhileMoving, tilePlacer.EffectiveSize);
        wallBuilder.RebuildPerimeter(_lastCoordWhileMoving, tilePlacer.EffectiveSize, tilePlacer.GetCells());

        // Add the original tile back to the placer and update walls
        tilePlacer.UpdateInstance(_originalEditCoord, _payload, _payload.rotation);
        wallBuilder.RebuildPerimeter(_originalEditCoord, tilePlacer.EffectiveSize, tilePlacer.GetCells());
        tilePlacer.IdentifyIslands(baseHologramTint, islandColor);
        
        // Reset
        gridMarker.Resize(Vector2Int.one);
        _payload = null;
    }

    /// <summary>
    /// Called when the player selects the delete button while holding a payload.
    /// </summary>
    public void DeletePayload() {
        if (_payload == null) { return; }

        // TODO: Show a popup dialog that asks the player to confirm this deletion

        // TODO: For any tiles that are tied with other systems (offices, doctors, patients, etc.)
        // reallocate the resources where necessary so the player doesn't lose them
        Destroy(_payload.instance.gameObject);

        UIManager.Instance.ToggleSelectedTileElement(false);
        _payload = null;
        previewer.Toggle(false);
        movePreviewer.Toggle(false);
        gridMarker.Resize(Vector2Int.one);
    }

    /// <summary>
    /// Switch between a world-space preview of the selected tile and a screen-space-mouse-anchored preview of the selected tile. True equates to world space.
    /// </summary>
    public void ToggleTileWorldPreview(bool status) {
        if (_payload == null) { return; }

        UIManager.Instance.ToggleSelectedTileElement(!status);
        previewer.Toggle(status);
    }

    /// <summary>
    /// For all the tiles, puts the game objects in a new layer that has an overlay material drawn on it.
    /// </summary>
    private void ToggleOverlays(bool status) {
        foreach (TileInstance tile in tilePlacer.tiles.Values) {
            if (status) {
                tile.instance.EnableOverlay(_holoLayerMask);
                tile.instance.SetTargetProperty(hologramColorProperty);
                tile.instance.SetOverlayColor(baseHologramTint);
            } else
                tile.instance.DisableOverlay();
        }
    }

    /// <summary>
    /// When holding a payload, for every valid position simulate the new state of the grid.
    /// </summary>
    public void EditOnCellChange(Vector2Int coord) {
        if (_payload == null) { return; }

        // Remove the payload information from the previously simulated cell
        tilePlacer.RemoveCellFootprint(_lastCoordWhileMoving, tilePlacer.EffectiveSize);
        wallBuilder.RebuildPerimeter(_lastCoordWhileMoving, tilePlacer.EffectiveSize, tilePlacer.GetCells());

        // If the coord and tile wouldn't be valid, don't simulate anything
        if (!tilePlacer.IsValidCoord) { return; }

        // At the new coord update the instance and generate the new all placement
        tilePlacer.UpdateInstance(coord, _payload);
    
        // Feedback to the user how this position affects islands.
        // If this newly hovered coord DOES NOT create islands then simulate the walls
        if (!tilePlacer.IdentifyIslands(baseHologramTint, islandColor)) {
            wallBuilder.RebuildPerimeter(coord, tilePlacer.EffectiveSize, tilePlacer.GetCells());
            tilePlacer.RemoveCellFootprint(coord, tilePlacer.EffectiveSize);
        }

        _lastCoordWhileMoving = coord;
    }

    /// <summary>
    /// Actual logic for picking up a tile and creating an edit payload with it.
    /// </summary>
    private void PickupTile() {
        // Get the tile definition at the selected cell
        _payload = tilePlacer.GetTileInstance(grid.HoveredCoord);
        if (_payload == null) {
            Debug.LogWarning("EditService: Attempting to pick up a tile that doesn't exist!");
            return;
        }

        // Set the icon in case the mouse leaves the grid
        UIManager.Instance.SetSelectedTileElement(_payload.def.icon);

        // Use the placement previewer to visualize where the tile will be moved to
        Vector3 localPos = tilePlacer.SetSelectedTile(_payload.def, _payload.rotation);
        previewer.SetPrefab(_payload.def.prefab);
        previewer.SetPositionRotation(_payload.instance.transform.position, Vector3.up * _payload.rotation * -90);
        previewer.SetLocalPositionRotation(localPos + Vector3.up, Vector3.up * _payload.rotation * -90);
        grid.UpdateFootprint(tilePlacer.EffectiveSize);
        gridMarker.Resize(tilePlacer.EffectiveSize);

        // Use the move previewer to show the original tile location
        movePreviewer.SetPrefab(_payload.def.prefab);
        movePreviewer.transform.position = _payload.instance.transform.position;
        movePreviewer.transform.rotation = _payload.instance.transform.rotation;
        movePreviewer.SetTint(activeHologramTint);

        // Hide the instance of the tile
        _payload.instance.gameObject.SetActive(false);

        // Capture the initial state of the tile and then remove it from the grid
        tilePlacer.RemoveCellFootprint(_payload.origin, tilePlacer.EffectiveSize);
        wallBuilder.RebuildPerimeter(_payload.origin, tilePlacer.EffectiveSize, tilePlacer.GetCells());

        // Immediately determine if any islands are formed by picking up the tile
        tilePlacer.IdentifyIslands(baseHologramTint, islandColor);
        _lastCoordWhileMoving = grid.HoveredCoord;
        _originalEditCoord = _payload.origin;

    }

    /// <summary>
    /// Actual logic for placing down the current edit payload.
    /// </summary>
    private void PlaceTile() {
        // This is also updated when the islands are identified
        if (!tilePlacer.IsValidCoord) { return; }

        // Sound!
        SoundManager.Instance.PlayBuildEffect(ConstructionManager.Instance.placementSound);

        // Move the instance and update the local walls
        tilePlacer.UpdateInstance(grid.HoveredCoord, _payload);
        _payload.instance.gameObject.SetActive(true);
        _payload.instance.PlaceTile();
        // wallBuilder.RebuildPerimeter(grid.HoveredCoord, tilePlacer.EffectiveSize, tilePlacer.GetCells());
        
        // Reset selection stuff
        _payload = null;
        previewer.Toggle(false);
        movePreviewer.Toggle(false);
        gridMarker.Resize(Vector2Int.one);
    }
}
