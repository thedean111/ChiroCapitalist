using System;
using System.Collections.Generic;
using DG.Tweening;
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
    public ParticleSystem levelUpEffect;
    [Range(0f, 10f)]
    public float outlineThickness = 2f;

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
    private PatientSpawner _focusedSpawner;

    private int _officeMask;
    private int _spawnMask;
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
        _spawnMask = LayerMask.GetMask("SpawnPoint");
    }

    /// <summary>
    /// Unity update method.
    /// </summary>
    void Update() {
        if (!Active) { return; }

        // If you hit the spawn point don't try to detect the tile
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawRay(ray.origin, ray.direction * 50);
        if (Physics.Raycast(ray, out RaycastHit hit, 50, _spawnMask)) {
            if (_focusedSpawner != null) {
                _focusedSpawner.ToggleOutline(false);
            }
            if (_hoveredOfficeTile != null) {
                _hoveredOfficeTile.DisableOutlines();
                _hoveredOfficeTile = null;
            }
            _focusedSpawner = hit.transform.GetComponent<PatientSpawner>();
            _focusedSpawner.ToggleOutline(true);
            return;
        }
        if (_focusedSpawner != null) {
            _focusedSpawner.ToggleOutline(false);
            _focusedSpawner = null;
        }

        Tile checkTile = null;
        if (Physics.Raycast(ray, out hit, 50, _officeMask)) {
            checkTile = hit.transform.GetComponent<Tile>();
        }

        // If previously hovering a tile, disable its outline if hovering over nothing or a new tile
        if (_hoveredOfficeTile != null && _hoveredOfficeTile != _focusedTile) {
            if (checkTile == null) {
                _hoveredOfficeTile.DisableOutlines();
                _hoveredOfficeTile = null;

            } else if (checkTile != _hoveredOfficeTile) {
                _hoveredOfficeTile.DisableOutlines();
                _hoveredOfficeTile = checkTile;
                _hoveredOfficeTile.EnableOutlines();

            }
        } else if (checkTile != null && checkTile != _focusedTile) {
            _hoveredOfficeTile = checkTile;
            _hoveredOfficeTile.EnableOutlines();

        }
    }

    /// <summary>
    /// Unity update method.
    /// </summary>
    public void Interact() {
        if (!Active || UIManager.Instance.IsPointerOverUI()) { return; }

        if (_focusedSpawner != null) {
            _focusedSpawner.TryRemovePatient("standing_idle_1");
        }

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

    /// <summary>
    /// Place the particle system at a position and play it.
    /// </summary>
    public void PlayLevelUpEffect(Vector3 position) {
        levelUpEffect.transform.position = position;
        levelUpEffect.Play();
    }

    /// <summary>
    /// Fade all the children of the root transform who have the dither fade support in their shader.
    /// </summary>
    public static void DitherFadeObject(Transform root, float targetValue, float duration, Action onComplete) {
        if (root == null) return;

        // 1. Grab all active child renderers
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(false);

        if (renderers.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        int alphaPropId = Shader.PropertyToID("_EffectiveAlpha");
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        // 2. Get the starting alpha from the first renderer's MPB or sharedMaterial
        float startValue = 0f;
        renderers[0].GetPropertyBlock(mpb);
        if (mpb.HasFloat(alphaPropId))
        {
            startValue = mpb.GetFloat(alphaPropId);
        }
        else if (renderers[0].sharedMaterial != null && renderers[0].sharedMaterial.HasProperty(alphaPropId))
        {
            startValue = renderers[0].sharedMaterial.GetFloat(alphaPropId);
        }

        // 3. Tween using the property block
        DOTween.To(() => startValue, x => 
        {
            startValue = x;
            
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // Fetch current block, modify it, and push it back
                renderer.GetPropertyBlock(mpb);
                mpb.SetFloat(alphaPropId, x);
                renderer.SetPropertyBlock(mpb);
            }
        }, targetValue, duration) // 1.0f duration
        .SetEase(Ease.InOutQuad)
        .OnComplete(() => 
        {
            onComplete?.Invoke();
        });
    }

    public void UpdateOutline(Transform root, bool status, Color color) {
        Outline[] outlines = root.GetComponentsInChildren<Outline>();
        foreach (Outline o in outlines) {
            o.OutlineColor = color;
            o.OutlineWidth = outlineThickness;
            o.enabled = status;
        }
    }
}
