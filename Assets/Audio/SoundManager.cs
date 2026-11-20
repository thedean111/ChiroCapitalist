using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections;
using Unity.Collections;

public class SoundManager : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public static SoundManager Instance {get; private set;}
    [Header("Parameters")]
    public float fadeTime = 1.5f;
    public AnimationCurve musicTimerScale;

    [Header("Audio Sources")]
    public AudioSource music;
    [Space(5)]public AudioSource crackFX;
    public Vector2 pitchRange;
    [Space(5)]public AudioSource sfx;

    [Header("Audio Clips")]
    public List<AudioClip> cracks = new List<AudioClip>();
    public MusicData baseMusic;
    public List<MusicData> minigameMusic;
    public AudioClip error;
    [Space(5)]public AudioClip chime;
    public Vector2 chimePitchRange;
    [Space(5)]public AudioClip success;
    public AudioClip patientSelect;
    public AudioClip patientSpawn;
    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    //*********************************************************************

    private void Start()
    {
        music.clip = baseMusic.clip;
        music.Play();

        foreach(MusicData data in minigameMusic) {
            data.source.clip = data.clip;
            data.source.Play();
            data.source.volume = 0f;
        }
    }

    public void UpdateMusicWeightScale(float w) {
        float pitch = musicTimerScale.Evaluate(w);
        baseMusic.source.pitch = pitch;
        foreach (MusicData md in minigameMusic) {
            md.source.pitch = pitch;
        }
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    /// <summary>
    /// Play a random cracking noise at a random pitch
    /// </summary>
    public void PlayRandomCrack() {
        crackFX.pitch = Random.Range(pitchRange.x, pitchRange.y);
        crackFX.PlayOneShot(cracks[Random.Range(0, cracks.Count)]);
    }

    public void PlayPatientSelect() {
        sfx.pitch = 1.0f;
        sfx.PlayOneShot(patientSelect);
    }

    public void PlayError() {
        sfx.pitch = 1.0f;
        sfx.PlayOneShot(error);
    }

    public void PlaySpawnSound() {
        sfx.pitch = 1.0f;
        sfx.PlayOneShot(patientSpawn);
    }

    public void PlaySuccess() {
        sfx.pitch = 1.0f;
        sfx.PlayOneShot(success);
    }

    public void PlayChime() {
        sfx.pitch = Random.Range(chimePitchRange.x, chimePitchRange.y);
        sfx.PlayOneShot(chime);
    }

    public void FadeMusicIn(MusicData data) {
        data.source.volume = 0;
        DOVirtual.Float(0f, 1, fadeTime, (x) => {
            data.source.volume = x;
        });
    }

    public void FadeMusicOut(MusicData data) {
        data.source.volume = 1f;
        DOVirtual.Float(1f, 0f, fadeTime, (x) => {
            data.source.volume = x;
        });
    }
}

[System.Serializable]
public class MusicData {
    public AudioSource source;
    public AudioClip clip;
}
