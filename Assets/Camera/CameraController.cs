using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cCam;
    public Transform camTarg;
    public InputActionAsset inputs;

    [Space(15)][Header("Params")]
    public Vector2 xLimits;
    public Vector2 zLimits;
    public float clickDragSpeed = 0.5f; // Units per second
    public float inputSpeed = 2f;

    private bool holding = false;

    // References to inputs and actions
    private InputActionMap gameActions;
    private InputAction move;
    private InputAction holdRight;
    private InputAction pan;

    // Subscribe to input events
    void OnEnable()
    {
        gameActions = inputs.FindActionMap("Player");
        move = gameActions.FindAction("Move");
        holdRight = gameActions.FindAction("HoldRight");
        pan = gameActions.FindAction("Pan");

        holdRight.performed += OnHoldStarted;
        holdRight.canceled += OnHoldCanceled;
        pan.performed += OnPan;
    }

    // Unsubscribe from input events
    void OnDisable()
    {
        holdRight.performed -= OnHoldStarted;
        holdRight.canceled -= OnHoldCanceled;
        pan.performed -= OnPan;
    }

    void OnPan(InputAction.CallbackContext ctx)
    {
        if (!holding) { return; }

        // Inverting the value feels better here
        MoveCameraTarget(-ctx.ReadValue<Vector2>(), clickDragSpeed);
    }

    private void OnHoldStarted(InputAction.CallbackContext ctx) => holding = true;

    private void OnHoldCanceled(InputAction.CallbackContext ctx) => holding = false;


    void Update()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();
        if (moveInput != Vector2.zero)
        {
            MoveCameraTarget(moveInput, inputSpeed);
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


}
