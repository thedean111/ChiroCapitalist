using System;
using UnityEngine;
using DG.Tweening;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    
    public PracticeData activeSaveData;

    [Header("Stat Scaling")]
    [Range(1, 999)] public int maxStatValue = 99; // The cap for each npc stat
    [Range(1, 50f)] public float basePatientStatPool = 10f; // Base value to scale stat pool off of
    [Range(0f, 2f)] public float rankStatMultiplier = 0.2f; // Multiplier that additively increases with rank
    [Range(0f, 5f)] public float tierStatMultiplier = 0.5f; // Multiplier that additively increases with tier
    [Range(.1f, 1f)] public float statTotalMinVariance = 1f; // What percent of the stat total should correlate to the minimum bound

    [Header("Practice Scaling")]
    [Range(1, 10)] public int maxTiers = 5;
    [Range(1, 10)] public int ranksPerTier = 5;
    [Range(1, 10)] public int tiersPerDifficulty = 2;

    [Header("Minigame Scaling")]
    [Range(1, 100)] public int baseGoldReward = 20;
    [Range(1, 100)] public int baseReputationReward = 5;

    [Header("Tile Scaling")]
    [Range(5, 10)] public int maxTileLevel = 8;
    public AnimationCurve tileLevelScale;
    public int baseTileCost;

    //
    // PRIVATE DATA
    //
    private uint rank = 0; // Rank will always increase, even when moving up a tier
    private uint tier = 0; // Tier increases have to be bought into
    private Tweener moneyTween;
    //
    //
    //

    void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    void Start()
    {
        UIManager.Instance.ConfigureDataLabels(activeSaveData);
        moneyTween = DOTween.To(() => activeSaveData.tweenMoney, x => activeSaveData.tweenMoney = x, activeSaveData.money, 1f).SetAutoKill(false);
    }

    /// <summary>
    /// This function will return a stat total based on the player's current progression
    /// </summary>
    public float GetStatTotal()
    {
        float total = basePatientStatPool * (1 + (rankStatMultiplier * rank)) * (1 + (tierStatMultiplier * tier));
        return UnityEngine.Random.Range(total * statTotalMinVariance, total);
    }

    /// <summary>
    /// This function will return the current difficulty of patients that should generate.
    /// </summary>
    public PatientDifficulty GetPatientDifficulty()
    {
        if (tier <= tiersPerDifficulty)
        {
            return PatientDifficulty.EASY;
        } else if (tier <= 2 * tiersPerDifficulty)
        {
            return PatientDifficulty.MEDIUM;
        } else
        {
            return PatientDifficulty.HARD;
        }
    }

    /// <summary>
    /// Adjust the player's reputation
    /// </summary>
    public void AdjustReputation(int val)
    {
        activeSaveData.reputation = Math.Max(0, activeSaveData.reputation + val);
    }

    /// <summary>
    /// Adjust the player's money. Ensure the appropriate game systems are updated.
    /// </summary>
    public void AdjustMoney(int val)
    {
        activeSaveData.money = Math.Max(0, activeSaveData.money + val);
        moneyTween.ChangeEndValue(activeSaveData.money, true).Restart();

        UIManager.Instance.RefreshTileList();
        UIManager.Instance.UpdateTileDetailsPanel();
    }

    /// <summary>
    /// Get the player's active money amount.
    /// </summary>
    public int Money() { return activeSaveData.money; }

    /// <summary>
    /// Use the max level and the animation curve to determine how much a tile costs.
    /// </summary>
    public int GetTileLevelUpCost(int level) {
        if (level == maxTileLevel) {
            return -1;
        }
        
        // Tile start at level 1, which is really 0
        float t = level / (float)maxTileLevel;
        return (int)Mathf.Round((float)(baseTileCost * tileLevelScale.Evaluate(t)) / 50f) * 50;
    }

    /// <summary>
    /// Simple check to evaluate if the player can afford something based on the input and the current save value.
    /// </summary>
    public bool CanAfford(int value) {
        return activeSaveData.money >= value;
    }

}