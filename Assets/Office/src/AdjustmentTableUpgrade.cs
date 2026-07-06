using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureUpgrade", menuName = "Game Data/FurnitureUpgrade")]
public class FurnitureUpgrade : ScriptableObject
{
    public UpgradeMode mode;
    public Mesh mesh; // The mesh to use when the upgrade mode is set to mesh swap
    public GameObject prefab; // The prefab to spawn if upgrade mode is set to prefab
}
public enum UpgradeMode { MeshSwap, Prefab }