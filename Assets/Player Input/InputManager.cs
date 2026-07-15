using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IMinigameActions
{
    public static InputManager Instance {get; private set;}
    private InputSystem_Actions controls;
    private InputActionMap  currentActionMap;
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        controls = new InputSystem_Actions();
        controls.Player.SetCallbacks(this);
        controls.Minigame.SetCallbacks(this);
    }

    // Subscribe to input events
    void OnEnable()
    {
        foreach (var map in InputSystem.actions.actionMaps)
        {
            map.Disable();
        }

        ToggleActionMap("Player");
    }

    void OnDisable()
    {
        if (currentActionMap != null) currentActionMap.Disable();
    }

    public void ToggleActionMap(string mapName) {
        var newMap = controls.asset.FindActionMap(mapName);
        if (newMap == null || currentActionMap == newMap) return;

        // Turn off whatever map is currently running, regardless of what it is
        if (currentActionMap != null)
        {
            currentActionMap.Disable();
        }

        // Enable the new map and track it
        currentActionMap = newMap;
        currentActionMap.Enable();
    }

    // ===================================================================================================
    // C A M E R A   C O N T R O L S
    // ===================================================================================================
    public void OnMousePan(InputAction.CallbackContext context)
    {
        if (context.performed) {
            CameraController.Instance.holding = true;
        } else if (context.canceled) {
            CameraController.Instance.holding = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.canceled) {
            CameraController.Instance.discreteMoveInput = Vector2.zero;
        } else if (context.performed) {
            CameraController.Instance.discreteMoveInput = context.ReadValue<Vector2>();
        }
    }

    public void OnPan(InputAction.CallbackContext context)
    {
        if (context.performed) {
            CameraController.Instance.OnPan(context.ReadValue<Vector2>());
        }
    }

    public void OnMouseZoom(InputAction.CallbackContext context)
    {
        if (context.performed) {
            CameraController.Instance.OnZoom(context.ReadValue<Vector2>().y);
        }
    }

    // ===================================================================================================
    // B U I L D I N G   C O N T R O L S
    // ===================================================================================================
    public void OnBuildMode(InputAction.CallbackContext context)
    {
        if (context.performed) {
            ServiceManager.Instance.ToggleService<BuildingService>(!BuildingService.Instance.Active);
        }
    }

    public void OnAddMoney(InputAction.CallbackContext context)
    {
        if (context.performed)
            ProgressionManager.Instance.AdjustMoney(500);
    }

    public void OnAttemptCellInteraction(InputAction.CallbackContext context)
    {
        if (context.performed) {
            BuildingService.Instance.InteractCell();
            EditService.Instance.InteractCell();
            PlayspaceService.Instance.Interact();
        }
    }

    public void OnRotateTile(InputAction.CallbackContext context)
    {
        if (context.performed) {
            ConstructionManager.Instance.RotateSelection();
        }
    }

    public void OnCancelEdit(InputAction.CallbackContext context)
    {
        if (context.performed) {
            EditService.Instance.CancelEdit();
        }
    }

    public void OnToggleEditMode(InputAction.CallbackContext context)
    {
        if (context.performed) {
            ServiceManager.Instance.ToggleService<EditService>(!EditService.Instance.Active);
        }
    }

    // ===================================================================================================
    // M I N I   G A M E S
    // ===================================================================================================
    public void OnClick_MG(InputAction.CallbackContext context)
    {
        if (context.performed) {
            MinigameService.Instance.ClickLogic();
        }
    }
}
