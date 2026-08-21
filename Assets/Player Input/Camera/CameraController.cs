using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    private static CameraController instance = null;
    public static CameraController Instance {get {return instance;} }

    [System.Serializable]
    public struct CameraPose
    {
        public Vector3 position;
        public float orthoSize;
        public CameraPose(Vector3 pos, float o) {
            position = pos;
            orthoSize = o;
        }
    }

    [Header("References")]
    public CinemachineCamera vCam;
    public Transform frontRoomTarget; // The location the camera should lock too when focused on the room
    public Transform backRoomTarget; 
    public Transform adjustmentTarget; // The location the camera should zoom to when adjustments are happening

    [Space(15)][Header("Params")]
    [Range(0f, 2f)] public float transitionDuration;
    public Vector2 swayAnchor; // x - horizontal; y - vertical
    public float swaySpeed;
    public Vector2 swayStrength; // x - horizontal; y - vertical
    public float zoomOrthoSize = 2;
    // public Vector2 xLimits;
    // public Vector2 zLimits;
    // public float clickDragSpeed = 0.5f; // Units per second
    // public float inputSpeed = 2f;
    // public float zoomSpeed = 1f;
    // public Vector2 zoomClamps = new Vector2(16, 26);

    // public bool holding;
    // public Vector2 discreteMoveInput;

    // [Header("Minigame Behavior")]
    // public float minigameZoomLevel = 24;

    private CinemachineOrbitalFollow followCam;
    private CinemachineHardLookAt lookCam;
    private CinemachineBasicMultiChannelPerlin cameraNoise;
    private float _mgZoomWeight = 0;
    private bool _mgZoomEnforced = false;
    private Sequence _currentBlendSequence;
    private Dictionary<RoomType, CameraPose> cameraPoses = new();

    void Awake()
    {
        if (instance == null) { instance = this; }
        followCam = vCam.GetComponent<CinemachineOrbitalFollow>();
        lookCam = vCam.GetComponent<CinemachineHardLookAt>();
        cameraNoise = vCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        cameraPoses[RoomType.Front] = new CameraPose(
            frontRoomTarget.transform.position,
            vCam.Lens.OrthographicSize
        );
        cameraPoses[RoomType.Adjustment] = new CameraPose(
            backRoomTarget.transform.position,
            3.5f
        );

        vCam.Target.TrackingTarget.transform.position = frontRoomTarget.transform.position;
    }

    void OnEnable()
    {
        MinigameService.Instance.onRoomTransition += SwapCameras;
    }
    void OnDisable()
    {
        MinigameService.Instance.onRoomTransition -= SwapCameras;
    }

    /// <summary>
    /// Change the priority of the camera to swap between them
    /// </summary>
    public void SwapCameras(RoomType type) {
        // Kill any currently running blend sequence so they don't fight
        _currentBlendSequence?.Kill();

        // Build a parallel tween sequence for position, rotation, and orthographic size
        _currentBlendSequence = DOTween.Sequence();

        _currentBlendSequence.Join(
            vCam.Target.TrackingTarget.transform.DOMove(cameraPoses[type].position, transitionDuration).SetEase(Ease.OutSine)
        );

        _currentBlendSequence.Join(
            DOTween.To(
                () => vCam.Lens.OrthographicSize,
                x => {
                    vCam.Lens.OrthographicSize = x;
                },
                cameraPoses[type].orthoSize,
                transitionDuration
            ).SetEase(Ease.OutSine)
        );
    }

    // public void OnPan(Vector2 mouseDelta)
    // {
    //     if (!holding) { return; }

    //     // Inverting the value feels better here
    //     MoveCameraTarget(-mouseDelta, clickDragSpeed);
    // }

    // public void OnZoom(float dir) {
    //     // -1 -> zoom out
    //     //  1 -> zoom in
    //     float target = Mathf.Clamp((-dir * zoomSpeed) + followCam.FollowOffset.y, zoomClamps.x, zoomClamps.y);
    //     DOTween.To(() => followCam.FollowOffset.y, x => followCam.FollowOffset.y = x, target, 1).SetEase(Ease.OutQuart);
    // }

    void Update()
    {
        // float time = Time.time * swaySpeed;
        // // Offset the sampling coordinates so X and Y don't track identically on a diagonal
        // float noiseX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
        // float noiseY = (Mathf.PerlinNoise(0f, time + 100f) - 0.5f) * 2f;

        // Vector3 swayOffset = new Vector3(noiseX * swayStrength.x, noiseY * swayStrength.y, 0f);
        
        // followCam.HorizontalAxis.Value = swayAnchor.x + (noiseX * swayStrength.x);
        // followCam.VerticalAxis.Value = swayAnchor.y + (noiseY * swayStrength.y);
    }

    /// <summary>
    /// Execute the coroutine that will enforce zoom
    /// </summary>
    public void StartMinigameCameraBehavior() {
        _mgZoomWeight = 0;
        _mgZoomEnforced = true;
        // DOVirtual.Float(cCam.Lens.OrthographicSize, orthoSizeLimits.y, 1.5f, (x) => {
        //     cCam.Lens.OrthographicSize = x;
        // })
        // .OnComplete(() => StartCoroutine(ForceZoomRoutine()))
        // .SetEase(Ease.OutQuart);
        // StartCoroutine(ForceZoomRoutine());
    }

    public void EndMinigameCameraBehavior() {
        _mgZoomEnforced = false;
        // Vector3 lookOffset = lookCam.LookAtOffset;
        // float orthoSize = cCam.Lens.OrthographicSize;
        // DOVirtual.Float(0, 1, 1.5f, (x) => {
        //     lookCam.LookAtOffset = Vector3.Lerp(lookOffset, Vector3.zero, x);
        //     cCam.Lens.OrthographicSize = Mathf.Lerp(orthoSize, defaultOrthoSize, x);
        // })
        // .SetEase(Ease.OutQuart);
    }

    public void SetCameraNoiseAmplitude(float val) {
        cameraNoise.AmplitudeGain = val;
    }

    public void SetMgZoomWeight(float val) {
        _mgZoomWeight = Mathf.Clamp01(val);
    }

    /// <summary>
    /// Slowly force the camera to the zoom level passed in. The flag should be toggled when
    /// this forcing wants to be ended
    /// </summary>
    private IEnumerator ForceZoomRoutine() {
        float baseOrthoSize = vCam.Lens.OrthographicSize;
        while (_mgZoomEnforced) {
            SetMgZoomWeight(_mgZoomWeight - 0.025f);
            Vector3 fullOffset = adjustmentTarget.position - vCam.Target.TrackingTarget.position;
            float orthoDiff = baseOrthoSize - zoomOrthoSize;
            lookCam.LookAtOffset = fullOffset * _mgZoomWeight;
            vCam.Lens.OrthographicSize = baseOrthoSize - (orthoDiff * _mgZoomWeight);
            yield return new WaitForSeconds(0.025f);
        }

        vCam.Lens.OrthographicSize = baseOrthoSize;

        yield return null;
    }
}
