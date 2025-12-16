using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UIElements;

public class BuildingManager : MonoBehaviour
{
    // Singleton
    public static BuildingManager Instance { get; private set;}

    // // ---
    // [Header("References")]
    // public BuildingGrid grid;
    // public GridPlacementMarker gridMarker;
    // public Material holographicMat;

    // [Space(15)][Header("Params")]
    // public float gridMarkerSpeed = 0.2f;
    // public float tilePlacementTime = 0.2f;
    // public float tileRotationTime = 0.3f;
    // public Color validPlacementColor;
    // public Color invalidPlacementColor;
    
    // [Space(15)][Header("Outline Effects")]
    // public float outlineWidth = 0.2f;
    // public Color outlineHighlightColor;
    // public Color outlineValidColor;
    // public Color outlineInvalidColor;
    // public Color outlineSelectedColor;

    // // ---
    // private bool active = false;
    // private bool editing = true;
    // private bool mouseOnGrid = false;
    // private bool validPlacementLocation = true;
    // private Vector2Int currentCoord; // what is the current coordinate the mouse is hovering over
    // private Vector2Int lastValidCoord;
    // private LayerMask gridMask;
    // private Vector3 hiddenMarkerPos = new Vector3(0, 50, 0);
    // private Button buildButton = null;
    // private Button editButton = null;
    // private Button moveButton = null;
    // private VisualElement editTileOptions = null;
    // private bool hoveringOverTile = false;
    // private bool movingTile = false;
    // private Vector3 originalTilePosition;
    // private Tile selectedHologram = null;
    // private PlaceableTile selectedTile;
    // // ===

    // /// <summary>
    // /// Unity Awake method.
    // /// </summary>
    // void Awake()
    // {
    //     if (Instance == null) { Instance = this; }

    //     gridMask = LayerMask.GetMask("BuildingGrid");
    // }

    // /// <summary>
    // /// Unity Start method.
    // /// </summary>
    // void Start()
    // {
    //     SetButtons();

    //     editTileOptions = UIManager.Instance.GetFromHud("edit-tile-opt-container");
    //     editTileOptions.SetEnabled(false);

    //     grid.Initialize();
    //     gridMarker.transform.position = Vector3.up * 50;
    //     gridMarker.Reset();

    //     // For edit mode to be true the first time the build UI is opened
    //     editing = false;
    //     ToggleEdit();
    // }

    // /// <summary>
    // /// Toggling logic for build mode. Ensures the external game state is evaluated before entering build mode.
    // /// </summary>
    // public void Toggle()
    // {
    //     active = !active;
    //     if (active) Activate();
    //     else Deactivate();    
    // }

    // /// <summary>
    // /// Perform the necessary logic to enter the default state in building mode.
    // /// </summary>
    // void Activate()
    // {
    //     grid.FadeGrid(true);
    //     if (buildButton != null) { buildButton.AddToClassList("hud-button-on"); }
    //     UIManager.Instance.ToggleBuildUI(true);
    // }

    // /// <summary>
    // /// Perform the necessary logic turn off building mode.
    // /// </summary>
    // void Deactivate()
    // {
    //     gridMarker.transform.DOKill();
    //     gridMarker.transform.position = hiddenMarkerPos;
    //     grid.FadeGrid(false);
    //     if (buildButton != null) { buildButton.RemoveFromClassList("hud-button-on"); }
    //     UIManager.Instance.ToggleBuildUI(false);

    //     if (!editing) { ToggleEdit(); }
    // }

    // /// <summary>
    // /// Unity Update method.
    // /// </summary>
    // void Update()
    // {
    //     if (!active) { return; }
    //     if (CameraController.Instance.holding) { return; }
    //     if (UIManager.Instance.IsPointerOverUI()) { return; }

    //     // Cast a ray and if it hits the grid then move the marker by cell position
    //     Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    //     Debug.DrawRay(ray.origin, ray.direction * 50);
    //     if (Physics.Raycast(ray, out RaycastHit hit, 50, gridMask)) {
    //         mouseOnGrid = true;
    //         Vector3 gridPos = grid.ClampPosition(hit.point, out currentCoord);
    //         Vector2Int targetSize = selectedTile == null ? Vector2Int.one : selectedTile.size;

