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

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    //---------------------------------------------------------------------


    void Awake() {
        if (Instance == null) { Instance = this;}
    }

    // /// <summary>
    // /// Performs any logic necessary for toggling edit mode on and off.
    // /// </summary>
    // public void ToggleEdit(bool status) {
    //     _editing = status;

    //     if (_editing) {
    //         previewer.Hide();
    //         _selectedTileDefinition = null;
    //         gridMarker.Resize(Vector2Int.one);
    //         grid.UpdateFootprint(Vector2Int.one);

    //         UIManager.Instance.ActivateEditButton(true);
    //         UIManager.Instance.ClearTileListSelection();
    //     } else {
    //         UIManager.Instance.ActivateEditButton(false);
    //     }
    // }

    //  else  if (_editing && _placement.HasCellData(grid.HoveredCoord)) {
    //         _focusedCell = grid.HoveredCoord;
    //         UIManager.Instance.ToggleEditPopup(true);
    //         CameraController.Instance.ForceCameraPosition(grid.GridToWorld(grid.HoveredCoord));
    //     } else if (holdingTile) {
        //     ConfirmEdit();
                // If the player picked up a tile this flag turns to true, attempt to place it at the mouse position.

        // // The player is interacting with an arbitrary cell
        // }

    
    /// <summary>
    /// Attempt to delete the focused tile.
    /// TODO: Check for anchor chain breakage before confirming a deletion.
    /// </summary>
    // public void DeleteFocusedTile() {
    //     if (!_editing) { return; }

    //     TileInstance tile = _placement.GetTileInstance(_focusedCell);
    //     if (tile == null) {
    //         Debug.LogWarning("BuildingService: Attempting to delete a tile that doesn't exist!");
    //         return;
    //     }

    //     _placement.RemoveCellFootprint(tile.origin, tile.def.size);
    //     wallBuilder.RebuildPerimeter(tile.origin, tile.def.size, _placement.GetCells());

    //     // TODO: If this tile is tied to other game objects (Office-doctor-patient), reallocate resources properly
    //     Destroy(tile.instance);

    //     UIManager.Instance.ToggleEditPopup(false);
    // }

    /// <summary>
    /// Pick up the tile that is currently focused.
    /// </summary>
    // public void PickupTile() {
    //     if (!_editing) { return; }

    //     // Get the tile definition at the selected cell
    //     TileInstance tile = _placement.GetTileInstance(_focusedCell);
    //     if (tile == null) {
    //         Debug.LogWarning("BuildingService: Attempting to pick up a tile that doesn't exist!");
    //         return;
    //     }
    //     UIManager.Instance.ToggleEditPopup(false);

    //     // Holding tile in the context of editing, but treat the movement like a normal placement
    //     holdingTile = true;
    //     _selectedTileDefinition = tile.def;
    //     _movingInstance = tile;

    //     // Use the placement previewer to visualize where the tile will be moved to
    //     previewer.SetPrefab(tile.def.prefab);
    //     Vector3 localPos = UpdateEffectiveSize(_rot, tile.def.size);
    //     _rot = tile.rotation;
    //     previewer.SetLocalPositionRotation(localPos, Vector3.up * _rot * -90);
    //     grid.UpdateFootprint(_selectedTileDefinition.size);
    //     gridMarker.Resize(_selectedTileDefinition.size);

    //     // Use the move previewer to show the original tile location
    //     movePreviewer.SetPrefab(tile.def.prefab);
    //     movePreviewer.transform.position = tile.instance.transform.position;
    //     movePreviewer.transform.rotation = tile.instance.transform.rotation;
    //     movePreviewer.SetTint(TilePreviewState.Pending_Move);

    //     // Hide the instance of the tile
    //     tile.instance.SetActive(false);

    //     // Capture the initial state of the tile and then remove it from the grid
    //     _placement.RemoveCellFootprint(tile.origin, tile.def.size);
    //     wallBuilder.RebuildPerimeter(tile.origin, tile.def.size, _placement.GetCells());
    // }


    /// <summary>
    /// If a tile is currently being moved, cancel the move and restore the original state.
    /// </summary>
    // public void CancelEdit() {
    //     if (!holdingTile) { return; }

    //     // Add the original instance back to the placement dictionary data
    //     _placement.UpdateInstance(_movingInstance.origin, _movingInstance, _movingInstance.rotation);

    //     // Reset the original instance
    //     // TODO: Play some undo sound
    //     _movingInstance.instance.SetActive(true);
    //     wallBuilder.RebuildPerimeter(_movingInstance.origin, _movingInstance.def.size, _placement.GetCells());

    //     // Hide previews        
    //     movePreviewer.Hide();
    //     previewer.Hide();

    //     // Set internal data
    //     holdingTile = false;
    //     _selectedTileDefinition = null;
    //     _movingInstance = null;
    //     gridMarker.Resize(Vector2Int.one);
    // }

    // /// <summary>
    // /// If a tile is currently being moved, place it down on the current grid coordinate--assuming its a valid position.
    // /// </summary>
    // public void ConfirmEdit() {
    //     if (_editing && _selectedTileDefinition != null) {
    //         if (!_validCoord) {
    //             Debug.LogWarning("Cannot move tile to an invalid coordinate!");
    //         }

    //         // Update the existing instance and build new walls
    //         // TODO: Play a special sound for moving instances
    //         _placement.UpdateInstance(grid.HoveredCoord, _movingInstance, _rot);
    //         _movingInstance.instance.SetActive(true);
    //         SoundManager.Instance.PlayBuildEffect(placementSound);
    //         wallBuilder.RebuildPerimeter(grid.HoveredCoord, _effectiveSize, _placement.GetCells());
            
    //         // Hide previews        
    //         movePreviewer.Hide();
    //         previewer.Hide();

    //         // Update flags
    //         _movingInstance = null;
    //         _selectedTileDefinition = null;
    //         holdingTile = false;
    //         gridMarker.Resize(Vector2Int.one);
    //     }
    // }
}
