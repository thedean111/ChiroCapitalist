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
    private InputAction placeTile;
    private InputAction rotateTile;

    // Subscribe to input events
    void OnEnable()
    {
        gameActions = inputs.FindActionMap("Player");
        buildMode = gameActions.FindAction("BuildMode");
        addMoney = gameActions.FindAction("AddMoney");
        placeTile = gameActions.FindAction("AttemptTilePlacement");
        rotateTile = gameActions.FindAction("RotateTile");

        buildMode.performed += EnterBuildMode;
        addMoney.performed += AddMoney;
        placeTile.performed += PlaceTile;
        rotateTile.performed += RotateTile;

    }

    // Unsubscribe from input events
    void OnDisable()
    {
        buildMode.performed -= EnterBuildMode;
        placeTile.performed -= PlaceTile;
        rotateTile.performed -= RotateTile;
    }

    // ===================================================================================================
    // WRAPPERS TO ALL THE UI INTERACTIONS
    // ===================================================================================================
    private void EnterBuildMode(InputAction.CallbackContext ctx) { BuildingManager.Instance.Toggle(); }
    private void AddMoney(InputAction.CallbackContext ctx) { ProgressionManager.Instance.AdjustMoney(500); }
    private void PlaceTile(InputAction.CallbackContext ctx) { BuildingManager.Instance.AttemptPlacement(); }
    private void RotateTile(InputAction.CallbackContext ctx) { BuildingManager.Instance.RotateTile(); }
}