    //         // Only move the tile if it actually will fit on the grid
    //         if (grid.FitsOnGrid(currentCoord, targetSize) && currentCoord != lastValidCoord) {
    //             lastValidCoord = currentCoord;
    //             if (editing && !movingTile) {hoveringOverTile = grid.HighlightTile(currentCoord, outlineHighlightColor, outlineWidth); }
    //             if (gridMarker.transform.position == hiddenMarkerPos) {
    //                 gridMarker.transform.position = gridPos;
    //                 UpdateTileHighlighter();
    //             } else {
    //                 gridMarker.transform.DOMove(gridPos, gridMarkerSpeed).OnComplete(() => UpdateTileHighlighter());
    //             }
    //         }
    //     } else
    //     {
    //         mouseOnGrid = false;
    //     }
    // }

    // /// <summary>
    // /// Provides the manager with a reference to the button that toggles the state. Also sets up the button click behavior.
    // /// </summary>
    // public void SetButtons() {
    //     if (buildButton != null) { buildButton.clicked -= Toggle; }

    //     buildButton = (Button)UIManager.Instance.GetFromHud("build-button");
    //     buildButton.clicked += Toggle;

    //     editButton = (Button)UIManager.Instance.GetFromHud("build-menu-edit-button");
    //     editButton.clicked += () => {if (!editing) ToggleEdit();};

    //     moveButton = (Button)UIManager.Instance.GetFromHud("edit-tile-move");
    //     moveButton.clicked += InitiateMove;
    // }

    // /// <summary>
    // /// Provides the manager with a reference to the button that toggles the state. Also sets up the button click behavior.
    // /// </summary>
    // public void AddTileToGrid()
    // {
        
    // }

    // /// <summary>
    // /// Behavior for when the user selects a tile from the list.
    // /// </summary>
    // public void SelectTile(PlaceableTile tile)
    // {
    //     // When a tile is selected, we are put in placement mode (which is just not editing)
    //     if (editing) { ToggleEdit(); }
    //     if (editTileOptions.enabledSelf) { StopCurrentEdit(); }
    //     // Replace the previous selection if there was one
    //     if (selectedHologram != null) { Destroy(selectedHologram.gameObject); }
    //     selectedHologram = Instantiate(tile.hologram, gridMarker.transform).GetComponent<Tile>();

    //     // If the new size won't fit on the grid, put it in the first available tile
    //     // NOTE: Assumption here is that resizing always happens in the +X/+Z direction
    //     if (!grid.FitsOnGrid(lastValidCoord, tile.size))
    //     {
    //         lastValidCoord = currentCoord = grid.FindValidPlacementCoord(lastValidCoord, tile.size);
    //         gridMarker.transform.DOMove(grid.GridToWorld(lastValidCoord), gridMarkerSpeed).OnComplete(() => UpdateTileHighlighter());
    //     } else {
    //         UpdateTileHighlighter();
    //     }

    //     // Resize the grid marker to accurately represent the selected tile
    //     gridMarker.Resize(tile.size);
    //     selectedTile = tile;
    // }

    // /// <summary>
    // /// When this method is fired, the manager will try and place the selected tile onto the current grid position. Requires a tile to be selected, no grid overlap, and building mode to be enabled.
    // /// </summary>
    // public void AttemptPlacement()
    // {
    //     if (!active || editing || UIManager.Instance.IsPointerOverUI() || selectedHologram == null || !validPlacementLocation || !mouseOnGrid) { return; }
    //     // TODO: Place the tile, deduct money, update grid
    //     Tile t = Instantiate(selectedTile.prefab, grid.GridToWorld(lastValidCoord), Quaternion.identity, transform).GetComponent<Tile>();
    //     t.Place(tilePlacementTime, selectedHologram.propParent.rotation.eulerAngles);
    //     grid.AddTileToGrid(lastValidCoord, selectedTile, selectedTile.defaultType, selectedHologram.specialCells, t);
    //     UpdateTileHighlighter();
    //     ProgressionManager.Instance.AdjustMoney(-selectedTile.cost);
    // }

    // /// <summary>
    // /// When in edit mode, 'pickup' the tile that is selected.
    // /// </summary>
    // public void InteractWithTile()
    // {
    //     // A tile will only be picked up if editing and the occupied cell is filled
    //     if (!editing || !hoveringOverTile || movingTile) { return; }

    //     // Get all the data at the cell that was selected
    //     BuildingGrid.CellData cellData = grid.GetCellData(lastValidCoord);
    //     selectedHologram = cellData.tile;

    //     // Change the color of the tile that is currently occupying that cell
    //     selectedHologram.lockOutline = true;
    //     selectedHologram.ChangeOutlineColor(outlineSelectedColor);

