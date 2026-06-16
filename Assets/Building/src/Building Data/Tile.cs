using System;
using System.Collections.Generic;
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
    public List<PropRuleAssociation> propRules;
    // These details are available for all tiles but not all will use them
    [Header("Details")]
    public int Level { get; protected set; }
    public event Action<float> OnProgressChange; // callback to be fired when the progress of the focused tile is changed
    public event Action OnProgressComplete;



    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private Renderer[] _renderers;
    private string _targetProperty;
    private MaterialPropertyBlock _mpb;
    protected bool _firstPlace = true;
    //---------------------------------------------------------------------

    /// <summary>
    /// What to do when this tile is clicked.
    /// </summary>
    public virtual void OnFocus() {}

    /// <summary>
    /// Update the progress bar with the provided float, if the UI subscribed to this Tile's method.
    /// </summary>
    public void UpdateProgress(float f) {
        OnProgressChange?.Invoke(f);
    }

    /// <summary>
    /// Progress is completed
    /// </summary>
    public void CompleteProgress() {
        OnProgressComplete?.Invoke();
    }

    /// <summary>
    /// Initialization of this tile the first time its placed down
    /// </summary>
    public virtual void Initialize() { 
        PlaceTile();
        Level = 1;
        foreach (PropRuleAssociation ra in propRules) {
            ra.ruleSet.InitializeRules();
        }
    }

    /// <summary>
    /// Logic for placing down the tile.
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
    /// What every tile should do when leveling up.
    /// </summary>
    public virtual void LevelUp() {
        if (Level >= ProgressionManager.Instance.maxTileLevel) {
            return;
        }

        Level++;
        UIManager.Instance.UpdateLevelText(Level);
        foreach (PropRuleAssociation ra in propRules) {
            if (ra.ruleSet.ruleDict.TryGetValue(Level, out PropUpgradeRule rule)) {
                rule.Execute(ra.prop);
            }
        }

        _renderers = GetComponentsInChildren<Renderer>();
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

    public void UpdateOutline(bool status, Color color) {
        for (int i = 0; i < _renderers.Length; i++) {
            if (_renderers[i].TryGetComponent(out Outline o)) {
                o.OutlineColor = color;
                o.enabled = status;
            }
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
    /// Change the color of all outline components on the game object.
    /// </summary>
    public void ChangeOutlineColor(Color color) {
        for (int i = 0; i < _renderers.Length; i++) {
            if (_renderers[i].TryGetComponent(out Outline o)) {
                o.OutlineColor = color;
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

    public virtual void ProgressCompleted(ProgressBar bar) { }
    public virtual void UpdateDoctorAssignment(DoctorData newDoctor) {}
}
