using System.Collections.Generic;
using UnityEngine;

/// 
public class ServiceManager : MonoBehaviour
{
    public static ServiceManager Instance {get; private set;}

    public List<ServiceState> services;

    private void Awake() { if (Instance == null) { Instance = this; }}

    /// <summary>
    /// Given a service type, toggle its mode. Ensure all other states are toggled off regardless of the target.
    /// This method may potentially have more complicated constraints depending on the service type.
    /// </summary>
    public void ToggleService<T>(bool status) where T : ServiceState {
        for (int i = 0; i < services.Count; i++) {
            if (services[i] == null) { continue; }
            bool targ = services[i].GetType() == typeof(T) && status;
            services[i].Toggle(targ);
        }
    }
}
