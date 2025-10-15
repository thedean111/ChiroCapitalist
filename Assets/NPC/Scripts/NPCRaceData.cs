using UnityEngine;

[CreateAssetMenu(fileName = "newRaceData", menuName = "Game Data/NPC Race", order =1)]
public class NPCRaceData : ScriptableObject
{
    public Mesh head;
    public Mesh[] torso;
    public Mesh[] pants;
    public Mesh[] hair;
    public Mesh[] shoes;
    public Gradient[] skinColor;
}
