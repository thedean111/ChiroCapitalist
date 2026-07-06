using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.Interactions;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// Building service for the game. Helps route functionality between different objects and performs basic validation.
/// This is a singleton because there is one service.
/// </summary>
public class BuildingService : ServiceState
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
    public TilePlacementSystem tilePlacer;  

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    
    //*********************************************************************

    /// <summary>
    /// Unity awake method.
    /// </summary>
    private void Awake() {
        if (Instance == null) { Instance = this; }
    }

    /// <summary>
    /// Unity start method.
    /// </summary>
    protected override void InitService() {
        base.InitService();
    }

    /// <summary>
    /// Performs any logic necessary for toggling the service on and off.
    /// </summary>
    public override void Toggle(bool status) {
        if (!CanToggle(status)) { return; }
        base.Toggle(status);

        // Ensure the grid is on and toggle the 
        if (Active) {
            grid.ToggleGrid(true);
            UIManager.Instance.ToggleTileCatalog(true);

        } else {
            grid.ToggleGrid(false);
            gridMarker.Reset();
            UIManager.Instance.ToggleTileCatalog(false);
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

        // This is for placing a new tile
        if (tilePlacer.TrySpawn(grid.HoveredCoord, transform, 0.15f, out TileInstance newTile)) {
            newTile.instance.Initialize();
            ProgressionManager.Instance.AdjustMoney(-newTile.def.cost);
            SoundManager.Instance.PlayBuildEffect(ConstructionManager.Instance.placementSound);
            wallBuilder.RebuildPerimeter(grid.HoveredCoord, tilePlacer.EffectiveSize, tilePlacer.GetCells());
            ConstructionManager.Instance.CheckMarkerValidity(grid.HoveredCoord);
        }
    }
}
