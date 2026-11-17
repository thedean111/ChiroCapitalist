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
    [Header("References")]
    public SpawnPointTile frontRoomTile;
    public Tile backRoomTile;

    [Header("Tile Interactivity")]
    public ParticleSystem levelUpEffect;
    [Range(0f, 10f)]
    public float outlineThickness = 2f;
    [Range(0f, 2f)] public float transitionDuration = 0.3f;

    [Header("Office Sequences")]
    public List<OfficeSequence> idleSequences = new();
    public List<OfficeSequence> strengthSequences = new();
    public List<OfficeSequence> techniqueSequences = new();
    public List<OfficeSequence> magicSequences = new();

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------

    private PatientSpawner _focusedSpawner;

    private int _officeMask;
    private int _spawnMask;
    private Renderer[] _frontRoomRenderers;
    private Renderer[] _backRoomRenderers;
    private MaterialPropertyBlock _mpb;
    private int _effectiveAlphaID;

    //*********************************************************************
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        _mpb = new MaterialPropertyBlock();
        _frontRoomRenderers = frontRoomTile.GetComponentsInChildren<Renderer>(true);
        _backRoomRenderers = backRoomTile.GetComponentsInChildren<Renderer>(true);
        _effectiveAlphaID = Shader.PropertyToID("_EffectiveAlpha");
    }

    void OnEnable()
    {
        MinigameService.Instance.onRoomTransition += FadeRooms;
    }
    void OnDisable()
    {
        MinigameService.Instance.onRoomTransition -= FadeRooms;
    }

    /// <summary>
    /// Reset any selection or hovering logic.
    /// </summary>
    public override void Toggle(bool status)
    {
        base.Toggle(status);
        if (status)
            InputManager.Instance.ToggleActionMap("Player");
    }

    /// <summary>
    /// Unity start method.
    /// </summary>
    protected override void Start() {
        base.Start();
        _spawnMask = LayerMask.GetMask("SpawnPoint");
        // FadeRooms(RoomType.Front);
        frontRoomTile.FadeTile(.5f, 1f);
        backRoomTile.FadeTile(.5f, 0f);
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
            PatientSpawner _spwnr = hit.transform.GetComponent<PatientSpawner>();
            if (_focusedSpawner != null) {
                if (_focusedSpawner != _spwnr) {
                    _focusedSpawner.ToggleOutline(false);
                    _focusedSpawner = _spwnr;
                    _focusedSpawner.ToggleOutline(true);
                    UIManager.Instance.ToggleMinigameInfo(true, _focusedSpawner.GetPatientStats());
                }
            } else {
                _focusedSpawner = _spwnr;
                _focusedSpawner.ToggleOutline(true);
                UIManager.Instance.ToggleMinigameInfo(true, _focusedSpawner.GetPatientStats());
            }
            return;
        } else if (_focusedSpawner != null) {
            _focusedSpawner.ToggleOutline(false);
            _focusedSpawner = null;
            UIManager.Instance.ToggleMinigameInfo(false, null);
        }
    }

    /// <summary>
    /// Unity update method.
    /// </summary>
    public void Interact() {
        
        if (!Active || UIManager.Instance.IsPointerOverUI()) { return; }
        Debug.Log("Interacting in Playspace manager");

        if (_focusedSpawner != null) {
            _focusedSpawner.ToggleOutline(false);
            ServiceManager.Instance.ToggleService<MinigameService>(true);
            MinigameService.Instance.PlayMinigame(_focusedSpawner.GetPatient(), _focusedSpawner.transform.position);
            return;
        }
    }

    /// <summary>
    /// The input is the room that should be visible, others should be invisible
    /// </summary>
    public void FadeRooms(RoomType type) {
        switch (type) {
            case RoomType.Front:
            frontRoomTile.FadeTile(0f, 1f);
            backRoomTile.FadeTile(1f, 0f);
            break;

            case RoomType.Adjustment:
            frontRoomTile.FadeTile(1f, 0f);
            backRoomTile.FadeTile(0f, 1f);
            break;
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

    public void ResolveFocusedPatient() {
        if (_focusedSpawner == null) { return; }

        _focusedSpawner.TryRemovePatient("standing_idle_1");
    }
}
