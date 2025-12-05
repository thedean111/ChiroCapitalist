using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set; }

    public UIDocument hud;

    public TileList tileList;

    // -----
    // PRIVATE
    // -----
    public VisualElement buildingContainer;
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
        BuildingManager.Instance.SetButton(hud.rootVisualElement.Q<Button>("build-button"));
        ToggleBuildUI(false);
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

    public bool IsPointerOverUI()
    {   
        return EventSystem.current.IsPointerOverGameObject();
    }

}
