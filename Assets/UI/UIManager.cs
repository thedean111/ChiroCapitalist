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

    private VisualElement _tileDetailsPanel;
    private Label _tileDescription;
    private VisualElement _tileProgressContainer;
    private VisualElement _tileLevelContainer;
    private Label _tileLevel;

    private bool _followMouse = false;
    private TileInstance _focusedTile;
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

        //__________________________________________________________________________________________
        // TILE DETAILS UI
        //__________________________________________________________________________________________
        _tileDetailsPanel = hud.rootVisualElement.Q<VisualElement>("tile-details-container");
        _tileDescription = hud.rootVisualElement.Q<Label>("tile-details-description");
        _tileLevel = hud.rootVisualElement.Q<Label>("tile-details-level-text");
        _tileLevelContainer = hud.rootVisualElement.Q<VisualElement>("tile-details-level-container");
        _tileProgressContainer = hud.rootVisualElement.Q<VisualElement>("tile-details-progress-container");

        hud.rootVisualElement.Q<Button>("tile-details-minimize-button").clicked += () => ToggleTileDetailsPanel(false);
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

    /// <summary>
    /// Turn the details panel on or off.
    /// </summary>
    public void ToggleTileDetailsPanel(bool status, int tileID = -1) {
        if (_tileDetailsPanel.enabledSelf == status) { return; }

        if (tileID != -1 && _focusedTile == ConstructionManager.Instance._tilePlacer.tiles[tileID]) { return; }

        if (status) {
            _focusedTile = ConstructionManager.Instance._tilePlacer.tiles[tileID];
            _focusedTile.instance.OnProgressChange += UpdateProgressBar;

            UpdateTileDetailsPanel();
        } else {
            _focusedTile.instance.OnProgressChange -= UpdateProgressBar;
            _focusedTile = null;
        }

        _tileDetailsPanel.SetEnabled(status);
    }

    /// <summary>
    /// Update the information in the panel with the current state of the focused tile.
    /// </summary>
    private void UpdateTileDetailsPanel() {
        _tileDescription.text = _focusedTile.def.description;

        // If the tile contains level-based information then show that section of the panel and update it
        if ((_focusedTile.instance.detailFlags & TileDetailsFlags.Level) != 0) {
            _tileLevelContainer.SetEnabled(true);
        } else {
            _tileLevelContainer.SetEnabled(false);
        }

        // If the tile contains logic that uses the progress bar then show that section of the panel and update it
        if ((_focusedTile.instance.detailFlags & TileDetailsFlags.Progress) != 0) {
            _tileProgressContainer.SetEnabled(true);
        } else
        {
            _tileProgressContainer.SetEnabled(false);
        }
    }

    /// <summary>
    /// What to do when updating the progress bar.
    /// </summary>
    private void UpdateProgressBar(float progress) {
    }

}
