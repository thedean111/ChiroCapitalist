using UnityEngine;
using UnityEngine.InputSystem;

public class PlayspaceService : ServiceState
{
    public static PlayspaceService Instance { get; private set; }
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public string officeTileLayer;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private Tile _hoveredOfficeTile;
    private int _officeMask;
    //*********************************************************************
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    /// <summary>
    /// Unity start method.
    /// </summary>
    protected override void Start() {
        base.Start();
        _officeMask = LayerMask.GetMask(officeTileLayer);
    }

    /// <summary>
    /// Unity update method.
    /// </summary>
    void Update() {
        if (!Active) { return; }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 50, _officeMask)) {
            if (_hoveredOfficeTile != null) { _hoveredOfficeTile.DisableOutlines(); }
            _hoveredOfficeTile = hit.transform.GetComponent<Tile>();
            _hoveredOfficeTile.EnableOutlines();
        } else {
            if (_hoveredOfficeTile != null) { _hoveredOfficeTile.DisableOutlines(); }
        }
    }

    /// <summary>
    /// Unity update method.
    /// </summary>
    public void Interact() {
        if (!Active) { return; }

        if (_hoveredOfficeTile == null) {
            UIManager.Instance.ToggleTileDetailsPanel(false);
            return;

        } else {
            UIManager.Instance.ToggleTileDetailsPanel(true, _hoveredOfficeTile.tileID);
        }

    }
}
