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
    private InputAction toggleEditMode;

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
        toggleEditMode = gameActions.FindAction("ToggleEditMode");

        buildMode.performed += TogglePlacementMode;
        toggleEditMode.performed += ToggleEditMode;
        addMoney.performed += AddMoney;
        interactCell.performed += InteractCell;
        rotateTile.performed += RotateTile;
        cancelEdit.performed += CancelEdit;

    }

    // Unsubscribe from input events
    void OnDisable()
    {
        buildMode.performed -= TogglePlacementMode;
        toggleEditMode.performed -= ToggleEditMode;
        interactCell.performed -= InteractCell;
        rotateTile.performed -= RotateTile;
        cancelEdit.performed -= CancelEdit;
    }

    // ===================================================================================================
    // WRAPPERS TO ALL THE UI INTERACTIONS
    // ===================================================================================================
    private void TogglePlacementMode(InputAction.CallbackContext ctx) { 
        ServiceManager.Instance.ToggleService<BuildingService>(!BuildingService.Instance.Active);
    }
    private void ToggleEditMode(InputAction.CallbackContext ctx) { 
        ServiceManager.Instance.ToggleService<EditService>(!EditService.Instance.Active);
    }
    
    private void AddMoney(InputAction.CallbackContext ctx) { ProgressionManager.Instance.AdjustMoney(500); }
    private void InteractCell(InputAction.CallbackContext ctx) { 
        BuildingService.Instance.InteractCell();
        EditService.Instance.InteractCell();
    }
    
    private void RotateTile(InputAction.CallbackContext ctx) { ConstructionManager.Instance.RotateSelection(); }
    private void CancelEdit(InputAction.CallbackContext ctx) { EditService.Instance.CancelEdit(); }
}
