using UnityEngine;
using UnityEngine.InputSystem;

public class PlayspaceService : ServiceState
{
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
}
