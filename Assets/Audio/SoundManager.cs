using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public static SoundManager Instance {get; private set;}
    [Header("Mixing Groups")]
    public AudioMixerGroup uiGroup;
    public AudioMixerGroup buildGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup masterGroup;

    [Header("Audio Sources")]
    public AudioSource buildEffects;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------

    //*********************************************************************

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    /// <summary>
    /// Unity awake method.
    /// </summary>
    public void PlayBuildEffect(SoundDefinition def) {
        buildEffects.PlayOneShot(def.clip, def.volume);
    }
}
