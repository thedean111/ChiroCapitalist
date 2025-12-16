using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TileList : MonoBehaviour
{
    [SerializeField] private UIDocument hud;
    [SerializeField] private VisualTreeAsset tileTemplate;
    [SerializeField] private List<TileDefinition> tiles;
    
    private ScrollView sv;
    private VisualElement currentSelection = null;

    void OnEnable()
    {
        sv = hud.rootVisualElement.Q<ScrollView>("build-tile-list");
    }

    void Start()
    {
        Build();
    }

    public void UpdateTileList(List<TileDefinition> t)
    {
        tiles = t;
        Build();
    }

    public void Build() {
        sv.Clear();
        foreach (TileDefinition tile in tiles)
        {
            VisualElement item = tileTemplate.Instantiate();
            BindItem(item, tile);
            sv.Add(item);
        }
    }

    public void Refresh()
    {
        foreach (VisualElement ve in sv.Children())
        {
            UpdateTileButtonStatus(ve);
        }
    }

    private void BindItem(VisualElement ve, TileDefinition data)
    {
        ve.userData = data;
        ve.Q<Button>("build-tile-item").RegisterCallback<ClickEvent>(evt =>
        {
            OnItemClicked(evt);
            BuildingService.Instance.SelectTile(data);
        });

        Label cost = ve.Q<Label>("build-tile-cost");
        VisualElement icon = ve.Q<VisualElement>("build-tile-icon");

        icon.style.backgroundImage = data.icon;
        cost.text = data.cost.ToString();

        UpdateTileButtonStatus(ve);
    }

    private void UpdateTileButtonStatus(VisualElement ve)
    {
        bool canAfford = (ve.userData as TileDefinition).cost <= ProgressionManager.Instance.Money;
        ve.SetEnabled(canAfford);
        ve.Q<VisualElement>("build-tile-icon").SetEnabled(canAfford);
    }

    private void OnItemClicked(ClickEvent evt)
    {
        VisualElement ve = (VisualElement)evt.currentTarget;

        if (currentSelection != null) { currentSelection.RemoveFromClassList("build-tile-selected"); }
        currentSelection = ve;
        currentSelection.AddToClassList("build-tile-selected");
    }
}
