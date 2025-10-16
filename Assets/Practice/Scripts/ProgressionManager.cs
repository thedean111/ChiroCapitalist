using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    private static ProgressionManager instance = null;
    public static ProgressionManager Instance { get { return instance; } }

    [Header("Stat Scaling")]
    [Range(1, 50f)] public float basePatientStatPool = 10f; // Base value to scale stat pool off of
    [Range(0f, 2f)] public float rankStatMultiplier = 0.2f; // Multiplier that additively increases with rank
    [Range(0f, 5f)] public float tierStatMultiplier = 0.5f; // Multiplier that additively increases with tier
    [Range(.1f, 1f)] public float statTotalMinVariance = 1f; // What percent of the stat total should correlate to the minimum bound

    [Header("Practice Scaling")]
    [Range(1, 10)] public int maxTiers = 5;
    [Range(1, 10)] public int ranksPerTier = 5;
    [Range(1, 10)] public int tiersPerDifficulty = 2;

    //
    // PRIVATE DATA
    //

    private uint rank = 0; // Rank will always increase, even when moving up a tier
    private uint tier = 0; // Tier increases have to be bought into

    //
    //
    //

    void Awake()
    {
        if (instance == null) { instance = this; }
    }

    /// <summary>
    /// This function will return a stat total based on the player's current progression
    /// </summary>
    public float GetStatTotal()
    {
        float total = basePatientStatPool * (1 + (rankStatMultiplier * rank)) * (1 + (tierStatMultiplier * tier));
        return Random.Range(total * statTotalMinVariance, total);
    }

    /// <summary>
    /// This function will return the current difficulty of patients that should generate
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

}