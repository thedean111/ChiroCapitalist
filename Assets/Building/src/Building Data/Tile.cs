using UnityEngine;

public class Tile : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------

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
    public virtual void Initialize() { PlaceTile(); }

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
}
