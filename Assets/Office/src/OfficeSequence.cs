using UnityEngine;

[CreateAssetMenu(fileName = "OfficeSequence", menuName = "Scriptable Objects/OfficeSequence")]
public class OfficeSequence : ScriptableObject
{
    public RoomPose objectPose;
    public AnimationClip doctorAnimation;
    public AnimationClip patientAnimation;
    public bool useCrossfade;
}
