using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class BuildingManager : MonoBehaviour
{
    // Singleton
    private static BuildingManager instance;
    public static BuildingManager Instance { get {return instance; }}

    // ---
    [Header("References")]
    public BuildingGrid grid;
    public Transform gridMarker;
    [Space(15)][Header("Params")]
    public float gridMarkerSpeed = 0.2f;
    // ---
    private bool active = false;
    private Vector2Int currentCoord; // what is the current coordinate the mouse is hovering over
    private LayerMask gridMask;
    private Vector3 hiddenMarkerPos = new Vector3(0, 50, 0);
    // ===

    /// <summary>
    /// Unity Awake method.
    /// </summary>
    void Awake()
    {
        if (instance == null) { instance = this; }

        gridMask = LayerMask.GetMask("BuildingGrid");
        gridMarker.position = Vector3.up * 50;
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
    }

    /// <summary>
    /// Perform the necessary logic turn off building mode.
    /// </summary>
    void Deactivate()
    {
        gridMarker.position = hiddenMarkerPos;
        grid.FadeGrid(false);
    }

    /// <summary>
    /// Unity Update method.
    /// </summary>
    void Update()
    {
        if (!active) { return; }
        if (CameraController.Instance.holding) { return; }

        // Cast a ray and if it hits the grid then move the marker by cell position
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawRay(ray.origin, ray.direction * 50);
        if (Physics.Raycast(ray, out RaycastHit hit, 50, gridMask)) {
            Vector3 gridPos = grid.ClampPosition(hit.point, out currentCoord);

            if (gridMarker.position == hiddenMarkerPos)
                gridMarker.position = gridPos;
            else
                gridMarker.DOMove(gridPos, gridMarkerSpeed);

        // Since we didn't hit the grid, hide the marker
        } else {
            gridMarker.position = hiddenMarkerPos;
        }
    }
}
