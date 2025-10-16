using System.Collections.Generic;
using UnityEngine;

public class Patient : NPC
{
    private PatientData data; // Patient data specifically contains the buildup and action animation that correlates to the NPC's stats

    /// <summary>
    /// When giving this object new data, automatically set the meshes.
    /// </summary>
    public void SetData(PatientData data)
    {
        base.SetData(data);
        this.data = data;
    }
}

public class PatientData : NPCData
{
    public AnimationClip buildup;
    public AnimationClip action;
    
}