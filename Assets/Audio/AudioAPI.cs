using UnityEngine;

public class AudioAPI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayRandomCrack() {
        SoundManager.Instance.PlayRandomCrack();
    }
}
