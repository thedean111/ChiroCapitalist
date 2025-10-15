using UnityEngine;

public class Patient : NPC
{
    /// <summary>
    /// When a patient NPC is done with, it should be: moved far away, disabled, and added back to the NPCFactory's stack.
    /// </summary>
    public override void Dismiss()
    {
        base.Dismiss();

        transform.position = Vector3.up * 100;
        NPCFactory.Instance.ReturnPatient(this);
    }
}