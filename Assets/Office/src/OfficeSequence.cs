using UnityEngine;

[CreateAssetMenu(fileName = "OfficeSequence", menuName = "Scriptable Objects/OfficeSequence")]
public class OfficeSequence : ScriptableObject
{
    public RoomPose objectPose;
    public string doctorAnimationName;
    public string patientAnimationName;
}
