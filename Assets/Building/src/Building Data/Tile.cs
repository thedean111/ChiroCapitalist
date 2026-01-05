using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    [HideInInspector] public int tileID;
    public TileDetailsFlags detailFlags { get; private set; }

    [Header("Detail Flags")]
    public bool usesLevel;
    public bool usesProgress;

    // These details are available for all tiles but not all will use them
    [Header("Details")]
    public int Level { get; protected set; }
    public event Action<float> OnProgressChange; // callback to be fired when the progress of the focused tile is changed

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private Renderer[] _renderers;
    private string _targetProperty;
    private MaterialPropertyBlock _mpb;
    protected bool _firstPlace = true;
    //---------------------------------------------------------------------

    /// <summary>
    /// Initialization of this tile the first time its placed down
    /// </summary>
    public virtual void Initialize() { 
        detailFlags = TileDetailsFlags.None;
        if (usesLevel) detailFlags |= TileDetailsFlags.Level;
        if (usesProgress) detailFlags |= TileDetailsFlags.Progress;

        PlaceTile();
    }

    /// <summary>
    /// Logic for placing down the tile..
    /// </summary>
    public virtual void PlaceTile() {}

    /// <summary>
    /// Unity enable method.
    /// </summary>
    void OnEnable()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Set the string of the property that should be manipulated by subsequent calls;
    /// </summary>
    public void SetTargetProperty(string propertyName) {
        _targetProperty = propertyName;
    }

    /// <summary>
    /// Add the game object to the layer for overlays
    /// </summary>
    public void EnableOverlay(int layer) {
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].gameObject.layer = layer;
        }
    }

    /// <summary>
    /// Change the color of the holographic overlay for the tile.
    /// </summary>
    public void SetOverlayColor(Color color) {
        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer r = _renderers[i];
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(_targetProperty, color);
            r.SetPropertyBlock(_mpb);
        }
    }

    /// <summary>
    /// Disable the holographic overlay for the tile.
    /// </summary>
    public void DisableOverlay() {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].gameObject.layer = 0;
        }
    }

    /// <summary>
    /// Disable the holographic overlay for the tile.
    /// </summary>
    public void EnableOutlines() {
        for (int i = 0; i < _renderers.Length; i++) {
            if (_renderers[i].TryGetComponent(out Outline o)) {
                o.enabled = true;
            }
        }
    }

    /// <summary>
    /// Disable the holographic overlay for the tile.
    /// </summary>
    public void DisableOutlines() {
        for (int i = 0; i < _renderers.Length; i++) {
            if (_renderers[i].TryGetComponent(out Outline o)) {
                o.enabled = false;
            }
        }
    }
}
