using System;
using System.Runtime.CompilerServices;
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
    private Button _toggleEditButton;
    private Button _activateEditMode;
    private Button _deleteTileButton;
    private VisualElement _selectedTileMouseElement;
    private bool _followMouse = false;
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
        // PLACEMENT MODE UI
        //__________________________________________________________________________________________
        _toggleBuildButton = hud.rootVisualElement.Q<Button>("placement-button");
        _toggleBuildButton.clicked += () => { 
            ServiceManager.Instance.ToggleService<BuildingService>(!BuildingService.Instance.Active);
        };

        _activateEditMode = hud.rootVisualElement.Q<Button>("build-menu-edit-button");

        tileList = GetComponent<TileList>();
        ToggleTileCatalog(false);
        //__________________________________________________________________________________________

        //__________________________________________________________________________________________
        // EDIT MODE UI
        //__________________________________________________________________________________________
        _toggleEditButton = hud.rootVisualElement.Q<Button>("edit-button");
        _toggleEditButton.clicked += () => { 
            ServiceManager.Instance.ToggleService<EditService>(!EditService.Instance.Active);
        };

        _deleteTileButton = hud.rootVisualElement.Q<Button>("edit-mode-delete-button");
        _deleteTileButton.clicked += EditService.Instance.DeletePayload;
        ToggleEditServiceUI(false);

        _selectedTileMouseElement = hud.rootVisualElement.Q<VisualElement>("selected-tile-mouse-element");
        _selectedTileMouseElement.SetEnabled(false);
        //__________________________________________________________________________________________

    }

    private void Update() {
        if (_followMouse) {
            Vector2 panel = ScreenToPanel(Mouse.current.position.ReadValue());
            _selectedTileMouseElement.style.left = panel.x;
            _selectedTileMouseElement.style.top  = panel.y;
        }
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
    public void ToggleTileCatalog(bool status)
    {
        buildingContainer.SetEnabled(status);
        ClearTileListSelection();
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
    /// Toggle relevant UI elements for the edit service.
    /// </summary>
    public void ToggleEditServiceUI(bool status) {
        _deleteTileButton.SetEnabled(status);
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

    /// <summary>
    /// Resets UI elements relating to the tile list in placement mode.
    /// </summary>
    public void ClearTileListSelection() {
        tileList.ClearSelection();
    }

    /// <summary>
    /// Turn on or off the element to follow the mouse
    /// </summary>
    public void ToggleSelectedTileElement(bool status) {
        _selectedTileMouseElement.SetEnabled(status);
        _followMouse = status;
        Vector2 panel = ScreenToPanel(Mouse.current.position.ReadValue());
        _selectedTileMouseElement.style.left = panel.x;
        _selectedTileMouseElement.style.top  = panel.y;
    }

    /// <summary>
    /// Sets the icon to use for the selected tile visual element.
    /// </summary>
    public void SetSelectedTileElement(Texture2D icon) {
        _selectedTileMouseElement.style.backgroundImage = icon;
    }

    private Vector2 ScreenToPanel(Vector2 screenPos) {
        screenPos.y = Screen.height - screenPos.y;

        // Convert Screen -> Panel coordinates
        Vector2 panel = RuntimePanelUtils.ScreenToPanel(hud.rootVisualElement.panel, screenPos);
        return panel;
    }

}
