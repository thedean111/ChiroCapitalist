using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set; }

    public UIDocument hud;


    // -----
    // PRIVATE
    // -----
    public VisualElement buildingContainer;
    private TileList tileList;
    private Button _toggleBuildButton;
    private Button _activateEditMode;
    private VisualElement _editPopup;
    // -----

    void Awake()
    {
        if (Instance == null) { Instance = this; }
    }


    /// <summary>
    /// Unity OnEnable method.
    /// </summary>
    void OnEnable()
    {
        buildingContainer = hud.rootVisualElement.Q<VisualElement>("building-container");
    }

    void Start()
    {
        //__________________________________________________________________________________________
        // BUILD MODE UI
        //__________________________________________________________________________________________
        _toggleBuildButton = hud.rootVisualElement.Q<Button>("build-button");
        _toggleBuildButton.clicked += () => { BuildingService.Instance.Toggle();};

        _activateEditMode = hud.rootVisualElement.Q<Button>("build-menu-edit-button");
        _activateEditMode.clicked += () => { BuildingService.Instance.ToggleEdit(true); };
                
        _editPopup = hud.rootVisualElement.Q<VisualElement>("edit-tile-opt-container");
        hud.rootVisualElement.Q<Button>("edit-tile-move").clicked += () => { BuildingService.Instance.PickupTile(); };
        ToggleEditPopup(false);
        ToggleBuildUI(false);
        tileList = GetComponent<TileList>();
        //__________________________________________________________________________________________


    }

    /// <summary>
    /// Update visible list elements such that they reflect up-to-date player progression.
    /// </summary>
    public void RefreshTileList()
    {
        tileList.Refresh();
    }

    /// <summary>
    /// Toggle the UI that contains the tile list for building.
    /// </summary>
    public void ToggleBuildUI(bool status)
    {
        buildingContainer.SetEnabled(status);
    }

    /// <summary>
    /// Use the event system to determine if the pointer is over any UI.
    /// </summary>
    public bool IsPointerOverUI()
    {   
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Query a HUD element.
    /// </summary>
    public VisualElement GetFromHud(string name)
    {
        return hud.rootVisualElement.Q(name);
    }

    /// <summary>
    /// Toggle class on edit button.
    /// </summary>
    public void ActivateEditButton(bool status) {
        if (status)
            _activateEditMode.AddToClassList("edit-mode-active");
        else
             _activateEditMode.RemoveFromClassList("edit-mode-active");
    }

    public void ToggleEditPopup(bool status) {
        _editPopup.SetEnabled(status);
    }

    public void ClearTileListSelection() {
        tileList.ClearSelection();
    }
}
