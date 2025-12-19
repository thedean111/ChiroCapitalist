using UnityEngine;

[CreateAssetMenu(fileName = "SoundDefinition", menuName = "Scriptable Objects/SoundDefinition")]
public class SoundDefinition : ScriptableObject
{
    public AudioClip clip;
    [Range(0,1)] public float volume;
}
