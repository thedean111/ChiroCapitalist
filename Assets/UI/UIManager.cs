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
        // Build mode related stuff
        _toggleBuildButton = hud.rootVisualElement.Q<Button>("build-button");
        _toggleBuildButton.clicked += () => { BuildingService.Instance.Toggle();};
        hud.rootVisualElement.Q<VisualElement>("edit-tile-opt-container").SetEnabled(false);

        ToggleBuildUI(false);
        tileList = GetComponent<TileList>();
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
}
