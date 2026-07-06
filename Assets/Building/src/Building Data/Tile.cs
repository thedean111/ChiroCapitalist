using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

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
    public int Seed { get; private set; }
    public event Action<float> OnProgressChange; // callback to be fired when the progress of the focused tile is changed
    public event Action OnProgressComplete;
    public System.Random rng;


    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private Renderer[] _renderers;
    private string _targetProperty;
    private MaterialPropertyBlock _mpb;
    private Transform props;
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
        Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        rng = new System.Random(Seed);

        foreach (PropRuleAssociation ra in propRules) {
            ra.Initialize(rng);
        }

        props = transform.Find("props");
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
    public virtual bool LevelUp() {
        if (Level >= ProgressionManager.Instance.maxTileLevel) {
            return false;
        }

        Level++;
        if (props != null) {
            props.transform.DOShakeScale(0.2f, 0.15f);
        }
        TileInstance _focusedTile = UIManager.Instance.GetFocusedTile();
        Vector2 tileCenter = ((Vector2)_focusedTile.def.size) / 2f * 3f;
        Vector3 tilePos =_focusedTile.instance.transform.position;
        Vector3 playPos = Vector3.zero;
        switch(_focusedTile.rotation) {
            case 0:
                playPos = new Vector3( tileCenter.x + tilePos.x, 0f, tileCenter.y + tilePos.z);
                break;
            case 1:
                playPos = new Vector3( tilePos.x - tileCenter.x, 0f, tileCenter.y + tilePos.z);
                break;
            case 2:
                playPos = new Vector3( tilePos.x - tileCenter.x, 0f, tilePos.z - tileCenter.y);
                break;
            case 3:
                playPos = new Vector3( tileCenter.x + tilePos.x, 0f, tilePos.z - tileCenter.y);
                break;
        }
        
        PlayspaceService.Instance.PlayLevelUpEffect(playPos);

        UIManager.Instance.UpdateLevelText(Level);
        foreach (PropRuleAssociation ra in propRules) {
            if (ra.GetRules().ruleDict.TryGetValue(Level, out PropUpgradeRule rule)) {
                rule.Execute(ra.prop);
            }
        }

        _renderers = GetComponentsInChildren<Renderer>();

        return true;
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
                o.OutlineWidth = PlayspaceService.Instance.outlineThickness;
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
