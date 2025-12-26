using UnityEngine;


public class Doctor : NPC
{
    private DoctorData data; // Doctor data contains various information about stats, levels, etc.

    /// <summary>
    /// When giving this object new data, automatically set the meshes.
    /// </summary>
    public void SetData(DoctorData data)
    {
        base.SetData(data);
        this.data = data;
    }

}

public class DoctorData : NPCData
{
    public int level;
    // TODO: exp, etc.
}