using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UIElements;

public class BuildingManager : MonoBehaviour
{
    // Singleton
    public static BuildingManager Instance { get; private set;}

    // ---
    [Header("References")]
    public BuildingGrid grid;
    public GridPlacementMarker gridMarker;
    [Space(15)][Header("Params")]
    public float gridMarkerSpeed = 0.2f;
    // ---
    private bool active = false;
    private Vector2Int currentCoord; // what is the current coordinate the mouse is hovering over
    private Vector2Int lastValidCoord;
    private LayerMask gridMask;
    private Vector2Int selectedTileSize = Vector2Int.one;
    private Vector3 hiddenMarkerPos = new Vector3(0, 50, 0);
    private Button buildButton = null;
    private Transform selectedTileHighlight = null;
    // ===

    /// <summary>
    /// Unity Awake method.
    /// </summary>
    void Awake()
    {
        if (Instance == null) { Instance = this; }

        gridMask = LayerMask.GetMask("BuildingGrid");
        gridMarker.transform.position = Vector3.up * 50;
        gridMarker.Reset();
    }

    /// <summary>
    /// Unity Start method.
    /// </summary>
    void Start()
    {
        grid.Initialize();    
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
        grid.FadeGrid(true);
        if (buildButton != null) { buildButton.AddToClassList("hud-button-on"); }
        UIManager.Instance.ToggleBuildUI(true);
    }

    /// <summary>
    /// Perform the necessary logic turn off building mode.
    /// </summary>
    void Deactivate()
    {
        if (selectedTileHighlight != null) { Destroy(selectedTileHighlight.gameObject); }
        selectedTileHighlight = null;
        gridMarker.transform.position = hiddenMarkerPos;
        gridMarker.Reset();
        selectedTileSize = Vector2Int.one;
        grid.FadeGrid(false);
        if (buildButton != null) { buildButton.RemoveFromClassList("hud-button-on"); }
        UIManager.Instance.ToggleBuildUI(false);
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
            Vector3 gridPos = grid.ClampPosition(hit.point, out currentCoord);

            // Only move the tile if it actually will fit on the grid
            if (grid.IsValidPlacement(currentCoord, selectedTileSize)) {
                lastValidCoord = currentCoord;

                if (gridMarker.transform.position == hiddenMarkerPos)
                    gridMarker.transform.position = gridPos;
                else
                    gridMarker.transform.DOMove(gridPos, gridMarkerSpeed);
            }

        // Since we didn't hit the grid, hide the marker
        } 
        // else {
        //     gridMarker.transform.position = hiddenMarkerPos;
        // }
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

        // Resize the grid marker to accurately represent the selected tile
        // TODO: Create some shader logic for showing a highlight of the prefab
        gridMarker.Resize(tile.size);
        selectedTileSize = tile.size;
    }
}
