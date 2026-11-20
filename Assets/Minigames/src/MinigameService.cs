using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Service that manages the minigames. Helps route functionality and player input between minigames
/// This is a singleton because there is one service.
/// </summary>
public class MinigameService : ServiceState
{
    public static MinigameService Instance { get; private set; }
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    [Header("Minigame List")]
    public List<BaseMinigame> minigames = new();

    [Header("References")]
    public Animator doctorAnim;
    public Animator patientAnim;
    [Range(0f, 2f)] public float transitionDuration;
    public event Action<RoomType> onRoomTransition;

    //*********************************************************************
    // Private
    //---------------------------------------------------------------------
    private int _activeGameIndex = 0;
    private Vector3 _patientPosition;
    //*********************************************************************


    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Determine what game to play based on the stats of the selected patient.
    /// </summary>
    public void PlayMinigame(Patient patient, Vector3 position) {
        if (!Active) { return; }


        doctorAnim.SetTrigger("isDoctor");
        patientAnim.SetTrigger("isPatient");

        StatCategory domStat = patient.GetStats().GetDominantStat();
        _activeGameIndex = 0;
        for (int i = 0; i < minigames.Count; i++) {
            if (minigames[i].mainStat == domStat) {
                _activeGameIndex = i;
                break;
            }
        }
        SoundManager.Instance.FadeMusicIn(SoundManager.Instance.minigameMusic[_activeGameIndex]);
        _patientPosition = position;
        minigames[_activeGameIndex].OpenGame(patient);

        // Transition to the back room
        onRoomTransition?.Invoke(RoomType.Adjustment);

        // Unlock the zoom/focusing behavior
        CameraController.Instance.StartMinigameCameraBehavior();
    }

    /// <summary>
    /// Listen to each of the minigames exit event.
    /// </summary>
    protected override void InitService() {
        base.InitService();

        foreach (BaseMinigame game in minigames) {
            game.onMinigameEnd += EndMinigameService;
            game.Init(doctorAnim, patientAnim);
        }
    }

    /// <summary>
    /// Turn on the minigame action map.
    /// </summary>
    public override void Toggle(bool status)
    {
        base.Toggle(status);

        if (Active) {
            SoundManager.Instance.PlayPatientSelect();
            InputManager.Instance.ToggleActionMap("Minigame");
            UIManager.Instance.ToggleMinigameInfo(false, null);
            UIManager.Instance.ToggleHud(false);
        }
    }

    /// <summary>
    /// When any of the minigames end they will call this method to end the service. The game will then go to playspace service
    /// until the player decides to play another minigame
    /// </summary>
    private void EndMinigameService() {
        AwardPlayer();
        PlayspaceService.Instance.ResolveFocusedPatient();
        CameraController.Instance.EndMinigameCameraBehavior();
        ServiceManager.Instance.ToggleService<MinigameService>(false);
        UIManager.Instance.ToggleHud(true);
        SoundManager.Instance.FadeMusicOut(SoundManager.Instance.minigameMusic[_activeGameIndex]);
        // Transition to the front room
        onRoomTransition?.Invoke(RoomType.Front);
        // TransitionRooms(0);
    }

    /// <summary>
    /// Determines how much rewards to give the player and distributes the data properly.
    /// </summary>
    private void AwardPlayer() {
        int gold, reputation;
        gold = (int)(minigames[_activeGameIndex].score * ProgressionManager.Instance.baseGoldReward);
        reputation = (int)(minigames[_activeGameIndex].score * ProgressionManager.Instance.baseReputationReward);
        Debug.Log("Awarding " + gold + " gold, and " + reputation + " reputation!");

        // TODO: Based on performance, determine how many particles to spawn
        UIManager.Instance.PlayRewardParticles(gold, reputation);

        // TODO: Play visual effects for awarding the player money and reputation after a manual adjustment.
    }

    public void ClickLogic() {
        minigames[_activeGameIndex].OnClick();
    }
}