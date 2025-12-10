using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph;

public class BuildingManager : MonoBehaviour
{
    // Singleton
    public static BuildingManager Instance { get; private set;}

    // ---
    [Header("References")]
    public BuildingGrid grid;
    public GridPlacementMarker gridMarker;
    public Material holographicMat;

    [Space(15)][Header("Params")]
    public float gridMarkerSpeed = 0.2f;
    public float tilePlacementTime = 0.2f;
    public float tileRotationTime = 0.3f;
    public Color validPlacementColor;
    public Color invalidPlacementColor;
    // ---
    private bool active = false;
    private bool mouseOnGrid = false;
    private bool validPlacementLocation = true;
    private Vector2Int currentCoord; // what is the current coordinate the mouse is hovering over
    private Vector2Int lastValidCoord;
    private LayerMask gridMask;
    private Vector3 hiddenMarkerPos = new Vector3(0, 50, 0);
    private Button buildButton = null;
    private Tile selectedHologram = null;
    private PlaceableTile selectedTile;
    // ===

    /// <summary>
    /// Unity Awake method.
    /// </summary>
    void Awake()
    {
        if (Instance == null) { Instance = this; }

        gridMask = LayerMask.GetMask("BuildingGrid");
    }

    /// <summary>
    /// Unity Start method.
    /// </summary>
    void Start()
    {
        grid.Initialize();
        gridMarker.transform.position = Vector3.up * 50;
        gridMarker.Reset();
    }

    /// <summary>
    /// Toggling logic for build mode. Ensures the external game state is evaluated before entering build mode.
    /// </summary>
    public void Toggle()
    {
        active = !active;
        if (active) Activate();
        else Deactivate();    
    }

    /// <summary>
    /// Perform the necessary logic to enter the default state in building mode.
    /// </summary>
    void Activate()
    {
        gridMarker.Reset();
        grid.FadeGrid(true);
        if (buildButton != null) { buildButton.AddToClassList("hud-button-on"); }
        UIManager.Instance.ToggleBuildUI(true);
    }

    /// <summary>
    /// Perform the necessary logic turn off building mode.
    /// </summary>
    void Deactivate()
    {
        gridMarker.transform.DOKill();
        gridMarker.transform.position = hiddenMarkerPos;
        grid.FadeGrid(false);
        if (buildButton != null) { buildButton.RemoveFromClassList("hud-button-on"); }
        UIManager.Instance.ToggleBuildUI(false);
        if (selectedHologram != null) { Destroy(selectedHologram); selectedHologram = null; }
        selectedTile = null;
    }

    /// <summary>
    /// Unity Update method.
    /// </summary>
    void Update()
    {
        if (!active) { return; }
        if (CameraController.Instance.holding) { return; }
        if (UIManager.Instance.IsPointerOverUI()) { return; }

        // Cast a ray and if it hits the grid then move the marker by cell position
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawRay(ray.origin, ray.direction * 50);
        if (Physics.Raycast(ray, out RaycastHit hit, 50, gridMask)) {
            mouseOnGrid = true;
            Vector3 gridPos = grid.ClampPosition(hit.point, out currentCoord);
            Vector2Int targetSize = selectedTile == null ? Vector2Int.one : selectedTile.size;

            // Only move the tile if it actually will fit on the grid
            if (grid.FitsOnGrid(currentCoord, targetSize) && currentCoord != lastValidCoord) {
                lastValidCoord = currentCoord;
                if (gridMarker.transform.position == hiddenMarkerPos) {
                    gridMarker.transform.position = gridPos;
                    UpdateTileHighlighter();
                } else {
                    gridMarker.transform.DOMove(gridPos, gridMarkerSpeed).OnComplete(() => UpdateTileHighlighter());
                }
            }
        } else
        {
            mouseOnGrid = false;
        }
    }

    /// <summary>
    /// Provides the manager with a reference to the button that toggles the state. Also sets up the button click behavior.
    /// </summary>
    public void SetButton(Button btn) {
        if (btn == null) { return; }
        if (buildButton != null) { buildButton.clicked -= Toggle; }

        buildButton = btn;
        buildButton.clicked += Toggle;
    }

    /// <summary>
    /// Behavior for when the user selects a tile from the list.
    /// </summary>
    public void SelectTile(PlaceableTile tile)
    {
        if (selectedHologram != null)
        {
            Destroy(selectedHologram.gameObject);
        }
        selectedHologram = Instantiate(tile.hologram, gridMarker.transform).GetComponent<Tile>();

        // If the new size won't fit on the grid, put it in the first available tile
        // NOTE: Assumption here is that resizing always happens in the +X/+Z direction
        if (!grid.FitsOnGrid(lastValidCoord, tile.size))
        {
            lastValidCoord = currentCoord = grid.FindValidPlacementCoord(lastValidCoord, tile.size);
            gridMarker.transform.DOMove(grid.GridToWorld(lastValidCoord), gridMarkerSpeed).OnComplete(() => UpdateTileHighlighter());
        } else {
            UpdateTileHighlighter();
        }

        // Resize the grid marker to accurately represent the selected tile
        // TODO: Create some shader logic for showing a highlight of the prefab
        gridMarker.Resize(tile.size);
        selectedTile = tile;
    }

    /// <summary>
    /// When this method is fired, the manager will try and place the selected tile onto the current grid position. Requires a tile to be selected, no grid overlap, and building mode to be enabled.
    /// </summary>
    public void AttemptPlacement()
    {
        if (!active || selectedHologram == null || !validPlacementLocation || !mouseOnGrid) { return; }
        // TODO: Place the tile, deduct money, update grid
        Tile t = Instantiate(selectedTile.prefab, grid.GridToWorld(lastValidCoord), Quaternion.identity, transform).GetComponent<Tile>();
        t.Place(tilePlacementTime, selectedHologram.propParent.rotation.eulerAngles);
        grid.AddTileToGrid(lastValidCoord, selectedTile.size, selectedTile.defaultType, selectedHologram.specialCells);
        UpdateTileHighlighter();
        ProgressionManager.Instance.AdjustMoney(-selectedTile.cost);
    }

    /// <summary>
    /// If a tile is selected call its rotate method.
    /// </summary>
    public void RotateTile() {
        if (selectedHologram != null) {
            selectedHologram.Rotate(tileRotationTime, UpdateTileHighlighter);
        }
    }

    // ++++++++++++++++++++++++++++++++++++++++++
    // PRIVATE HELPER
    // ++++++++++++++++++++++++++++++++++++++++++
    private void UpdateTileHighlighter()
    {
        if (selectedTile != null)
        {
            validPlacementLocation = grid.CanPlaceTile(lastValidCoord, selectedTile.size, selectedHologram.specialCells);
            holographicMat.color = validPlacementLocation ? validPlacementColor : invalidPlacementColor;
        }
    }
}
