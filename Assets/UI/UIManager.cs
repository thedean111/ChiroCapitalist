using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set; }

    public UIDocument hud;
    public VisualTreeAsset adjustmentButtonTemplate;

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

    private Label _tileName;
    private VisualElement _tileDetailsPanel;
    private Label _tileDescription;
    private VisualElement _tileProgressContainer;
    private VisualElement _tileLevelContainer;
    private VisualElement _tileDoctorContainer;
    private Label _tileLevel;
    private Label _tileLevelUpCost;
    private ProgressBar _tileProgress;
    private VisualElement _tileDoctorInfo;
    private Button _tileAssignDoctorBtn;
    private Label _tileDetailsDoctorName;
    private Label _tileDetailsDoctorLevel;
    private VisualElement _tileDetailsDoctorIcon;
    private Button _tileDetailsLevelUpBtn;

    private Label _moneyDisplay;

    private bool _followMouse = false;
    private TileInstance _focusedTile;
    private int currentLevelUpCost;
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

    /// <summary>
    /// Configure all UI that uses the practice data.
    /// </summary>
    public void ConfigureDataLabels(PracticeData data) {
        _moneyDisplay = hud.rootVisualElement.Q<Label>("money-display");
        _moneyDisplay.dataSource = data;
        _moneyDisplay.SetBinding("text", new DataBinding {
            dataSourcePath = new Unity.Properties.PropertyPath(nameof(data.tweenMoney))
        });
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
        _tileName = hud.rootVisualElement.Q<Label>("tile-details-header-text");
        _tileDetailsPanel = hud.rootVisualElement.Q<VisualElement>("tile-details-container");
        _tileDescription = hud.rootVisualElement.Q<Label>("tile-details-description");
        _tileLevel = hud.rootVisualElement.Q<Label>("tile-details-level-text");
        _tileLevelContainer = hud.rootVisualElement.Q<VisualElement>("tile-details-level-container");
        _tileProgressContainer = hud.rootVisualElement.Q<VisualElement>("tile-details-progress-container");
        _tileProgress = hud.rootVisualElement.Q<ProgressBar>("tile-details-progress-bar");
        _tileDoctorContainer = hud.rootVisualElement.Q<VisualElement>("tile-details-doctor-container");
        _tileDoctorInfo = hud.rootVisualElement.Q<VisualElement>("tile-details-doctor-info");
        _tileAssignDoctorBtn = hud.rootVisualElement.Q<Button>("tile-details-assign-doctor");
        _tileDetailsDoctorName = hud.rootVisualElement.Q<Label>("tile-details-doctor-name");
        _tileDetailsDoctorLevel = hud.rootVisualElement.Q<Label>("tile-details-doctor-level");
        _tileDetailsDoctorIcon = hud.rootVisualElement.Q<VisualElement>("tile-details-doctor-icon");
        _tileDetailsLevelUpBtn = hud.rootVisualElement.Q<Button>("tile-details-upgrade-button");
        _tileLevelUpCost = hud.rootVisualElement.Q<Label>("tile-level-up-cost");
        hud.rootVisualElement.Q<Button>("tile-details-minimize-button").clicked += () => ToggleTileDetailsPanel(false);

        // TODO: This should actually open a records menu/panel of currently owned doctors
        _tileAssignDoctorBtn.clicked += () => _focusedTile.instance.UpdateDoctorAssignment(NPCFactory.Instance.GenerateDoctorData()); // TEMP
        _tileDetailsLevelUpBtn.clicked += () => {
            if (_focusedTile.instance.LevelUp()) {
                ProgressionManager.Instance.AdjustMoney(-currentLevelUpCost);
                UpdateTileDetailsPanel();
            }
        };
            

        hud.rootVisualElement.Q<Button>("tile-details-remove-button").clicked += () => _focusedTile.instance.UpdateDoctorAssignment(null);
        // hud.rootVisualElement.Q<Button>("tile-details-info-button").clicked +=

        _tileDetailsPanel.SetEnabled(false);
        //__________________________________________________________________________________________

    }

    public TileInstance GetFocusedTile() {
        return _focusedTile;
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
        if (tileID != -1 && _focusedTile == ConstructionManager.Instance._tilePlacer.tiles[tileID]) { return; }
        
        // Either focusing a new tile or closing the menu
        if (_focusedTile != null) {
            _focusedTile.instance.OnProgressChange -= UpdateProgressBarProgress;
            _focusedTile.instance.OnProgressComplete -= UpdateProgressBarText;
            _focusedTile = null;

        }

        // If we want to show the details (on a clicked tile), update the panel
        if (status) {
            _focusedTile = ConstructionManager.Instance._tilePlacer.tiles[tileID];
            _focusedTile.instance.OnProgressChange += UpdateProgressBarProgress;
            _focusedTile.instance.OnProgressComplete += UpdateProgressBarText;

            UpdateTileDetailsPanel();
        } else {
            PlayspaceService.Instance.ResetFocus();
        }

        _tileDetailsPanel.SetEnabled(status);
    }

    /// <summary>
    /// Update the information in the panel with the current state of the focused tile.
    /// </summary>
    public void UpdateTileDetailsPanel() {
        if (_focusedTile == null) { return; }

        _tileName.text = _focusedTile.def.tileName;
        _tileDescription.text = _focusedTile.def.description;

        // If the tile contains level-based information then show that section of the panel and update it
        if ((_focusedTile.def.detailFlags & TileDetailsFlags.Level) != 0) {
            _tileLevelContainer.SetEnabled(true);
            _tileLevel.text = $"Lv. {_focusedTile.instance.Level}";
            currentLevelUpCost = ProgressionManager.Instance.GetTileLevelUpCost(_focusedTile.instance.Level);
            if (currentLevelUpCost == -1) {
                _tileDetailsLevelUpBtn.enabledSelf = false;
                _tileLevelUpCost.text = $"MAX";
            } else {
                _tileDetailsLevelUpBtn.enabledSelf = ProgressionManager.Instance.CanAfford(currentLevelUpCost);
                _tileLevelUpCost.text = $"{currentLevelUpCost}";
            }
        } else {
            _tileLevelContainer.SetEnabled(false);
        }

        // If the tile contains logic that uses the progress bar then show that section of the panel and update it
        if ((_focusedTile.def.detailFlags & TileDetailsFlags.Progress) != 0) {
            _tileProgressContainer.SetEnabled(true);
        } else
        {
            _tileProgressContainer.SetEnabled(false);
        }

        // If the tile contains logic that uses the progress bar then show that section of the panel and update it
        if ((_focusedTile.def.detailFlags & TileDetailsFlags.Doctor) != 0) {
            _tileDoctorContainer.SetEnabled(true);
        } else
        {
            _tileDoctorContainer.SetEnabled(false);
        }
    }

    /// <summary>
    /// Just set the text to the passed in level. Could probably use data binding as well.
    /// </summary>
    public void UpdateLevelText(int Level) {
        _tileLevel.text = $"Lv. {Level}";
    }

    /// <summary>
    /// What to do when updating the progress bar. Takes a float in the range [0,100].
    /// </summary>
    public void UpdateProgressBarProgress(float progress) {
        _tileProgress.value = progress;
    }

    /// <summary>
    /// How to update the progress bar when the selected tiles progress is completed
    /// </summary>
    public void UpdateProgressBarText(string text) {
        _tileProgress.title = text;
    }

    public void UpdateProgressBarText() {
        _focusedTile.instance.ProgressCompleted(_tileProgress);
    }

    /// <summary>
    /// Given a doctor, update the details panel with the doctor information.
    /// </summary>
    public void UpdateDoctorDetails(DoctorData doctorData) {
        if (doctorData == null) {
            _tileAssignDoctorBtn.SetEnabled(true);
            _tileDoctorInfo.SetEnabled(false);
        } else {
            _tileAssignDoctorBtn.SetEnabled(false);
            _tileDoctorInfo.SetEnabled(true);

            _tileDetailsDoctorName.text = doctorData.name;
            _tileDetailsDoctorLevel.text = $"Lv. {doctorData.level}";
            _tileDetailsDoctorIcon.style.backgroundImage = doctorData.icon;

        }
    }

}
