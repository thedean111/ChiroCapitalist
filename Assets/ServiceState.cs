using UnityEngine;
using UnityEngine.UIElements;

public abstract class ServiceState : MonoBehaviour
{
    public bool Active {get; protected set;}

    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    [Header("UI Information")]
    public string hudButtonName;
    public string buttonActiveClassName;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private Button _hudButton;
    //---------------------------------------------------------------------

    /// <summary>
    /// Calls initialization for a service.
    /// </summary>
    protected virtual void Start() {
        Active = false;
        InitService();
    }

    /// <summary>
    /// General initialization for every service.
    /// </summary>
    protected virtual void InitService() {
        if (hudButtonName.Length == 0 || buttonActiveClassName.Length == 0) { return; }
        
        _hudButton = (Button)UIManager.Instance.GetFromHud(hudButtonName);
    }


    /// <summary>
    /// Determines if this service can successfully transition to the target state.
    /// </summary>
    public virtual bool CanToggle(bool status) {
        return !(status == Active);
    }

    /// <summary>
    /// Defines the minimum behavior for toggling a service state.
    /// </summary>
    public virtual void Toggle(bool status) {
        Active = status;
        if (Active) {
            if (_hudButton != null)
                _hudButton.AddToClassList(buttonActiveClassName);
        } else {
            if (_hudButton != null)
                _hudButton.RemoveFromClassList(buttonActiveClassName);
        }
    }
}