    //     // Move the camera to the tile that is selected
    //     CameraController.Instance.ForceCameraPosition(selectedHologram.transform.position);

    //     // Open the menu that contains edit options
    //     editTileOptions.SetEnabled(true);
    // }

    // private void InitiateMove()
    // {
    //     movingTile = true;

    //     // Let the tile move with the marker
    //     originalTilePosition = selectedHologram.transform.localPosition;
    //     selectedHologram.transform.parent = gridMarker.transform;
    //     selectedHologram.transform.DOLocalMove(Vector3.up, 0.3f).SetEase(Ease.OutBack);

    //     // Remove the selected tile from the grid and update the local walls
    //     BuildingGrid.CellData data = grid.RemoveTile(grid.WorldToGrid(originalTilePosition));
    //     // grid.UpdateGrid();
    //     grid.UpdateWalls(grid.WorldToGrid(originalTilePosition), data.blueprint.size);

    //     // Update the validity of each tile in the grid
    //     grid.UpdateTileValidity(outlineValidColor, outlineInvalidColor);
        
    //     // Update the grid as if the tile that is being moved isn't on the grid (so the walls get fixed)
    //     // If islands are caused by this movement then highlight the tiles that are invalid 
    //     //  --> TODO: Tiles will need some sort of status that they are validly connected, that way when I check tiles if they are not next to valid ones it is an island
    //     // Every time the cursor moves, check the state of the grid!
    //     // TODO: Make a generic method that evaluates the state of the grid

    //     editTileOptions.SetEnabled(false);
    // }

    // /// <summary>
    // /// If a tile is being picked up, this will place it down. In a valid spot or back to the original if the grid is invalid.
    // /// </summary>
    // public void SetDownTile() {
    //     if (!movingTile) { return; }

    //     // TODO: If the placement is invalid, warn the user and do nothing else
        
    //     // TODO: If the placement is valid add the tile to the grid at the hovered position
    //     movingTile = false;
    //     hoveringOverTile = false;

    //     // TODO: Unlock the outline of all tiles
    //     // selectedHologram.transform.parent = transform;
    //     // selectedHologram.lockOutline = false;
    //     // selectedHologram.ToggleOutline(false);
    //     // selectedHologram.ChangeOutlineColor(outlineHighlightColor);
    //     // selectedHologram.transform.DOLocalMove(originalTilePosition, 0.3f).SetEase(Ease.OutBack);
    //     // selectedHologram = null;
    // }

    // /// <summary>
    // /// If a tile is selected call its rotate method.
    // /// </summary>
    // public void RotateTile() {
    //     if (selectedHologram != null) {
    //         selectedHologram.Rotate(tileRotationTime, UpdateTileHighlighter);
    //     }
    // }

    // /// <summary>
    // /// Edit mode is the default state where no tile is selected from the menu. In this state existing tiles may be selected and moved or deleted.
    // /// </summary>
    // public void ToggleEdit()
    // {
    //     editing = !editing;
    //     // When edit mode is entered, ensure no tiles are active
    //     if (editing) {
    //         editButton.AddToClassList("edit-mode-active");
    //         gridMarker.Reset();
    //         if (selectedHologram != null) { Destroy(selectedHologram.gameObject); selectedHologram = null; }
    //         selectedTile = null;

    //     } else {
    //         editButton.RemoveFromClassList("edit-mode-active");
    //     }
    // }

    // /// <summary>
    // /// When this is called, the player state should be in edit mode, but with no selections.
    // /// </summary>
    // public void StopCurrentEdit() {
    //     // We want the player to be able to move the camera while editing, of course
    //     if (movingTile) { return; }

    //     // But if not already moving something, them reset the menu
    //     if (editTileOptions.enabledSelf) { 
    //         editTileOptions.SetEnabled(false);
    //         selectedHologram.lockOutline = false;
    //         selectedHologram.ChangeOutlineColor(outlineHighlightColor);
    //         selectedHologram.ToggleOutline(false);
    //         selectedHologram = null;
    //     }
    // }

    // // ++++++++++++++++++++++++++++++++++++++++++
    // // PRIVATE HELPER
    // // ++++++++++++++++++++++++++++++++++++++++++
    // private void UpdateTileHighlighter()
    // {
    //     if (selectedTile != null)
    //     {
    //         validPlacementLocation = grid.CanPlaceTile(lastValidCoord, selectedTile.size, selectedHologram.specialCells);
    //         holographicMat.color = validPlacementLocation ? validPlacementColor : invalidPlacementColor;
    //     }
    // }
}
