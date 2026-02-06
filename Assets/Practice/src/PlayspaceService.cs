using System.Collections.Generic;
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
    [Header("Tile Interactivity")]
    public string officeTileLayer;
    public Color tileHoverColor;
    public Color tileFocusColor;

    [Header("Office Sequences")]
    public List<OfficeSequence> idleSequences = new();
    public List<OfficeSequence> strengthSequences = new();
    public List<OfficeSequence> techniqueSequences = new();
    public List<OfficeSequence> magicSequences = new();

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private Tile _hoveredOfficeTile;
    private Tile _focusedTile;
    private int _officeMask;
    //*********************************************************************
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    /// <summary>
    /// Reset any selection or hovering logic.
    /// </summary>
    public override void Toggle(bool status)
    {
        base.Toggle(status);
        UIManager.Instance.ToggleTileDetailsPanel(false);
        if (_hoveredOfficeTile) {
            _hoveredOfficeTile.DisableOutlines();
            _hoveredOfficeTile = null;

        }
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
        Debug.DrawRay(ray.origin, ray.direction * 50);
        if (Physics.Raycast(ray, out RaycastHit hit, 50, _officeMask)) {
            Tile t = hit.transform.GetComponent<Tile>();
            if (_hoveredOfficeTile != null) {
                if (t != _hoveredOfficeTile) {
                    if (_focusedTile != null) {
                        if (_hoveredOfficeTile.tileID != _focusedTile.tileID) {
                            _hoveredOfficeTile.DisableOutlines();
                        }
                    } else {
                        _hoveredOfficeTile.DisableOutlines();
                    }
                
                // Return early if hovering over the same tile
                } else {
                    return;
                }
            } 

            _hoveredOfficeTile = t;
            if (!(_focusedTile != null && _hoveredOfficeTile.tileID == _focusedTile.tileID)) {
                _hoveredOfficeTile.EnableOutlines();
            }

        } else {
            if (_hoveredOfficeTile != null) {
                if (_focusedTile != null && _hoveredOfficeTile.tileID != _focusedTile.tileID)
                    _hoveredOfficeTile.DisableOutlines();
                _hoveredOfficeTile = null;
            }
        }
    }

    /// <summary>
    /// Unity update method.
    /// </summary>
    public void Interact() {
        if (!Active || UIManager.Instance.IsPointerOverUI()) { return; }



        if (_hoveredOfficeTile == null) {
            UIManager.Instance.ToggleTileDetailsPanel(false);

        } else {
            if (_focusedTile != null && _focusedTile.tileID != _hoveredOfficeTile.tileID) {
                _focusedTile.UpdateOutline(false, tileHoverColor);
            }

            _focusedTile = _hoveredOfficeTile;
            _focusedTile.ChangeOutlineColor(tileFocusColor);
            _focusedTile.OnFocus();
            UIManager.Instance.ToggleTileDetailsPanel(true, _focusedTile.tileID);
        }

    }

    /// <summary>
    /// If there is a focused tile then unfocus it.
    /// </summary>
    public void ResetFocus() {
        if (_focusedTile != null) {
            _focusedTile.UpdateOutline(false, tileHoverColor);
            _focusedTile = null;
        }
    }
}
