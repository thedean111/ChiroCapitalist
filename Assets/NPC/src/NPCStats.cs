using System.Reflection;
using UnityEngine;

public class NPCStats
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public float strength;
    public float technique;
    public float magic;

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    
    //*********************************************************************

    public NPCStats() : this(0f,0f,0f) {}

    public NPCStats(float str, float tec, float mag) {
        strength = str;
        technique = tec;
        magic = mag;
    }

    /// <summary>
    /// Given another set of NPCStats, compute a compatibility factor relative to this one. That is to say, how well this objects stats satisfy the provided stats.
    /// </summary>
    public float ComputeCompatibility(NPCStats other) {
        return 0;
    }

    /// <summary>
    /// Evaluate the patient's greatest need. This will drive choices during the adjustment sequence.
    /// </summary>
    public StatCategory GetDominantStat() {
        if (strength >= technique && strength >= magic) {
            return StatCategory.Strength;
        } else if (technique >= strength && technique >= magic) {
            return StatCategory.Technique;
        }
        return StatCategory.Magic;

    }
}
