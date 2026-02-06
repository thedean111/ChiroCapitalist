using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "RoomPose", menuName = "Scriptable Objects/RoomPose")]
public class RoomPose : ScriptableObject
{
    public List<ObjectPose> decorPose = new();
    public ObjectPose doctorPose;
    public ObjectPose patientPose;
}

[Serializable]
public class ObjectPose {
    public Vector3 localPosition;
    public Quaternion localRotation;

    public ObjectPose(Vector3 pos, Quaternion rot) {
        localPosition = pos;
        localRotation = rot;
    }
}