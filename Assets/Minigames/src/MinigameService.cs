using System.Collections.Generic;
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

    /// <summary>
    /// Determine what game to play based on the stats of the selected patient.
    /// </summary>
    public void PlayMinigame(NPCStats stats, Vector3 position) {
        if (!Active) { return; }

        StatCategory domStat = stats.GetDominantStat();
        _activeGameIndex = 0;
        for (int i = 0; i < minigames.Count; i++) {
            if (minigames[i].mainStat == domStat) {
                _activeGameIndex = i;
                break;
            }
        }
        _patientPosition = position;
        minigames[_activeGameIndex].OpenGame(stats);
    }

    /// <summary>
    /// Listen to each of the minigames exit event.
    /// </summary>
    protected override void InitService() {
        base.InitService();

        foreach (BaseMinigame game in minigames) {
            game.onMinigameEnd += EndMinigameService;
        }
    }

    /// <summary>
    /// Turn on the minigame action map.
    /// </summary>
    public override void Toggle(bool status)
    {
        base.Toggle(status);

        if (Active) {
            InputManager.Instance.ToggleActionMap("Minigame");
            UIManager.Instance.ToggleMinigameInfo(false, null);
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
        UIManager.Instance.PlayRewardParticles(_patientPosition, (int)(gold / 10f), (int)(reputation / 10f));

        // TODO: Play visual effects for awarding the player money and reputation after a manual adjustment.
    }

    public void ClickLogic() {
        minigames[_activeGameIndex].OnClick();
    }
}