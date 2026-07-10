using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

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

    private CinemachineFollow followCam;

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


}
