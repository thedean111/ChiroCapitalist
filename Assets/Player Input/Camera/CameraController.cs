using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private static CameraController instance = null;
    public static CameraController Instance {get {return instance;} }

    [Header("References")]
    public CinemachineCamera cCam;
    public Transform camTarg;

    [Space(15)][Header("Params")]
    public Vector2 xLimits;
    public Vector2 zLimits;
    public float clickDragSpeed = 0.5f; // Units per second
    public float inputSpeed = 2f;
    public float zoomSpeed = 1f;
    public Vector2 zoomClamps = new Vector2(16, 26);

    public bool holding;
    public Vector2 discreteMoveInput;

    [Header("Minigame Behavior")]
    public float minigameZoomLevel = 24;

    private CinemachineFollow followCam;
    private bool _forcingZoomLevel = false;

    void Awake()
    {
        if (instance == null) { instance = this; }
        followCam = cCam.GetComponent<CinemachineFollow>();
    }

    public void OnPan(Vector2 mouseDelta)
    {
        if (!holding) { return; }

        // Inverting the value feels better here
        MoveCameraTarget(-mouseDelta, clickDragSpeed);
    }

    public void OnZoom(float dir) {
        // -1 -> zoom out
        //  1 -> zoom in
        float target = Mathf.Clamp((-dir * zoomSpeed) + followCam.FollowOffset.y, zoomClamps.x, zoomClamps.y);
        DOTween.To(() => followCam.FollowOffset.y, x => followCam.FollowOffset.y = x, target, 1).SetEase(Ease.OutQuart);
    }

    void Update()
    {
        if (discreteMoveInput != Vector2.zero)
        {
            MoveCameraTarget(discreteMoveInput, inputSpeed);
        }
    }

    /// <summary>
    /// Move the camera target that the cinemachine camera follows.
    /// </summary>
    /// <param name="direction">2D input direction.</param>
    /// <param name="speed">Value to scale the movement by.</param>
    void MoveCameraTarget(Vector2 direction, float speed)
    {
        // Rotate the direction such that direction.y correlates to forward camera movement
        Vector3 forward = cCam.transform.forward;
        Vector3 right = cCam.transform.right;

        // Project the forward and right vectors on to the XZ plane
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();

        // Scale the forward and right vectors according to the input
        Vector3 effectiveDirection = Vector3.zero;
        effectiveDirection += right * direction.x;
        effectiveDirection += forward * direction.y;
        effectiveDirection *= speed * Time.deltaTime;

        // Move the camera target -- cinemachine will automatically follow this
        camTarg.position += new Vector3(effectiveDirection.x, 0, effectiveDirection.z);
        
        // Ensure the target stays in the world boundary
        float x = Math.Clamp(camTarg.position.x, xLimits.x, xLimits.y);
        float z = Math.Clamp(camTarg.position.z, zLimits.x, zLimits.y);
        camTarg.position = new Vector3(x, 0, z);
    }

    /// <summary>
    /// Force the camera target to the input position.
    /// </summary>
    /// <param name="pos">Position to set the camera target to.</param>
    public void ForceCameraPosition(Vector3 pos)
    {
        camTarg.DOMove(pos, 0.5f);
    }

    /// <summary>
    /// Execute the coroutine that will enforce zoom
    /// </summary>
    public void StartMinigameCameraBehavior(Vector3 pos) {
        ForceCameraPosition(pos);
        DOTween.To(() => followCam.FollowOffset.y, x => followCam.FollowOffset.y = x, minigameZoomLevel, 1).SetEase(Ease.OutQuart);
        StartCoroutine(ForceZoomRoutine(minigameZoomLevel));
    }

    public void EndMinigameCameraBehavior() {
        _forcingZoomLevel = false;
        DOTween.To(() => followCam.FollowOffset.y, x => followCam.FollowOffset.y = x, minigameZoomLevel, 1).SetEase(Ease.OutQuart);
    }

    /// <summary>
    /// Slowly force the camera to the zoom level passed in. The flag should be toggled when
    /// this forcing wants to be ended
    /// </summary>
    private IEnumerator ForceZoomRoutine(float zoomLevel) {
        _forcingZoomLevel = true;
        while (_forcingZoomLevel) {
            float dir = zoomLevel - followCam.FollowOffset.y;
            float dirMag = Mathf.Abs(dir);
            if (!(dirMag <= 0.05f)) {
                dir /= dirMag;
                followCam.FollowOffset.y += dir * 0.05f;
            }

            yield return new WaitForSeconds(0.01f);
        }

        yield return null;
    }

    /// <summary>
    /// Simple zoom tween.
    /// TODO: Save this off and reuse for better zoom state handling.
    /// </summary>
    public void PunchZoom(float delta, float t) {
        DOTween.To(() => followCam.FollowOffset.y, x => followCam.FollowOffset.y = x, followCam.FollowOffset.y + delta, t).SetEase(Ease.OutQuart);
    }
}
