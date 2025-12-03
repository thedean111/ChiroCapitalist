using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIDocument hud;

    /// <summary>
    /// Unity OnEnable method.
    /// </summary>
    void OnEnable()
    {
        // Wire up all the hud buttons to their respective managers
        hud.rootVisualElement.Q<Button>("build-button").clicked += () => BuildingManager.Instance.Toggle();
    }
}
