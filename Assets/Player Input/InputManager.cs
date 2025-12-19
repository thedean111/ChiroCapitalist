using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {get; private set;}
    public InputActionAsset inputs;

    // References to inputs and actions
    private InputActionMap gameActions;
    private InputAction buildMode;
    private InputAction addMoney;
    private InputAction interactCell;
    private InputAction rotateTile;
    private InputAction cancelEdit;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    // Subscribe to input events
    void OnEnable()
    {
        gameActions = inputs.FindActionMap("Player");
        buildMode = gameActions.FindAction("BuildMode");
        addMoney = gameActions.FindAction("AddMoney");
        interactCell = gameActions.FindAction("AttemptCellInteraction");
        rotateTile = gameActions.FindAction("RotateTile");
        cancelEdit = gameActions.FindAction("CancelEdit");

        buildMode.performed += EnterBuildMode;
        addMoney.performed += AddMoney;
        interactCell.performed += InteractCell;
        rotateTile.performed += RotateTile;
        cancelEdit.performed += CancelEdit;

    }

    // Unsubscribe from input events
    void OnDisable()
    {
        buildMode.performed -= EnterBuildMode;
        interactCell.performed -= InteractCell;
        rotateTile.performed -= RotateTile;
        cancelEdit.performed -= CancelEdit;
    }

    // ===================================================================================================
    // WRAPPERS TO ALL THE UI INTERACTIONS
    // ===================================================================================================
    private void EnterBuildMode(InputAction.CallbackContext ctx) { BuildingService.Instance.Toggle(); }
    private void AddMoney(InputAction.CallbackContext ctx) { ProgressionManager.Instance.AdjustMoney(500); }
    private void InteractCell(InputAction.CallbackContext ctx) { BuildingService.Instance.InteractCell(); }
    private void RotateTile(InputAction.CallbackContext ctx) { BuildingService.Instance.RotateSelection(); }
    private void CancelEdit(InputAction.CallbackContext ctx) { BuildingService.Instance.CancelEdit(); }
}
