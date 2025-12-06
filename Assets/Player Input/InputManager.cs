using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputActionAsset inputs;

    // References to inputs and actions
    private InputActionMap gameActions;
    private InputAction buildMode;
    private InputAction addMoney;

    // Subscribe to input events
    void OnEnable()
    {
        gameActions = inputs.FindActionMap("Player");
        buildMode = gameActions.FindAction("BuildMode");
        addMoney = gameActions.FindAction("AddMoney");

        buildMode.performed += EnterBuildMode;
        addMoney.performed += AddMoney;
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
    private void AddMoney(InputAction.CallbackContext ctx) { ProgressionManager.Instance.AdjustMoney(500); }
}
