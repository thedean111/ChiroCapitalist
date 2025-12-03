using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputActionAsset inputs;

    // References to inputs and actions
    private InputActionMap gameActions;
    private InputAction buildMode;

    // Subscribe to input events
    void OnEnable()
    {
        gameActions = inputs.FindActionMap("Player");
        buildMode = gameActions.FindAction("BuildMode");

        buildMode.performed += EnterBuildMode;
    }

    // Unsubscribe from input events
    void OnDisable()
    {
        buildMode.performed -= EnterBuildMode;
    }

    // ===================================================================================================
    // WRAPPERS TO ALL THE UI INTERACTIONS
    // ===================================================================================================
    private void EnterBuildMode(InputAction.CallbackContext ctx) { BuildingManager.Instance.Toggle(); }
}
