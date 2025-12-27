using System.Collections.Generic;
using UnityEngine;

/// 
public class ServiceManager : MonoBehaviour
{
    public static ServiceManager Instance {get; private set;}

    public List<ServiceState> services;
    public ServiceState defaultService; // the service that should be operating by default when nothing else is on

    private void Awake() { if (Instance == null) { Instance = this; }}

    /// <summary>
    /// Unity start method.
    /// </summary>
    void Start()
    {
        defaultService.Toggle(true);
    }

    /// <summary>
    /// Given a service type, toggle its mode. Ensure all other states are toggled off regardless of the target.
    /// This method may potentially have more complicated constraints depending on the service type.
    /// </summary>
    public void ToggleService<T>(bool status) where T : ServiceState {
        bool anyOn = false;
        for (int i = 0; i < services.Count; i++) {
            if (services[i] == null) { continue; }
            bool targ = services[i].GetType() == typeof(T) && status;
            anyOn |= targ;
            services[i].Toggle(targ);
        }

        // Default service is active when no other service is operating.
        if (!anyOn) {
            defaultService.Toggle(true);
        } else {
            defaultService.Toggle(false);
        }
    }
}
