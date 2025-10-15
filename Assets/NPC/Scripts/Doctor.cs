using UnityEngine;

public class Doctor : NPC
{
    private DoctorData data;

    /// <summary>
    /// When giving this object new data, automatically set the meshes.
    /// </summary>
    public void SetData(DoctorData data)
    {
        this.data = data;
        SetSkinnedMeshes(data.hair, data.head, data.pants, data.shoes, data.torso);
    }

}

public struct DoctorData
{
    public Mesh hair, head, torso, pants, shoes;
    public int level;

}